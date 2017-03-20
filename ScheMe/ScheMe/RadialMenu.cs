using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ScheMe
{
    public class RadialMenu : Panel
    {
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(RadialMenu), null);
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register("StartAngle", typeof(double), typeof(RadialMenu), null);
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public static readonly DependencyProperty EndAngleProperty = DependencyProperty.Register("EndAngle", typeof(double), typeof(RadialMenu), null);
        public double EndAngle
        {
            get => (double)GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        public static readonly DependencyProperty ChildSizeProperty = DependencyProperty.Register("ChildSize", typeof(Size), typeof(RadialMenu), null);
        public Size ChildSize
        {
            get => (Size)GetValue(ChildSizeProperty);
            set => SetValue(ChildSizeProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var child in Children)
            {
                child.Measure(ChildSize);
            }
            return new Size(Radius * 2, Radius * 2);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double end = EndAngle;
            if (end < StartAngle) end += 360;
            double angle = StartAngle * Math.PI / 180;
            double add = (EndAngle - StartAngle) * Math.PI / 180;

            foreach (var child in Children)
            {
                double x = finalSize.Width / 2 + Radius * Math.Cos(angle);
                double y = finalSize.Height / 2 + Radius * Math.Sin(angle);

                child.Arrange(new Rect(new Point(0,0), ChildSize));
                child.RenderTransform = new TranslateTransform() { X = x, Y = y};

                angle += add;
            }
            return finalSize;
        }
    }
}
