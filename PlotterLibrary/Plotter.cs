using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.Data;

namespace PlotterLibrary
{
    public class Plotter
    {
        string _reportFolder;
        string _connectionString;
        string _reportHeader;
        //PerformanceTesterLibrary.PerformanceChecker perfChecker;
        
        public Plotter(string reportFolder) //, string connectionString, string reportHeader)
        {
            _reportFolder = reportFolder;
            //_connectionString = connectionString;
            //_reportHeader = reportFolder;

            //perfChecker = PerformanceTesterLibrary.PerformanceChecker.GetInstance(_reportFolder, _connectionString, _reportHeader);
        }

        public void BuildProcessorLoadsCharts(DataTable dtProcessorMetrics)
        {
            //// group values by day - calculate daily average
            DataTable dtMetricsProcessorDaily = new DataTable();
            dtMetricsProcessorDaily.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsProcessorDaily.Columns.Add("Value", typeof(double));

            DateTime nowDT = DateTime.Now;
            DateTime beginningYear = new DateTime(nowDT.Year, 1, 1);

            var queryDaily = from row in dtProcessorMetrics.AsEnumerable()
                             /*where (DateTime.Compare(row.Field<DateTime>("Date"), nowDT) < 0 &&
                                DateTime.Compare(row.Field<DateTime>("Date"), beginningYear) > 0)*/
                             group row by row.Field<DateTime>("Date") into grp
                        select new { DateValue = grp.Key, Avg = grp.Average(x => x.Field<double>("Value")) };

            foreach (var row in queryDaily)
            {
                DataRow rw = dtMetricsProcessorDaily.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Avg;
                dtMetricsProcessorDaily.Rows.Add(rw);

            }
            //// plot daily chart
            PlotTimeSeriesChart(dtMetricsProcessorDaily, SeriesChartType.Line, Color.FromArgb(204, 0, 204), 2, //// thicker line
                string.Format("Average Daily Values of Processor Load for {0} to {1}", beginningYear.ToString("dd/MM/yyyy"), DateTime.Today.ToString("dd/MM/yyyy")),
                string.Format("ProcDaily_{0}", DateTime.Now.Date.ToString("yyyyMMdd")));

            //// group values by hour - calculate hourly average
             DateTime beginningWeek = DateTime.Today.AddDays(-7); //// one week
            var queryHourly = from row in dtProcessorMetrics.AsEnumerable()
                         where (DateTime.Compare(row.Field<DateTime>("Date"), nowDT) < 0 &&
                            DateTime.Compare(row.Field<DateTime>("Date"), beginningWeek) > 0)
                         group row by row.Field<DateTime>("DateHour") into grp
                         select new { DateValue = grp.Key, Avg = grp.Average(x => x.Field<double>("Value")) };

            DataTable dtMetricsProcessorHourly = new DataTable();
            dtMetricsProcessorHourly.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsProcessorHourly.Columns.Add("Value", typeof(double));

            foreach (var row in queryHourly)
            {
                DataRow rw = dtMetricsProcessorHourly.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Avg;
                dtMetricsProcessorHourly.Rows.Add(rw);
            }

            //// plot hourly chart
            //PlotTimeSeriesChart(dtMetricsProcessorHourly, SeriesChartType.Column, Color.FromArgb(0, 0, 139), 1, //// thin line
            //    "Average Hourly Values of Processor Load", string.Format("ProcHourly_{0}", DateTime.Now.Date.ToString("yyyyMMdd")));

            PlotTimeSeriesChart(dtMetricsProcessorHourly, SeriesChartType.Line, Color.FromArgb(0, 0, 139), 2, //// thicker line
                string.Format("Average Hourly Values of Processor Load for {0} to {1}", beginningWeek.ToString("dd/MM/yyyy"), DateTime.Today.ToString("dd/MM/yyyy")),
                string.Format("ProcHourly_{0}", DateTime.Now.Date.ToString("yyyyMMdd")));


            //DateTime beginningMonth = new DateTime(nowDT.Year, nowDT.Month, 1);
            var queryMonthly = from row in dtProcessorMetrics.AsEnumerable()
                              where (DateTime.Compare(row.Field<DateTime>("Date"), nowDT) < 0 &&
                                 DateTime.Compare(row.Field<DateTime>("Date"), beginningWeek) > 0)
                               select new { DateValue = row.Field<DateTime>("DateTime"), Value = row.Field<double>("Value") };

            DataTable dtMetricsProcessorWeeklyUngrouped = new DataTable();
            dtMetricsProcessorWeeklyUngrouped.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsProcessorWeeklyUngrouped.Columns.Add("Value", typeof(double));

            foreach (var row in queryMonthly)
            {
                DataRow rw = dtMetricsProcessorWeeklyUngrouped.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Value;
                dtMetricsProcessorWeeklyUngrouped.Rows.Add(rw);
            }
            //// plotting Kagi chart
            PlotTimeSeriesChart(dtMetricsProcessorWeeklyUngrouped, SeriesChartType.Kagi, Color.FromArgb(112, 255, 200), 1,//// thin line
               string.Format("Actual Processor Load Values for {0} to {1}", beginningWeek.ToString("dd/MM/yyyy"), DateTime.Today.ToString("dd/MM/yyyy")),
               string.Format("Proc_{0}", DateTime.Now.Date.ToString("yyyyMMdd")));
        }

        public void BuildSqlMemoryLoadsCharts(DataTable dtSqlMetrics)
        {
            //// group values by day - calculate daily average
            DataTable dtMetricsSqlDaily = new DataTable();
            dtMetricsSqlDaily.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsSqlDaily.Columns.Add("Value", typeof(double));

            DateTime nowDT = DateTime.Now;
            DateTime beginningYear = new DateTime(nowDT.Year, 1, 1);

            var queryDaily = from row in dtSqlMetrics.AsEnumerable()
                             /*where (DateTime.Compare(row.Field<DateTime>("Date"), nowDT) < 0 &&
                                DateTime.Compare(row.Field<DateTime>("Date"), beginningYear) > 0)*/
                             group row by row.Field<DateTime>("Date") into grp
                             select new { DateValue = grp.Key, Avg = grp.Average(x => x.Field<double>("Value")) };

            foreach (var row in queryDaily)
            {
                DataRow rw = dtMetricsSqlDaily.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Avg;
                dtMetricsSqlDaily.Rows.Add(rw);

            }
            //// plot daily chart

            DateTime beginningWeek = DateTime.Today.AddDays(-7); //// one week
            PlotTimeSeriesChart(dtMetricsSqlDaily, SeriesChartType.Line, Color.FromArgb(204, 0, 204), 2, //// thicker line
                string.Format("Average Daily Values of SQL Memory Load for {0} to {1}", beginningYear.ToString("dd/MM/yyyy"), DateTime.Today.ToString("dd/MM/yyyy")),
                string.Format("SqlDaily_{0}", DateTime.Now.Date.ToString("yyyyMMdd")));

            //// group values by hour - calculate hourly average
            var queryHourly = from row in dtSqlMetrics.AsEnumerable()
                              where (DateTime.Compare(row.Field<DateTime>("Date"), nowDT) < 0 &&
                                 DateTime.Compare(row.Field<DateTime>("Date"), beginningWeek) > 0)
                              group row by row.Field<DateTime>("DateHour") into grp
                              select new { DateValue = grp.Key, Avg = grp.Average(x => x.Field<double>("Value")) };

            DataTable dtMetricsSqlHourly = new DataTable();
            dtMetricsSqlHourly.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsSqlHourly.Columns.Add("Value", typeof(double));

            foreach (var row in queryHourly)
            {
                DataRow rw = dtMetricsSqlHourly.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Avg;
                dtMetricsSqlHourly.Rows.Add(rw);
            }

            PlotTimeSeriesChart(dtMetricsSqlHourly, SeriesChartType.Line, Color.FromArgb(0, 0, 139), 2, //// thicker line
                string.Format("Average Hourly Values of SQL Memory Load for {0} to {1}", beginningWeek.ToString("dd/MM/yyyy"), DateTime.Today.ToString("dd/MM/yyyy")),
                string.Format("SqlHourly_{0}", DateTime.Now.Date.ToString("yyyyMMdd")));


            /* //DateTime beginningMonth = new DateTime(nowDT.Year, nowDT.Month, 1);
            var queryMonthly = from row in dtSqlMetrics.AsEnumerable()
                               where (DateTime.Compare(row.Field<DateTime>("Date"), nowDT) < 0 &&
                                  DateTime.Compare(row.Field<DateTime>("Date"), beginningWeek) > 0)
                               select new { DateValue = row.Field<DateTime>("DateTime"), Value = row.Field<double>("Value") };

            DataTable dtMetricsSqlWeeklyUngrouped = new DataTable();
            dtMetricsSqlWeeklyUngrouped.Columns.Add("DateTime", typeof(DateTime));
            dtMetricsSqlWeeklyUngrouped.Columns.Add("Value", typeof(double));

            foreach (var row in queryMonthly)
            {
                DataRow rw = dtMetricsSqlWeeklyUngrouped.NewRow();
                rw[0] = row.DateValue;
                rw[1] = row.Value;
                dtMetricsSqlWeeklyUngrouped.Rows.Add(rw);
            }
            //// plotting Kagi chart
            PlotTimeSeriesChart(dtMetricsSqlWeeklyUngrouped, SeriesChartType.Kagi, Color.FromArgb(112, 255, 200), 1,//// thin line
               string.Format("Actual SQL Memory Load Values for {0} to {1}", beginningWeek.ToString("dd/MM/yyyy"), DateTime.Today.ToString("dd/MM/yyyy")),
               string.Format("Sql_{0}", DateTime.Now.Date.ToString("yyyyMMdd"))); */
        }

        private void PlotTimeSeriesChart(DataTable dTable, SeriesChartType chartType, Color lineColor,
            int lineWidth, string chartTitle, string fileName)
        {
            try
            {
                //prepare chart control...
                Chart chart = new Chart();
                chart.DataSource = dTable;//dataSet.Tables[0];
                chart.Width = 1000;
                chart.Height = 350;
                //create serie...
                Series serie1 = new Series();
                serie1.Name = "Serie1";
                serie1.Color = lineColor; //Color.FromArgb(112, 255, 200); //// slightly green
                serie1.BorderColor = Color.FromArgb(164, 164, 164);
                serie1.ChartType = chartType; // SeriesChartType.Line;//Column
                serie1.BorderDashStyle = ChartDashStyle.Solid;
                serie1.BorderWidth = lineWidth;
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
                chart.Titles.Add(chartTitle);
                //databind...
                chart.DataBind();
                //save result...
                chart.SaveImage(string.Format("{0}\\{1}_{2}", _reportFolder, fileName, ".png"), ChartImageFormat.Png);
            }
            catch { }
        }
    }
}
