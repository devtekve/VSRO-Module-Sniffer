using ModuleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTCPForwarder
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.LoadEverything();
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Title = "Packet Analyzer by [ROOT].";
            new TcpForwarderSlim().Start(
                new IPEndPoint(IPAddress.Parse(FilterMain.cRemoteIP), FilterMain.FindThisPort),
                new IPEndPoint(IPAddress.Parse(FilterMain.cRemoteIP), FilterMain.ReplaceByThisPort));
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
