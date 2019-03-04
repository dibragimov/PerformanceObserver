using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FarmPerformanceMonitorService
{
    public class Logger
    {
        private string directoryPath;
        private string fullFileName;
        private static Logger _logger;
        private Logger(string dirPath)
        {
            directoryPath = dirPath;
            try
            {
                if (!System.IO.Directory.Exists(directoryPath))
                    System.IO.Directory.CreateDirectory(directoryPath);

                fullFileName = directoryPath + System.IO.Path.DirectorySeparatorChar + "PerfMonitorLogs_{0}.dat";

            }
            catch (Exception ex)
            {
                string eventname = string.Format("Error while starting monitor logs: {0} - {1}",
                    ex.Message, ex.StackTrace);
                string log = "Application";
                string source = "PerformanceCounter";

                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, log);
                EventLog.WriteEntry(source, eventname);
                EventLog.WriteEntry(source, eventname, EventLogEntryType.Error);
            }
        }

        public static Logger Instance(){
            if (_logger == null) _logger = new Logger(Settings.ReportFolder);//System.IO.Directory.GetCurrentDirectory());
            return _logger;
        }

        public void log(string data)
        {
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                    string.Format(fullFileName, DateTime.Now.ToString("yyyyMMdd")), true);
                sw.WriteLine(data);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                string eventname = string.Format("Error while writing monitor logs: {0} - {1}, data buffer: {2}",
                    ex.Message, ex.StackTrace, data);
                string log = "Application";
                string source = "PerformanceCounter";

                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, log);
                EventLog.WriteEntry(source, eventname);
                EventLog.WriteEntry(source, eventname, EventLogEntryType.Error);
            }
        }
    }
}
