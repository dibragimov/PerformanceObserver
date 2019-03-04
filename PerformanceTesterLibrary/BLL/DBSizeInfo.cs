using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTesterLibrary.BLL
{
    public class DBSizeInfo
    {
        public string DBName { get; set; }
        public double RowSize { get; set; }
        public double LogSize { get; set; }
        public double TotalSize { get; set; }

        public static DBSizeInfo CreateFromString(string value)
        {
            string[] vls = value.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            DBSizeInfo mi = new DBSizeInfo();
            mi.RowSize = Double.Parse(vls[1]);
            mi.DBName = vls[0];
            mi.LogSize = Double.Parse(vls[2]);
            mi.TotalSize = Double.Parse(vls[3]);
            return mi;
        }

        public static DBSizeInfo CreateFromSqlReader(System.Data.SqlClient.SqlDataReader reader)
        {

            DBSizeInfo mi = new DBSizeInfo() { 
                DBName = reader.GetString(0), RowSize = reader.GetDouble(1), LogSize = reader.GetDouble(2), TotalSize = reader.GetDouble(3)
            };
            return mi;
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}", DBName, RowSize, LogSize, TotalSize);
        }
    }
}
