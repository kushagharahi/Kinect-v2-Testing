using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectTesting
{
    class Vect
    {
        public double x, y;

        public Vect()
        {
        }

        public Vect(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double findDistanceBetween(Vect v)
        {
            double a = this.y - v.y;
            double b = this.x - v.x;
            double c = Math.Sqrt((a * a) + (b * b));
            return c;
        }

        public double findDistanceBetweenSq(Vect v)
        {
            double a = this.y - v.y;
            double b = this.x - v.x;
            double c = ((a * a) + (b * b));
            return c;
        }

        public double getLength()
        {
            return Math.Sqrt((this.x * this.x) + (this.y * this.y));
        }

        public void setLength(double length)
        {
            double l = this.getLength();
            this.x = (this.x / l) * length;
            this.y = (this.y / l) * length;
        }

        public Vect sub(Vect b)
        {
            return new Vect(this.x - b.x, this.y - b.y);
        }

        public Vect add(Vect b)
        {
            return new Vect(this.x + b.x, this.y + b.y);
        }

        public Vect mult(Vect b)
        {
            return new Vect(this.x * b.x, this.y * b.y);
        }

        public Vect mult(Double b)
        {
            return new Vect(this.x * b, this.y * b);
        }

        public Vect div(Vect b)
        {
            return new Vect(this.x / b.x, this.y / b.y);
        }
        public String toString()
        {
            return this.x + " " + this.y;
        }
    }
}
