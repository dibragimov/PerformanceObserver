using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTesterLibrary.BLL
{
    public class MetricInfo
    {
        public static MetricInfo CreateFromString(string value)
        {
            string[] vls = value.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            MetricInfo mi = new MetricInfo();
            //mi.MetricTime = DateTime.Parse(vls[0]);
            mi.MetricTime = DateTime.Parse(vls[0],  new System.Globalization.CultureInfo("en-GB").DateTimeFormat);
            mi.MetricType = vls[1];
            mi.MetricName = vls[2];
            mi.MetricValue = Double.Parse(vls[3]);
            return mi;
        }

        public static MetricInfo CreateFromString(string[] values)
        {
            MetricInfo mi = new MetricInfo();
            mi.MetricTime = DateTime.Parse(values[0]);
            mi.MetricType = values[1];
            mi.MetricName = values[2];
            mi.MetricValue = Double.Parse(values[3]);
            return mi;
        }

        public string MetricType { get; set; }
        public string MetricName { get; set; }
        public double MetricValue { get; set; }
        public DateTime MetricTime { get; set; }
    }
}
