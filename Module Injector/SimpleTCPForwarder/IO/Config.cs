using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Threading;
using System.IO;

namespace ModuleInjector
{
    class Config
    {
        public static iniFile cfg = new iniFile("settings/config.ini");
        public static void LoadEverything()
        {
            try
            {
                if (!File.Exists("settings/config.ini"))
                {
                    Console.WriteLine("Missing config.ini!");
                }
                else
                {
                    // Read config
                    FilterMain.cRemoteIP = cfg.IniReadValue("SETTINGS", "IP");
                    FilterMain.cRealGatewayPort = int.Parse(cfg.IniReadValue("SETTINGS", "GATEWAY"));
                    FilterMain.cRealAgentPort = int.Parse(cfg.IniReadValue("SETTINGS", "AGENT"));
                    FilterMain.cBindIP = cfg.IniReadValue("SETTINGS", "BINDIP");
                    FilterMain.cFakeGport = int.Parse(cfg.IniReadValue("SETTINGS", "GWBIND"));
                    FilterMain.cFakeAport = int.Parse(cfg.IniReadValue("SETTINGS", "AGBIND"));
                    FilterMain.FindThisPort = Int16.Parse(cfg.IniReadValue("SETTINGS", "REAL_MODULE_PORT "));
                    FilterMain.ReplaceByThisPort = Int16.Parse(cfg.IniReadValue("SETTINGS", "FAKE_NEW_MODULE_PORT "));


                }
            }
            catch {
                Console.WriteLine("Probably missing config.ini or something is wrong.");
            }
        }
    }
}
