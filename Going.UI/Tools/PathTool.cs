using Going.UI.Enums;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Tools
{
    public class PathTool
    {
        #region Box
        public static SKPath Box(SKRect rect, GoRoundType round, float corner)
        {
            var path = new SKPath();

            var rt = new SKRoundRect(rect, corner);
            switch (round)
            {
                case GoRoundType.Rect: rt.SetNinePatch(rect, 0, 0, 0, 0); break;
                case GoRoundType.All: rt.SetNinePatch(rect, corner, corner, corner, corner); break;
                case GoRoundType.L: rt.SetNinePatch(rect, corner, corner, 0, corner); break;
                case GoRoundType.R: rt.SetNinePatch(rect, 0, corner, corner, corner); break;
                case GoRoundType.T: rt.SetNinePatch(rect, corner, corner, corner, 0); break;
                case GoRoundType.B: rt.SetNinePatch(rect, corner, 0, corner, corner); break;
                case GoRoundType.LT: rt.SetNinePatch(rect, corner, corner, 0, 0); break;
                case GoRoundType.RT: rt.SetNinePatch(rect, 0, corner, corner, 0); break;
                case GoRoundType.LB: rt.SetNinePatch(rect, corner, 0, 0, corner); break;
                case GoRoundType.RB: rt.SetNinePatch(rect, 0, 0, corner, corner); break;
                case GoRoundType.Ellipse: rt.SetOval(rect); break;
            }

            path.AddRoundRect(rt);

            return path;
        }
        #endregion
        #region Check
        public static SKPath Check(SKRect rect)
        {
            var INF = rect.Width / 4;
            var rtCheck = Util.FromRect(rect.Left, rect.Top, rect.Width, rect.Height);
            rtCheck.Inflate(-INF, -INF);

            var path = new SKPath();

            var points = new SKPoint[] { new SKPoint(rtCheck.Left, rtCheck.MidY),
                                         new SKPoint(rtCheck.MidX, rtCheck.Bottom),
                                         new SKPoint(rtCheck.Right, rtCheck.Top)};


            path.MoveTo(points[0]);
            path.LineTo(points[1]);
            path.LineTo(points[2]);

            return path;
        }
        #endregion
        #region Circle
        public static SKPath Circle(float x, float y, float r)
        {
            var path = new SKPath();

            path.AddCircle(x, y, r);

            return path;
        }
        #endregion
        #region RoundedPolygon
        public static SKPath RoundedPolygon(SKPoint[] points, float radius)
        {
            SKPath retval = new SKPath();
            if (points.Length < 3) throw new ArgumentException();

            var rects = new SKRect[points.Length];
            SKPoint pt1, pt2;
            Vector v1, v2, n1 = new Vector(), n2 = new Vector();
            SizeF size = new SizeF(2 * radius, 2 * radius);
            SKPoint center = new SKPoint();

            for (int i = 0; i < points.Length; i++)
            {
                pt1 = points[i];
                pt2 = points[i == points.Length - 1 ? 0 : i + 1];
                v1 = new Vector(pt2.X, pt2.Y) - new Vector(pt1.X, pt1.Y);
                pt2 = points[i == 0 ? points.Length - 1 : i - 1];
                v2 = new Vector(pt2.X, pt2.Y) - new Vector(pt1.X, pt1.Y);

                float sweepangle = (float)Vector.AngleBetween(v1, v2);
                if (sweepangle < 0)
                {
                    n1 = new Vector(v1.Y, -v1.X);
                    n2 = new Vector(-v2.Y, v2.X);
                }
                else
                {
                    n1 = new Vector(-v1.Y, v1.X);
                    n2 = new Vector(v2.Y, -v2.X);
                }

                n1.Normalize(); n2.Normalize();
                n1 *= radius; n2 *= radius;

                SKPoint pt = points[i];
                pt1 = new SKPoint((float)(pt.X + n1.X), (float)(pt.Y + n1.Y));
                pt2 = new SKPoint((float)(pt.X + n2.X), (float)(pt.Y + n2.Y));

                double m1 = v1.Y / v1.X, m2 = v2.Y / v2.X;
                if (v1.X == 0)
                {
                    center.X = pt1.X;
                    center.Y = (float)(m2 * (pt1.X - pt2.X) + pt2.Y);
                }
                else if (v1.Y == 0)
                {
                    center.X = (float)((pt1.Y - pt2.Y) / m2 + pt2.X);
                    center.Y = pt1.Y;
                }
                else if (v2.X == 0)
                {
                    center.X = pt2.X;
                    center.Y = (float)(m1 * (pt2.X - pt1.X) + pt1.Y);
                }
                else if (v2.Y == 0)
                {
                    center.X = (float)((pt2.Y - pt1.Y) / m1 + pt1.X);
                    center.Y = pt2.Y;
                }
                else
                {
                    center.X = (float)((pt2.Y - pt1.Y + m1 * pt1.X - m2 * pt2.X) / (m1 - m2));
                    center.Y = (float)(pt1.Y + m1 * (center.X - pt1.X));
                }

                rects[i] = Util.FromRect(center.X - 2, center.Y - 2, 4, 4);
                n1.Negate(); n2.Negate();
                pt1 = new SKPoint((float)(center.X + n1.X), (float)(center.Y + n1.Y));
                pt2 = new SKPoint((float)(center.X + n2.X), (float)(center.Y + n2.Y));

                var rect = Util.FromRect(center.X - radius, center.Y - radius, size.Width, size.Height);
                sweepangle = (float)Vector.AngleBetween(n2, n1);
                if (i == 0) retval.AddArc(rect, (float)Vector.AngleBetween(new Vector(1, 0), n2), sweepangle);
                else retval.ArcTo(rect, (float)Vector.AngleBetween(new Vector(1, 0), n2), sweepangle, false);
            }
            retval.Close();
            return retval;
        }
        #endregion
    }

    #region struct : Vector
    internal struct Vector
    {
        #region Properties
        #region X
        internal double _x;
        public double X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }

        }
        #endregion
        #region Y
        internal double _y;
        public double Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }

        }
        #endregion
        #region Length
        public double Length
        {
            get
            {
                return Math.Sqrt(_x * _x + _y * _y);
            }
        }
        #endregion
        #region LengthSquared
        public double LengthSquared
        {
            get
            {
                return _x * _x + _y * _y;
            }
        }
        #endregion
        #endregion

        #region Constructor
        public Vector(double x, double y)
        {
            _x = x;
            _y = y;
        }
        #endregion

        #region Method
        #region Equals
        public override bool Equals(object o)
        {
            if ((null == o) || !(o is Vector))
            {
                return false;
            }

            Vector value = (Vector)o;
            return Vector.Equals(this, value);
        }

        public bool Equals(Vector value)
        {
            return Vector.Equals(this, value);
        }
        #endregion
        #region GetHashCode
        public override int GetHashCode()
        {
            // Perform field-by-field XOR of HashCodes
            return X.GetHashCode() ^
                   Y.GetHashCode();
        }
        #endregion
        #region Normalize
        public void Normalize()
        {
            this /= Math.Max(Math.Abs(_x), Math.Abs(_y));
            this /= Length;
        }
        #endregion
        #region Negate
        public void Negate()
        {
            _x = -_x;
            _y = -_y;
        }
        #endregion
        #endregion

        #region Static Method
        #region Equals
        public static bool Equals(Vector vector1, Vector vector2)
        {
            return vector1.X.Equals(vector2.X) &&
                   vector1.Y.Equals(vector2.Y);
        }
        #endregion
        #region CrossProduct
        public static double CrossProduct(Vector vector1, Vector vector2)
        {
            return vector1._x * vector2._y - vector1._y * vector2._x;
        }
        #endregion
        #region AngleBetween
        public static double AngleBetween(Vector vector1, Vector vector2)
        {
            double sin = vector1._x * vector2._y - vector2._x * vector1._y;
            double cos = vector1._x * vector2._x + vector1._y * vector2._y;

            return Math.Atan2(sin, cos) * (180 / Math.PI);
        }
        #endregion
        #region Operator
        public static bool operator ==(Vector vector1, Vector vector2)
        {
            return vector1.X == vector2.X &&
                   vector1.Y == vector2.Y;
        }

        public static bool operator !=(Vector vector1, Vector vector2)
        {
            return !(vector1 == vector2);
        }

        public static Vector operator -(Vector vector)
        {
            return new Vector(-vector._x, -vector._y);
        }

        public static Vector operator +(Vector vector1, Vector vector2)
        {
            return new Vector(vector1._x + vector2._x,
                              vector1._y + vector2._y);
        }

        public static Vector Add(Vector vector1, Vector vector2)
        {
            return new Vector(vector1._x + vector2._x,
                              vector1._y + vector2._y);
        }

        public static Vector operator -(Vector vector1, Vector vector2)
        {
            return new Vector(vector1._x - vector2._x,
                              vector1._y - vector2._y);
        }

        public static Vector Subtract(Vector vector1, Vector vector2)
        {
            return new Vector(vector1._x - vector2._x,
                              vector1._y - vector2._y);
        }

        public static PointF operator +(Vector vector, Point point)
        {
            return new PointF(Convert.ToSingle(point.X + vector.X), Convert.ToSingle(point.Y + vector.Y));
        }

        public static PointF Add(Vector vector, Point point)
        {
            return new PointF(Convert.ToSingle(point.X + vector.X), Convert.ToSingle(point.Y + vector.Y));
        }

        public static Vector operator *(Vector vector, double scalar)
        {
            return new Vector(vector._x * scalar,
                              vector._y * scalar);
        }

        public static Vector Multiply(Vector vector, double scalar)
        {
            return new Vector(vector._x * scalar,
                              vector._y * scalar);
        }

        public static Vector operator *(double scalar, Vector vector)
        {
            return new Vector(vector._x * scalar,
                              vector._y * scalar);
        }

        public static Vector Multiply(double scalar, Vector vector)
        {
            return new Vector(vector._x * scalar,
                              vector._y * scalar);
        }

        public static Vector operator /(Vector vector, double scalar)
        {
            return vector * (1.0 / scalar);
        }

        public static Vector Divide(Vector vector, double scalar)
        {
            return vector * (1.0 / scalar);
        }

        /*
        public static Vector operator *(Vector vector, Matrix matrix)
        {
            return matrix.Transform(vector);
        }

        public static Vector Multiply(Vector vector, Matrix matrix)
        {
            return matrix.Transform(vector);
        }
        */
        public static double operator *(Vector vector1, Vector vector2)
        {
            return vector1._x * vector2._x + vector1._y * vector2._y;
        }

        public static double Multiply(Vector vector1, Vector vector2)
        {
            return vector1._x * vector2._x + vector1._y * vector2._y;
        }

        public static double Determinant(Vector vector1, Vector vector2)
        {
            return vector1._x * vector2._y - vector1._y * vector2._x;
        }
        /*
        public static explicit operator Size(Vector vector)
        {
            return new Size(Math.Abs(vector._x), Math.Abs(vector._y));
        }

        public static explicit operator Point(Vector vector)
        {
            return new Point(vector._x, vector._y);
        }
        */
        #endregion
        #endregion
    }
    #endregion
}
