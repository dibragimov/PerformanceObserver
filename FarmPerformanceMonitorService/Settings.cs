using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarmPerformanceMonitorService
{
    class Settings
    {
        private static readonly System.Configuration.Configuration Config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ExecutableFilePath);
        public static string ExecutableFilePath
        {
            get
            {
                return System.Reflection.Assembly.GetAssembly(typeof(PerformanceMonitorService)).Location;
            }
        }


        public static string ConnectionString
        {
            get { return Config.ConnectionStrings.ConnectionStrings["ConnectionString"].ConnectionString; }
        }


        public static string Emails
        {
            get { return Config.AppSettings.Settings["Emails"].Value; }
        }

        public static string ReportHeader
        {
            get { return Config.AppSettings.Settings["ReportHeader"].Value; }
        }

        public static string RunTime
        {
            get { return Config.AppSettings.Settings["RunTime"].Value; }
        }

        public static string ReportFolder
        {
            get { return Config.AppSettings.Settings["ReportFolder"].Value; }
        }

        public static int CheckInterval
        {
            get
            {
                if (Config.AppSettings.Settings["CheckInterval"] != null
                    && !string.IsNullOrEmpty(Config.AppSettings.Settings["CheckInterval"].Value)
                    && !Config.AppSettings.Settings["CheckInterval"].Value.Equals("xxx"))
                    return Int32.Parse(Config.AppSettings.Settings["CheckInterval"].Value);
                else
                    return 1;
            }
        }

        public static bool MonitorContinuously
        {
            get //// returns true only if value is set to true
            {
                if (Config.AppSettings.Settings["ContinuousMonitor"] != null
                    && !string.IsNullOrEmpty(Config.AppSettings.Settings["ContinuousMonitor"].Value))
                    return Config.AppSettings.Settings["ContinuousMonitor"].Value.ToLower().Equals("true");
                else
                    return false;
            }
        }

        #region SMTP Settings
        public static string SmtpServer
        {
            get
            {
                if (Config.AppSettings.Settings["SmtpServer"] != null &&
                    !string.IsNullOrEmpty(Config.AppSettings.Settings["SmtpServer"].Value))
                    return Config.AppSettings.Settings["SmtpServer"].Value;
                else
                    return "smtp.gmail.com";////default 
            }
        }

        public static string SmtpUsername
        {
            get
            {
                if (Config.AppSettings.Settings["SmtpUsername"] != null 
                    && !string.IsNullOrEmpty(Config.AppSettings.Settings["SmtpUsername"].Value)
                    && !Config.AppSettings.Settings["SmtpUsername"].Value.StartsWith("xxxx"))
                    return Config.AppSettings.Settings["SmtpUsername"].Value;
                else
                    return "smcheckservice";
            }
        }

        public static string SmtpPassword
        {
            get
            {
                if (Config.AppSettings.Settings["SmtpPassword"] != null 
                    && !string.IsNullOrEmpty(Config.AppSettings.Settings["SmtpPassword"].Value)
                    && !Config.AppSettings.Settings["SmtpPassword"].Value.StartsWith("xxxx"))
                    return Config.AppSettings.Settings["SmtpPassword"].Value;
                else
                    return "P@$$WORD";
            }
        }

        public static int SmtpPort
        {
            get
            {
                if (Config.AppSettings.Settings["SmtpPort"] != null 
                    && !string.IsNullOrEmpty(Config.AppSettings.Settings["SmtpPort"].Value)
                    && !Config.AppSettings.Settings["SmtpPort"].Value.Equals("xxx"))
                    return Int32.Parse(Config.AppSettings.Settings["SmtpPort"].Value);
                else
                    return 587;
            }
        }

        public static bool SmtpEnableSSL
        {
            get
            {
                if (Config.AppSettings.Settings["SmtpEnableSSL"] != null &&
                  !string.IsNullOrEmpty(Config.AppSettings.Settings["SmtpEnableSSL"].Value))
                    return Config.AppSettings.Settings["SmtpEnableSSL"].Value.ToLower().Equals("true");
                else
                    return true;
            }
        }
        #endregion
    }
}
