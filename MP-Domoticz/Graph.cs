using MediaPortal.GUI.Library;
using OxyPlot;
using OxyPlot.Annotations;
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
       
        public static void GenerateGraph(DomoticzServer DS, int idx, string fileName, string graphType)
        {
            var plotModel1 = new PlotModel();
            var dateTimeAxis1 = new DateTimeAxis();
            var linearAxis1 = new LinearAxis();
            var linearAxis2 = new LinearAxis();
            var lineSeries1 = new LineSeries();            

            Log.Info("Generategraph:" + idx + " " + graphType);
            DomoticzServer.GraphResponse GR = null;

            switch (graphType)
            {
                case "Temp":
                case "TempHum":
                    GR = DS.GetGraphData(idx, "temp", "month");

                    //plotModel1.Title = "Temperatur senaste månaden";
                    //var linearAxis1 = new LinearAxis();


                    dateTimeAxis1.IntervalType = DateTimeIntervalType.Weeks;
                    dateTimeAxis1.MajorGridlineStyle = LineStyle.Solid;
                    dateTimeAxis1.StringFormat = "dd.MMM";
                    dateTimeAxis1.Position = AxisPosition.Bottom;
                    plotModel1.Axes.Add(dateTimeAxis1);


                    linearAxis1 = new LinearAxis();
                    linearAxis1.Key = "hum";


                    linearAxis2 = new LinearAxis();
                    linearAxis2.Key = "temp";
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


                    var lineSeriesTempHum = new LineSeries();
                    lineSeriesTempHum.Color = OxyColors.Green;
                    lineSeriesTempHum.MarkerFill = OxyColors.Transparent;
                    lineSeriesTempHum.DataFieldX = "Time";
                    lineSeriesTempHum.DataFieldY = "Value";
                    //lineSeries1.Title = "Average";                       

                    lineSeries1 = new LineSeries();
                    lineSeries1.Color = OxyColors.Blue;
                    lineSeries1.MarkerFill = OxyColors.Transparent;
                    lineSeries1.DataFieldX = "Time";
                    lineSeries1.DataFieldY = "Value";
                    //lineSeries1.Title = "Average";                       

                    if (GR != null && !IsNullOrEmpty(GR.result))
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
                            lineSeriesTempHum.Points.Add(new DataPoint(dt.ToOADate(), res.hu));
                            i++;
                        }
                        linearAxis2.Maximum = maximum;
                        linearAxis2.Minimum = minimum;
                    }

                    lineSeries1.YAxisKey = "temp";

                    areaSeries1.Smooth = true;
                    lineSeries1.Smooth = true;
                    plotModel1.Series.Add(areaSeries1);
                    plotModel1.Series.Add(lineSeries1);
                    if (graphType == "TempHum")
                    {
                        linearAxis1.Position = AxisPosition.Right;
                        plotModel1.Axes.Add(linearAxis1);
                        lineSeriesTempHum.YAxisKey = "hum";
                        plotModel1.Series.Add(lineSeriesTempHum);
                    }
                    break;

                case "Barometer":
                    GR = DS.GetGraphData(idx, "temp", "month");


                    dateTimeAxis1.IntervalType = DateTimeIntervalType.Weeks;
                    dateTimeAxis1.MajorGridlineStyle = LineStyle.Solid;
                    dateTimeAxis1.StringFormat = "dd.MMM";
                    dateTimeAxis1.Position = AxisPosition.Bottom;
                    plotModel1.Axes.Add(dateTimeAxis1);


                    linearAxis2 = new LinearAxis();
                    plotModel1.Axes.Add(linearAxis2);

                    lineSeries1 = new LineSeries();
                    lineSeries1.Color = OxyColors.Blue;
                    lineSeries1.MarkerFill = OxyColors.Transparent;
                    lineSeries1.DataFieldX = "Time";
                    lineSeries1.DataFieldY = "Value";
                    lineSeries1.Title = "Barometer";

                    if (GR != null && !IsNullOrEmpty(GR.result))
                    {
                        int i = 0;
                        double maximum = -1000;
                        double minimum = 1000;
                        foreach (DomoticzServer.GraphResult res in GR.result)
                        {
                            DateTime dt = Convert.ToDateTime(res.d);

                            if (res.ba < minimum)
                            {
                                minimum = res.ba - 2;
                            }

                            if (res.ba > maximum)
                            {
                                maximum = res.ba + 2;
                            }


                            lineSeries1.Points.Add(new DataPoint(dt.ToOADate(), res.ba));
                            i++;
                        }
                        linearAxis2.Maximum = maximum;
                        linearAxis2.Minimum = minimum;
                    }

                    lineSeries1.Smooth = true;
                    plotModel1.Series.Add(lineSeries1);
                    break;

                case "Percentage":
                    GR = DS.GetGraphData(idx, "Percentage", "month");

                    dateTimeAxis1.IntervalType = DateTimeIntervalType.Weeks;
                    dateTimeAxis1.MajorGridlineStyle = LineStyle.Solid;
                    dateTimeAxis1.StringFormat = "dd.MMM";
                    dateTimeAxis1.Position = AxisPosition.Bottom;
                    plotModel1.Axes.Add(dateTimeAxis1);


                    linearAxis2 = new LinearAxis();
                    plotModel1.Axes.Add(linearAxis2);

                    var lineSeriesAvg = new LineSeries();
                    lineSeriesAvg.Color = OxyColors.Blue;
                    lineSeriesAvg.MarkerFill = OxyColors.Transparent;
                    lineSeriesAvg.DataFieldX = "Time";
                    lineSeriesAvg.DataFieldY = "Value";
                    lineSeriesAvg.Title = "Average";

                    var lineSeriesMin = new LineSeries();
                    lineSeriesMin.Color = OxyColors.Yellow;
                    lineSeriesMin.MarkerFill = OxyColors.Transparent;
                    lineSeriesMin.DataFieldX = "Time";
                    lineSeriesMin.DataFieldY = "Value";
                    lineSeriesMin.Title = "min";

                    var lineSeriesMax = new LineSeries();
                    lineSeriesMax.Color = OxyColors.Green;
                    lineSeriesMax.MarkerFill = OxyColors.Transparent;
                    lineSeriesMax.DataFieldX = "Time";
                    lineSeriesMax.DataFieldY = "Value";
                    lineSeriesMax.Title = "max";

                    if (GR != null && !IsNullOrEmpty(GR.result))
                    {
                        int i = 0;
                        double maximum = -1000;
                        double minimum = 1000;
                        foreach (DomoticzServer.GraphResult res in GR.result)
                        {
                            DateTime dt = Convert.ToDateTime(res.d);

                            if (res.v_min < minimum)
                            {
                                minimum = res.v_min - 5;
                            }

                            if (res.v_max > maximum)
                            {
                                maximum = res.v_max + 5;
                            }

                            lineSeriesAvg.Points.Add(new DataPoint(dt.ToOADate(), res.v_avg));
                            lineSeriesMin.Points.Add(new DataPoint(dt.ToOADate(), res.v_min));
                            lineSeriesMax.Points.Add(new DataPoint(dt.ToOADate(), res.v_max));
                            i++;
                        }
                        linearAxis2.Maximum = maximum;
                        linearAxis2.Minimum = minimum;
                    }

                    lineSeriesAvg.Smooth = true;
                    lineSeriesMin.Smooth = true;
                    lineSeriesMax.Smooth = true;
                    plotModel1.Series.Add(lineSeriesAvg);
                    plotModel1.Series.Add(lineSeriesMin);
                    plotModel1.Series.Add(lineSeriesMax);
                    break;

                case "Rain":                    
                    GR = DS.GetGraphData(idx, "rain", "week");                    
                    plotModel1.LegendBorderThickness = 0;
                    plotModel1.LegendOrientation = LegendOrientation.Horizontal;
                    plotModel1.LegendPlacement = LegendPlacement.Outside;
                    plotModel1.LegendPosition = LegendPosition.BottomCenter;
                    // plotModel1.Title = "Simple model";
                    var categoryAxis1 = new CategoryAxis();
                    categoryAxis1.MinorStep = 1;


                    linearAxis1 = new LinearAxis();
                    linearAxis1.AbsoluteMinimum = 0;
                    linearAxis1.MaximumPadding = 0.06;
                    linearAxis1.MinimumPadding = 0;
                    plotModel1.Axes.Add(linearAxis1);

                    var columnSeries1 = new ColumnSeries();
                    columnSeries1.StrokeThickness = 1;
                    
                    if (GR.result != null)
                        foreach (DomoticzServer.GraphResult res in GR.result)
                        {
                            DateTime dt = Convert.ToDateTime(res.d);
                            categoryAxis1.Labels.Add(dt.ToString("dd.MMM"));
                            columnSeries1.Items.Add(new ColumnItem(res.mm, -1));                         
                        }

                    plotModel1.Axes.Add(categoryAxis1);
                    plotModel1.Series.Add(columnSeries1);                    
                    break;

                case "lightlog":                    
                    var LR = DS.GetLightLog(idx);                    
                    dateTimeAxis1.IntervalType = DateTimeIntervalType.Weeks;
                    dateTimeAxis1.MajorGridlineStyle = LineStyle.Solid;
                    dateTimeAxis1.StringFormat = "dd.MMM";
                    dateTimeAxis1.Position = AxisPosition.Bottom;
                    plotModel1.Axes.Add(dateTimeAxis1);

                    linearAxis2 = new LinearAxis();
                    plotModel1.Axes.Add(linearAxis2);

                    var stairStepSeries1 = new StairStepSeries();
                    stairStepSeries1.VerticalStrokeThickness = 0.4;
                    stairStepSeries1.Color = OxyColors.SkyBlue;
                    stairStepSeries1.StrokeThickness = 3;

                    if (LR!=null && !IsNullOrEmpty(LR.result))
                    {
                        Log.Info("3");
                        Log.Info(":>" + LR.result.Count);
                        Log.Info("4");
                        foreach (DomoticzServer.LightLogResult res in LR.result)
                        {
                            
                            DateTime dt = Convert.ToDateTime(res.Date);

                            int level = 0;
                            switch (res.Status)
                            {
                                case "On":
                                case "Group On":
                                case "Open":
                                    level = 100;
                                    break;
                                default:
                                    level = 0;
                                    break;
                            }
                            stairStepSeries1.Points.Add(new DataPoint(dt.ToOADate(), level));
                        }
                    }
                    else
                    {
                        var rectangleAnnotationRA = new RectangleAnnotation();
                        rectangleAnnotationRA.Fill = OxyColor.FromArgb(99, 0, 0, 255);
                        rectangleAnnotationRA.StrokeThickness = 2;
                        rectangleAnnotationRA.MinimumX = 20;
                        rectangleAnnotationRA.MaximumX = 70;
                        rectangleAnnotationRA.MinimumY = 10;
                        rectangleAnnotationRA.MaximumY = 40;
                        rectangleAnnotationRA.Text = "No data found!";
                        rectangleAnnotationRA.TextRotation = 10;
                        plotModel1.Annotations.Add(rectangleAnnotationRA);
                    }

                    plotModel1.Series.Add(stairStepSeries1);
                    break;

                case "Wind":
                    GR = DS.GetGraphData(idx, "wind", "month");

                    dateTimeAxis1.IntervalType = DateTimeIntervalType.Weeks;
                    dateTimeAxis1.MajorGridlineStyle = LineStyle.Solid;
                    dateTimeAxis1.StringFormat = "dd.MMM";
                    dateTimeAxis1.Position = AxisPosition.Bottom;
                    plotModel1.Axes.Add(dateTimeAxis1);


                    linearAxis2 = new LinearAxis();
                    linearAxis2.Key = "wind";
                    plotModel1.Axes.Add(linearAxis2);



                    lineSeries1 = new LineSeries();
                    lineSeries1.Color = OxyColors.Blue;
                    lineSeries1.MarkerFill = OxyColors.Transparent;
                    lineSeries1.DataFieldX = "Time";
                    lineSeries1.DataFieldY = "Value";
                    //lineSeries1.Title = "Average";                       

                    double maxwind = 0;
                    if (GR != null && !IsNullOrEmpty(GR.result))
                    {
                        int i = 0;
                        
                        foreach (DomoticzServer.GraphResult res in GR.result)
                        {
                            DateTime dt = Convert.ToDateTime(res.d);                            
                            lineSeries1.Points.Add(new DataPoint(dt.ToOADate(), res.sp));
                            if(maxwind< res.sp)
                            {
                                maxwind = res.sp;
                            }
                            i++;
                        }                        
                    }

                    plotModel1.Series.Add(lineSeries1);
                    
                    break;

                default:
                    {
                        //plotModel1.Title = "RectangleAnnotation";
                        linearAxis1 = new LinearAxis();
                        linearAxis1.Position = AxisPosition.Bottom;
                        plotModel1.Axes.Add(linearAxis1);
                        linearAxis2 = new LinearAxis();
                        plotModel1.Axes.Add(linearAxis2);
                        var rectangleAnnotationRA = new RectangleAnnotation();
                        rectangleAnnotationRA.Fill = OxyColor.FromArgb(99, 0, 0, 255);
                        rectangleAnnotationRA.StrokeThickness = 2;
                        rectangleAnnotationRA.MinimumX = 20;
                        rectangleAnnotationRA.MaximumX = 70;
                        rectangleAnnotationRA.MinimumY = 10;
                        rectangleAnnotationRA.MaximumY = 40;
                        rectangleAnnotationRA.Text = "No graph data defined!";
                        rectangleAnnotationRA.TextRotation = 10;
                        plotModel1.Annotations.Add(rectangleAnnotationRA);
                    }
                    break;
            }
            
            var pngExporter = new PngExporter();
            PngExporter.Export(plotModel1, fileName, 600, 300, Brushes.White);
        }

        /// <summary>
        /// Determines whether the collection is null or contains no elements.
        /// </summary>
        /// <typeparam name="T">The IEnumerable type.</typeparam>
        /// <param name="enumerable">The enumerable, which may be null or empty.</param>
        /// <returns>
        ///     <c>true</c> if the IEnumerable is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>( IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }
            /* If this is a list, use the Count property for efficiency. 
             * The Count property is O(1) while IEnumerable.Count() is O(N). */
            var collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                return collection.Count < 1;
            }
            return !enumerable.Any();
        }


    }
    
}
