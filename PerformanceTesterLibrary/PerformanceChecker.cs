using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using PerformanceTesterLibrary.Logging;
using PerformanceTesterLibrary.BLL;
using IronPdf;

namespace PerformanceTesterLibrary
{
    public class PerformanceChecker
    {
        private string ReportDirectory;
        private string ConnectionString;
        private string ReportTitle;
        private static PerformanceChecker _instance;
        private static bool IsRunning = false;
        private PerformanceChecker(string reportDirectory, string connStr, string reportTitle)
        {
            ReportDirectory = reportDirectory;
            ConnectionString = connStr;
            ReportTitle = reportTitle;
            IsRunning = false;
        }

        public static PerformanceChecker GetInstance(string reportDirectory, string connStr, string reportTitle)
        {
            if (_instance != null) 
                return _instance;
            else
                return new PerformanceChecker(reportDirectory, connStr, reportTitle);
        }

        public void CollectCounter(DateTime endTime)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                //List<MetricInfo> infos = new List<MetricInfo>();
                //// get the counters you want to monitor - sql and processor
                var processorCategory = PerformanceCounterCategory.GetCategories()
                    .FirstOrDefault(cat => cat.CategoryName == "Processor");
                var countersInCategory = processorCategory.GetCounters("_Total");
                var sqlServerMemoryCategory = PerformanceCounterCategory.GetCategories()
                    .FirstOrDefault(cat => cat.CategoryName == "SQLServer:Memory Manager");
                PerformanceCounter sqlPerformanceCounter = sqlServerMemoryCategory.GetCounters().First(cnt => cnt.CounterName.StartsWith("Total Server "));
                PerformanceCounter processorPerformanceCounter = countersInCategory.First(cnt => cnt.CounterName == "% Processor Time");
                PerformanceCounter[] performanceCounters = new PerformanceCounter[] { sqlPerformanceCounter, processorPerformanceCounter };

                LogWriter collectWriter = new LogWriter(ReportDirectory);
                while (true)
                {
                    foreach (PerformanceCounter performanceCounter in performanceCounters)
                    {
                        string miStr = string.Format("{3}\t{0}\t{1}\t{2}",
                        performanceCounter.CategoryName, performanceCounter.CounterName, performanceCounter.NextValue(), DateTime.Now.ToString());
                        collectWriter.WriteToFile(miStr);

                    }
                    System.Threading.Thread.Sleep(2000); //// collect every 2 seconds

                    /// break condition - after 9:30
                    if (DateTime.Now.CompareTo(endTime) > 0)
                    {
                        IsRunning = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This method will be collecting statistics constantly
        /// </summary>
        public void CollectCounter()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                //List<MetricInfo> infos = new List<MetricInfo>();
                //// get the counters you want to monitor - sql and processor
                var processorCategory = PerformanceCounterCategory.GetCategories()
                    .FirstOrDefault(cat => cat.CategoryName == "Processor");
                var countersInCategory = processorCategory.GetCounters("_Total");
                var sqlServerMemoryCategory = PerformanceCounterCategory.GetCategories()
                    .FirstOrDefault(cat => cat.CategoryName == "SQLServer:Memory Manager");
                PerformanceCounter sqlPerformanceCounter = sqlServerMemoryCategory.GetCounters().First(cnt => cnt.CounterName.StartsWith("Total Server "));
                PerformanceCounter processorPerformanceCounter = countersInCategory.First(cnt => cnt.CounterName == "% Processor Time");
                PerformanceCounter[] performanceCounters = new PerformanceCounter[] { sqlPerformanceCounter, processorPerformanceCounter };

                LogWriter collectWriter = new LogWriter(ReportDirectory);
                while (true)
                {
                    foreach (PerformanceCounter performanceCounter in performanceCounters)
                    {
                        string miStr = string.Format("{3}\t{0}\t{1}\t{2}",
                        performanceCounter.CategoryName, performanceCounter.CounterName, performanceCounter.NextValue(), DateTime.Now.ToString());
                        collectWriter.WriteToFile(miStr);

                    }
                    System.Threading.Thread.Sleep(3000); //// collect every 3 seconds because it is constantly collecting

                }
                IsRunning = false;
            }
        }

        /// <summary>
        /// Retrieves metrics for today
        /// </summary>
        /// <returns></returns>
        public List<MetricInfo> RetrieveMetricsFromFile()
        {
            List<MetricInfo> infos = new List<MetricInfo>();
            string fullFileName = ReportDirectory + System.IO.Path.DirectorySeparatorChar + "Metrics_{0}.dat";
            System.IO.TextReader tr = new System.IO.StreamReader(string.Format(fullFileName, DateTime.Now.ToString("yyyyMMdd")), true);
            string metricLine;
            while((metricLine = tr.ReadLine()) != null)
            {
                infos.Add(MetricInfo.CreateFromString(metricLine));
            }
            return infos;
        }

        /// <summary>
        /// retrieves metrics for a specific day
        /// </summary>
        /// <param name="dateInfo"></param>
        /// <returns></returns>
        public List<MetricInfo> RetrieveMetricsFromFile(DateTime dateInfo)
        {
            List<MetricInfo> infos = new List<MetricInfo>();
            string fullFileName = ReportDirectory + System.IO.Path.DirectorySeparatorChar + "Metrics_{0}.dat";
            System.IO.TextReader tr = new System.IO.StreamReader(string.Format(fullFileName, dateInfo.ToString("yyyyMMdd")), true);
            string metricLine;
            while ((metricLine = tr.ReadLine()) != null)
            {
                infos.Add(MetricInfo.CreateFromString(metricLine));
            }
            return infos;
        }

        /// <summary>
        /// retrieves metrics between two specific datetime instances
        /// </summary>
        /// <param name="dateInfo"></param>
        /// <returns></returns>
        public List<MetricInfo> RetrieveMetricsFromFile(DateTime startTime, DateTime endTime)
        {
            List<MetricInfo> infos = new List<MetricInfo>();
            //// specify how many days to get
            List<DateTime> dates = new List<DateTime>();
            for (DateTime date = startTime.Date; date <= endTime; date = date.AddDays(1))
                dates.Add(date);

            string fullFileName = ReportDirectory + System.IO.Path.DirectorySeparatorChar + "Metrics_{0}.dat";

            foreach (DateTime item in dates)
            {
                if (System.IO.File.Exists(string.Format(fullFileName, item.ToString("yyyyMMdd"))))
                {
                    System.IO.TextReader tr = new System.IO.StreamReader(string.Format(fullFileName, item.ToString("yyyyMMdd")), true);
                    string metricLine;
                    while ((metricLine = tr.ReadLine()) != null)
                    {
                        MetricInfo mi = MetricInfo.CreateFromString(metricLine);
                        //if ((mi.MetricTime.CompareTo(startTime) > 0) && (mi.MetricTime.CompareTo(endTime) < 0)) 
                        ////no need to compare - all metrics are valied
                            infos.Add(mi);
                    }
                    tr.Close();
                }
            }
            
            return infos;
        }

        /*public void CreateReport(string connStr)
        {
            string memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML;
            List<MetricInfo> infos = RetrieveMetricsFromFile();
            memoryHTML = Helper.GetMemoryUsage(infos);
            processorHTML = Helper.GetProcessorUsage(infos);
            SqlClient clnt = new SqlClient(connStr);
            dbTableHTML = Helper.ConvertToHTMLTable(clnt.SqlDBSizeCheck());
            dbSize = clnt.SqlTotalSizeCheck();
            eventsHTML = Helper.ConvertToHTMLTable(Helper.EventLogCheck());
            string fileContent = Helper.createHTMLReport("SQLFARM 2", memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML);
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                    string.Format(ReportDirectory + System.IO.Path.DirectorySeparatorChar + "Report_{0}.html", DateTime.Now.ToString("yyyyMMdd")), false);
                sw.WriteLine(fileContent);
                sw.Flush();
                sw.Close();
            }
            catch { }
        }*/

        public void SaveReportAsPDF()
        {
            string memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML;
            List<MetricInfo> infos = RetrieveMetricsFromFile();
            memoryHTML = Helper.GetMemoryUsage(infos);
            processorHTML = Helper.GetProcessorUsage(infos);
            SqlClient clnt = new SqlClient(ConnectionString);
            dbTableHTML = Helper.ConvertToHTMLTable(clnt.SqlDBSizeCheck());
            dbSize = clnt.SqlTotalSizeCheck();
            eventsHTML = Helper.EventLogCheck();
            if (string.IsNullOrEmpty(eventsHTML)) eventsHTML = "No errors related to SQL in last 7 days.";
            else
            {
                eventsHTML = Helper.ConvertToHTMLTable(eventsHTML);
            }
            string fileContent = Helper.createHTMLReport(ReportTitle, memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML);
            var Renderer = new IronPdf.HtmlToPdf();
            var PDF = Renderer.RenderHtmlAsPdf(fileContent);
            var OutputPath = string.Format(ReportDirectory + System.IO.Path.DirectorySeparatorChar + "Report_{1}_{0}.pdf"
                , DateTime.Now.ToString("yyyyMMdd"), ReportTitle);
            PDF.SaveAs(OutputPath);            
        }

        public void SaveReportAsPDF(DateTime startTime, DateTime endTime)
        {
            string memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML;
            List<MetricInfo> infos = RetrieveMetricsFromFile(startTime, endTime);
            memoryHTML = Helper.GetMemoryUsage(infos);
            processorHTML = Helper.GetProcessorUsage(infos);
            SqlClient clnt = new SqlClient(ConnectionString);
            dbTableHTML = Helper.ConvertToHTMLTable(clnt.SqlDBSizeCheck());
            dbSize = clnt.SqlTotalSizeCheck();
            eventsHTML = Helper.EventLogCheck();
            if (string.IsNullOrEmpty(eventsHTML)) eventsHTML = "No errors related to SQL in last 7 days.";
            else
            {
                eventsHTML = Helper.ConvertToHTMLTable(eventsHTML);
            }
            string fileContent = Helper.createHTMLReport(ReportTitle, memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML);
            var Renderer = new IronPdf.HtmlToPdf();
            var PDF = Renderer.RenderHtmlAsPdf(fileContent);
            var OutputPath = string.Format(ReportDirectory + System.IO.Path.DirectorySeparatorChar + "Report_{1}_{0}.pdf"
                , endTime.ToString("yyyyMMdd"), ReportTitle);
            PDF.SaveAs(OutputPath);
        }

        public void SaveReportAsPDF(List<MetricInfo> infos, string procWeeklyPath, string procAvgWeeklyPath, string procYearlyPath,
            string sqlAvgWeeklyPath, string sqlYearlyPath)
        {
            string memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML;
            memoryHTML = string.Empty;//Helper.GetMemoryUsage(infos);
            processorHTML = string.Empty; //Helper.GetProcessorUsage(infos);
            SqlClient clnt = new SqlClient(ConnectionString);
            dbTableHTML = Helper.ConvertToHTMLTable(clnt.SqlDBSizeCheck());
            dbSize = clnt.SqlTotalSizeCheck();
            eventsHTML = Helper.EventLogCheck();
            if (string.IsNullOrEmpty(eventsHTML)) eventsHTML = "No errors related to SQL in last 7 days.";
            else
            {
                eventsHTML = Helper.ConvertToHTMLTable(eventsHTML);
            }
            string fileContent = Helper.createGraphicalHTMLReport(ReportTitle, memoryHTML, processorHTML, dbTableHTML, dbSize, eventsHTML,
                procWeeklyPath, procAvgWeeklyPath, procYearlyPath, sqlAvgWeeklyPath, sqlYearlyPath);
            var Renderer = new IronPdf.HtmlToPdf();
            var PDF = Renderer.RenderHtmlAsPdf(fileContent);
            var OutputPath = string.Format(ReportDirectory + System.IO.Path.DirectorySeparatorChar + "Report_{1}_{0}.pdf"
                , DateTime.Today.ToString("yyyyMMdd"), ReportTitle);
            PDF.SaveAs(OutputPath);
        }
    }
}
