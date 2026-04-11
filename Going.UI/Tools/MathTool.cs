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
    /// <summary>
    /// 수학 연산 유틸리티 클래스입니다. 값 매핑, 제한, 각도, 거리, 좌표 계산 등을 지원합니다.
    /// </summary>
    public class MathTool
    {
        #region Map
        /// <summary>
        /// 값을 한 범위에서 다른 범위로 매핑합니다.
        /// </summary>
        /// <param name="val">변환할 값</param>
        /// <param name="min">원본 범위의 최솟값</param>
        /// <param name="max">원본 범위의 최댓값</param>
        /// <param name="convert_min">변환 범위의 최솟값</param>
        /// <param name="convert_max">변환 범위의 최댓값</param>
        /// <returns>변환된 값</returns>
        public static long Map(long val, long min, long max, long convert_min, long convert_max)
        {
            long ret = 0;
            if ((max - min) != 0) ret = (val - min) * (convert_max - convert_min) / (max - min) + convert_min;
            return ret;
        }

        /// <summary>
        /// 값을 한 범위에서 다른 범위로 매핑합니다.
        /// </summary>
        /// <param name="val">변환할 값</param>
        /// <param name="min">원본 범위의 최솟값</param>
        /// <param name="max">원본 범위의 최댓값</param>
        /// <param name="convert_min">변환 범위의 최솟값</param>
        /// <param name="convert_max">변환 범위의 최댓값</param>
        /// <returns>변환된 값</returns>
        public static double Map(double val, double min, double max, double convert_min, double convert_max)
        {
            double ret = 0;
            if ((max - min) != 0) ret = (val - min) * (convert_max - convert_min) / (max - min) + convert_min;
            return ret;
        }
        #endregion

        #region Constrain
        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static byte Constrain(byte val, byte min, byte max)
        {
            byte ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static short Constrain(short val, short min, short max)
        {
            short ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static int Constrain(int val, int min, int max)
        {
            int ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static long Constrain(long val, long min, long max)
        {
            long ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static sbyte Constrain(sbyte val, sbyte min, sbyte max)
        {
            sbyte ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static ushort Constrain(ushort val, ushort min, ushort max)
        {
            ushort ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static uint Constrain(uint val, uint min, uint max)
        {
            uint ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static ulong Constrain(ulong val, ulong min, ulong max)
        {
            ulong ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }


        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static double Constrain(double val, double min, double max)
        {
            double ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
        public static float Constrain(float val, float min, float max)
        {
            float ret = val;
            if (ret < min) ret = min;
            if (ret > max) ret = max;
            if (min > max) ret = min;
            return ret;
        }

        /// <summary>
        /// 값을 최솟값과 최댓값 범위 내로 제한합니다.
        /// </summary>
        /// <param name="val">제한할 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        /// <returns>범위 내로 제한된 값</returns>
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
        /// <summary>
        /// 두 점 사이의 각도를 도(degree) 단위로 계산합니다.
        /// </summary>
        /// <param name="from">시작점</param>
        /// <param name="to">끝점</param>
        /// <returns>각도 (도 단위)</returns>
        public static double GetAngle(SKPoint from, SKPoint to)
        {
            return Math.Atan2(to.Y - from.Y, to.X - from.X) * 180.0 / Math.PI;
        }
        #endregion

        #region GetDistance
        /// <summary>
        /// 두 점 사이의 유클리드 거리를 계산합니다.
        /// </summary>
        /// <param name="a">첫 번째 점</param>
        /// <param name="b">두 번째 점</param>
        /// <returns>두 점 사이의 거리</returns>
        public static double GetDistance(SKPoint a, SKPoint b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// 점에서 선분까지의 최단 거리를 계산합니다.
        /// </summary>
        /// <param name="LN1">선분의 시작점</param>
        /// <param name="LN2">선분의 끝점</param>
        /// <param name="pt">거리를 측정할 점</param>
        /// <returns>점에서 선분까지의 거리</returns>
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
        
        /// <summary>
        /// 3D 공간에서 두 점 사이의 유클리드 거리를 계산합니다.
        /// </summary>
        /// <param name="p1">첫 번째 3D 점</param>
        /// <param name="p2">두 번째 3D 점</param>
        /// <returns>두 점 사이의 거리</returns>
        public static double GetDistance(SKPoint3 p1, SKPoint3 p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// N차원 공간에서 두 점 사이의 유클리드 거리를 계산합니다.
        /// </summary>
        /// <param name="first">첫 번째 점의 좌표 배열</param>
        /// <param name="second">두 번째 점의 좌표 배열</param>
        /// <returns>두 점 사이의 거리</returns>
        public static double GetDistance(double[] first, double[] second)
        {
            var sum = first.Select((x, i) => (x - second[i]) * (x - second[i])).Sum();
            return Math.Sqrt(sum);
        }
        #endregion

        #region RotatePoint
        /// <summary>
        /// 중심점을 기준으로 대상 점을 지정된 각도만큼 회전합니다.
        /// </summary>
        /// <param name="Center">회전 중심점</param>
        /// <param name="Target">회전할 대상 점</param>
        /// <param name="angle">회전 각도 (도 단위)</param>
        /// <returns>회전된 점</returns>
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
        /// <summary>
        /// 두 값의 중간값을 계산합니다.
        /// </summary>
        /// <param name="p1">첫 번째 값</param>
        /// <param name="p2">두 번째 값</param>
        /// <returns>두 값의 중간값</returns>
        public static int Center(int p1, int p2) => p1 + ((p2 - p1) / 2);
        /// <summary>
        /// 두 값의 중간값을 계산합니다.
        /// </summary>
        /// <param name="p1">첫 번째 값</param>
        /// <param name="p2">두 번째 값</param>
        /// <returns>두 값의 중간값</returns>
        public static float Center(float p1, float p2) => p1 + ((p2 - p1) / 2F);

        /// <summary>
        /// 시작 위치에서 거리의 중간 지점을 계산합니다.
        /// </summary>
        /// <param name="x">시작 위치</param>
        /// <param name="dist">거리</param>
        /// <returns>중간 지점</returns>
        public static int CenterDist(int x, int dist) => x + (dist / 2);
        /// <summary>
        /// 시작 위치에서 거리의 중간 지점을 계산합니다.
        /// </summary>
        /// <param name="x">시작 위치</param>
        /// <param name="dist">거리</param>
        /// <returns>중간 지점</returns>
        public static float CenterDist(float x, float dist) => x + (dist / 2F);
        #endregion

        #region CenterPoint
        #region CenterPoint ( Rectangle )
        /// <summary>
        /// 사각형의 중심점을 계산합니다.
        /// </summary>
        /// <param name="rt">사각형</param>
        /// <returns>사각형의 중심점</returns>
        public static SKPoint CenterPoint(SKRect rt)
        {
            return new SKPoint(rt.Left + rt.Width / 2F, rt.Top + rt.Height / 2F);
        }
        #endregion
        #region CenterPoint ( p1, p2 )
        /// <summary>
        /// 두 점의 중심점을 계산합니다.
        /// </summary>
        /// <param name="p1">첫 번째 점</param>
        /// <param name="p2">두 번째 점</param>
        /// <returns>두 점의 중심점</returns>
        public static SKPoint CenterPoint(SKPoint p1, SKPoint p2) => new SKPoint((p1.X + p2.X) / 2F, (p1.Y + p2.Y) / 2F);
        #endregion
        #region CenterPoint ( List<Point> )
        /// <summary>
        /// 다각형의 무게중심(센트로이드)을 계산합니다.
        /// </summary>
        /// <param name="sourceList">다각형의 꼭짓점 목록</param>
        /// <returns>다각형의 무게중심</returns>
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
        /// <summary>
        /// 사각형의 중심에 지정된 크기의 사각형을 생성합니다.
        /// </summary>
        /// <param name="rect">기준 사각형</param>
        /// <param name="size">생성할 사각형의 크기</param>
        /// <returns>중심에 배치된 사각형</returns>
        public static SKRect MakeRectangle(SKRect rect, SKSize size)
            => Util.FromRect(rect.Left + (rect.Width / 2F) - (size.Width / 2F), rect.Top + (rect.Height / 2F) - (size.Height / 2F), size.Width, size.Height);

        /// <summary>
        /// 사각형 내에서 지정된 정렬 방식에 따라 사각형을 생성합니다.
        /// </summary>
        /// <param name="rect">기준 사각형</param>
        /// <param name="size">생성할 사각형의 크기</param>
        /// <param name="align">정렬 방식</param>
        /// <returns>정렬된 사각형</returns>
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

        /// <summary>
        /// 두 점을 대각선 꼭짓점으로 하는 사각형을 생성합니다.
        /// </summary>
        /// <param name="pt1">첫 번째 꼭짓점</param>
        /// <param name="pt2">두 번째 꼭짓점</param>
        /// <returns>생성된 사각형</returns>
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
        
        /// <summary>
        /// 점 컬렉션을 모두 포함하는 최소 경계 사각형을 생성합니다.
        /// </summary>
        /// <param name="pts">점 컬렉션 (최소 2개 이상)</param>
        /// <returns>최소 경계 사각형</returns>
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

        /// <summary>
        /// 점을 중심으로 정사각형을 생성합니다.
        /// </summary>
        /// <param name="pt">중심점</param>
        /// <param name="Size">한 변의 길이</param>
        /// <returns>생성된 정사각형</returns>
        public static SKRect MakeRectangle(SKPoint pt, float Size) { return Util.FromRect(pt.X - (Size / 2F), pt.Y - (Size / 2F), Size, Size); }
        /// <summary>
        /// 점을 중심으로 사각형을 생성합니다.
        /// </summary>
        /// <param name="pt">중심점</param>
        /// <param name="rWIdth">가로 반지름</param>
        /// <param name="rHeight">세로 반지름</param>
        /// <returns>생성된 사각형</returns>
        public static SKRect MakeRectangle(SKPoint pt, float rWIdth, float rHeight) { return Util.FromRect(pt.X - rWIdth, pt.Y - rHeight, rWIdth * 2F, rHeight * 2F); }
        /// <summary>
        /// 좌표를 중심으로 정사각형을 생성합니다.
        /// </summary>
        /// <param name="X">중심 X 좌표</param>
        /// <param name="Y">중심 Y 좌표</param>
        /// <param name="Size">한 변의 길이</param>
        /// <returns>생성된 정사각형</returns>
        public static SKRect MakeRectangle(float X, float Y, float Size) { return Util.FromRect(X - (Size / 2F), Y - (Size / 2F), Size, Size); }
        /// <summary>
        /// 점을 중심으로 지정 크기의 사각형을 생성합니다.
        /// </summary>
        /// <param name="pt">중심점</param>
        /// <param name="sz">사각형 크기</param>
        /// <returns>생성된 사각형</returns>
        public static SKRect MakeRectangle(SKPoint pt, SKSize sz) => Util.FromRect(pt.X - (sz.Width / 2F), pt.Y - (sz.Height / 2F), sz.Width, sz.Height);
        #endregion

        #region GetPoints
        /// <summary>
        /// 사각형의 네 꼭짓점을 배열로 반환합니다. 좌상, 우상, 우하, 좌하 순서입니다.
        /// </summary>
        /// <param name="rt">사각형</param>
        /// <returns>꼭짓점 배열</returns>
        public static SKPoint[] GetPoints(SKRect rt) => [new SKPoint(rt.Left, rt.Top), new SKPoint(rt.Right, rt.Top), new SKPoint(rt.Right, rt.Bottom), new SKPoint(rt.Left, rt.Bottom)];
        #endregion

        #region GetPoint
        /// <summary>
        /// 점에서 지정된 각도와 거리만큼 떨어진 점을 계산합니다.
        /// </summary>
        /// <param name="p">시작점</param>
        /// <param name="angle">각도 (도 단위)</param>
        /// <param name="dist">거리</param>
        /// <returns>계산된 점</returns>
        public static SKPoint GetPointWithAngle(SKPoint p, float angle, float dist)
        {
            float x = GetX_WithAngle(p, angle, dist);
            float y = GetY_WithAngle(p, angle, dist);
            return new SKPoint(x, y);
        }

        /// <summary>
        /// 사각형의 타원 경계 위에서 지정된 각도의 점을 계산합니다.
        /// </summary>
        /// <param name="rt">타원의 경계 사각형</param>
        /// <param name="angle">각도 (도 단위)</param>
        /// <returns>타원 경계 위의 점</returns>
        public static SKPoint GetPointWithAngle(SKRect rt, double angle)
        {
            var vangle = Math.PI * angle / 180.0;

            var cp = MathTool.CenterPoint(rt);
            var pX = Convert.ToSingle(Math.Cos(vangle) * rt.Width / 2F);
            var pY = Convert.ToSingle(Math.Sin(vangle) * rt.Height / 2F);

            return new SKPoint(cp.X + pX, cp.Y + pY);
        }

        /// <summary>
        /// 점에서 지정된 각도와 거리만큼 떨어진 X 좌표를 계산합니다.
        /// </summary>
        /// <param name="p">시작점</param>
        /// <param name="angle">각도 (도 단위)</param>
        /// <param name="dist">거리</param>
        /// <returns>계산된 X 좌표</returns>
        public static float GetX_WithAngle(SKPoint p, float angle, float dist) { return p.X + dist * Convert.ToSingle(Math.Cos(angle * Math.PI / 180.0)); }
        /// <summary>
        /// 점에서 지정된 각도와 거리만큼 떨어진 Y 좌표를 계산합니다.
        /// </summary>
        /// <param name="p">시작점</param>
        /// <param name="angle">각도 (도 단위)</param>
        /// <param name="dist">거리</param>
        /// <returns>계산된 Y 좌표</returns>
        public static float GetY_WithAngle(SKPoint p, float angle, float dist) { return p.Y + dist * Convert.ToSingle(Math.Sin(angle * Math.PI / 180.0)); }
        #endregion

        #region LinearEquation
        /// <summary>
        /// 두 점을 지나는 직선 위에서 주어진 X 좌표에 대한 Y 값을 계산합니다.
        /// </summary>
        /// <param name="p1">첫 번째 점</param>
        /// <param name="p2">두 번째 점</param>
        /// <param name="x">X 좌표</param>
        /// <returns>해당 X 좌표에서의 Y 값</returns>
        public static float LinearEquationY(SKPoint p1, SKPoint p2, float x)
        {
            float x1 = p1.X, x2 = p2.X, y1 = p1.Y, y2 = p2.Y;
            float a = (y2 - y1) / (x2 - x1);
            float b = y1 - a * x1;
            float y = a * x + b;

            return y;
        }

        /// <summary>
        /// 두 점을 지나는 직선 위에서 주어진 Y 좌표에 대한 X 값을 계산합니다.
        /// </summary>
        /// <param name="p1">첫 번째 점</param>
        /// <param name="p2">두 번째 점</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>해당 Y 좌표에서의 X 값</returns>
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
        /// <summary>
        /// 각도를 0~360 범위로 정규화합니다.
        /// </summary>
        /// <param name="angle">정규화할 각도</param>
        /// <returns>0~360 범위의 각도</returns>
        public static int StandardAngle(int angle)
        {
            int ret = angle;
            if (ret > 360) ret -= 360;
            if (ret < 0) ret += 360;
            return ret;
        }

        /// <summary>
        /// 각도를 0~360 범위로 정규화합니다.
        /// </summary>
        /// <param name="angle">정규화할 각도</param>
        /// <returns>0~360 범위의 각도</returns>
        public static float StandardAngle(float angle)
        {
            float ret = angle;
            if (ret > 360) ret -= 360;
            if (ret < 0) ret += 360;
            return ret;
        }

        /// <summary>
        /// 각도를 0~360 범위로 정규화합니다.
        /// </summary>
        /// <param name="angle">정규화할 각도</param>
        /// <returns>0~360 범위의 각도</returns>
        public static double StandardAngle(double angle)
        {
            double ret = angle;
            if (ret > 360) ret -= 360;
            if (ret < 0) ret += 360;
            return ret;
        }
        #endregion

        #region CompareAngle
        /// <summary>
        /// 각도가 시작 각도와 끝 각도 사이에 있는지 확인합니다.
        /// </summary>
        /// <param name="Angle">확인할 각도</param>
        /// <param name="StartAngle">시작 각도</param>
        /// <param name="EndAngle">끝 각도</param>
        /// <returns>각도가 범위 내에 있으면 true</returns>
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
