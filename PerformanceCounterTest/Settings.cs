using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceCounterTest
{
    class Settings
    {
        private static readonly System.Configuration.Configuration Config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ExecutableFilePath);
        public static string ExecutableFilePath
        {
            get
            {
                return System.Reflection.Assembly.GetAssembly(typeof(Program)).Location;
            }
        }


        public static string ConnectionString
        {
            get { return Config.ConnectionStrings.ConnectionStrings["ConnectionString"].ConnectionString; }
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
    }
}
