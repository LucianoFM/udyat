using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class
{
    public class Log
    {
        public const string LOG_FILE = "UdyatLog.log";
        public static string logFullFileName;
        public static bool useLog = true;

        public static void AddLog(string pProcess, string pLogText)
        {
            if (!useLog) { return; }
            using (StreamWriter w = File.AppendText(logFullFileName))
            {
                LogError(pProcess, pLogText, w);
            }
        }

        public static void AddStartLog(bool pTestMode, string pTempPath, string pTargetPath)
        {
            if (!useLog) { return; }
            using (StreamWriter w = File.AppendText(logFullFileName))
            {
                LogStart(pTestMode, pTempPath, pTargetPath, w);
            }
        }

        public static void AddFileMovingLog(string pFile)
        {
            if (!useLog) { return; }
            using (StreamWriter w = File.AppendText(logFullFileName))
            {
                LogFileMoving(pFile, w);
            }
        }

        public static void AddStartingClockLog(string pClockType, DateTime pDateTime)
        {
            if (!useLog) { return; }
            using (StreamWriter w = File.AppendText(logFullFileName))
            {
                LogStartingClock(pClockType, pDateTime, w);
            }
        }

        public static void LogError(string pProcess, string logMessage, TextWriter w)
        {
            w.Write("\r\n");
            w.WriteLine("{0} {1}", string.Format("{0:dd/MM/yyyy HH:mm:ss tt}", mainClass.internalClock), "");
            w.WriteLine("  :");
            w.WriteLine("  :{0}", pProcess);
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        public static void LogStartingClock(string pClockType, DateTime pDateTime, TextWriter w)
        {
            w.Write("\r\n");
            w.WriteLine("{0}", "Inicializando relógio");
            w.WriteLine("  :{0} {1}", string.Format("{0:dd/MM/yyyy HH:mm:ss tt}", pDateTime), "");
            w.WriteLine("  :{0}", pClockType);
        }

        public static void LogStart(bool pTestMode, string pTempPath, string pTargetPath, TextWriter w)
        {
            w.Write("\r\n");
            w.Write("\r\n");
            w.WriteLine("{0} {1}", string.Format("{0:dd/MM/yyyy HH:mm:ss tt}", mainClass.internalClock), "");
            w.WriteLine("  :");
            if (pTestMode)
            {
                w.WriteLine("  :{0}", "INÍCIO - Modo Teste");
            }
            else
            {
                w.WriteLine("  :{0}", "INÍCIO");
            }            
            w.WriteLine("  :{0}", "IP: " + Util.GetLocalIpAddress());
            w.WriteLine("  :{0}", "Temp: " + pTempPath);
            w.WriteLine("  :{0}", "Target: " + pTargetPath);
            w.WriteLine("-------------------------------");
        }

        public static void LogFileMoving(string pFile, TextWriter w)
        {
            w.WriteLine("{0} {1}", string.Format("{0:dd/MM/yyyy HH:mm:ss tt}", mainClass.internalClock), "");
            w.WriteLine("  :{0}", "Movido: " + pFile);
        }

    }
    
}
