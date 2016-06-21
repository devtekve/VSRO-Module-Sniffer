using ModuleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTCPForwarder
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Diagnostics;
    using System.Threading;
    using Framework;
    using ExploitFilter.SecurityApi;

    class Program
    {
        /// <summary>
        /// Escribe en colores
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="tipo">! para azul, - Rojo, + Verde, * Sin icono al inicio pero darkblue</param>
        public static void echo(string mensaje, string tipo = "!")
        {
            if (tipo == "!")
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("[!] " + mensaje);
                Console.ForegroundColor = ConsoleColor.Black;

            }
            else if (tipo == "-")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[-] " + mensaje);
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (tipo == "+")
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("[+] " + mensaje);
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (tipo == "*")
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(mensaje);
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        class Context
        {
            public Socket Socket { get; set; }
            public Security Security { get; set; }
            public TransferBuffer Buffer { get; set; }
            public Security RelaySecurity { get; set; }

            public Context()
            {
                Socket = null;
                Security = new Security();
                RelaySecurity = null;
                Buffer = new TransferBuffer(8192);
            }
        }

        static void Main(string[] args)
        {
            ModuleInjector.Config.LoadEverything();
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Title = "Packet Analyzer by [ROOT].";
            try
            {
                String local_host;
                Int32 local_port;

                String remote_host;
                Int32 remote_port;

                local_host = FilterMain.cRemoteIP;
                local_port = FilterMain.FindThisPort;

                remote_host = FilterMain.cRemoteIP;
                remote_port = FilterMain.ReplaceByThisPort;

                Context local_context = new Context();
                local_context.Security.GenerateSecurity(true, false, true);

                Context remote_context = new Context();

                remote_context.RelaySecurity = local_context.Security;
                local_context.RelaySecurity = remote_context.Security;

                List<Context> contexts = new List<Context>();
                contexts.Add(local_context);
                contexts.Add(remote_context);

                using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    server.Bind(new IPEndPoint(IPAddress.Parse(local_host), local_port));
                    server.Listen(1);

                    local_context.Socket = server.Accept();
                }

                using (local_context.Socket)
                {
                    using (remote_context.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        remote_context.Socket.Connect(remote_host, remote_port);
                        while (true)
                        {
                            if (Console.KeyAvailable == true) // Application event processing
                            {
                                ConsoleKeyInfo key = Console.ReadKey(true);
                                if (key.Key == ConsoleKey.Escape)
                                {
                                    break;
                                }
                            }

                            foreach (Context context in contexts) // Network input event processing
                            {
                                if (context.Socket.Poll(0, SelectMode.SelectRead))
                                {
                                    int count = context.Socket.Receive(context.Buffer.Buffer);
                                    if (count == 0)
                                    {
                                        throw new Exception("The remote connection has been lost.");
                                    }
                                    context.Security.Recv(context.Buffer.Buffer, 0, count);
                                }
                            }

                            foreach (Context context in contexts) // Logic event processing
                            {
                                List<Packet> packets = context.Security.TransferIncoming();
                                if (packets != null)
                                {
                                    foreach (Packet packet in packets)
                                    {
                                        if (packet.Opcode == 0x5000 || packet.Opcode == 0x9000) // ignore always
                                        {
                                        }
                                        else if (packet.Opcode == 0x2001)
                                        {
                                            string ModuleName = packet.ReadAscii();
                                            if (context == remote_context) // ignore local to proxy only
                                            {
                                                Console.WriteLine("Remote Context: Module: " + ModuleName);
                                                context.RelaySecurity.Send(packet); // proxy to remote is handled by API                                           
                                            }
                                            else if (context == local_context) // ignore local to proxy only
                                            {
                                                Console.WriteLine("Local Context: Module: " + ModuleName);
                                                context.RelaySecurity.Send(packet); // proxy to remote is handled by API                                            
                                            }
                                        }
                                        else if (packet.Opcode == 0x7025)
                                        {
                                            int chat_type = packet.ReadUInt8();
                                            if (chat_type == 3)
                                            {
                                                uint id = packet.ReadUInt8();
                                                string message = packet.ReadAscii();
                                                if (message.ToLower().Contains("envia") || message.ToLower().Contains("send"))
                                                {
                                                    int JID = Convert.ToInt32(message.Replace("envia ", "").Replace("send ", ""));
                                                    /* 01 A9 61 2A 02 FA FF 05 07 03 00 00 00 */
                                                    Console.Title = "Envia recibido!";
                                                    Packet move = new Packet(0x7021);
                                                    move.WriteUInt8(0x01);
                                                    move.WriteUInt8(0xA9);
                                                    move.WriteUInt8(0x61);
                                                    move.WriteUInt8(0x2A);
                                                    move.WriteUInt8(0x02);
                                                    move.WriteUInt8(0xFA);
                                                    move.WriteUInt8(0xFF);
                                                    move.WriteUInt8(0x05);
                                                    move.WriteUInt8(0x07);
                                                    move.WriteUInt8(JID);
                                                    move.WriteUInt8(0x00);
                                                    move.WriteUInt8(0x00);
                                                    move.WriteUInt8(0x00);
                                                    context.RelaySecurity.Send(move);
                                                }
                                            }
                                            context.RelaySecurity.Send(packet);
                                        }
                                       
                                        else
                                        {
                                            context.RelaySecurity.Send(packet);
                                        }
                                    }
                                }
                            }

                            foreach (Context context in contexts) // Network output event processing
                            {
                                if (context.Socket.Poll(0, SelectMode.SelectWrite))
                                {
                                    List<KeyValuePair<TransferBuffer, Packet>> buffers = context.Security.TransferOutgoing();
                                    if (buffers != null)
                                    {
                                        foreach (KeyValuePair<TransferBuffer, Packet> kvp in buffers)
                                        {
                                            TransferBuffer buffer = kvp.Key;
                                            Packet packet = kvp.Value;

                                            byte[] packet_bytes = packet.GetBytes();
                                            Console.WriteLine("[{0}][{1:X4}][{2} bytes]{3}{4}{6}{5}{6}", context == local_context ? "GS->AS" : "AS->GS", packet.Opcode, packet_bytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Utility.HexDump(packet_bytes), Environment.NewLine);

                                            while (true)
                                            {
                                                int count = context.Socket.Send(buffer.Buffer, buffer.Offset, buffer.Size, SocketFlags.None);
                                                buffer.Offset += count;
                                                if (buffer.Offset == buffer.Size)
                                                {
                                                    break;
                                                }
                                                Thread.Sleep(1);
                                            }
                                        }
                                    }
                                }
                            }

                            Thread.Sleep(1); // Cycle complete, prevent 100% CPU usage
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
   
}
