using Framework;
using ModuleInjector.SecurityApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTCPForwarder
{
    class TcpForwarderSlim
    {

        static Security m_SilkroadSecurity = new Security();

        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Start(IPEndPoint local, IPEndPoint remote)
        {
            _mainSocket.Bind(local);
            _mainSocket.Listen(10);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TcpForwarderSlim();
                var state = new State(source, destination._mainSocket);
                destination.Connect(remote, source);
                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
            }
        }

        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(_mainSocket, destination);
            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            var state = (State)result.AsyncState;
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    byte[] m_Buffer = state.Buffer;
                    m_SilkroadSecurity.Recv(m_Buffer, 0, m_Buffer.Length);
                    List<Packet> RemotePackets = m_SilkroadSecurity.TransferIncoming();

                    if (RemotePackets != null)
                    {
                        foreach (Packet _pck in RemotePackets)
                        {
                            if (_pck.Opcode == 0x5000 || _pck.Opcode == 0x9000 || _pck.Opcode == 0x0000) // ignore always
                            {
                            }
                            else
                            {
                                byte[] bytes = _pck.GetBytes();

                                //Console.WriteLine("[S->C][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine);
                                Program.echo(string.Format("[UNKNOWN DIRECTION YET][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine), "*");
                                ModuleInjector.IO.Logger.LogIt(string.Format("[SUNKNOWN DIRECTION YET][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", (object)_pck.Opcode, (object)bytes.Length, _pck.Encrypted ? (object)"[Encrypted]" : (object)"", _pck.Massive ? (object)"[Massive]" : (object)"", (object)Environment.NewLine, (object)Utility.HexDump(bytes), (object)Environment.NewLine), "GATEWAY");
                            }
                        }
                    }
                    Console.WriteLine("bytesRead: " + bytesRead);
                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                     state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }

            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[8192];
            }
        }
    }
}
