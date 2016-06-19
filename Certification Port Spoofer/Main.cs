using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
namespace ModuleInjector
{
    sealed class FilterMain
    {
        // Listen IP, PORTS
        public static string cBindIP = "127.0.0.1"; // Local server
        public static int cFakeGport = 5001; // Gatewaylisten
        public static int cFakeAport = 5002; // Agentlisten

        // Remote IP, PORTS
        public static string cRemoteIP = ""; // Remote server
        public static int cRealGatewayPort = 15779; // Gatewaysend
        public static int cRealAgentPort = 15884; // Agentsend
        public static string cRemoteAgentIP = ""; // Remote server del agente, sera dinamico.

        // Spoof Ports!
        public static Int16 FindThisPort = 15885; //Original Module Port, we will spoof it so the target module starts listening in another port. 
        public static Int16 ReplaceByThisPort = 14885; //Port that target module will be listening on

        //2000 bytes / sec
        public const double dMaxBytesPerSec_Gateway = 1000.00;
        //3000 bytes / sec
        public const double dMaxBytesPerSec_Agent = 1000.00;
    }
}