using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PerformanceTesterLibrary.BLL;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.Data;

namespace ChartPlotter
{
    class Plotter
    {
        static void Main(string[] args)
        {

            PerformanceTesterLibrary.PerformanceChecker perfChecker = PerformanceTesterLibrary.PerformanceChecker.GetInstance(Settings.ReportFolder, 
                Settings.ConnectionString, Settings.ReportHeader);
            List<MetricInfo> mtrList = perfChecker.RetrieveMetricsFromFile(new DateTime(DateTime.Today.Year, 1, 1), DateTime.Now);


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

            perfChecker.SaveReportAsPDF(mtrList,
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("Proc_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("ProcHourly_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("ProcDaily_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("SqlHourly_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"),
                string.Format("{0}\\{1}_{2}", Settings.ReportFolder, string.Format("SqlDaily_{0}", DateTime.Now.Date.ToString("yyyyMMdd")), ".png"));
            /*
            var query = from row in dtMetricsProcessor.AsEnumerable()
                        group row by row.Field<DateTime>("Date") into grp
                       select new { DateValue = grp.Key, Avg = grp.Average(x => x.Field<double>("Value")) };

            DataTable dtMetricsProcessorDaily = new DataTable();
            dtMetricsProcessorDaily.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsProcessorDaily.Columns.Add("Value", typeof(double));

            foreach (var row in query)
            {
                DataRow rw = dtMetricsProcessorDaily.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Avg; 
                dtMetricsProcessorDaily.Rows.Add(rw);
                
            }

            try
            {
                //prepare chart control...
                Chart chart = new Chart();
                chart.DataSource = dtMetricsProcessorDaily;//dataSet.Tables[0];
                chart.Width = 1000;
                chart.Height = 350;
                //create serie...
                Series serie1 = new Series();
                serie1.Name = "Serie1";
                serie1.Color = Color.FromArgb(112, 255, 200);
                serie1.BorderColor = Color.FromArgb(164, 164, 164);
                serie1.ChartType = SeriesChartType.Line;//Column
                serie1.BorderDashStyle = ChartDashStyle.Solid;
                serie1.BorderWidth = 1;
                serie1.ShadowColor = Color.FromArgb(128, 128, 128);
                serie1.ShadowOffset = 1;
                serie1.IsValueShownAsLabel = false;//true;
                serie1.XValueMember = "DateTime";
                serie1.YValueMembers = "Value";
                serie1.Font = new Font("Tahoma", 8.0f);
                serie1.BackSecondaryColor = Color.FromArgb(0, 102, 153);
                serie1.LabelForeColor = Color.FromArgb(100, 100, 100);
                chart.Series.Add(serie1);
                //create chartareas...
                ChartArea ca = new ChartArea();
                ca.Name = "ChartArea1";
                ca.BackColor = Color.White;
                ca.BorderColor = Color.FromArgb(26, 59, 105);
                ca.BorderWidth = 0;
                ca.BorderDashStyle = ChartDashStyle.NotSet;//Solid;
                ca.AxisX = new Axis();
                ca.AxisY = new Axis();
                chart.ChartAreas.Add(ca);
                chart.Titles.Add("Average Daily Values");
                //databind...
                chart.DataBind();
                //save result...
                chart.SaveImage(string.Format("{0}\\{1}_{2}", Settings.ReportFolder, "ProcessorDaily", "myChart.png"), ChartImageFormat.Png);
            }
            catch { }

            var query1 = from row in dtMetricsProcessor.AsEnumerable()
                        group row by row.Field<DateTime>("DateHour") into grp
                        select new { DateValue = grp.Key, Avg = grp.Average(x => x.Field<double>("Value")) };

            DataTable dtMetricsProcessorHourly = new DataTable();
            dtMetricsProcessorHourly.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsProcessorHourly.Columns.Add("Value", typeof(double));

            foreach (var row in query1)
            {
                DataRow rw = dtMetricsProcessorHourly.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Avg;
                dtMetricsProcessorHourly.Rows.Add(rw);
            }

            try
            {
                //prepare chart control...
                Chart chart = new Chart();
                chart.DataSource = dtMetricsProcessorHourly;//dataSet.Tables[0];
                chart.Width = 1000;
                chart.Height = 350;
                //create serie...
                Series serie1 = new Series();
                serie1.Name = "Serie1";
                serie1.Color = Color.FromArgb(0, 0, 139);
                serie1.BorderColor = Color.FromArgb(164, 164, 164);
                serie1.ChartType = SeriesChartType.Line;//Column
                serie1.BorderDashStyle = ChartDashStyle.Solid;
                serie1.BorderWidth = 2;
                serie1.ShadowColor = Color.FromArgb(128, 128, 128);
                serie1.ShadowOffset = 1;
                serie1.IsValueShownAsLabel = false;//true;
                serie1.XValueMember = "DateTime";
                serie1.YValueMembers = "Value";
                serie1.Font = new Font("Tahoma", 8.0f);
                serie1.BackSecondaryColor = Color.FromArgb(0, 102, 153);
                serie1.LabelForeColor = Color.FromArgb(100, 100, 100);
                chart.Series.Add(serie1);
                //create chartareas...
                ChartArea ca = new ChartArea();
                ca.Name = "ChartArea1";
                ca.BackColor = Color.White;
                ca.BorderColor = Color.FromArgb(26, 59, 105);
                ca.BorderWidth = 0;
                ca.BorderDashStyle = ChartDashStyle.Dot;//Solid;
                ca.AxisX = new Axis();
                ca.AxisY = new Axis();
                chart.ChartAreas.Add(ca);
                //databind...
                chart.DataBind();
                //save result...
                chart.SaveImage(string.Format("{0}\\{1}_{2}", Settings.ReportFolder, "ProcessorHourly", "myChart.png"), ChartImageFormat.Png);
            }
            catch { }*/

            Console.ReadLine();
            /*
            foreach (var item in Enum.GetValues(typeof(SeriesChartType)))
            {

                try
                {
                    //prepare chart control...
                    Chart chart = new Chart();
                    chart.DataSource = dtMetricsProcessor;//dataSet.Tables[0];
                    chart.Width = 1000;
                    chart.Height = 350;
                    //create serie...
                    Series serie1 = new Series();
                    serie1.Name = "Serie1";
                    serie1.Color = Color.FromArgb(112, 255, 200);
                    serie1.BorderColor = Color.FromArgb(164, 164, 164);
                    serie1.ChartType = (SeriesChartType)item; //SeriesChartType.Spline;//Column
                    serie1.BorderDashStyle = ChartDashStyle.Solid;
                    serie1.BorderWidth = 1;
                    serie1.ShadowColor = Color.FromArgb(128, 128, 128);
                    serie1.ShadowOffset = 1;
                    serie1.IsValueShownAsLabel = false;//true;
                    serie1.XValueMember = "DateTime";
                    serie1.YValueMembers = "Value";
                    serie1.Font = new Font("Tahoma", 8.0f);
                    serie1.BackSecondaryColor = Color.FromArgb(0, 102, 153);
                    serie1.LabelForeColor = Color.FromArgb(100, 100, 100);
                    chart.Series.Add(serie1);
                    //create chartareas...
                    ChartArea ca = new ChartArea();
                    ca.Name = "ChartArea1";
                    ca.BackColor = Color.White;
                    ca.BorderColor = Color.FromArgb(26, 59, 105);
                    ca.BorderWidth = 0;
                    ca.BorderDashStyle = ChartDashStyle.Solid;
                    ca.AxisX = new Axis();
                    ca.AxisY = new Axis();
                    chart.ChartAreas.Add(ca);
                    //databind...
                    chart.DataBind();
                    //save result...
                    chart.SaveImage(string.Format("{0}\\{1}_{2}", Settings.ReportFolder, item.ToString(), "myChart.png"), ChartImageFormat.Png);
                }
                catch { }
            }
            */
        }
    }
}
