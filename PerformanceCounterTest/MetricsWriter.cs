using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PerformanceCounterTest
{
    public class MetricsWriter
    {
        private string directoryPath;
        private string fullFileName;
        public MetricsWriter(string dirPath)
        {
            directoryPath = dirPath;
            try
            {
                if (!System.IO.Directory.Exists(directoryPath))
                    System.IO.Directory.CreateDirectory(directoryPath);

                fullFileName = directoryPath + System.IO.Path.DirectorySeparatorChar + "Metrics_{0}.dat";

            }
            catch (Exception ex)
            {
                // TBD Ignore Error
            }
        }

        public void WriteToFile(string data)
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
                string eventname = string.Format("Error while writing perf metrics: {0} - {1}, data buffer: {2}",
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
