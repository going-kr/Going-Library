using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Tools
{
    /// <summary>
    /// SkiaSharp SKPath 생성 유틸리티 클래스입니다. 박스, 체크, 원, 게이지, 탭 등의 경로를 생성합니다.
    /// </summary>
    public class PathTool
    {
        #region Box
        /// <summary>
        /// 둥근 모서리를 가진 사각형 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="rect">사각형 영역</param>
        /// <param name="round">모서리 둥글기 타입</param>
        /// <param name="corner">모서리 둥글기 크기</param>
        public static void Box(SKPath path, SKRect rect, GoRoundType round, float corner)
        {
            path.Reset();

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
        }
        #endregion
        #region Check
        /// <summary>
        /// 체크 표시(V) 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="rect">체크 표시를 그릴 사각형 영역</param>
        public static void Check(SKPath path, SKRect rect)
        {
            path.Reset();

            var INF = rect.Width / 4;
            var rtCheck = Util.FromRect(rect.Left, rect.Top, rect.Width, rect.Height);
            rtCheck.Inflate(-INF, -INF);


            var points = new SKPoint[] { new SKPoint(rtCheck.Left, rtCheck.MidY),
                                         new SKPoint(rtCheck.MidX, rtCheck.Bottom),
                                         new SKPoint(rtCheck.Right, rtCheck.Top)};

            path.MoveTo(points[0]);
            path.LineTo(points[1]);
            path.LineTo(points[2]);
        }
        #endregion
        #region Circle
        /// <summary>
        /// 원 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="x">중심 X 좌표</param>
        /// <param name="y">중심 Y 좌표</param>
        /// <param name="r">반지름</param>
        public static void Circle(SKPath path, float x, float y, float r)
        {
            path.Reset();
            path.AddCircle(x, y, r);
        }
        #endregion
        #region RoundedPolygon
        /// <summary>
        /// 둥근 모서리를 가진 다각형 경로를 생성합니다.
        /// </summary>
        /// <param name="retval">경로 객체</param>
        /// <param name="points">다각형 꼭짓점 배열</param>
        /// <param name="radius">모서리 둥글기 반지름</param>
        public static void RoundedPolygon(SKPath retval, SKPoint[] points, float radius)
        {
            retval.Reset();

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
        }
        #endregion

        #region Gauge
        /// <summary>
        /// 게이지 호 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="rtGauge">게이지 경계 사각형</param>
        /// <param name="startAngle">시작 각도</param>
        /// <param name="sweepAngle">호의 각도</param>
        /// <param name="barSize">게이지 바 두께</param>
        public static void Gauge(SKPath path, SKRect rtGauge, float startAngle, float sweepAngle, float barSize)
        {
            path.Reset();

            #region var
            var cp = MathTool.CenterPoint(rtGauge);
            var rtGaugeIn = Util.FromRect(rtGauge); rtGaugeIn.Inflate(-barSize, -barSize);

            var rtOut = rtGauge;
            var rtIn = rtGaugeIn;

            var pl1_1 = MathTool.GetPointWithAngle(rtOut, startAngle);
            var pl1_2 = MathTool.GetPointWithAngle(rtIn, startAngle);
            var pl1_C = MathTool.CenterPoint(pl1_1, pl1_2);
            var pl2_1 = MathTool.GetPointWithAngle(rtOut, startAngle + sweepAngle);
            var pl2_2 = MathTool.GetPointWithAngle(rtIn, startAngle + sweepAngle);
            var pl2_C = MathTool.CenterPoint(pl2_1, pl2_2);
            #endregion

            #region Path
            path.ArcTo(MathTool.MakeRectangle(pl1_C, barSize), startAngle, -180, false);
            path.ArcTo(rtIn, startAngle, sweepAngle, false);
            path.ArcTo(MathTool.MakeRectangle(pl2_C, barSize), startAngle + sweepAngle + 180, -180, false);
            path.ArcTo(rtOut, startAngle + sweepAngle, -sweepAngle, false);
            path.Close();
            #endregion
        }
        #endregion
        #region Needle
        /// <summary>
        /// 게이지 바늘 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="rtContent">콘텐츠 영역</param>
        /// <param name="rtGauge">게이지 경계 사각형</param>
        /// <param name="value">현재 값</param>
        /// <param name="minimum">최솟값</param>
        /// <param name="maximum">최댓값</param>
        /// <param name="startAngle">시작 각도</param>
        /// <param name="sweepAngle">호의 각도</param>
        /// <param name="remarkFontSize">눈금 글꼴 크기</param>
        public static void Needle(SKPath path, SKRect rtContent, SKRect rtGauge,
            double value, double minimum, double maximum, float startAngle, float sweepAngle, float remarkFontSize)
        {
            path.Reset();

            #region var
            var rwh = rtGauge.Width / 2F;
            var distN = rwh - remarkFontSize - (GoMeter.GIN - 5);
            var cp = new SKPoint(rtContent.MidX, rtContent.MidY);
            #endregion

            #region Path
            var vang = Convert.ToSingle(MathTool.Map(MathTool.Constrain(value, minimum, maximum), minimum, maximum, 0, sweepAngle));
            var pt = MathTool.GetPointWithAngle(cp, vang + startAngle, distN);

            var rtS = MathTool.MakeRectangle(pt, 3);
            var rtL = MathTool.MakeRectangle(cp, MathTool.Constrain(distN / 5F, 10, 30));

            path.AddArc(rtL, vang + startAngle + 90, 180);
            path.ArcTo(rtS, vang + startAngle + 90 + 180, 180, false);
            path.Close();
            #endregion
        }
        #endregion
        #region Knob
        /// <summary>
        /// 노브(원형 다이얼) 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="rtContent">콘텐츠 영역</param>
        /// <param name="rtKnob">노브 경계 사각형</param>
        public static void Knob(SKPath path, SKRect rtContent, SKRect rtKnob)
        {
            path.Reset();

            var cp = new SKPoint(rtContent.MidX, rtContent.MidY);
            path.AddCircle(cp.X, cp.Y, rtKnob.Width / 2F);
        }
        #endregion
        #region KnobCursor
        /// <summary>
        /// 노브 커서 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="pt1">시작점</param>
        /// <param name="pt2">끝점</param>
        /// <param name="vang">각도</param>
        /// <param name="width">커서 너비</param>
        public static void KnobCursor(SKPath path, SKPoint pt1, SKPoint pt2, float vang, int width)
        {
            path.Reset();

            var sz = width;
            var rt1 = MathTool.MakeRectangle(pt1, sz);
            var rt2 = MathTool.MakeRectangle(pt2, sz);

            path.AddArc(SKRect.Create(rt1.Left, rt1.Top, rt1.Width, rt1.Height), vang - 90, 180);
            path.ArcTo(SKRect.Create(rt2.Left, rt2.Top, rt2.Width, rt2.Height), vang + 90, 180, false);
            path.Close();
        }
        #endregion

        #region Tab
        /// <summary>
        /// 탭 모양의 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="rtTab">탭 경계 사각형</param>
        /// <param name="tabPosition">탭 위치 방향</param>
        /// <param name="corner">모서리 둥글기 크기</param>
        public static void Tab(SKPath path, SKRect rtTab,  GoDirection tabPosition, float corner)
        {
            path.Reset();

            var corner2 = corner * 2F;
            switch (tabPosition)
            {
                case GoDirection.Up:
                    {
                        path.ArcTo(Util.FromRect(rtTab.Left - corner2, rtTab.Bottom - corner2, corner2, corner2), 90, -90, true);
                        path.ArcTo(Util.FromRect(rtTab.Left, rtTab.Top, corner2, corner2), 180, 90, false);
                        path.ArcTo(Util.FromRect(rtTab.Right - corner2, rtTab.Top, corner2, corner2), -90, 90, false);
                        path.ArcTo(Util.FromRect(rtTab.Right, rtTab.Bottom - corner2, corner2, corner2), 180, -90, false);
                    }
                    break;
                case GoDirection.Down:
                    {
                        path.ArcTo(Util.FromRect(rtTab.Left - corner2, rtTab.Top, corner2, corner2), -90, 90, true);
                        path.ArcTo(Util.FromRect(rtTab.Left, rtTab.Bottom - corner2, corner2, corner2), 180, -90, false);
                        path.ArcTo(Util.FromRect(rtTab.Right - corner2, rtTab.Bottom - corner2, corner2, corner2), 90, -90, false);
                        path.ArcTo(Util.FromRect(rtTab.Right, rtTab.Top, corner2, corner2), 180, 90, false);
                    }
                    break;
                case GoDirection.Left:
                    {
                        path.ArcTo(Util.FromRect(rtTab.Right - corner2, rtTab.Top - corner2, corner2, corner2), 0, 90, true);
                        path.ArcTo(Util.FromRect(rtTab.Left, rtTab.Top, corner2, corner2), -90, -90, false);
                        path.ArcTo(Util.FromRect(rtTab.Left, rtTab.Bottom - corner2, corner2, corner2), 180, -90, false);
                        path.ArcTo(Util.FromRect(rtTab.Right - corner2, rtTab.Bottom, corner2, corner2), -90, 90, false);
                    }
                    break;
                case GoDirection.Right:
                    {
                        path.ArcTo(Util.FromRect(rtTab.Left, rtTab.Top - corner2, corner2, corner2), 180, -90, true);
                        path.ArcTo(Util.FromRect(rtTab.Right - corner2, rtTab.Top, corner2, corner2), -90, 90, false);
                        path.ArcTo(Util.FromRect(rtTab.Right - corner2, rtTab.Bottom - corner2, corner2, corner2), 0, 90, false);
                        path.ArcTo(Util.FromRect(rtTab.Left, rtTab.Bottom, corner2, corner2), -90, -90, false);
                    }
                    break;
            }
        }
        #endregion

        #region FlowSystem

        #region CylinderFrame
        /// <summary>
        /// 원통형 탱크의 외곽 및 내곽 프레임 경로를 생성합니다.
        /// </summary>
        /// <param name="pathOut">외곽 경로 객체</param>
        /// <param name="pathIn">내곽 경로 객체</param>
        /// <param name="bounds">탱크 경계 사각형</param>
        /// <param name="frameSize">프레임 두께</param>
        /// <param name="pipeSize">배관 크기</param>
        public static void CylinderFrame(SKPath pathOut, SKPath pathIn, SKRect bounds, float frameSize, float pipeSize)
        {
            var innerBounds = bounds; innerBounds.Inflate(-frameSize, -frameSize);
            Cylinder(pathOut, bounds, pipeSize, 10);
            Cylinder(pathIn, innerBounds, pipeSize, 10 * (innerBounds.Width / bounds.Width));
        }

        /// <summary>
        /// 둥근 모서리를 가진 원통형 경로를 생성합니다.
        /// </summary>
        private static void Cylinder(SKPath path, SKRect bounds, float pipeSize, float corner)
        {
            path.Reset();
            path.AddRoundRect(bounds, corner, corner);
        }
        #endregion

        #region SiloFrame
        /// <summary>
        /// 사일로형 탱크의 외곽 및 내곽 프레임 경로를 생성합니다.
        /// 상단은 반원형, 하단은 역삼각형(콘) 형태입니다.
        /// </summary>
        /// <param name="pathOut">외곽 경로 객체</param>
        /// <param name="pathIn">내곽 경로 객체</param>
        /// <param name="bounds">탱크 경계 사각형</param>
        /// <param name="frameSize">프레임 두께</param>
        /// <param name="pipeSize">배관 크기</param>
        public static void SiloFrame(SKPath pathOut, SKPath pathIn, SKRect bounds, float frameSize, float pipeSize)
        {
            var innerBounds = bounds; innerBounds.Inflate(-frameSize, -frameSize);
            Silo(pathOut, bounds, pipeSize);
            Silo(pathIn, innerBounds, pipeSize);
        }

        /// <summary>
        /// 사일로 형태의 경로를 생성합니다.
        /// </summary>
        private static void Silo(SKPath path, SKRect bounds, float pipeSize)
        {
            float x = bounds.Left;
            float y = bounds.Top;
            float width = bounds.Width;
            float height = bounds.Height;

            float pipeWidth = pipeSize;
            float topCapHeight = height * 0.15f;
            float bodyHeight = height * 0.60f;
            float topY = y + topCapHeight;
            float bodyBottom = topY + bodyHeight;
            float coneBottom = y + height;
            float pipeLeft = x + (width - pipeWidth) / 2;
            float pipeRight = x + (width + pipeWidth) / 2;

            path.Reset();

            path.MoveTo(x, topY);
            path.ArcTo(
                new SKRect(x, y, x + width, y + topCapHeight * 2),
                180, 180, false);
            path.LineTo(x + width, bodyBottom);
            path.LineTo(pipeRight, coneBottom);
            path.LineTo(pipeLeft, coneBottom);
            path.LineTo(x, bodyBottom);
            path.Close();
        }
        #endregion

        #region LiquidWave
        private static DateTime _baseTime = DateTime.Now;

        /// <summary>
        /// 탱크 내부의 액체 파동 경로를 생성합니다.
        /// 믹서 동작 여부에 따라 파형 속도와 높이가 달라집니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="bounds">액체 영역 경계 사각형</param>
        /// <param name="level">현재 수위 값</param>
        /// <param name="min">최소 수위</param>
        /// <param name="max">최대 수위</param>
        /// <param name="mixonoff">믹서 동작 여부</param>
        public static void LiquidWave(SKPath path, SKRect bounds, double level, double min, double max, bool mixonoff)
        {
            path.Reset();

            var w = mixonoff ? (DateTime.Now - _baseTime).TotalMilliseconds % 300D / 300D * MathF.PI * 2D :
                               (DateTime.Now - _baseTime).TotalMilliseconds % 2000D / 2000D * MathF.PI * 2D;
            float wavePhase = Convert.ToSingle(w);
            float waveHeight = mixonoff ? 3F : 2F;
            int waveCount = mixonoff ? 3 : 2;

            double percent = 0;
            if (max > min) percent = Math.Clamp((level - min) / (max - min) * 100f, 0, 100);
            if (percent <= 0) return;

            float x = bounds.Left;
            float y = bounds.Top;
            float width = bounds.Width;
            float height = bounds.Height;

            float liquidTop = y + height - Convert.ToSingle(height * percent / 100.0);

            path.MoveTo(x, liquidTop);
            DrawWaveLine(path, x, x + width, liquidTop, wavePhase, waveHeight, waveCount);
            path.LineTo(x + width, y + height);
            path.LineTo(x, y + height);
            path.Close();
        }

        /// <summary>
        /// 믹서(교반기) 날개 형태의 경로를 생성합니다.
        /// 중앙 축을 기준으로 좌우 대칭 날개와 중심 원으로 구성됩니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="center">믹서 중심점</param>
        /// <param name="wingW">날개 너비</param>
        /// <param name="wingH">날개 높이(팁 반경)</param>
        /// <param name="frameSize">프레임 두께</param>
        public static void Mixer(SKPath path, SKPoint center, float wingW, float wingH, float frameSize)
        {
            float centerX = center.X;
            float centerY = center.Y;

            float wingLength = wingW;
            float tipRadius = wingH;
            float rootWidth = frameSize / 2F;

            path.Reset();

            path.MoveTo(centerX, centerY - rootWidth);

            path.CubicTo(
                centerX + wingLength * 0.3f, centerY - rootWidth,
                centerX + wingLength - tipRadius, centerY - tipRadius * 1.5f,
                centerX + wingLength - tipRadius, centerY - tipRadius
            );

            path.ArcTo(
                new SKRect(centerX + wingLength - tipRadius * 2, centerY - tipRadius,
                           centerX + wingLength, centerY + tipRadius),
                270, 180, false);

            path.CubicTo(
                centerX + wingLength - tipRadius, centerY + tipRadius * 1.5f,
                centerX + wingLength * 0.3f, centerY + rootWidth,
                centerX, centerY + rootWidth
            );

            path.LineTo(centerX, centerY + rootWidth);

            path.CubicTo(
                centerX - wingLength * 0.3f, centerY + rootWidth,
                centerX - (wingLength - tipRadius), centerY + tipRadius * 1.5f,
                centerX - (wingLength - tipRadius), centerY + tipRadius
            );

            path.ArcTo(
                new SKRect(centerX - wingLength, centerY - tipRadius,
                           centerX - wingLength + tipRadius * 2, centerY + tipRadius),
                90, 180, false);

            path.CubicTo(
                centerX - (wingLength - tipRadius), centerY - tipRadius * 1.5f,
                centerX - wingLength * 0.3f, centerY - rootWidth,
                centerX, centerY - rootWidth
            );

            path.Close();

            path.AddCircle(centerX, centerY, rootWidth * 2f);
        }

        /// <summary>
        /// 사인파 형태의 파동 선을 그립니다.
        /// </summary>
        private static void DrawWaveLine(SKPath path, float startX, float endX, float baseY, float phase, float height, int waveCount)
        {
            float width = endX - startX;
            int segments = Math.Max(30, (int)(width / 3));

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float currentX = startX + width * t;

                float angle = (t * waveCount * 2 * MathF.PI) + phase;
                float waveY = baseY + MathF.Sin(angle) * height;

                path.LineTo(currentX, waveY);
            }
        }
        #endregion

        #region GlobeValve
        /// <summary>
        /// 글로브 밸브 형태의 경로를 생성합니다. 중앙 원형 본체, 좌우 배관 연결부, 상단 핸들로 구성됩니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="bounds">밸브 경계 사각형</param>
        /// <param name="pipeSize">배관 크기</param>
        /// <param name="frameSize">프레임 두께</param>
        /// <param name="handleSize">핸들 너비</param>
        public static void GlobeValve(SKPath path, SKRect bounds, float pipeSize, float frameSize, float handleSize)
        {
            path.Reset();

            var center = new SKPoint(bounds.MidX, bounds.MidY);
            float radius = (pipeSize + 10) / 2f;
            float halfPipe = pipeSize / 2f;
            float gap = pipeSize / 4f;

            float handleWidth = handleSize;
            float handleHeight = 10f;
            float stemDistance = 5f;
            float stemWidth = pipeSize / 3f;
            float cornerRadius = 4f;

            float halfStem = stemWidth / 2f;
            float stemAngleRad = (float)Math.Asin(halfStem / radius);
            float stemAngleDeg = (float)(stemAngleRad * 180 / Math.PI);
            float stemIntersectY = (float)Math.Sqrt(radius * radius - halfStem * halfStem);

            float bodyAngleRad = (float)Math.Atan2(halfPipe, pipeSize + gap);
            float bodyAngleDeg = (float)(bodyAngleRad * 180 / Math.PI);

            var oval = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

            // 상단 핸들
            float handleBottomY = center.Y - radius - stemDistance;
            float handleTopY = handleBottomY - handleHeight;

            path.MoveTo(center.X + halfStem, center.Y - stemIntersectY);
            path.LineTo(center.X + halfStem, handleBottomY);

            path.LineTo(center.X + (handleWidth / 2f) - cornerRadius, handleBottomY);
            path.ArcTo(new SKRect(center.X + (handleWidth / 2f) - cornerRadius * 2, handleBottomY - cornerRadius * 2, center.X + (handleWidth / 2f), handleBottomY), 90, -90, false);
            path.LineTo(center.X + (handleWidth / 2f), handleTopY + cornerRadius);
            path.ArcTo(new SKRect(center.X + (handleWidth / 2f) - cornerRadius * 2, handleTopY, center.X + (handleWidth / 2f), handleTopY + cornerRadius * 2), 0, -90, false);
            path.LineTo(center.X - (handleWidth / 2f) + cornerRadius, handleTopY);
            path.ArcTo(new SKRect(center.X - (handleWidth / 2f), handleTopY, center.X - (handleWidth / 2f) + cornerRadius * 2, handleTopY + cornerRadius * 2), 270, -90, false);
            path.LineTo(center.X - (handleWidth / 2f), handleBottomY - cornerRadius);
            path.ArcTo(new SKRect(center.X - (handleWidth / 2f), handleBottomY - cornerRadius * 2, center.X - (handleWidth / 2f) + cornerRadius * 2, handleBottomY), 180, -90, false);

            path.LineTo(center.X - halfStem, handleBottomY);
            path.LineTo(center.X - halfStem, center.Y - stemIntersectY);

            // 좌측 바디
            path.ArcTo(oval, 270 - stemAngleDeg, -(90 - bodyAngleDeg - stemAngleDeg), false);

            float leftBodyX = center.X - gap - pipeSize;
            path.QuadTo(leftBodyX + (pipeSize / 2), center.Y - halfPipe, leftBodyX, center.Y - halfPipe);
            path.LineTo(leftBodyX, center.Y + halfPipe);
            path.QuadTo(leftBodyX + (pipeSize / 2), center.Y + halfPipe, center.X - radius * (float)Math.Cos(bodyAngleRad), center.Y + radius * (float)Math.Sin(bodyAngleRad));

            // 하단 원호
            path.ArcTo(oval, 180 - bodyAngleDeg, -(180 - 2 * bodyAngleDeg), false);

            // 우측 바디
            float rightBodyX = center.X + gap + pipeSize;
            path.QuadTo(rightBodyX - (pipeSize / 2), center.Y + halfPipe, rightBodyX, center.Y + halfPipe);
            path.LineTo(rightBodyX, center.Y - halfPipe);
            path.QuadTo(rightBodyX - (pipeSize / 2), center.Y - halfPipe, center.X + radius * (float)Math.Cos(bodyAngleRad), center.Y - radius * (float)Math.Sin(bodyAngleRad));

            path.ArcTo(oval, bodyAngleDeg, -(bodyAngleDeg - (stemAngleDeg - 90)), false);
            path.Close();
        }
        #endregion

        #region CPump
        /// <summary>
        /// 원심 펌프 형태의 경로를 생성합니다. 원형 본체에 토출구와 흡입구가 연결됩니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="bounds">펌프 경계 사각형</param>
        /// <param name="pipeSize">배관 크기</param>
        /// <param name="frameSize">프레임 두께</param>
        /// <param name="useInlet">흡입구 사용 여부</param>
        /// <param name="reverse">좌우 반전 여부</param>
        public static void CPump(SKPath path, SKRect bounds, float pipeSize, float frameSize, bool useInlet, bool reverse)
        {
            path.Reset();

            float centerX = bounds.MidX;
            float centerY = bounds.MidY;
            float radius = pipeSize * 1.5f;
            float dir = reverse ? -1f : 1f;

            SKRect bodyRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

            path.MoveTo(centerX, centerY - radius);

            // 토출구
            path.LineTo(centerX + (radius * dir), centerY - radius);
            path.LineTo(centerX + (radius * dir) + (frameSize * dir), centerY - radius);
            path.LineTo(centerX + (radius * dir) + (frameSize * dir), centerY - radius + pipeSize);
            path.LineTo(centerX + (radius * dir), centerY - radius + pipeSize);

            if (!reverse)
                path.ArcTo(bodyRect, -25f, 115f, false);
            else
                path.ArcTo(bodyRect, 205f, -115f, false);

            if (useInlet)
            {
                // 흡입구
                path.LineTo(centerX - (radius * dir), centerY + radius);
                path.LineTo(centerX - (radius * dir) - (frameSize * dir), centerY + radius);
                path.LineTo(centerX - (radius * dir) - (frameSize * dir), centerY + radius - pipeSize);
                path.LineTo(centerX - (radius * dir), centerY + radius - pipeSize);

                if (!reverse)
                    path.ArcTo(bodyRect, 155f, 115f, false);
                else
                    path.ArcTo(bodyRect, 25f, -115f, false);
            }
            else
            {
                path.ArcTo(bodyRect, 90f, reverse ? -180f : 180f, false);
            }

            path.Close();
        }
        #endregion

        #region Impeller
        /// <summary>
        /// 팬 임펠러(회전 날개) 형태의 경로를 생성합니다.
        /// </summary>
        /// <param name="path">경로 객체</param>
        /// <param name="center">중심점</param>
        /// <param name="radius">임펠러 반지름</param>
        /// <param name="bladeCount">날개 수</param>
        public static void Impeller(SKPath path, SKPoint center, float radius, int bladeCount)
        {
            path.Reset();

            if (path == null) throw new ArgumentNullException(nameof(path));
            if (bladeCount <= 0) return;

            float hubRadius = radius * 0.12f;
            float angleStep = 360f / bladeCount;

            float innerWidth = angleStep * 0.05f;
            float outerWidth = angleStep * 0.65f;
            float curvature = 35f;

            for (int i = 0; i < bladeCount; i++)
            {
                float baseAngle = i * angleStep;

                float startAngleInner = baseAngle;
                float endAngleInner = baseAngle + innerWidth;
                float startAngleOuter = baseAngle + curvature;
                float endAngleOuter = baseAngle + curvature + outerWidth;

                SKPoint pInnerStart = GetPolarPoint(center, hubRadius, startAngleInner);
                if (i == 0) path.MoveTo(pInnerStart);
                else path.LineTo(pInnerStart);

                SKPoint pOuterStart = GetPolarPoint(center, radius, startAngleOuter);
                SKPoint cpFront = GetPolarPoint(center, radius * 0.6f, startAngleInner);
                path.QuadTo(cpFront, pOuterStart);

                SKRect outerRect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
                path.ArcTo(outerRect, startAngleOuter, outerWidth, false);

                SKPoint pInnerEnd = GetPolarPoint(center, hubRadius, endAngleInner);
                SKPoint cpBack = GetPolarPoint(center, radius * 0.5f, endAngleOuter - (outerWidth * 0.2f));
                path.QuadTo(cpBack, pInnerEnd);

                SKRect innerRect = new SKRect(center.X - hubRadius, center.Y - hubRadius, center.X + hubRadius, center.Y + hubRadius);
                path.ArcTo(innerRect, endAngleInner, -innerWidth, false);
            }

            path.Close();
        }

        /// <summary>
        /// 극좌표(중심, 거리, 각도)를 직교 좌표로 변환합니다.
        /// </summary>
        private static SKPoint GetPolarPoint(SKPoint center, float distance, float degrees)
        {
            float radians = degrees * (float)Math.PI / 180f;
            return new SKPoint(
                center.X + distance * (float)Math.Cos(radians),
                center.Y + distance * (float)Math.Sin(radians)
            );
        }
        #endregion

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
