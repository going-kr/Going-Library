using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Tools
{
    /// <summary>
    /// 수학 유틸리티 클래스.
    /// 값 매핑, 범위 제한, 각도/거리 계산, 좌표 변환, 사각형 생성, 선형 방정식 등의 기능을 제공한다.
    /// </summary>
    public class MathTool
    {
        #region Map
        /// <summary>값을 한 범위에서 다른 범위로 선형 매핑한다.</summary>
        /// <param name="val">변환할 값</param>
        /// <param name="min">원본 범위 최솟값</param>
        /// <param name="max">원본 범위 최댓값</param>
        /// <param name="convert_min">대상 범위 최솟값</param>
        /// <param name="convert_max">대상 범위 최댓값</param>
        /// <returns>대상 범위로 매핑된 값</returns>
        public static long Map(long val, long min, long max, long convert_min, long convert_max)
        {
            long ret = 0;
            if ((max - min) != 0) ret = (val - min) * (convert_max - convert_min) / (max - min) + convert_min;
            return ret;
        }

        /// <summary>값을 한 범위에서 다른 범위로 선형 매핑한다. (double 오버로드)</summary>
        /// <param name="val">변환할 값</param>
        /// <param name="min">원본 범위 최솟값</param>
        /// <param name="max">원본 범위 최댓값</param>
        /// <param name="convert_min">대상 범위 최솟값</param>
        /// <param name="convert_max">대상 범위 최댓값</param>
        /// <returns>대상 범위로 매핑된 값</returns>
        public static double Map(double val, double min, double max, double convert_min, double convert_max)
        {
            double ret = 0;
            if ((max - min) != 0) ret = (val - min) * (convert_max - convert_min) / (max - min) + convert_min;
            return ret;
        }
        #endregion

        #region Constrain
        /// <summary>값을 지정 범위 내로 제한한다. min &gt; max이면 자동으로 스왑한다.</summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static byte Constrain(byte val, byte min, byte max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static short Constrain(short val, short min, short max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static int Constrain(int val, int min, int max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static long Constrain(long val, long min, long max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static sbyte Constrain(sbyte val, sbyte min, sbyte max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static ushort Constrain(ushort val, ushort min, ushort max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static uint Constrain(uint val, uint min, uint max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static ulong Constrain(ulong val, ulong min, ulong max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static double Constrain(double val, double min, double max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static float Constrain(float val, float min, float max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        /// <inheritdoc cref="Constrain(byte, byte, byte)"/>
        public static decimal Constrain(decimal val, decimal min, decimal max)
        {
            if (min > max) (min, max) = (max, min);
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }
        #endregion

        #region GetAngle
        /// <summary>두 점 사이의 각도를 계산한다 (도 단위).</summary>
        /// <param name="from">시작점</param>
        /// <param name="to">끝점</param>
        /// <returns>각도 (도 단위, -180 ~ 180)</returns>
        public static double GetAngle(Point from, Point to)
        {
            return Math.Atan2(to.Y - from.Y, to.X - from.X) * 180.0 / Math.PI;
        }


        /// <inheritdoc cref="GetAngle(Point, Point)"/>
        public static double GetAngle(PointF from, PointF to)
        {
            return Math.Atan2(to.Y - from.Y, to.X - from.X) * 180.0 / Math.PI;
        }
        #endregion

        #region GetDistance
        /// <summary>두 점 사이의 유클리드 거리를 계산한다.</summary>
        /// <param name="a">첫 번째 점</param>
        /// <param name="b">두 번째 점</param>
        /// <returns>두 점 사이의 거리</returns>
        public static double GetDistance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <inheritdoc cref="GetDistance(Point, Point)"/>
        public static double GetDistance(PointF a, PointF b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>선분과 점 사이의 최단 거리를 계산한다.</summary>
        /// <param name="LN1">선분의 시작점</param>
        /// <param name="LN2">선분의 끝점</param>
        /// <param name="pt">대상 점</param>
        /// <returns>선분과 점 사이의 최단 거리</returns>
        public static double GetDistance(PointF LN1, PointF LN2, PointF pt)
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

        /// <summary>3차원 공간에서 두 점 사이의 유클리드 거리를 계산한다.</summary>
        /// <param name="x1">첫 번째 점의 X 좌표</param>
        /// <param name="y1">첫 번째 점의 Y 좌표</param>
        /// <param name="z1">첫 번째 점의 Z 좌표</param>
        /// <param name="x2">두 번째 점의 X 좌표</param>
        /// <param name="y2">두 번째 점의 Y 좌표</param>
        /// <param name="z2">두 번째 점의 Z 좌표</param>
        /// <returns>두 점 사이의 거리</returns>
        public static double GetDistance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            double dz = z2 - z1;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>N차원 공간에서 두 점 사이의 유클리드 거리를 계산한다.</summary>
        /// <param name="first">첫 번째 점의 좌표 배열</param>
        /// <param name="second">두 번째 점의 좌표 배열 (first와 같은 길이여야 한다)</param>
        /// <returns>두 점 사이의 거리</returns>
        public static double GetDistance(double[] first, double[] second)
        {
            var sum = first.Select((x, i) => (x - second[i]) * (x - second[i])).Sum();
            return Math.Sqrt(sum);
        }
        #endregion

        #region RotatePoint
        /// <summary>중심점을 기준으로 대상 점을 지정 각도만큼 회전시킨다.</summary>
        /// <param name="Center">회전 중심점</param>
        /// <param name="Target">회전시킬 대상 점</param>
        /// <param name="angle">회전 각도 (도 단위)</param>
        /// <returns>회전된 점의 좌표</returns>
        public static PointF RotatePoint(PointF Center, PointF Target, float angle)
        {
            double angleInRadians = angle * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new PointF
            {
                X = Convert.ToSingle((cosTheta * (Target.X - Center.X) - sinTheta * (Target.Y - Center.Y) + Center.X)),
                Y = Convert.ToSingle((sinTheta * (Target.X - Center.X) + cosTheta * (Target.Y - Center.Y) + Center.Y))
            };
        }
        #endregion

        #region Center
        /// <summary>두 좌표의 중간값을 계산한다.</summary>
        /// <param name="p1">첫 번째 좌표</param>
        /// <param name="p2">두 번째 좌표</param>
        /// <returns>중간 좌표</returns>
        public static int Center(int p1, int p2) => p1 + ((p2 - p1) / 2);
        /// <inheritdoc cref="Center(int, int)"/>
        public static float Center(float p1, float p2) => p1 + ((p2 - p1) / 2F);
        /// <summary>시작 좌표에서 거리의 절반을 더한 중심 좌표를 반환한다.</summary>
        /// <param name="x">시작 좌표</param>
        /// <param name="dist">전체 거리</param>
        /// <returns>중심 좌표</returns>
        public static int CenterDist(int x, int dist) => x + (dist / 2);
        /// <inheritdoc cref="CenterDist(int, int)"/>
        public static float CenterDist(float x, float dist) => x + (dist / 2F);
        #endregion

        #region CenterPoint
        #region CenterPoint ( Rectangle )
        /// <summary>사각형의 중심점을 반환한다.</summary>
        /// <param name="rt">대상 사각형</param>
        /// <returns>중심점 좌표</returns>
        public static Point CenterPoint(Rectangle rt)
        {
            return new Point(rt.X + rt.Width / 2, rt.Y + rt.Height / 2);
        }

        /// <inheritdoc cref="CenterPoint(Rectangle)"/>
        public static PointF CenterPoint(RectangleF rt)
        {
            return new PointF(rt.X + rt.Width / 2F, rt.Y + rt.Height / 2F);
        }
        #endregion
        #region CenterPoint ( p1, p2 )
        /// <summary>두 점의 중심점을 반환한다.</summary>
        /// <param name="p1">첫 번째 점</param>
        /// <param name="p2">두 번째 점</param>
        /// <returns>중심점 좌표</returns>
        public static Point CenterPoint(Point p1, Point p2) => new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        /// <inheritdoc cref="CenterPoint(Point, Point)"/>
        public static PointF CenterPoint(PointF p1, PointF p2) => new PointF((p1.X + p2.X) / 2F, (p1.Y + p2.Y) / 2F);
        #endregion
        #region CenterPoint ( List<Point> )
        /// <summary>다각형의 무게중심(centroid)을 계산한다.</summary>
        /// <param name="sourceList">다각형 꼭짓점 목록</param>
        /// <returns>무게중심 좌표</returns>
        public static PointF CenterPoint(List<PointF> sourceList)
        {
            float centerX = 0F;
            float centerY = 0F;
            float polygonArea = 0F;

            int firstIndex;
            int secondIndex;
            int sourceCount = sourceList.Count;

            PointF firstPoint;
            PointF secondPoint;

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

            return new PointF(centerX, centerY);
        }
        #endregion
        #endregion

        #region MakeRectangle
        #region MakeRectangle ( Rectangle Center )
        /// <summary>사각형의 중심에 지정 크기의 새 사각형을 생성한다.</summary>
        /// <param name="rect">기준 사각형</param>
        /// <param name="size">새 사각형 크기</param>
        /// <returns>중심 정렬된 새 사각형</returns>
        public static Rectangle MakeRectangle(Rectangle rect, Size size)
        {
            return new Rectangle(rect.X + (rect.Width / 2) - (size.Width / 2), rect.Y + (rect.Height / 2) - (size.Height / 2), size.Width, size.Height);
        }

        /// <inheritdoc cref="MakeRectangle(Rectangle, Size)"/>
        public static RectangleF MakeRectangle(RectangleF rect, SizeF size)
        {
            return new RectangleF(rect.X + (rect.Width / 2F) - (size.Width / 2F), rect.Y + (rect.Height / 2F) - (size.Height / 2F), size.Width, size.Height);
        }
        #endregion
        #region MakeRectangle ( Two Point )
        /// <summary>두 점을 대각선 꼭짓점으로 하는 사각형을 생성한다.</summary>
        /// <param name="pt1">첫 번째 꼭짓점</param>
        /// <param name="pt2">두 번째 꼭짓점</param>
        /// <returns>두 점을 포함하는 사각형</returns>
        public static Rectangle MakeRectangle(Point pt1, Point pt2)
        {
            int minx = Math.Min(pt1.X, pt2.X);
            int miny = Math.Min(pt1.Y, pt2.Y);
            int maxx = Math.Max(pt1.X, pt2.X);
            int maxy = Math.Max(pt1.Y, pt2.Y);

            int rx = maxx - minx - 0; if (rx < 0) rx = 0;
            int ry = maxy - miny - 0; if (ry < 0) ry = 0;

            return new Rectangle(minx, miny, rx, ry);
        }

        /// <inheritdoc cref="MakeRectangle(Point, Point)"/>
        public static RectangleF MakeRectangle(PointF pt1, PointF pt2)
        {
            var minx = Math.Min(pt1.X, pt2.X);
            var miny = Math.Min(pt1.Y, pt2.Y);
            var maxx = Math.Max(pt1.X, pt2.X);
            var maxy = Math.Max(pt1.Y, pt2.Y);

            var rx = maxx - minx - 0; if (rx < 0F) rx = 0F;
            var ry = maxy - miny - 0; if (ry < 0F) ry = 0F;

            return new RectangleF(minx, miny, rx, ry);
        }
        #endregion
        #region MakeRectangle ( Points )
        /// <summary>점 집합을 모두 포함하는 최소 바운딩 사각형을 생성한다.</summary>
        /// <param name="pts">점 집합 (2개 이상 필요)</param>
        /// <returns>모든 점을 포함하는 사각형</returns>
        public static RectangleF MakeRectangle(IEnumerable<Point> pts)
        {
            if (pts.Count() >= 2)
            {
                float minx = pts.Min(x => x.X);
                float miny = pts.Min(x => x.Y);
                float maxx = pts.Max(x => x.X);
                float maxy = pts.Max(x => x.Y);

                float rx = maxx - minx - 0; if (rx < 0F) rx = 0F;
                float ry = maxy - miny - 0; if (ry < 0F) ry = 0F;

                return new RectangleF(minx, miny, rx, ry);
            }
            else throw new Exception("POINTS 개수가 2개 미만입니다.");
        }
        #endregion
        #region MakeRectangle ( Center Point )
        /// <summary>중심점과 크기로 정사각형을 생성한다.</summary>
        /// <param name="pt">중심점</param>
        /// <param name="Size">한 변의 길이</param>
        /// <returns>중심 정렬된 정사각형</returns>
        public static Rectangle MakeRectangle(Point pt, int Size) { return new Rectangle(pt.X - (Size / 2), pt.Y - (Size / 2), Size, Size); }
        /// <inheritdoc cref="MakeRectangle(Point, int)"/>
        public static RectangleF MakeRectangle(PointF pt, float Size) { return new RectangleF(pt.X - (Size / 2F), pt.Y - (Size / 2F), Size, Size); }
        /// <summary>중심점과 반너비/반높이로 사각형을 생성한다.</summary>
        /// <param name="pt">중심점</param>
        /// <param name="rWIdth">반너비 (전체 너비의 절반)</param>
        /// <param name="rHeight">반높이 (전체 높이의 절반)</param>
        /// <returns>중심 정렬된 사각형</returns>
        public static Rectangle MakeRectangle(Point pt, int rWIdth, int rHeight) { return new Rectangle(pt.X - rWIdth, pt.Y - rHeight, rWIdth * 2, rHeight * 2); }
        /// <inheritdoc cref="MakeRectangle(Point, int, int)"/>
        public static RectangleF MakeRectangle(PointF pt, float rWIdth, float rHeight) { return new RectangleF(pt.X - rWIdth, pt.Y - rHeight, rWIdth * 2F, rHeight * 2F); }
        /// <summary>좌표와 크기로 정사각형을 생성한다.</summary>
        /// <param name="X">중심 X 좌표</param>
        /// <param name="Y">중심 Y 좌표</param>
        /// <param name="Size">한 변의 길이</param>
        /// <returns>중심 정렬된 정사각형</returns>
        public static Rectangle MakeRectangle(int X, int Y, int Size) { return new Rectangle(X - (Size / 2), Y - (Size / 2), Size, Size); }
        /// <inheritdoc cref="MakeRectangle(int, int, int)"/>
        public static RectangleF MakeRectangle(float X, float Y, float Size) { return new RectangleF(X - (Size / 2F), Y - (Size / 2F), Size, Size); }
        #endregion
        #endregion

        #region GetPoints
        /// <summary>사각형의 네 꼭짓점을 배열로 반환한다 (좌상, 우상, 우하, 좌하 순).</summary>
        /// <param name="rt">대상 사각형</param>
        /// <returns>네 꼭짓점 배열</returns>
        public static PointF[] GetPoints(RectangleF rt)
        {
            return new PointF[] { new PointF(rt.Left, rt.Top), new PointF(rt.Right, rt.Top), new PointF(rt.Right, rt.Bottom), new PointF(rt.Left, rt.Bottom) };
        }
        #endregion

        #region GetPoint
        /// <summary>기준점에서 지정 각도와 거리만큼 떨어진 점의 좌표를 계산한다.</summary>
        /// <param name="p">기준점</param>
        /// <param name="angle">각도 (도 단위)</param>
        /// <param name="dist">거리</param>
        /// <returns>계산된 점의 좌표</returns>
        public static PointF GetPointWithAngle(PointF p, float angle, float dist)
        {
            float x = GetX_WithAngle(p, angle, dist);
            float y = GetY_WithAngle(p, angle, dist);
            return new PointF(x, y);
        }

        /// <inheritdoc cref="GetPointWithAngle(PointF, float, float)"/>
        public static PointF GetPointWithAngle(Point p, float angle, float dist)
        {
            float x = GetX_WithAngle(p, angle, dist);
            float y = GetY_WithAngle(p, angle, dist);
            return new PointF(x, y);
        }

        /// <summary>타원(사각형 내접) 위에서 지정 각도에 해당하는 점의 좌표를 계산한다.</summary>
        /// <param name="rt">타원이 내접하는 사각형</param>
        /// <param name="angle">각도 (도 단위)</param>
        /// <returns>타원 위의 점 좌표</returns>
        public static PointF GetPointWithAngle(RectangleF rt, double angle)
        {
            var vangle = Math.PI * angle / 180.0;

            var cp = MathTool.CenterPoint(rt);
            var pX = Convert.ToSingle(Math.Cos(vangle) * rt.Width / 2F);
            var pY = Convert.ToSingle(Math.Sin(vangle) * rt.Height / 2F);

            return new PointF(cp.X + pX, cp.Y + pY);
        }

        /// <summary>기준점에서 지정 각도와 거리만큼 이동한 X 좌표를 계산한다.</summary>
        public static float GetX_WithAngle(PointF p, float angle, float dist) { return p.X + dist * Convert.ToSingle(Math.Cos(angle * Math.PI / 180.0)); }

        /// <summary>기준점에서 지정 각도와 거리만큼 이동한 Y 좌표를 계산한다.</summary>
        public static float GetY_WithAngle(PointF p, float angle, float dist) { return p.Y + dist * Convert.ToSingle(Math.Sin(angle * Math.PI / 180.0)); }

        /// <inheritdoc cref="GetX_WithAngle(PointF, float, float)"/>
        public static float GetX_WithAngle(Point p, float angle, float dist) { return p.X + dist * Convert.ToSingle(Math.Cos(angle * Math.PI / 180.0)); }
        /// <inheritdoc cref="GetY_WithAngle(PointF, float, float)"/>
        public static float GetY_WithAngle(Point p, float angle, float dist) { return p.Y + dist * Convert.ToSingle(Math.Sin(angle * Math.PI / 180.0)); }
        #endregion

        #region LinearEquation
        /// <summary>두 점을 지나는 직선에서 주어진 X에 대한 Y 값을 계산한다.</summary>
        /// <param name="p1">직선 위의 첫 번째 점</param>
        /// <param name="p2">직선 위의 두 번째 점</param>
        /// <param name="x">X 좌표</param>
        /// <returns>해당 X에서의 Y 값</returns>
        public static float LinearEquationY(PointF p1, PointF p2, float x)
        {
            float x1 = p1.X, x2 = p2.X, y1 = p1.Y, y2 = p2.Y;
            float a = (y2 - y1) / (x2 - x1);
            float b = y1 - a * x1;
            float y = a * x + b;

            return y;
        }

        /// <summary>두 점을 지나는 직선에서 주어진 Y에 대한 X 값을 계산한다.</summary>
        /// <param name="p1">직선 위의 첫 번째 점</param>
        /// <param name="p2">직선 위의 두 번째 점</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>해당 Y에서의 X 값</returns>
        public static float LinearEquationX(PointF p1, PointF p2, float y)
        {
            float x1 = p1.X, x2 = p2.X, y1 = p1.Y, y2 = p2.Y;
            float a = (y2 - y1) / (x2 - x1);
            float b = y1 - a * x1;
            float x = (y - b) / a;

            return x;
        }
        #endregion

        #region StandardAngle
        /// <summary>각도를 0~360 범위로 정규화한다.</summary>
        /// <param name="angle">정규화할 각도</param>
        /// <returns>0~360 범위의 각도</returns>
        public static int StandardAngle(int angle)
        {
            int ret = angle;

            var v = Convert.ToInt32(Math.Floor(Math.Abs(ret) / 360.0));

            if (ret > 360) ret -= 360 * v;
            if (ret < 0) ret += 360 * v;
            return ret;
        }

        /// <inheritdoc cref="StandardAngle(int)"/>
        public static float StandardAngle(float angle)
        {
            float ret = angle;

            var v = Convert.ToInt32(Math.Floor(Math.Abs(ret) / 360.0));

            if (ret > 360) ret -= 360 * v;
            if (ret < 0) ret += 360 * v;
            return ret;
        }

        /// <inheritdoc cref="StandardAngle(int)"/>
        public static double StandardAngle(double angle)
        {
            double ret = angle;

            var v = Convert.ToInt32(Math.Floor(Math.Abs(ret) / 360.0));

            if (ret > 360) ret -= 360 * v;
            if (ret < 0) ret += 360 * v;
            return ret;
        }
        #endregion

        #region CompareAngle
        /// <summary>각도가 시작 각도와 끝 각도 사이에 있는지 확인한다. 0도를 넘는 범위도 처리한다.</summary>
        /// <param name="Angle">확인할 각도</param>
        /// <param name="StartAngle">시작 각도</param>
        /// <param name="EndAngle">끝 각도</param>
        /// <returns>범위 내에 있으면 true</returns>
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
