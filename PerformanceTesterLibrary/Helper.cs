using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using PerformanceTesterLibrary.BLL;

namespace PerformanceTesterLibrary
{
    class Helper
    {
        public static string GetMemoryUsage(List<MetricInfo> metricList)
        {
            //// for breaking - https://stackoverflow.com/questions/10148986/split-a-period-of-time-into-multiple-time-periods
            List<MetricInfo> memoryMetrics = metricList.Where(d => d.MetricType.Equals("SQLServer:Memory Manager")).OrderBy(d => d.MetricTime).ToList();

            double maxMemoryMetric = memoryMetrics.Max(d => d.MetricValue) / 1048576;
            double minMemoryMetric = memoryMetrics.Min(d => d.MetricValue) / 1048576;
            double avgMemoryMetric = memoryMetrics.Average(d => d.MetricValue) / 1048576;

            double totalMemoryMB = PerformanceInfo.GetTotalMemoryInMiB();

            return string.Format(@"During the period, the SQL memory consumption was:
MAX - {0}GB ({3}), MIN - {1}GB ({4}), AVERAGE - {2}GB ({5})", maxMemoryMetric.ToString("F"), 
                                                            minMemoryMetric.ToString("F"),
                                                            avgMemoryMetric.ToString("F"), 
                                                            (maxMemoryMetric / (totalMemoryMB / 1024)).ToString("P"),
                                                            (minMemoryMetric / (totalMemoryMB / 1024)).ToString("P"), 
                                                            (avgMemoryMetric / (totalMemoryMB / 1024)).ToString("P"));
        }

        public static string GetProcessorUsage(List<MetricInfo> metricList)
        {
            //// for breaking - https://stackoverflow.com/questions/10148986/split-a-period-of-time-into-multiple-time-periods
            List<MetricInfo> processorMetrics = metricList.Where(d => d.MetricType.Equals("Processor")).OrderBy(d => d.MetricTime).ToList();

            double maxProcMetric = processorMetrics.Max(d => d.MetricValue);
            double minProcMetric = processorMetrics.Min(d => d.MetricValue);
            double avgProcMetric = processorMetrics.Average(d => d.MetricValue);
            return string.Format(@"During the high load period, the Processor load was:
MAX - {0}%, MIN - {1}%, AVERAGE - {2}%", maxProcMetric.ToString("F"), minProcMetric.ToString("F"), avgProcMetric.ToString("F"));
        }

        public static string EventLogCheck()
        {
            EventLog log = new EventLog("Application");
            var entries = log.Entries.Cast<EventLogEntry>()
                                     .Where(x => (x.Source.ToLower().Contains("sql") 
                                         && x.TimeWritten.CompareTo(DateTime.Now.AddDays(-7)) > 0
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
                if(sb.Length <2) sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}", "Source", "Message", "Time Generated", "Category"));
                sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}", item.Source, item.Message, item.TimeGenerated, item.Category));
            }
            if (sb.Length < 2) return string.Empty;//// if no events  - return empty string
            return sb.ToString();
        }

        public static string ConvertToHTMLTable(string tabSeparatedString)
        {
            StringReader sr = new StringReader(tabSeparatedString);
            StringBuilder sbTable = new StringBuilder();
            string line;
            while((line = sr.ReadLine()) != null)
            {
                StringBuilder sb = new StringBuilder();
                string[] cellValues = line.Split(new string[] { "\t" }, StringSplitOptions.None);
                if (sbTable.Length < 1) //// the beginning - column headers
                {
                    foreach (string item in cellValues)
                    {
                        sb.Append(string.Format("<th>{0}</th>", item));
                    }
                }
                else //// table content
                {
                    foreach (string item in cellValues)
                    {
                        sb.Append(string.Format("<td>{0}</td>", item));
                    }
                }
                sbTable.AppendLine(string.Format("<tr>{0}</tr>", sb.ToString()));
            }
            return string.Format("<table style=\"width:100%\">{0}</table>", sbTable.ToString());
        }


        public static string createHTMLReport(string title, string memoryHTML, string processorHTML, string dbTableHTML, string dbSize, string eventsHTML)
        {
            string reportTemplate = @"<!DOCTYPE html>
<html>
<head>
<style>
table, th, td {{
    border: 1px solid black;
    border-collapse: collapse;
}}
th, td {{
    padding: 5px;
    text-align: left;
}}
</style>
</head>
<body>

<h2>{0}</h2>

<p> 1. Processor: {1}</p>
<p> 2. Memory: {2}</p>
<p> 3. Databases: </p>
{3}
<p> Total DB sizes: {4}</p>
<p> 4. EventLog: {5}</p>
</body>
</html>";
            return string.Format(reportTemplate, title, processorHTML, memoryHTML, dbTableHTML, dbSize, eventsHTML);
        }

        public static string createGraphicalHTMLReport(string title, string memoryHTML, string processorHTML, string dbTableHTML, 
            string dbSize, string eventsHTML,
            string procWeeklyPath, string procAvgWeeklyPath, string procYearlyPath,
            string sqlAvgWeeklyPath, string sqlYearlyPath)
        {
            string procWeeklyImg = string.Empty;
            if (File.Exists(procWeeklyPath)) procWeeklyImg = string.Format("<img src=\"{0}\" alt=\"Processor Loads\"/>", procWeeklyPath);
            string procAvgWeeklyImg = string.Empty;
            if (File.Exists(procAvgWeeklyPath)) procAvgWeeklyImg = string.Format("<img src=\"{0}\" alt=\"Average Processor Loads\"/>", procAvgWeeklyPath);
            string procYearlyImg = string.Empty;
            if (File.Exists(procYearlyPath)) procYearlyImg = string.Format("<img src=\"{0}\" alt=\"Yearly Processor Loads\"/>", procYearlyPath);
            string sqlAvgWeeklyImg = string.Empty;
            if (File.Exists(sqlAvgWeeklyPath)) sqlAvgWeeklyImg = string.Format("<img src=\"{0}\" alt=\"Yearly Processor Loads\"/>", sqlAvgWeeklyPath);
            string sqlYearlyImg = string.Empty;
            if (File.Exists(sqlYearlyPath)) sqlYearlyImg = string.Format("<img src=\"{0}\" alt=\"Average Processor Loads\"/>", sqlYearlyPath);

            string reportTemplate = @"<!DOCTYPE html>
<html>
<head>
<style>
table, th, td {{
    border: 1px solid black;
    border-collapse: collapse;
}}
th, td {{
    padding: 5px;
    text-align: left;
}}
</style>
</head>
<body>

<h2>{0}</h2>

<p> 1. Information on Processor Load: {1}</p>
{6}
{7}
{8}
<p> 2. Information on Memory Consumption: {2}</p>
{9}
{10}
<p> 3. Databases: </p>
{3}
<p> Total DB sizes: {4}</p>
<p> 4. EventLog: {5}</p>
</body>
</html>";
            return string.Format(reportTemplate, title, processorHTML, memoryHTML, dbTableHTML, dbSize, eventsHTML,
                procWeeklyImg, procAvgWeeklyImg, procYearlyImg, sqlAvgWeeklyImg, sqlYearlyImg);
        }

    }
}
