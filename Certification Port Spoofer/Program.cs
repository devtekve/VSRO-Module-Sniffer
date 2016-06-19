using System;
using ModuleInjector.NetEngine;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ModuleInjector
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        int uFlags);

        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;

        static void ConsolePoolThread()
        {
            while (true)
            {
               string cmd = Console.ReadLine();

                if (cmd == "/clear")
                {
                    Console.Clear();
                }
                else if (cmd == "/topmost")
                {
                    IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

                    SetWindowPos(hWnd,
                        new IntPtr(HWND_TOPMOST),
                        0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE);
                }
                else if (cmd == "/hide")
                {
                    IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

                    SetWindowPos(hWnd,
                        new IntPtr(1),
                        0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE);
                }
                else if (cmd == "/envia")
                {
                    GatewayContext.envia = true;
                }
                else if (cmd != "")
                {
                    IO.Logger.LogIt("NOTA: "+cmd, "GATEWAY");
                }

                    Thread.Sleep(1);
            }
        }
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Title = "Packet Analyzer by [ROOT].";
            echo("Packet Analyzer by [ROOT], thanks Goofie, PushEDX, Florian0, Neks.");

            Config.LoadEverything();

            AsyncServer GatewayServer = new AsyncServer();
            AsyncServer AgentServer = new AsyncServer();

            GatewayServer.Start(FilterMain.cBindIP, FilterMain.cFakeGport, AsyncServer.E_ServerType.GatewayServer, FilterMain.cRealGatewayPort);
            Console.Write("\r\n\r\n");
            echo("\r\n/clear borra todo el log \r\n/topmost mantiene esta ventana siempre visible\r\n/hide permite esconder esta pantalla\r\n-----------------------------------\r\n");

            new Thread(ConsolePoolThread).Start();

        }
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
    }
}
