using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModuleInjector.NetEngine
{
    sealed class AsyncServer
    {
        Socket m_ListenerSock = null;
        E_ServerType m_ServerType;

        static int m_ClientCount_Gateway = 0, m_ClientCount_Agent = 0;

        ManualResetEvent m_Waiter = new ManualResetEvent(false);
        Thread m_AcceptInitThread = null;

        public int GatewayClientCount
        {
            get
            {
                return m_ClientCount_Gateway;
            }
        }

        public int AgentClientCount
        {
            get
            {
                return m_ClientCount_Agent;
            }
        }


        public enum E_ServerType : byte
        {
            GatewayServer,
            AgentServer
        }
        public delegate void delClientDisconnect(ref Socket ClientSocket, E_ServerType HandlerType);




        public bool Start(string BindAddr, int nPort, E_ServerType ServType, int toPort = 0)
        {
            bool res = false;
            if (m_ListenerSock != null)
            {
                throw new Exception("AsyncServer::Trying to start server on socket which is already in use");
            }

            m_ServerType = ServType;

            m_ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                m_ListenerSock.Bind(new IPEndPoint(IPAddress.Parse(BindAddr), nPort));
                m_ListenerSock.Listen(5);

                m_AcceptInitThread = new Thread(AcceptInitThread);
                m_AcceptInitThread.Start();

                Program.echo(String.Format("Listen:["+ BindAddr + ":"+ nPort + "] [{0}] => [" + FilterMain.cRemoteIP + ":" + toPort + "]", ServType.ToString()),"+");
            }
            catch (SocketException SocketEx)
            {
                Program.echo(String.Format("AsyncServer::Could not bind/listen/BeginAccept socket. Exception: {0}", SocketEx.ToString()),"-");
            }
            
            return res;
        }

        void AcceptInitThread()
        {
            while (m_ListenerSock != null)
            {
                m_Waiter.Reset();
                try
                {
                    
                    m_ListenerSock.BeginAccept(
                        new AsyncCallback(AcceptConnectionCallback), null
                        );
                }
                catch (Exception AnyEx)
                {
                }
                m_Waiter.WaitOne();
            }
        }

        void AcceptConnectionCallback(IAsyncResult iar)
        {
            Program.echo("Conexion recibida...");
            Socket ClientSocket = null;

            m_Waiter.Set();

            try
            {
                ClientSocket = m_ListenerSock.EndAccept(iar);
            }
            catch (SocketException SocketEx)
            {
                Console.WriteLine("AsyncServer::AcceptConnectionCallback()::SocketException while EndAccept. Exception: {0}", SocketEx.ToString());
            }
            catch (ObjectDisposedException ObjDispEx)
            {
                Console.WriteLine("AsyncServer::AcceptConnectionCallback()::ObjectDisposedException while EndAccept. Is server shutting down ? Exception: {0}", ObjDispEx.ToString());
            }

            try
            {
                switch (m_ServerType)
                {
                    case E_ServerType.GatewayServer:
                        {
                            new GatewayContext(ClientSocket, OnClientDisconnect);

                           
                            m_ClientCount_Gateway++;
                            //Console.Title = string.Format("Client count [GatewayServer: {0}] [AgentServer: {1}]", GatewayClientCount, AgentClientCount);
                        }
                        break;
                    default:
                        {
                            Console.WriteLine("AsyncServer::AcceptConnectionCallback()::Unknown server type");
                        }
                        break;
                }
            }
            catch (SocketException SocketEx)
            {
                Console.WriteLine("AsyncServer::AcceptConnectionCallback()::Error while starting context. Exception: {0}", SocketEx.ToString());
            }
        }

        void OnClientDisconnect(ref Socket ClientSock, E_ServerType HandlerType)
        {
            if (ClientSock == null) 
                return;

            switch (HandlerType)
            {
                case E_ServerType.GatewayServer:
                    {

               
                        m_ClientCount_Gateway--;
                        //Console.Title = string.Format("Client count [GatewayServer: {0}] [AgentServer: {1}]", GatewayClientCount, AgentClientCount);
                    }
                    break;
                case E_ServerType.AgentServer:
                    {

                     
                        m_ClientCount_Agent--;
                        //Console.Title = string.Format("Client count [GatewayServer: {0}] [AgentServer: {1}]", GatewayClientCount, AgentClientCount);
                    }
                    break;
            }

            try
            {
                ClientSock.Close();

            }
            catch (SocketException SocketEx)
            {
                Console.WriteLine("AsyncServer::OnClientDisconnect()::Error closing socket. Exception: {0}", SocketEx.ToString());
            }
            catch (ObjectDisposedException ObjDispEx)
            {
                Console.WriteLine("AsyncServer::OnClientDisconnect()::Error closing socket (socket already disposed?). Exception: {0}", ObjDispEx.ToString());
            }
            catch (Exception e)
            {
                Program.echo(String.Format("Exception: {0}", e.ToString()), "-");
            }

            
            ClientSock = null;
            GC.Collect();
        }
    }

}
