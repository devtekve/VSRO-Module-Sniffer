using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Framework;
using ModuleInjector.SecurityApi;
using System.Text;

namespace ModuleInjector.NetEngine
{
    sealed class GatewayContext
    {
        Socket m_ClientSocket = null;
        AsyncServer.E_ServerType m_HandlerType;
        AsyncServer.delClientDisconnect m_delDisconnect;
        object m_Lock = new object();
        Socket m_ModuleSocket = null;
        byte[] m_LocalBuffer = new byte[8192];
        byte[] m_RemoteBuffer = new byte[8192];
        public static bool envia = false;

        Security m_LocalSecurity = new Security();
        Security m_RemoteSecurity = new Security();


        static Queue<Packet> m_LastPackets = new Queue<Packet>(20);
        DateTime m_StartTime = DateTime.Now;
        ulong m_BytesRecvFromClient = 0;
        public static List<string> iplist = new List<string>();
        public static List<string> netcafeiplist = new List<string>();
        double GetBytesPerSecondFromClient()
        {
            double res = 0.0;

            TimeSpan diff = (DateTime.Now - m_StartTime);
            if (m_BytesRecvFromClient > int.MaxValue)
                m_BytesRecvFromClient = 0;

            if (m_BytesRecvFromClient > 0)
            {
                try
                {
                    unchecked
                    {
                        double div = diff.TotalSeconds;
                        if (diff.TotalSeconds < 1.0)
                            div = 1.0;
                        res = Math.Round((m_BytesRecvFromClient / div), 2);
                    }
                }
                catch (Exception e)
                {
                    Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
                }
            }

            return res;
        }

        public GatewayContext(Socket ClientSocket, AsyncServer.delClientDisconnect delDisconnect)
        {
            this.m_delDisconnect = delDisconnect;
            this.m_ClientSocket = ClientSocket;
            this.m_HandlerType = AsyncServer.E_ServerType.GatewayServer;
            this.m_ModuleSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                this.m_ModuleSocket.Connect(new IPEndPoint(IPAddress.Parse(FilterMain.cRemoteIP), FilterMain.cRealGatewayPort));
                this.m_LocalSecurity.GenerateSecurity(true, true, true);
                this.DoRecvFromClient();
                Send(false);
            }
            catch (Exception e)
            {
                Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
            }
        }

        public static void DumpLastPackets()
        {
            if (m_LastPackets.Count == 0) return;

            try
            {

                for (int i = 0; i < 20; i++)
                {
                    Packet p = m_LastPackets.Dequeue();

                    byte[] packet_bytes = p.GetBytes();


                }
            }
            catch { }
        }

        void DisconnectModuleSocket()
        {
            try
            {
                if (this.m_ModuleSocket != null)
                {
                    this.m_ModuleSocket.Close();
                }

                this.m_ModuleSocket = null;
            }
            catch (Exception e)
            {
                Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
            }
        }

        private static byte[] Replace(byte[] input, byte[] pattern, byte[] replacement)
        {
            if (pattern.Length == 0)
            {
                return input;
            }

            List<byte> result = new List<byte>();

            int i;

            for (i = 0; i <= input.Length - pattern.Length; i++)
            {
                bool foundMatch = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (input[i + j] != pattern[j])
                    {
                        foundMatch = false;
                        break;
                    }
                }

                if (foundMatch)
                {
                    result.AddRange(replacement);
                    i += pattern.Length - 1;
                }
                else
                {
                    result.Add(input[i]);
                }
            }

            for (; i < input.Length; i++)
            {
                result.Add(input[i]);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Handles the reception of incoming packets
        /// </summary>
        /// <param name="iar"></param>
        void OnReceive_FromServer(IAsyncResult iar)
        {
            
            lock (m_Lock)
            {
                try
                {
                    int nReceived = m_ModuleSocket.EndReceive(iar);
                    if (nReceived > 0)
                    {

                        m_RemoteSecurity.Recv(m_RemoteBuffer, 0, nReceived);

                        List<Packet> RemotePackets = m_RemoteSecurity.TransferIncoming();

                        if (RemotePackets != null)
                        {
                            foreach (Packet _pck in RemotePackets)
                            {
                                if (_pck.Opcode == 0xB204 || _pck.Opcode == 0xB009 || _pck.Opcode == 0xA003)
                                {
                                }
                                else
                                {
                                    byte[] bytes = _pck.GetBytes();

                                    //Console.WriteLine("[S->C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine);
                                    Program.echo("[GATEWAY]", "*");
                                    Program.echo(string.Format("[S->C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine), "*");
                                    IO.Logger.LogIt(string.Format("[S->C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine), "GATEWAY");
                                }
                                if (_pck.Opcode == 0x5000 || _pck.Opcode == 0x9000)
                                {
                                    Send(true);
                                    continue;
                                }
                                if (_pck.Opcode == 0xA003)
                                {
                                    byte[] bytes = _pck.GetBytes();

                                    Int16 FindThisPort = 15885;
                                    Int16 ReplaceByThisPort = 14885;

                                    byte[] B_FindThisPort = BitConverter.GetBytes(FilterMain.FindThisPort);
                                    byte[] B_ReplaceByThisPort = BitConverter.GetBytes(FilterMain.ReplaceByThisPort);

                                    byte[] new_cert = Replace(bytes, B_FindThisPort, B_ReplaceByThisPort);

                                    Packet Cert = new Packet(0xA003,false,true,new_cert); // Avatar blues
                                    m_LocalSecurity.Send(Cert); // Send to server files
                                    Send(false); // Send to server

                                    // Avoid packet sent to others.
                                    continue;


                                }

                                m_LocalSecurity.Send(_pck);
                                Send(false);
                            }
                        }
                    }
                    else
                    {

                        this.DisconnectModuleSocket();
                        this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                        return;
                    }
                    DoRecvFromServer();
                }
                catch (Exception e)
                {
                    Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
                    this.DisconnectModuleSocket();
                    this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                }
            }
        }

        /// <summary>
        /// Handles the sending of outgoing packets
        /// </summary>
        /// <param name="iar"></param>
        void OnReceive_FromClient(IAsyncResult iar)
        {
            lock (m_Lock)
            {
                try
                {
                    int nReceived = m_ClientSocket.EndReceive(iar);

                    if (nReceived > 0)
                    {


                        m_LocalSecurity.Recv(m_LocalBuffer, 0, nReceived);

                        List<Packet> ReceivedPackets = m_LocalSecurity.TransferIncoming();

                        if (ReceivedPackets != null)
                        {

                            foreach (Packet _pck in ReceivedPackets)
                            {
                                
                                    if (_pck.Opcode == 0x7204 || _pck.Opcode == 0x2002 || _pck.Opcode == 0x7005 || _pck.Opcode == 0x7009)
                                    {
                                    }
                                    else
                                    {
                                        byte[] bytes = _pck.GetBytes();
                                    Program.echo("[GATEWAY]", "*");
                                    Program.echo(string.Format("[C->S][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine), "*");
                                    IO.Logger.LogIt(string.Format("[C->S][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine), "GATEWAY");
                                }

                                if (_pck.Opcode == 0x2001)
                                {
                                    DoRecvFromServer();
                                    continue;
                                }

                                if (_pck.Opcode == 0x5000 || _pck.Opcode == 0x9000)
                                {
                                    Send(false);
                                    continue;
                                }

                                if (_pck.Opcode == 0x6102)
                                {
                                    int testint = _pck.ReadUInt8();
                                    string user = _pck.ReadAscii();
                                    string pw = _pck.ReadAscii();
                                    int locale = _pck.ReadUInt16();
                                    Packet packet = new Packet(0x6102);
                                    packet.WriteUInt8((byte)0x16);
                                    packet.WriteAscii(user);
                                    packet.WriteAscii(pw);
                                    packet.WriteUInt16((ushort)0x40);
                                    m_RemoteSecurity.Send(packet); // Send to server files
                                    Send(true); // Send to server
                                }

                                if (m_LastPackets.Count > 100)
                                {
                                    m_LastPackets.Clear();
                                }
                                Packet CopyOfPacket = _pck;
                                m_LastPackets.Enqueue(CopyOfPacket);


                                m_RemoteSecurity.Send(_pck);
                                Send(true);
                            }
                        }

                    }
                    else
                    {
                        this.DisconnectModuleSocket();
                        this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                        return;
                    }


                    this.DoRecvFromClient();
                }
                catch (Exception e)
                {
                    Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
                    this.DisconnectModuleSocket();
                    this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                }
            }
        }

        /// <summary>
        /// Sends a packet to socket
        /// </summary>
        /// <param name="ToHost">True: Sends to RemoteSecurity; False: Sends to LocalSecurity</param>
        public void Send(bool ToHost)
        {
            lock (m_Lock)
                foreach (var p in (ToHost ? m_RemoteSecurity : m_LocalSecurity).TransferOutgoing())
                {
                    Socket ss = (ToHost ? m_ModuleSocket : m_ClientSocket);

                    ss.Send(p.Key.Buffer);
                    if (ToHost)
                    {
                        try
                        {
                            m_BytesRecvFromClient += (ulong)p.Key.Size;

                            //MAX BPS
                            double nBps = GetBytesPerSecondFromClient();
                            if (nBps > FilterMain.dMaxBytesPerSec_Agent)
                            {
                                Console.WriteLine("GatewayServer::Client disconnected due to high BPS: {0} / {1}", ((IPEndPoint)(m_ClientSocket.RemoteEndPoint)).Address, nBps);

                                this.DisconnectModuleSocket();
                                this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                            }
                        }
                        catch (Exception e)
                        {
                            Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
                            this.DisconnectModuleSocket();
                            this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
                        }

                    }
                }
        }

        /// <summary>
        /// Initializes listening of incoming packets
        /// </summary>
        void DoRecvFromServer()
        {
            try
            {
                m_ModuleSocket.BeginReceive(m_RemoteBuffer, 0, m_RemoteBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnReceive_FromServer), null);
            }
            catch (Exception e)
            {
                Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
                this.DisconnectModuleSocket();
                this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
            }
        }

        /// <summary>
        /// Initializes listening of outgoing packets
        /// </summary>
        void DoRecvFromClient()
        {
            try
            {
                m_ClientSocket.BeginReceive(m_LocalBuffer, 0, m_LocalBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(OnReceive_FromClient), null);

            }
            catch (Exception AnyEx)
            {
                try
                {

                }
                catch { }
                this.DisconnectModuleSocket();
                this.m_delDisconnect.Invoke(ref m_ClientSocket, m_HandlerType);
            }
        }
    }
}
