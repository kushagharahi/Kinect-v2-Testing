using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace KinectTesting
{
    class Ball
    {
        public Vect pos, oldPos, acc, vel;
        public double rad, mass;

        public Ball(Vect pos, Vect acc, double rad)
        {
            this.pos = pos;
            this.acc = acc;
            this.oldPos = pos;
            this.rad = rad;
            this.vel = new Vect();
            mass = this.rad;
        }

        public Ball(Vect pos, Vect acc, double rad, double mass)
        {
            this.pos = pos;
            this.acc = acc;
            this.oldPos = pos;
            this.rad = rad;
            this.vel = new Vect();
            this.mass = mass;
        }

        public void update()
        {
            vel = pos.sub(oldPos);
            oldPos = pos;
            pos = pos.add(vel.add(acc));
        }

        public void updateVel()
        {
            vel = pos.sub(oldPos);
            oldPos = pos;
        }

        public bool isInCir(Vect pos)
        {
            double db = Math.Abs(this.pos.findDistanceBetween(pos));
            if (db <= this.rad)
                return true;
            return false;
        }

        public void setMagnitudeVelocity(double m)
        {
            Vect vel = pos.sub(oldPos);
            vel.setLength(m);
            this.oldPos = pos.sub(vel);
        }

        public void wallColl(double fac, int xMax, int yMax)
        {
            int xMin = 0;
            int yMin = 0;
            double vX = pos.x - oldPos.x;
            double vY = pos.y - oldPos.y;
            vX *= fac;
            vY *= fac;
            if (this.pos.x - rad < xMin)
            {
                this.pos.x = Math.Max(xMin, this.pos.x - this.rad) + this.rad;
                this.oldPos.x = this.pos.x + vX;
            }
            if (this.pos.x + rad > xMax)
            {
                this.pos.x = Math.Min(xMax, this.pos.x + this.rad) - this.rad;
                this.oldPos.x = this.pos.x + vX;
            }
            if (this.pos.y - rad < yMin)
            {
                this.pos.y = Math.Max(yMin, this.pos.y - this.rad) + this.rad;
                this.oldPos.y = this.pos.y + vY;
            }
            if (this.pos.y + rad > yMax)
            {
                this.pos.y = Math.Min(yMax, this.pos.y + this.rad) - this.rad;
                this.oldPos.y = this.pos.y + vY;
            }
        }
        public void wallColl(double fac, int xMin, int yMin, int xMax, int yMax)
        {

            double vX = pos.x - oldPos.x;
            double vY = pos.y - oldPos.y;
            vX *= fac;
            vY *= fac;
            if (this.pos.x - rad < xMin)
            {
                this.pos.x = Math.Max(xMin, this.pos.x - this.rad) + this.rad;
                this.oldPos.x = this.pos.x + vX;
            }
            if (this.pos.x + rad > xMax)
            {
                this.pos.x = Math.Min(xMax, this.pos.x + this.rad) - this.rad;
                this.oldPos.x = this.pos.x + vX;
            }
            if (this.pos.y - rad < yMin)
            {
                this.pos.y = Math.Max(yMin, this.pos.y - this.rad) + this.rad;
                this.oldPos.y = this.pos.y + vY;
            }
            if (this.pos.y + rad > yMax)
            {
                this.pos.y = Math.Min(yMax, this.pos.y + this.rad) - this.rad;
                this.oldPos.y = this.pos.y + vY;
            }
        }

        public void CheckColl(Ball a)	//Checking for Circle Collisions
        {
            double db = a.pos.findDistanceBetweenSq(this.pos);
            double er = a.rad + this.rad;
            if (db < er * er)
            {
                if (db == 0)
                    db = 1;
                double m1m2 = a.mass + this.mass;
                Vect d = a.pos.sub(this.pos);
                d.setLength(er - Math.Sqrt(db));
                a.pos = a.pos.add(d.mult((this.mass / m1m2)));
            }
        }

        public void CheckColl(List<Ball> a)
        {
            foreach (Ball x in a)
            {

                if (x != this)
                {
                    double db = x.pos.findDistanceBetweenSq(this.pos); //distance between
                    double er = x.rad + this.rad;

                    if (db < er * er)
                    {
                        double m1m2 = x.mass + this.mass;
                        Vect d = x.pos.sub(this.pos);
                        d.setLength(er - Math.Sqrt(db));
                        x.pos = x.pos.add(d.mult((this.mass / m1m2)));
                        this.pos = this.pos.sub(d.mult((x.mass / m1m2)));

                    }

                }
            }
        }

        public void drawBall(DrawingContext dc, SolidColorBrush brush)
        {
            dc.DrawEllipse(brush, null, new Point(this.pos.x, this.pos.y), this.rad, this.rad);
        }

        public void drawBall(DrawingContext dc, Pen pen)
        {
            dc.DrawEllipse(null, pen, new Point(this.pos.x, this.pos.y), this.rad, this.rad);
        }

        public Color IntToCol(int a)
        {
            byte red = (byte)((a & 0xFF0000) >> 4);
            byte green = (byte)((a & 0x00FF00) >> 2);
            byte blue = (byte)((a & 0x0000FF));
            return Color.FromRgb(red, green, blue);
        }

    }
}
