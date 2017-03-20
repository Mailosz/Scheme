using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ScheMe
{
    public sealed partial class Node : UserControl
    {
        static Node selNode;
        public static Node SelectedNode
        {
            get { return selNode; }
            set
            {
                if (selNode != null)
                {
                    selNode.OnUnselected();
                }
                selNode = value;
                if (selNode != null)
                {
                    selNode.OnSelected();
                }
            }
        }
        public enum ClickType { None, Click, Move, Connect}
        public ClickType Clicked = ClickType.None;
        public Dictionary<Node, Connection> Connections = new Dictionary<Node, Connection>();

        Point lastPoint;

        public string NodeName { get { return nameBox.Text; } set { nameBox.Text = value; } }

        public string Description { get { return descBox.Text; } set { descBox.Text = value; } }

        public Node()
        {
            this.InitializeComponent();
            VisualStateManager.GoToState(this, "Unselected", false);
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            SelectedNode = this;
        }

        private void moveButton_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Translate(e.Delta.Translation);
        }

        public void Translate(Point translation)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + (translation.X));
            Canvas.SetTop(this, Canvas.GetTop(this) + (translation.Y));
            var d = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var connection in Connections)
                {
                    connection.Value.Invalidate();
                }
            });
        }

        private void connectButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var line = new ConnectLine(this); 

            var parent = this.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Add(line);
                var b = line.CapturePointer(e.Pointer);
            }
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                Clicked = ClickType.Click;
                lastPoint = e.GetCurrentPoint(null).Position;
                CapturePointer(e.Pointer);
            }
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Clicked == ClickType.Click)
            {
                var current = e.GetCurrentPoint(null).Position;
                if (lastPoint.Distance(current) > 5)
                {
                    Clicked = ClickType.Move;
                    lastPoint = current;
                }
            }
            else if (Clicked == ClickType.Move)
            {
                var current = e.GetCurrentPoint(null).Position;
                Translate(new Point(current.X - lastPoint.X, current.Y - lastPoint.Y));
                lastPoint = current;
            }
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Clicked = ClickType.None;
        }

        private void UserControl_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            Clicked = ClickType.None;
        }

        private void OnSelected()
        {
            nameBox.IsEnabled = true;
            VisualStateManager.GoToState(selNode, "Selected", true);
        }

        private void OnUnselected()
        {
            nameBox.IsEnabled = false;
            VisualStateManager.GoToState(selNode, "Unselected", true);
        }

        private void nameBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            var t = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                nameBox.FontSize = 16;
                nameBox.TextWrapping = TextWrapping.NoWrap;
                var size = new Size(Double.PositiveInfinity, double.PositiveInfinity);
                nameBox.Measure(size);
                if (nameBox.DesiredSize.Width > nameBox.ActualWidth)
                {
                    nameBox.FontSize = 14;
                    nameBox.Measure(size);
                    if (nameBox.DesiredSize.Width > nameBox.ActualWidth)
                    {
                        nameBox.FontSize = 12;
                        nameBox.Measure(size);
                        if (nameBox.DesiredSize.Width > nameBox.ActualWidth)
                        {
                            nameBox.FontSize = 10;
                            nameBox.Measure(size);
                            if (nameBox.DesiredSize.Width > nameBox.ActualWidth)
                            {
                                nameBox.TextWrapping = TextWrapping.Wrap;
                            }
                        }
                    }
                }
            });
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            var action = new NodeRemoveAction(this);
            action.Perform();
        }

    }

    public sealed class PressAndHoldButton : Button
    {
        public event PointerEventHandler PointerPressPreview;

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            PointerPressPreview(this, e);
            base.OnPointerPressed(e);
        }
    }
}
