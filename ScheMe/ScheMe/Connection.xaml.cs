using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ScheMe
{
    public sealed partial class Connection : UserControl
    {
        Node start;
        Node end;

        List<Point> points = new List<Point>();

        public Node From { get => start; set => start = value; }
        public Node To { get => end; set => end = value; }

        public List<Point> Bends { get => points; }

        public Connection(Node from, Node to)
        {
            this.InitializeComponent();
            start = from;
            end = to;
            Invalidate();
        }


        private void Remove()
        {
            var parent = (this.Parent as Panel);
            if (parent != null)
            {
                parent.Children.Remove(this);
                if (start != null && end != null)
                {
                    if (start.Connections.ContainsValue(this)) start.Connections.Remove(end);
                    if (end.Connections.ContainsValue(this)) end.Connections.Remove(start);
                }
            }
        }

        public void Invalidate()
        {
            if (points.Count == 0)
            {
                if (start != null && end != null)
                {
                    PointCollection pc = new PointCollection();
                    pc.Add(new Point(Canvas.GetLeft(start) + start.ActualWidth / 2, (Canvas.GetTop(start) + start.ActualHeight / 2)));
                    pc.Add(new Point(Canvas.GetLeft(end) + start.ActualWidth / 2, (Canvas.GetTop(end) + start.ActualHeight / 2)));
                    line.Points = pc;
                }
            }
        }
    }
}
