using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class ConnectLine : UserControl
    {
        public Node start;
        public Node end;

        public List<Point> points = new List<Point>();

        public ConnectLine(Node from)
        {
            this.InitializeComponent();
            start = from;
            PointerReleased += ConnectLine_PointerReleased;
        }

        private void ConnectLine_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var parent = Parent as Panel;
            if (parent != null)
            {
                Point point = e.GetCurrentPoint(parent).Position;
                foreach (var element in parent.Children)
                {
                    Node node = element as Node;

                    if (node != null && node != start)
                    {
                        double left = Canvas.GetLeft(node);
                        double top = Canvas.GetTop(node);
                        if (point.X > left && point.Y > top && point.X < left + node.ActualWidth && point.Y < top + node.ActualHeight)
                        {
                            end = node;
                            break;
                        }
                    }
                }
            }

            if (start != null && end != null && !start.Connections.ContainsKey(end) && !end.Connections.ContainsKey(start))
            {
                var page = (Window.Current.Content as Frame)?.Content as MainPage;
                if (page != null)
                {
                    var action = new LineCreationAction(start, end);
                    action.Perform(page.Current, page.Canvas);
                }

            }
            else
            {
                
            }

            ReleasePointerCaptures();
            Remove();
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (start != null)
            {
                PointCollection pc = new PointCollection();
                pc.Add(new Point(Canvas.GetLeft(start) + start.ActualWidth / 2, (Canvas.GetTop(start) + start.ActualHeight / 2)));
                pc.Add(e.GetCurrentPoint(this.Parent as UIElement).Position);
                line.Points = pc;
            }
            e.Handled = true;
        }

        private void UserControl_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            Remove();
        }

        private void Remove()
        {
            var parent = (this.Parent as Panel);
            if (parent != null)
            {
                parent.Children.Remove(this);
            }
        }
    }
}
