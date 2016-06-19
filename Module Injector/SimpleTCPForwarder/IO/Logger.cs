using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModuleInjector.IO
{
    public static class Logger
    {

        public static void LogIt(string Text, string module = null)
        {
            DateTime d = DateTime.Now.Date;
            DateTime dateOnly = d.Date;
            string fecha = dateOnly.ToString("MM/dd/yyyy");

            /*  MethodInvoker action = delegate { ventana.txtLogBox.Text += sBuilder.ToString(); };
              ventana.txtLogBox.BeginInvoke(action);*/


            //write this record to log file
            try
            {
                using (StreamWriter sWriter = File.AppendText(@"logs\log_["+module+"]_[" + fecha.Replace("/", "-") + "  " + "" +  "]_.txt"))
                {
                    sWriter.Write(String.Format("[{0}] ↓\r\n{1} \r\n", DateTime.Now, Text));
                    sWriter.Close();
                }
            }
            catch (Exception e)
            {
                Random rnd1 = new Random();
                using (StreamWriter sWriter = File.AppendText(@"logs\EXCEPTION_ERROR_[" + fecha.Replace("/", "-") + "]_[" + rnd1.Next(9000) + "].txt"))
                {
                    sWriter.Write(String.Format("[{0}] -> Excepción no controlada \r\n======================================\r\n{1}\r\n======================================\r\n", DateTime.Now, e.ToString()));
                    sWriter.Write(String.Format("[{0}] -> {1} \r\n", DateTime.Now, Text));
                    sWriter.Close();
                }
            }


        }

        public static void Exception(string Text)
        {
            DateTime d = DateTime.Now.Date;
            DateTime dateOnly = d.Date;
            string fecha = dateOnly.ToString("MM/dd/yyyy");



            //write this record to log file
            try
            {
                Random rnd1 = new Random();
                using (StreamWriter sWriter = File.AppendText(@"logs\EXCEPTION_ERROR_[" + fecha.Replace("/", "-") + "]_[" + rnd1.Next(9999999) + "].txt"))
                {
                    sWriter.Write(String.Format("[{0}] -> {1} \r\n", DateTime.Now, Text));
                    sWriter.Close();
                }
            }
            catch (Exception e)
            {
                Random rnd1 = new Random();
                using (StreamWriter sWriter = File.AppendText(@"logs\EXCEPTION_ERROR_[" + fecha.Replace("/", "-") + "]_[" + rnd1.Next(9999999) + "].txt"))
                {
                    sWriter.Write(String.Format("[{0}] -> Excepción no controlada \r\n======================================\r\n{1}\r\n======================================\r\n", DateTime.Now, e.ToString()));
                    sWriter.Write(String.Format("[{0}] -> {1} \r\n", DateTime.Now, Text));
                    sWriter.Close();
                }
            }


        }
    }
}
