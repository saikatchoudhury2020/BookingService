using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;

namespace logWriter
{
    public class LogWriter
    {
        private string m_exePath = string.Empty;
        public LogWriter(string logMessage)
        {
            LogWrite(logMessage);
        }

        public LogWriter()
        {
        }

        public void LogWrite(string logMessage)
        {
            m_exePath = Digimaker.Config.Custom.AppSettings["braathenlog"].ToString(); //"F:";
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "braathenlog.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd"),
                     DateTime.Now.ToLongTimeString());
                txtWriter.WriteLine("Braathen      :");
                txtWriter.WriteLine("Order :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
            }
        }
    }
}