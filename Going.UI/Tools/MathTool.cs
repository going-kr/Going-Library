using Going.UI.Enums;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Tools
{
    public class MathTool
    {
        #region Map
        public static long Map(long val, long min, long max, long convert_min, long convert_max)
        {
            long ret = 0;
            if ((max - min) != 0) ret = (val - min) * (convert_max - convert_min) / (max - min) + convert_min;
            return ret;
        }

        public static double Map(double val, double min, double max, double convert_min, double convert_max)
        {
            double ret = 0;
            if ((max - min) != 0) ret = (val - min) * (convert_max - convert_min) / (max - min) + convert_min;
            return ret;
        }
        #endregion

        #region Constrain
        public static byte Constrain(byte val, byte min, byte max)
        {
            byte ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static short Constrain(short val, short min, short max)
        {
            short ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static int Constrain(int val, int min, int max)
        {
            int ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static long Constrain(long val, long min, long max)
        {
            long ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static sbyte Constrain(sbyte val, sbyte min, sbyte max)
        {
            sbyte ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static ushort Constrain(ushort val, ushort min, ushort max)
        {
            ushort ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static uint Constrain(uint val, uint min, uint max)
        {
            uint ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static ulong Constrain(ulong val, ulong min, ulong max)
        {
            ulong ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }


        /// <summary>
        /// 제한값 구하기
        /// </summary>
        /// <param name="val">현재값</param>
        /// <param name="min">최소값</param>
        /// <param name="max">최대값</param>
        /// <returns>제한값</returns>
        public static double Constrain(double val, double min, double max)
        {
            double ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static float Constrain(float val, float min, float max)
        {
            float ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        public static decimal Constrain(decimal val, decimal min, decimal max)
        {
            decimal ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }
        #endregion

        #region GetAngle
        public static double GetAngle(SKPoint from, SKPoint to)
        {
            return Math.Atan2(to.Y - from.Y, to.X - from.X) * 180.0 / Math.PI;
        }
        #endregion

        #region GetDistance
        public static double GetDistance(SKPoint a, SKPoint b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static double GetDistance(SKPoint LN1, SKPoint LN2, SKPoint pt)
        {
            double a, b, c, d;
            double rM;
            double C;
            a = Math.Sqrt(Math.Pow(LN1.X - LN2.X, 2) + Math.Pow(LN1.Y - LN2.Y, 2));
            b = Math.Sqrt(Math.Pow(LN2.X - pt.X, 2) + Math.Pow(LN2.Y - pt.Y, 2));
            c = Math.Sqrt(Math.Pow(LN1.X - pt.X, 2) + Math.Pow(LN1.Y - pt.Y, 2));

            if (Math.Pow(a, 2) + Math.Pow(b, 2) <= Math.Pow(c, 2))
                return b;
            else if (Math.Pow(a, 2) + Math.Pow(c, 2) <= Math.Pow(b, 2))
                return c;

            if (LN1.X == LN2.X)
            {
                d = pt.X - LN1.X;
                return d;
            }
            if (LN1.Y == LN2.Y)
            {
                d = pt.Y - LN1.Y;
                return d;
            }

            rM = (double)(LN1.Y - LN2.Y) / (double)(LN1.X - LN2.X);
            C = LN1.Y - rM * LN1.X;
            d = Math.Abs((double)(rM * (double)pt.X - (double)pt.Y + C)) / Math.Sqrt(Math.Pow(rM, 2) + 1);
            return d;
        }
        
        public static double GetDistance(SKPoint3 p1, SKPoint3 p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;

            return Math.Sqrt(dx * dx + dy * dy * dz * dz);
        }

        public static double GetDistance(double[] first, double[] second)
        {
            var sum = first.Select((x, i) => (x - second[i]) * (x - second[i])).Sum();
            return Math.Sqrt(sum);
        }
        #endregion

        #region RotatePoint
        public static SKPoint RotatePoint(SKPoint Center, SKPoint Target, float angle)
        {
            double angleInRadians = angle * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new SKPoint
            {
                X = Convert.ToSingle((cosTheta * (Target.X - Center.X) - sinTheta * (Target.Y - Center.Y) + Center.X)),
                Y = Convert.ToSingle((sinTheta * (Target.X - Center.X) + cosTheta * (Target.Y - Center.Y) + Center.Y))
            };
        }
        #endregion

        #region Center
        public static int Center(int p1, int p2) => p1 + ((p2 - p1) / 2);
        public static float Center(float p1, float p2) => p1 + ((p2 - p1) / 2F);

        public static int CenterDist(int x, int dist) => x + (dist / 2);
        public static float CenterDist(float x, float dist) => x + (dist / 2F);
        #endregion

        #region CenterPoint
        #region CenterPoint ( Rectangle )
        public static SKPoint CenterPoint(SKRect rt)
        {
            return new SKPoint(rt.Left + rt.Width / 2F, rt.Top + rt.Height / 2F);
        }
        #endregion
        #region CenterPoint ( p1, p2 )
        public static SKPoint CenterPoint(SKPoint p1, SKPoint p2) => new SKPoint((p1.X + p2.X) / 2F, (p1.Y + p2.Y) / 2F);
        #endregion
        #region CenterPoint ( List<Point> )
        public static SKPoint CenterPoint(List<SKPoint> sourceList)
        {
            float centerX = 0F;
            float centerY = 0F;
            float polygonArea = 0F;

            int firstIndex;
            int secondIndex;
            int sourceCount = sourceList.Count;

            SKPoint firstPoint;
            SKPoint secondPoint;

            float factor = 0F;

            for (firstIndex = 0; firstIndex < sourceCount; firstIndex++)
            {
                secondIndex = (firstIndex + 1) % sourceCount;

                firstPoint = sourceList[firstIndex];
                secondPoint = sourceList[secondIndex];

                factor = ((firstPoint.X * secondPoint.Y) - (secondPoint.X * firstPoint.Y));

                polygonArea += factor;
                centerX += (firstPoint.X + secondPoint.X) * factor;
                centerY += (firstPoint.Y + secondPoint.Y) * factor;
            }

            polygonArea /= 2F;
            polygonArea *= 6F;

            factor = 1F / polygonArea;

            centerX *= factor;
            centerY *= factor;

            return new SKPoint(centerX, centerY);
        }
        #endregion
        #endregion

        #region MakeRectangle
        public static SKRect MakeRectangle(SKRect rect, SKSize size)
            => Util.FromRect(rect.Left + (rect.Width / 2F) - (size.Width / 2F), rect.Top + (rect.Height / 2F) - (size.Height / 2F), size.Width, size.Height);
      
        public static SKRect MakeRectangle(SKRect rect, SKSize size, GoContentAlignment align)
        {
            float y = rect.Top;
            if (align == GoContentAlignment.MiddleLeft || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.MiddleRight) y = rect.MidY - size.Height / 2F;
            else if (align == GoContentAlignment.BottomLeft || align == GoContentAlignment.BottomCenter || align == GoContentAlignment.BottomRight) y = rect.Bottom - size.Height;

            float x = rect.Left;
            if (align == GoContentAlignment.TopCenter || align == GoContentAlignment.MiddleCenter || align == GoContentAlignment.BottomCenter) x = rect.MidX - size.Width / 2;
            else if (align == GoContentAlignment.TopRight || align == GoContentAlignment.MiddleRight || align == GoContentAlignment.BottomRight) x = rect.Right - size.Width;

            return Util.FromRect(x, y, size.Width, size.Height);
        }

        public static SKRect MakeRectangle(SKPoint pt1, SKPoint pt2)
        {
            var minx = Math.Min(pt1.X, pt2.X);
            var miny = Math.Min(pt1.Y, pt2.Y);
            var maxx = Math.Max(pt1.X, pt2.X);
            var maxy = Math.Max(pt1.Y, pt2.Y);

            var rx = maxx - minx - 0; if (rx < 0F) rx = 0F;
            var ry = maxy - miny - 0; if (ry < 0F) ry = 0F;

            return Util.FromRect(minx, miny, rx, ry);
        }
        
        public static SKRect MakeRectangle(IEnumerable<SKPoint> pts)
        {
            if (pts.Count() >= 2)
            {
                float minx = pts.Min(x => x.X);
                float miny = pts.Min(x => x.Y);
                float maxx = pts.Max(x => x.X);
                float maxy = pts.Max(x => x.Y);

                float rx = maxx - minx - 0; if (rx < 0F) rx = 0F;
                float ry = maxy - miny - 0; if (ry < 0F) ry = 0F;

                return Util.FromRect(minx, miny, rx, ry);
            }
            else throw new Exception("POINTS 개수가 2개 미만입니다.");
        }

        public static SKRect MakeRectangle(SKPoint pt, float Size) { return Util.FromRect(pt.X - (Size / 2F), pt.Y - (Size / 2F), Size, Size); }
        public static SKRect MakeRectangle(SKPoint pt, float rWIdth, float rHeight) { return Util.FromRect(pt.X - rWIdth, pt.Y - rHeight, rWIdth * 2F, rHeight * 2F); }
        public static SKRect MakeRectangle(float X, float Y, float Size) { return Util.FromRect(X - (Size / 2F), Y - (Size / 2F), Size, Size); }
        public static SKRect MakeRectangle(SKPoint pt, SKSize sz) => Util.FromRect(pt.X - (sz.Width / 2F), pt.Y - (sz.Height / 2F), sz.Width, sz.Height);
        #endregion

        #region GetPoints
        public static SKPoint[] GetPoints(SKRect rt) => [new SKPoint(rt.Left, rt.Top), new SKPoint(rt.Right, rt.Top), new SKPoint(rt.Right, rt.Bottom), new SKPoint(rt.Left, rt.Bottom)];
        #endregion

        #region GetPoint
        public static SKPoint GetPointWithAngle(SKPoint p, float angle, float dist)
        {
            float x = GetX_WithAngle(p, angle, dist);
            float y = GetY_WithAngle(p, angle, dist);
            return new SKPoint(x, y);
        }

        public static SKPoint GetPointWithAngle(SKRect rt, double angle)
        {
            var vangle = Math.PI * angle / 180.0;

            var cp = MathTool.CenterPoint(rt);
            var pX = Convert.ToSingle(Math.Cos(vangle) * rt.Width / 2F);
            var pY = Convert.ToSingle(Math.Sin(vangle) * rt.Height / 2F);

            return new SKPoint(cp.X + pX, cp.Y + pY);
        }

        public static float GetX_WithAngle(SKPoint p, float angle, float dist) { return p.X + dist * Convert.ToSingle(Math.Cos(angle * Math.PI / 180.0)); }
        public static float GetY_WithAngle(SKPoint p, float angle, float dist) { return p.Y + dist * Convert.ToSingle(Math.Sin(angle * Math.PI / 180.0)); }
        #endregion

        #region LinearEquation
        public static float LinearEquationY(SKPoint p1, SKPoint p2, float x)
        {
            float x1 = p1.X, x2 = p2.X, y1 = p1.Y, y2 = p2.Y;
            float a = (y2 - y1) / (x2 - x1);
            float b = y1 - a * x1;
            float y = a * x + b;

            return y;
        }

        public static float LinearEquationX(SKPoint p1, SKPoint p2, float y)
        {
            float x1 = p1.X, x2 = p2.X, y1 = p1.Y, y2 = p2.Y;
            float a = (y2 - y1) / (x2 - x1);
            float b = y1 - a * x1;
            float x = (y - b) / a;

            return x;
        }
        #endregion

        #region StandardAngle
        public static int StandardAngle(int angle)
        {
            int ret = angle;
            if (ret > 360) ret -= 360;
            if (ret < 0) ret += 360;
            return ret;
        }

        public static float StandardAngle(float angle)
        {
            float ret = angle;
            if (ret > 360) ret -= 360;
            if (ret < 0) ret += 360;
            return ret;
        }

        public static double StandardAngle(double angle)
        {
            double ret = angle;
            if (ret > 360) ret -= 360;
            if (ret < 0) ret += 360;
            return ret;
        }
        #endregion

        #region CompareAngle
        public static bool CompareAngle(double Angle, double StartAngle, double EndAngle)
        {
            bool ret = false;

            var ang = MathTool.StandardAngle(Angle);
            var stang = MathTool.StandardAngle(StartAngle);
            var edang = MathTool.StandardAngle(EndAngle);
            if (stang > edang)
            {
                double s1 = stang - 360, e1 = edang;
                double s2 = stang, e2 = edang + 360;

                ret = (s1 <= ang && ang <= e1) || (s2 <= ang && ang <= e2);
            }
            else
            {
                ret = stang <= ang && ang <= edang;
            }
            return ret;
        }
        #endregion
    }
}
