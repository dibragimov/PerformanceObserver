using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartPlotter
{
    public class Settings
    {
        private static readonly System.Configuration.Configuration Config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ExecutableFilePath);
        public static string ExecutableFilePath
        {
            get
            {
                return System.Reflection.Assembly.GetAssembly(typeof(Plotter)).Location;
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

        public static string ReportFolder
        {
            get { return Config.AppSettings.Settings["ReportFolder"].Value; }
        }
    }
}
