using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ScheMe
{
    static class Extensions
    {
        public static double Distance(this Point p)
        {
            return Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2));
        }

        public static double Distance(this Point p, Point to)
        {
            return Math.Sqrt(Math.Pow(p.X - to.X, 2) + Math.Pow(p.Y - to.Y, 2));
        }
    }
}
