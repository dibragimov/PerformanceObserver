using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Net.Mail;
using System.Net;
using PerformanceTesterLibrary.BLL;

namespace FarmPerformanceMonitorService
{
    public partial class PerformanceMonitorService : ServiceBase
    {
        private List<TimeSpan> statusCheckTimes = new List<TimeSpan>();
        private Timer runCheckTimer = new Timer();
        private Timer senderTimer = new Timer();

        public PerformanceMonitorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var statusCheckTimesSplit = Settings.RunTime.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            DateTime curDate = DateTime.Now;

            if (statusCheckTimesSplit.Any())
            {
                statusCheckTimesSplit.Sort();

                foreach (string item in statusCheckTimesSplit)
                {
                    string[] parts = item.Split(new char[] { ':' });
                    TimeSpan tsParts = new TimeSpan(Int32.Parse(parts[0]), Int32.Parse(parts[1]), 0);
                    statusCheckTimes.Add(tsParts);
                }

                DateTime runTime = DateTime.Now.Date.Add(statusCheckTimes[0]);
                foreach (TimeSpan item in statusCheckTimes)
                {
                    DateTime runTime1 = DateTime.Now.Date.Add(item);
                    if (curDate.CompareTo(runTime1) < 0)//// RunTime is later than current time - will execute today
                    {
                        runTime = runTime1;
                        break;
                    }
                }
                //string[] time = ServiceConfig.AppSettings.Settings["RunTime"].Value.Split(new char[] { ':' });

                
                TimeSpan ts;
                if (curDate.CompareTo(runTime) < 0)//// RunTime is later than current time
                {
                    ts = runTime.Subtract(curDate);
                    runCheckTimer.Interval = ts.TotalMilliseconds;
                }
                else ////RunTime has passed, put it to the next day
                {
                    ts = runTime.AddDays(1).Subtract(curDate);
                    runCheckTimer.Interval = ts.TotalMilliseconds;
                }
                if (Settings.MonitorContinuously)
                {
                    ///// start monitoring already
                    runCheckTimer.Interval = new TimeSpan(0, 0, 30).TotalMilliseconds;
                    
                    runCheckTimer.AutoReset = false; // execute every day at specified time
                    //runCheckTimer_Elapsed(null, null); //// no need to run a thread
                    runCheckTimer.Elapsed += new ElapsedEventHandler(runCheckTimer_Elapsed);
                    runCheckTimer.Start();
                }
                else
                {
                    runCheckTimer.AutoReset = true; // execute every day at specified time
                    runCheckTimer.Elapsed += new ElapsedEventHandler(runCheckTimer_Elapsed);
                    runCheckTimer.Start();
                }
                

                //// make it run on Sunday
                if (curDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    DateTime nextRun = NextDayOfWeek( //// shpould execute coming Sunday at runtime
                        curDate.Date.Add(runTime.TimeOfDay) //// run time of today
                        , DayOfWeek.Sunday);
                    ts = nextRun.Subtract(curDate); ////
                }
                //// else - It is Sunday, try to execute it at runtime ( or tomorrow )
                
                senderTimer.Interval = new TimeSpan(0, 0, 5, 0).Add(ts).TotalMilliseconds;
                senderTimer.AutoReset = true; // execute every Sunday at specified time
                senderTimer.Elapsed += new ElapsedEventHandler(senderTimer_Elapsed);
                senderTimer.Start();
                Logger.Instance().log("Status checks are scheduled to execute in " + ts.ToString());
            }
        }

        protected override void OnStop()
        {
            try
            {
                if(runCheckTimer != null)
                    runCheckTimer.Stop();
                if (senderTimer != null)
                    senderTimer.Stop();
            }
            catch { }
        }

        void runCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ////if sender is null - probably this is a continuous monitor setting
            if (!Settings.MonitorContinuously)//sender != null)
            {
                ////run next time after this interval:
                DateTime curDate = DateTime.Now;
                //curDate = new DateTime(curDate.Year, curDate.Month, curDate.Day, curDate.Hour, curDate.Minute, curDate.Second);
                DateTime runTime = DateTime.Now.Date.Add(statusCheckTimes[0]);
                foreach (TimeSpan item in statusCheckTimes)
                {
                    DateTime runTime1 = DateTime.Now.Date.Add(item);
                    if (curDate.AddSeconds(10).CompareTo(runTime1) < 0)//// RunTime is later than current time - will execute today (10 seconds for timer that starts little earlier)
                    {
                        runTime = runTime1;
                        break;
                    }
                }

                //string[] time = ServiceConfig.AppSettings.Settings["RunTime"].Value.Split(new char[] { ':' });
                //TimeSpan ts = curDate.Date.AddDays(1).AddHours(Int32.Parse(time[0])).AddMinutes(Int32.Parse(time[1])).Subtract(curDate);
                Timer tmr = (Timer)sender;

                TimeSpan ts;
                if (runTime.Subtract(curDate).TotalSeconds > 10) //curDate.CompareTo(runTime) < 0)//// RunTime is later than current time
                {
                    ///// timer may start a bit earlier, therefore this check of 10 seconds
                    ts = runTime.Subtract(curDate);
                    tmr.Interval = ts.TotalMilliseconds;
                }
                else
                {
                    ts = runTime.AddDays(1).Subtract(curDate);  
                    tmr.Interval = ts.TotalMilliseconds;
                }
                //SyncCheckLogger.Instance().log("Next time scheduled to execute in " + ts.ToString());
                //// end of run next time after this interval.

                PerformanceTesterLibrary.PerformanceChecker pch = PerformanceTesterLibrary.PerformanceChecker.GetInstance(
                    Settings.ReportFolder, Settings.ConnectionString, Settings.ReportHeader);
                pch.CollectCounter(DateTime.Now.AddMinutes(Settings.CheckInterval));
                pch.SaveReportAsPDF(DateTime.Now.AddMinutes((-1) * Settings.CheckInterval), DateTime.Now); //// from past to present
            }
            else //// probably monitor continuously
            {
                PerformanceTesterLibrary.PerformanceChecker pch = PerformanceTesterLibrary.PerformanceChecker.GetInstance(
                    Settings.ReportFolder, Settings.ConnectionString, Settings.ReportHeader);
                if (Settings.MonitorContinuously)
                {
                    pch.CollectCounter();
                    //tmr.AutoReset = false; //// will be doing it constantly - no need to reset
                }
            }
        }

        void senderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ////run next time after this interval:
            DateTime curDate = DateTime.Now;
            //DateTime runTime = DateTime.Now.Date.Add(statusCheckTimes[0]);
            Timer tmr = (Timer)sender;
            TimeSpan ts;
            ts = NextDayOfWeek(curDate, DayOfWeek.Sunday).Subtract(curDate);  //// Do it every Sunday
            tmr.Interval = ts.TotalMilliseconds;
            
            #region Send mail
            //SyncCheckLogger.Instance().log("Sending mail...");

            var sb = new StringBuilder();
            sb.AppendLine("Dear all,");
            sb.AppendLine("Please find attached the DBA report");
            sb.AppendLine("Thanks,");
            sb.Append("dilshod.");

            Logger.Instance().log("Message to be sent: " + sb.ToString());

            try
            {
                /*for (int i = 0; i < 5; i++)  //// get the report for 5 days. Later we will do the comparison for all seven days
                {
                    try
                    {
                        string OutputPath = string.Format(Settings.ReportFolder + System.IO.Path.DirectorySeparatorChar + "Report_{1}_{0}.pdf",
                            DateTime.Now.AddDays((-1) * i).ToString("yyyyMMdd"), Settings.ReportHeader);

                        //// check if exists; delete if exists, otherwise create
                        if (!System.IO.File.Exists(OutputPath))
                        {
                            PerformanceTesterLibrary.PerformanceChecker pch = PerformanceTesterLibrary.PerformanceChecker.GetInstance(Settings.ReportFolder,
                                Settings.ConnectionString, Settings.ReportHeader);
                            pch.SaveReportAsPDF(DateTime.Now.Date.AddDays((-1) * i).Add(statusCheckTimes[0]),
                                DateTime.Now.Date.AddDays((-1) * i).Add(statusCheckTimes[0]).AddMinutes(Settings.CheckInterval));

                            //// allow some time to create a report
                            System.Threading.Thread.Sleep(1000 * 60 * 2);///// 30 seconds for file to finish writing
                        }
                    }
                    catch (System.IO.IOException exc)
                    {
                        Logger.Instance().log(string.Format("{0} During the execution of the senderTimer_Elapsed the following IO error occurred: {1}, {2}",
                            DateTime.Now.ToString(), exc.Message, exc.StackTrace));
                    }
                } */

                try
                {
                    string OutputPath = string.Format(Settings.ReportFolder + System.IO.Path.DirectorySeparatorChar + "Report_{1}_{0}.pdf",
                        DateTime.Now.ToString("yyyyMMdd"), Settings.ReportHeader);

                    //// check if exists; delete if exists, otherwise create
                    if (!System.IO.File.Exists(OutputPath))
                    {
                        
                        CreateGraphicalReport();

                        //// allow some time to write a report to a file
                        System.Threading.Thread.Sleep(1000 * 30);///// 30 seconds for file to finish writing
                    }
                }
                catch (System.IO.IOException exc)
                {
                    Logger.Instance().log(string.Format("{0} During the execution of the senderTimer_Elapsed the following IO error occurred: {1}, {2}",
                        DateTime.Now.ToString(), exc.Message, exc.StackTrace));
                }

                if (!string.IsNullOrEmpty(Settings.Emails))
                {
                    string[] emails = Settings.Emails.Split(new char[] { ',' });

                    //SyncCheckLogger.Instance().log("Number of entries: " + emails.Length);

                    int port = Settings.SmtpPort;
                    string smtpServer = Settings.SmtpServer;
                    string username = Settings.SmtpUsername;
                    string password = Settings.SmtpPassword;
                    bool sslEnabled = Settings.SmtpEnableSSL;

                    var smtpClient = new SmtpClient(smtpServer);
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Port = port;
                    smtpClient.Credentials = new NetworkCredential(username, password);
                    smtpClient.EnableSsl = sslEnabled;


                    var mail = new MailMessage();
                    mail.From = new MailAddress(username); //+ smtpServer);
                    foreach (string item in emails)
                    {
                        mail.To.Add(item);
                    }
                    mail.Subject = "DBA Report for " + ((string.IsNullOrEmpty(Settings.ReportHeader)) ? string.Join(" ", emails) : Settings.ReportHeader);
                    mail.Body = sb.ToString();
                    /*for (int i = 0; i < 5; i++)  //// get the report for 5 days. Later we will do the comparison for all seven days
                    {
                        string OutputPath = string.Format(Settings.ReportFolder + System.IO.Path.DirectorySeparatorChar + "Report_{1}_{0}.pdf",
                            DateTime.Now.AddDays((-1) * i).ToString("yyyyMMdd"), Settings.ReportHeader);

                        
                        if (System.IO.File.Exists(OutputPath))
                        {
                            mail.Attachments.Add(new Attachment(OutputPath, System.Net.Mime.MediaTypeNames.Application.Octet));
                        }
                        else
                        {
                            OutputPath = string.Format(Settings.ReportFolder + System.IO.Path.DirectorySeparatorChar + "Report_{0}.pdf",
                            DateTime.Now.AddDays((-1) * i).ToString("yyyyMMdd"));
                            if (System.IO.File.Exists(OutputPath))
                                mail.Attachments.Add(new Attachment(OutputPath, System.Net.Mime.MediaTypeNames.Application.Octet));
                        }
                        
                    }*/

                    string OutputPath = string.Format(Settings.ReportFolder + System.IO.Path.DirectorySeparatorChar + "Report_{1}_{0}.pdf",
                            DateTime.Now.ToString("yyyyMMdd"), Settings.ReportHeader);


                    if (System.IO.File.Exists(OutputPath))
                    {
                        mail.Attachments.Add(new Attachment(OutputPath, System.Net.Mime.MediaTypeNames.Application.Octet));
                    }

                    ServicePointManager.ServerCertificateValidationCallback = delegate(object s,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
                    {
                        const System.Net.Security.SslPolicyErrors ignoredErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors |  // self-signed
                            System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch;  // name mismatch
                        string nameOnCertificate = certificate.Subject;
                        if ((sslPolicyErrors & ~ignoredErrors) == System.Net.Security.SslPolicyErrors.None)
                        {
                            return true;
                        }
                        else return false;
                    };

                    smtpClient.Send(mail);

                    Logger.Instance().log("Message was sent.");

                    System.Threading.Thread.Sleep(20 * 1000); // why? in order not to overwhelm the system

                }
            }
            catch (Exception ex)
            {
                Logger.Instance().log(string.Format("{0} During the execution of the SendMail the following error occurred: {1}, {2}", DateTime.Now.ToString(), ex.Message, ex.StackTrace));
            }

            #endregion
        }

        public static DateTime NextDayOfWeek(DateTime from, DayOfWeek dayOfWeek)
        {
            int start = (int)from.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return from.AddDays(target - start);
        }

        private void CreateGraphicalReport()
        {
            PerformanceTesterLibrary.PerformanceChecker pch = PerformanceTesterLibrary.PerformanceChecker.GetInstance(Settings.ReportFolder,
                            Settings.ConnectionString, Settings.ReportHeader);
            List<MetricInfo> mtrList = pch.RetrieveMetricsFromFile(new DateTime(DateTime.Today.Year, 1, 1), DateTime.Now);
            PlotterLibrary.Plotter p = new PlotterLibrary.Plotter(Settings.ReportFolder); //, Settings.ConnectionString, Settings.ReportHeader);

            DataTable dtMetricsProcessor = new DataTable();
            dtMetricsProcessor.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsProcessor.Columns.Add("Value", typeof(double));
            dtMetricsProcessor.Columns.Add("Date", typeof(DateTime));
            dtMetricsProcessor.Columns.Add("DateHour", typeof(DateTime));

            DataTable dtMetricsSql = new DataTable();
            dtMetricsSql.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsSql.Columns.Add("Value", typeof(double));
            dtMetricsSql.Columns.Add("Date", typeof(DateTime));
            dtMetricsSql.Columns.Add("DateHour", typeof(DateTime));

            List<MetricInfo> mtrProcList = mtrList.Where(c => c.MetricType.StartsWith("Processor")).ToList();
            foreach (MetricInfo item in mtrProcList)
            {
                DataRow rw = dtMetricsProcessor.NewRow();
                rw[0] = item.MetricTime;
                rw[1] = item.MetricValue;
                rw[2] = item.MetricTime.Date;
                rw[3] = item.MetricTime.Date.AddHours(item.MetricTime.Hour);
                dtMetricsProcessor.Rows.Add(rw);
            }

            List<MetricInfo> mtrSqlList = mtrList.Where(c => c.MetricType.StartsWith("SQLServer")).ToList();
            foreach (MetricInfo item in mtrSqlList)
            {
                DataRow rw = dtMetricsSql.NewRow();
                rw[0] = item.MetricTime;
                rw[1] = item.MetricValue;
                rw[2] = item.MetricTime.Date;
                rw[3] = item.MetricTime.Date.AddHours(item.MetricTime.Hour);
                dtMetricsSql.Rows.Add(rw);
            }
            mtrList = null;

            p.BuildProcessorLoadsCharts(dtMetricsProcessor);

            p.BuildSqlMemoryLoadsCharts(dtMetricsSql);

            pch.SaveReportAsPDF(mtrList,
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("Proc_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("ProcHourly_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("ProcDaily_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("SqlHourly_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("SqlDaily_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"));
        }
    }
}
