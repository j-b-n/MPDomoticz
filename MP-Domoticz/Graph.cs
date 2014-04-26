using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP_Domoticz
{
    class Graph
    {
        public static void GenerateGraph(DomoticzServer DS, int idx, string fileName)
        {                                                
            
            DomoticzServer.GraphResponse GR = DS.GetGraphData(idx, "temp", "month");

            var plotModel1 = new PlotModel();
            //plotModel1.Title = "Temperatur senaste månaden";
            //var linearAxis1 = new LinearAxis();

            var dateTimeAxis1 = new DateTimeAxis();
            dateTimeAxis1.IntervalType = DateTimeIntervalType.Weeks;
            dateTimeAxis1.MajorGridlineStyle = LineStyle.Solid;
            dateTimeAxis1.StringFormat = "dd.MMM";
            dateTimeAxis1.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(dateTimeAxis1);


            var linearAxis2 = new LinearAxis();
            plotModel1.Axes.Add(linearAxis2);
            var areaSeries1 = new AreaSeries();
            areaSeries1.DataFieldX2 = "Time";
            areaSeries1.DataFieldY2 = "Minimum";
            areaSeries1.Fill = OxyColors.LightBlue;
            areaSeries1.Color = OxyColors.Red;
            areaSeries1.MarkerFill = OxyColors.Transparent;
            areaSeries1.StrokeThickness = 0;
            areaSeries1.DataFieldX = "Time";
            areaSeries1.DataFieldY = "Maximum";
            //areaSeries1.Title = "Maximum/Minimum";

            var lineSeries1 = new LineSeries();
            lineSeries1.Color = OxyColors.Blue;
            lineSeries1.MarkerFill = OxyColors.Transparent;
            lineSeries1.DataFieldX = "Time";
            lineSeries1.DataFieldY = "Value";
            //lineSeries1.Title = "Average";                       

            if (GR != null)
            {
                int i = 0;
                double maximum = -1000;
                double minimum = 1000;
                foreach (DomoticzServer.GraphResult res in GR.result)
                {                    
                    DateTime dt = Convert.ToDateTime(res.d);

                    if (res.tm < minimum)
                    {
                        minimum = res.tm - 2;
                    }

                    if (res.te > maximum)
                    {
                        maximum = res.te + 2;
                    }

                    areaSeries1.Points2.Add(new DataPoint(dt.ToOADate(), res.tm));
                    areaSeries1.Points.Add(new DataPoint(dt.ToOADate(), res.te));
                    lineSeries1.Points.Add(new DataPoint(dt.ToOADate(), res.ta));
                    i++;
                }
                linearAxis2.Maximum = maximum;
                linearAxis2.Minimum = minimum;
            }

            plotModel1.Series.Add(areaSeries1);
            plotModel1.Series.Add(lineSeries1);
            
            var pngExporter = new PngExporter();
            PngExporter.Export(plotModel1, fileName, 600, 300, Brushes.White);            
        }
    }
}
