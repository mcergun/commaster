using CoMaster.Commands;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CoMaster.ViewModels
{
    internal class PingViewModel : Notifier
    {
        public int MaximumPlotEntries { get; set; }
        public PlotModel Plot { get; set; }
        public LineSeries PlotItems { get; set; }
        public string Destination { get; set; }
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                if (value != count)
                {
                    count = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public PointCollection Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }

        public ICommand SendPingCommand
        {
            get
            {
                if (sendPing == null)
                {
                    sendPing = new RelayCommand<object>(SendPing, CanSendPing);
                }
                return sendPing;
            }
        }

        public PingViewModel()
        {
            Destination = "google.com";
            Count = 1;
            Plot = new PlotModel { Title = "Latencies" };
            MaximumPlotEntries = 20;
            LinearAxis yAx = new LinearAxis()
            {
                Position = AxisPosition.Left,
                MinimumRange = 10,
                MaximumRange = 300,
                AbsoluteMinimum = 0,
            };
            LinearAxis xAx = new LinearAxis() {
                Position = AxisPosition.Bottom,
                MinimumRange = 5,
                MaximumRange = MaximumPlotEntries,
                AbsoluteMaximum = MaximumPlotEntries,
                AbsoluteMinimum = 0,
                MinorStep = 1,
                MinimumPadding = 8,
            };
            Plot.Axes.Add(xAx);
            Plot.Axes.Add(yAx);
            PlotItems = new LineSeries();
            PlotItems.EdgeRenderingMode = EdgeRenderingMode.PreferSharpness;
            PlotItems.InterpolationAlgorithm = InterpolationAlgorithms.UniformCatmullRomSpline;
            Plot.Series.Add(PlotItems);
        }

        public async void SendPing(object count)
        {
            if (!(count is int)) return;
            jobDone = false;
            try
            {
                int ct = (int)count;
                using(Pinger pg = new Pinger(Destination, 1000))
                {
                    pg.PacketTimedOut += Pg_PacketTimedOut;
                    pg.ReplyReceived += Pg_ReplyReceived;
                    pg.RequestFinished += Pg_RequestFinished;
                    if (ct > 1) await pg.SendMultipleAsync("Hello There!", ct);
                    else await pg.SendSingleAsync("Hello There!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                jobDone = true;
            }
        }

        public bool CanSendPing(object val)
        {
            return jobDone;
        }

        private void AddPoint(double latency)
        {
            if (pointCount > MaximumPlotEntries)
            {
                PlotItems.Points.RemoveAt(0);
            }
            PlotItems.Points.Add(new DataPoint(pointCount++, latency));
            for (int i = 0; i < PlotItems.Points.Count; i++)
            {
                PlotItems.Points[i] = new DataPoint(i, PlotItems.Points[i].Y);
            }
            Plot.InvalidatePlot(true);
        }

        private void Pg_RequestFinished(object sender, PingResults e)
        {
            jobDone = true;
            Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
        }

        private void Pg_ReplyReceived(object sender, PingResults e)
        {
            //throw new NotImplementedException();
            AddPoint(e.LastLatency);
        }

        private void Pg_PacketTimedOut(object sender, PingResults e)
        {
            AddPoint(1000);
        }

        PointCollection points = new PointCollection();
        int count = 0;
        ICommand sendPing = null;
        int pointCount = 0;
        bool jobDone = true;
    }
}
