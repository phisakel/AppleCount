using System;
using System.Collections.Generic;
using System.Text;

namespace AppleCount
{
    public class Position
    {
        public Position(double x, double y, double r, int z)
        {
            X = x;
            Y = y;
            R = r;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double R { get; set; }

        public int Z { get; set; }


    }
}
