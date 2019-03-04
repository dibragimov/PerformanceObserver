using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;

namespace PerformanceCounterTest
{
    class Program
    {
        static MetricsWriter collectWriter = new MetricsWriter(System.IO.Directory.GetCurrentDirectory());
                                    
        static void Main()
        {
            //Console.WriteLine("Recording performance metrics...");
            //var processorCategory = PerformanceCounterCategory.GetCategories()
            //    .FirstOrDefault(cat => cat.CategoryName == "Processor");
            //var countersInCategory = processorCategory.GetCounters("_Total");
            
            //var sqlServerMemoryCategory = PerformanceCounterCategory.GetCategories()
            //    .FirstOrDefault(cat => cat.CategoryName == "SQLServer:Memory Manager");
            //PerformanceCounter sqlPerformanceCounter = sqlServerMemoryCategory.GetCounters().First(cnt => cnt.CounterName.StartsWith("Total Server "));

            ////var processorCategory1 = PerformanceCounterCategory.GetCategories().
            ////    //Where(cat => cat.CategoryName.ToLower().StartsWith("memory") || cat.CategoryName.ToLower().StartsWith("sql")).
            ////    Where(cat => cat.CategoryName.ToLower().StartsWith("memory") || cat.CategoryName.StartsWith("SQLServer:Memory Manager")).
            ////    SelectMany(c => c.GetCounters()).Select(cat => cat.CounterName).
            ////    ToArray<string>();
            ////foreach (var item in processorCategory1)
            ////{
            ////    Console.WriteLine(item);
            ////}
            //PerformanceCounter processorPerformanceCounter = countersInCategory.First(cnt => cnt.CounterName == "% Processor Time");
            //DisplayCounter(new PerformanceCounter[]{sqlPerformanceCounter, processorPerformanceCounter});
            
            Console.WriteLine("Recording performance metrics...");
            PerformanceTesterLibrary.PerformanceChecker pch = PerformanceTesterLibrary.PerformanceChecker.GetInstance(
                System.IO.Directory.GetCurrentDirectory(), Settings.ConnectionString, "SQL Farm N");
            pch.CollectCounter(DateTime.Now.AddSeconds(10));
            Console.WriteLine("Reading from file...");
            //pch.SaveReportAsPDF();

            //// test if it creates reports
            List<TimeSpan> statusCheckTimes = new List<TimeSpan>();
            var statusCheckTimesSplit = Settings.RunTime.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (statusCheckTimesSplit.Any())
            {
                statusCheckTimesSplit.Sort();

                foreach (string item in statusCheckTimesSplit)
                {
                    string[] parts = item.Split(new char[] { ':' });
                    TimeSpan tsParts = new TimeSpan(Int32.Parse(parts[0]), Int32.Parse(parts[1]), 0);
                    statusCheckTimes.Add(tsParts);
                }
            }
            for (int i = 0; i < 5; i++)  //// get the report for 5 days. Later we will do the comparison for all seven days
            {
                pch.SaveReportAsPDF(DateTime.Now.Date.AddDays((-1) * i).Add(statusCheckTimes[0]),
                                    DateTime.Now.Date.AddDays((-1) * i).Add(statusCheckTimes[0]).AddMinutes(Settings.CheckInterval));
            }
        }

        /*
        private static void DisplayCounter(PerformanceCounter[] performanceCounters)
        {
            List<MetricInfo> infos = new List<MetricInfo>();
            while (!Console.KeyAvailable)
            {
                foreach (PerformanceCounter performanceCounter in performanceCounters)
                {
                    string miStr = string.Format("{3}\t{0}\t{1}\t{2}",
                    performanceCounter.CategoryName, performanceCounter.CounterName, performanceCounter.NextValue(), DateTime.Now.ToString());
                    collectWriter.WriteToFile(miStr);
                    infos.Add(MetricInfo.CreateFromString(miStr));
                
                }
                //collectWriter.WriteToFile(PerformanceInfo.GetMemoryInfo());
                System.Threading.Thread.Sleep(2000);

                /// break condition - after 9:30
                if (DateTime.Now.Hour >= 9 && DateTime.Now.Minute >= 0)
                {
                    //Console.WriteLine(Calculate(infos));
                    //Console.WriteLine(SqlCheck());
                    //Console.WriteLine(EventLogCheck());
                    collectWriter.WriteToFile(Calculate(infos));
                    collectWriter.WriteToFile(SqlCheck());
                    collectWriter.WriteToFile(EventLogCheck());
                    break;
                }
            }
        }

        private static string Calculate(List<MetricInfo> metricList)
        {
            //// for breaking - https://stackoverflow.com/questions/10148986/split-a-period-of-time-into-multiple-time-periods
            List<MetricInfo> processorMetrics = metricList.Where(d => d.MetricType.Equals("Processor")).OrderBy(d => d.MetricTime).ToList();
            List<MetricInfo> memoryMetrics = metricList.Where(d => d.MetricType.Equals("SQLServer:Memory Manager")).OrderBy(d => d.MetricTime).ToList();

            double maxMemoryMetric = memoryMetrics.Max(d => d.MetricValue);
            double minMemoryMetric = memoryMetrics.Min(d => d.MetricValue);
            double avgMemoryMetric = memoryMetrics.Average(d => d.MetricValue);

            double maxProcMetric = processorMetrics.Max(d => d.MetricValue);
            double minProcMetric = processorMetrics.Min(d => d.MetricValue);
            double avgProcMetric = processorMetrics.Average(d => d.MetricValue);
            return string.Format(@"During the high load period the SQL memory consumption was:
MAX - {0}, MIN - {1}, AVERAGE - {2}
the Processor load was:
MAX - {3}, MIN - {4}, AVERAGE - {5}", maxMemoryMetric, minMemoryMetric, avgMemoryMetric, maxProcMetric, minProcMetric, avgProcMetric);
        }

        private static string SqlCheck()
        {
            StringBuilder sb = new StringBuilder(string.Format("{0}\t{1}\t{2}\t{3}", "DatabaseName", "RowSizeMB", "LogSizeMB", "TotalSizeMB"));
            sb.AppendLine();
            try
            {
                using (SqlConnection conn = new SqlConnection(Settings.ConnectionString))
                {
                    SqlCommand command = new SqlCommand(SqlCommands.AllDBSize);
                    command.Connection = conn;
                    conn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            sb.AppendLine(
                                string.Format("{0}\t{1}\t{2}\t{3}", reader.GetString(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetDouble(3))
                                );
                        }
                    }
                    conn.Close();

                    command = new SqlCommand(SqlCommands.TotalSize);
                    command.Connection = conn;
                    conn.Open();
                    object retObj = command.ExecuteScalar();
                    sb.AppendLine(string.Format("Total Size: {0}", Double.Parse(retObj.ToString())));
                    conn.Close();
                }
            }
            catch { }
            return sb.ToString();
        }

        private static string EventLogCheck()
        {
            EventLog log = new EventLog("Application");
            var entries = log.Entries.Cast<EventLogEntry>()
                                     .Where(x => (x.Source.ToLower().Contains("sql") && x.TimeWritten.CompareTo(DateTime.Now.AddDays(-7)) > 0 
                                         && x.EntryType == EventLogEntryType.Error))
                                     .Select(x => x
                                     //    new
                                     //{
                                     //    x.MachineName,
                                     //    x.Site,
                                     //    x.Source,
                                     //    x.Message
                                     //}
                                     ).OrderByDescending(x => x.TimeGenerated).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var item in entries)
            {
                sb.AppendLine(string.Format("{0}\t{1}", item.Source, item.Message));
            }
            return sb.ToString();
        }
        */
    }
}
