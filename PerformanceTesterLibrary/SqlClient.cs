using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace PerformanceTesterLibrary
{
    class SqlClient
    {
        private string ConnectionString;
        public SqlClient(string connStr)
        {
            ConnectionString = connStr;
        }

        public string SqlDBSizeCheck()
        {
            StringBuilder sb = new StringBuilder(string.Format("{0}\t{1}\t{2}\t{3}", "DatabaseName", "RowSizeMB", "LogSizeMB", "TotalSizeMB"));
            sb.AppendLine();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
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
                }
            }
            catch { }
            return sb.ToString();
        }

        public string SqlTotalSizeCheck()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(SqlCommands.TotalSize);
                    command.Connection = conn;
                    conn.Open();
                    object retObj = command.ExecuteScalar();
                    sb.AppendLine(string.Format("{0} MB", Double.Parse(retObj.ToString()).ToString("F")));
                    conn.Close();
                }
            }
            catch { }
            return sb.ToString();
        }
    }

    internal class SqlCommands
    {
        public static string AllDBSize = @"-- List of all DB with the sizes
SELECT
    DB_NAME(db.database_id) DatabaseName,
    (CAST(mfrows.RowSize AS FLOAT)*8)/1024 RowSizeMB
    ,(CAST(mflog.LogSize AS FLOAT)*8)/1024 LogSizeMB
	,((CAST(mfrows.RowSize AS FLOAT)*8)/1024 + (CAST(mflog.LogSize AS FLOAT)*8)/1024 ) TotalSizeMB
    --,(CAST(mfstream.StreamSize AS FLOAT)*8)/1024 StreamSizeMB
    --,(CAST(mftext.TextIndexSize AS FLOAT)*8)/1024 TextIndexSizeMB
FROM sys.databases db
    LEFT JOIN (SELECT database_id, SUM(size) RowSize FROM sys.master_files WHERE type = 0 GROUP BY database_id, type) mfrows ON mfrows.database_id = db.database_id
    LEFT JOIN (SELECT database_id, SUM(size) LogSize FROM sys.master_files WHERE type = 1 GROUP BY database_id, type) mflog ON mflog.database_id = db.database_id
    LEFT JOIN (SELECT database_id, SUM(size) StreamSize FROM sys.master_files WHERE type = 2 GROUP BY database_id, type) mfstream ON mfstream.database_id = db.database_id
    LEFT JOIN (SELECT database_id, SUM(size) TextIndexSize FROM sys.master_files WHERE type = 4 GROUP BY database_id, type) mftext ON mftext.database_id = db.database_id
ORDER BY TotalSizeMB DESC";

        public static string TotalSize = @"-- total size of all DB
SELECT
    SUM((CAST(mfrows.RowSize AS FLOAT)*8)/1024 +
    (CAST(mflog.LogSize AS FLOAT)*8)/1024 ) as total
FROM sys.databases db
    LEFT JOIN (SELECT database_id, SUM(size) RowSize FROM sys.master_files WHERE type = 0 GROUP BY database_id, type) mfrows ON mfrows.database_id = db.database_id
    LEFT JOIN (SELECT database_id, SUM(size) LogSize FROM sys.master_files WHERE type = 1 GROUP BY database_id, type) mflog ON mflog.database_id = db.database_id";

    }
}
