using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Going.UI.Controls.TrendGraph
{
    /// <summary>
    /// 트렌드 그래프 렌더링 관리자
    /// </summary>
    public class TrendRenderer : IDisposable
    {
        #region Fields

        private readonly SKPaint _backgroundPaint;
        private readonly SKPaint _borderPaint;
        private readonly SKPaint _axisPaint;
        private readonly SKPaint _gridPaint;
        private readonly SKPaint _linePaint;
        private readonly SKPaint _fillPaint;
        private readonly SKPaint _pointPaint;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _legendPaint;
        private readonly SKPaint _legendTextPaint;
        private readonly SKPaint _scrollBarPaint;
        private readonly SKPaint _scrollHandlePaint;

        private readonly List<(float X, float Y)> _cachedPoints = new();
        private bool _disposed;

        #endregion

        #region Properties

        /// <summary>
        /// 그래프 영역
        /// </summary>
        public SKRect GraphArea { get; set; }

        /// <summary>
        /// 레전드 영역
        /// </summary>
        public SKRect LegendArea { get; set; }

        /// <summary>
        /// 스크롤바 영역
        /// </summary>
        public SKRect ScrollBarArea { get; set; }

        /// <summary>
        /// 보이는 X축 최소값
        /// </summary>
        public DateTime VisibleXMin { get; set; }

        /// <summary>
        /// 보이는 X축 최대값
        /// </summary>
        public DateTime VisibleXMax { get; set; }

        /// <summary>
        /// Y축 최소값
        /// </summary>
        public double DisplayYMin { get; set; }

        /// <summary>
        /// Y축 최대값
        /// </summary>
        public double DisplayYMax { get; set; }

        /// <summary>
        /// 그래프 설정
        /// </summary>
        public TrendGraphSettings Settings { get; set; } = new();

        /// <summary>
        /// 시리즈 데이터
        /// </summary>
        public IReadOnlyList<TrendSeries> Series { get; set; } = Array.Empty<TrendSeries>();

        /// <summary>
        /// 애니메이션 진행 상태 (0.0 ~ 1.0)
        /// </summary>
        public float AnimationProgress { get; set; } = 1.0f;

        /// <summary>
        /// 스크롤 핸들 X 위치
        /// </summary>
        public float ScrollHandleX { get; set; }

        /// <summary>
        /// 스크롤 핸들 너비
        /// </summary>
        public float ScrollHandleWidth { get; set; }

        /// <summary>
        /// 호버링된 데이터 포인트
        /// </summary>
        public TrendDataPoint? HoveredPoint { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 생성자
        /// </summary>
        public TrendRenderer()
        {
            // 페인트 객체 초기화
            _backgroundPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _borderPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            _axisPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            _gridPaint = new SKPaint {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0)
            };
            _linePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
            _fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _pointPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _textPaint = new SKPaint { IsAntialias = true };
            _legendPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _legendTextPaint = new SKPaint { IsAntialias = true };
            _scrollBarPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _scrollHandlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 그래프 렌더링
        /// </summary>
        public void DrawGraph(SKCanvas canvas, GoTheme thm)
        {
            if (canvas == null || thm == null)
                return;

            try
            {
                // 배경 그리기
                DrawBackground(canvas, thm);

                // 그리드 및 축 그리기
                if (Settings.ShowGrid)
                {
                    DrawGrid(canvas, thm);
                }
                DrawAxes(canvas, thm);

                // 모든 시리즈 그리기
                foreach (var series in Series)
                {
                    DrawSeries(canvas, series, thm);
                }

                // 데이터 포인트 그리기
                if (Settings.ShowDataPoints)
                {
                    foreach (var series in Series)
                    {
                        DrawDataPoints(canvas, series, thm);
                    }
                }

                // 레이블 그리기
                if (Settings.ShowLabels)
                {
                    DrawLabels(canvas, thm);
                }

                // 레전드 그리기
                if (Settings.ShowLegend && Series.Count > 0)
                {
                    DrawLegend(canvas, thm);
                }

                // 스크롤바 그리기
                if (Settings.ShowScrollBar && Settings.EnableScrolling)
                {
                    DrawScrollBar(canvas, thm);
                }

                // 호버링된 데이터 포인트 그리기
                if (HoveredPoint != null)
                {
                    DrawHoveredPoint(canvas, HoveredPoint.Value, thm);
                }
            }
            catch (Exception)
            {
                // 예외 발생 시 그리기 중단
                // 실제 애플리케이션에서는 로깅 추가
            }
        }

        #endregion

        #region Drawing Methods

        private void DrawBackground(SKCanvas canvas, GoTheme thm)
        {
            if (Settings.BackgroundDraw)
            {
                // 배경 그리기
                _backgroundPaint.Color = thm.ToColor(Settings.BgColor);
                Util.DrawBox(canvas, Settings.Bounds, Settings.BorderOnly ? SKColors.Transparent : _backgroundPaint.Color, Settings.Round, thm.Corner);

                // 테두리 그리기
                if (!Settings.BorderOnly)
                {
                    _borderPaint.Color = thm.ToColor(Settings.BorderColor);
                }
            }
        }

        private void DrawGrid(SKCanvas canvas, GoTheme thm)
        {
            _gridPaint.Color = thm.ToColor(Settings.GridColor);

            // Y축 그리드 라인
            float stepY = GraphArea.Height / Settings.GridLineCount;
            for (int i = 0; i <= Settings.GridLineCount; i++)
            {
                float y = GraphArea.Bottom - i * stepY;
                canvas.DrawLine(GraphArea.Left, y, GraphArea.Right, y, _gridPaint);
            }

            // X축 그리드 라인 (시간 간격)
            int xDivisions = 5; // X축 분할 개수
            float stepX = GraphArea.Width / xDivisions;
            for (int i = 0; i <= xDivisions; i++)
            {
                float x = GraphArea.Left + i * stepX;
                canvas.DrawLine(x, GraphArea.Top, x, GraphArea.Bottom, _gridPaint);
            }
        }

        private void DrawAxes(SKCanvas canvas, GoTheme thm)
        {
            _axisPaint.Color = thm.ToColor(Settings.AxisColor);

            // X축
            canvas.DrawLine(GraphArea.Left, GraphArea.Bottom, GraphArea.Right, GraphArea.Bottom, _axisPaint);

            // Y축
            canvas.DrawLine(GraphArea.Left, GraphArea.Top, GraphArea.Left, GraphArea.Bottom, _axisPaint);
        }

        private void DrawSeries(SKCanvas canvas, TrendSeries series, GoTheme thm)
        {
            if (series.DataPoints == null || series.DataPoints.Count < 2)
                return;

            var seriesColor = GetSeriesColor(series.Name, thm);
            _linePaint.Color = seriesColor;
            _linePaint.StrokeWidth = Settings.LineThickness;

            // 그라디언트 채우기 색상
            if (Settings.FillArea)
            {
                var fillColor = seriesColor.WithAlpha((byte)Settings.FillOpacity);
                _fillPaint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(GraphArea.Left, GraphArea.Top),
                    new SKPoint(GraphArea.Left, GraphArea.Bottom),
                    new[] { fillColor, fillColor.WithAlpha(0) },
                    new[] { 0.0f, 1.0f },
                    SKShaderTileMode.Clamp
                );
            }

            // 캐시 초기화
            _cachedPoints.Clear();

            // 보이는 범위에 있는 데이터 포인트 필터링
            var visiblePoints = series.DataPoints
                .Where(p => p.Timestamp >= VisibleXMin && p.Timestamp <= VisibleXMax)
                .OrderBy(p => p.Timestamp)
                .ToList();

            // 보이는 범위 바로 바깥의 포인트를 추가하여 부드러운 라인 보장
            var firstVisible = visiblePoints.FirstOrDefault();
            var lastVisible = visiblePoints.LastOrDefault();

            if (firstVisible.Timestamp > DateTime.MinValue)
            {
                var outsideLeft = series.DataPoints
                    .Where(p => p.Timestamp < VisibleXMin)
                    .OrderByDescending(p => p.Timestamp)
                    .FirstOrDefault();

                if (outsideLeft.Timestamp > DateTime.MinValue)
                {
                    visiblePoints.Insert(0, outsideLeft);
                }
            }

            if (lastVisible.Timestamp > DateTime.MinValue)
            {
                var outsideRight = series.DataPoints
                    .Where(p => p.Timestamp > VisibleXMax)
                    .OrderBy(p => p.Timestamp)
                    .FirstOrDefault();

                if (outsideRight.Timestamp > DateTime.MinValue)
                {
                    visiblePoints.Add(outsideRight);
                }
            }

            if (visiblePoints.Count < 2) return;

            // 라인 경로 생성
            using SKPath linePath = new();
            using SKPath? fillPath = Settings.FillArea ? new SKPath() : null;

            if (Settings.FillArea)
            {
                fillPath!.MoveTo(GraphArea.Left, GraphArea.Bottom); // 시작점은 X축 위
            }

            bool first = true;
            float prevX = 0, prevY = 0;

            foreach (var point in visiblePoints)
            {
                float x = MapXToCanvas(point.Timestamp);
                float y = MapYToCanvas(point.Value);

                // 애니메이션 적용
                if (Settings.EnableAnimation && AnimationProgress < 1.0f)
                {
                    if (first)
                    {
                        prevX = x;
                        prevY = y;
                    }
                    else
                    {
                        // 시작점에서 현재 진행 상태에 맞게 선형 보간
                        x = prevX + (x - prevX) * AnimationProgress;
                        y = prevY + (y - prevY) * AnimationProgress;
                    }
                }

                // 점 캐싱
                _cachedPoints.Add((x, y));

                if (first)
                {
                    linePath.MoveTo(x, y);
                    if (Settings.FillArea)
                    {
                        fillPath!.LineTo(x, y);
                    }
                    first = false;
                }
                else
                {
                    linePath.LineTo(x, y);
                    if (Settings.FillArea)
                    {
                        fillPath!.LineTo(x, y);
                    }
                }

                prevX = x;
                prevY = y;
            }

            // 채우기 경로 완성
            if (Settings.FillArea && !first)
            {
                fillPath!.LineTo(prevX, GraphArea.Bottom);
                fillPath.Close();

                if (Settings.EnableShadow)
                {
                    using var shadow = SKImageFilter.CreateDropShadow(2, 2, 5, 5, new SKColor(0, 0, 0, 50));
                    _fillPaint.ImageFilter = shadow;
                }

                canvas.DrawPath(fillPath, _fillPaint);
                _fillPaint.ImageFilter = null;
            }

            // 선 그리기
            if (!first)
            {
                if (Settings.EnableShadow)
                {
                    using var shadow = SKImageFilter.CreateDropShadow(1, 1, 3, 3, new SKColor(0, 0, 0, 80));
                    _linePaint.ImageFilter = shadow;
                }

                canvas.DrawPath(linePath, _linePaint);
                _linePaint.ImageFilter = null;
            }
        }

        private void DrawDataPoints(SKCanvas canvas, TrendSeries series, GoTheme thm)
        {
            if (series.DataPoints == null)
                return;

            var seriesColor = GetSeriesColor(series.Name, thm);
            _pointPaint.Color = seriesColor;

            // 보이는 범위에 있는 데이터 포인트만 그리기
            foreach (var point in series.DataPoints)
            {
                if (point.Timestamp < VisibleXMin || point.Timestamp > VisibleXMax)
                    continue;

                float x = MapXToCanvas(point.Timestamp);
                float y = MapYToCanvas(point.Value);

                // 뷰포트 밖에 있으면 그리지 않음
                if (x < GraphArea.Left || x > GraphArea.Right)
                    continue;

                // 점 그리기
                canvas.DrawCircle(x, y, Settings.DataPointRadius, _pointPaint);

                // 테두리 그리기
                _pointPaint.Style = SKPaintStyle.Stroke;
                _pointPaint.StrokeWidth = 1;
                _pointPaint.Color = SKColors.White;
                canvas.DrawCircle(x, y, Settings.DataPointRadius, _pointPaint);

                // 스타일 복원
                _pointPaint.Style = SKPaintStyle.Fill;
                _pointPaint.Color = seriesColor;
            }
        }

        private void DrawLabels(SKCanvas canvas, GoTheme thm)
        {
            _textPaint.Color = thm.ToColor(Settings.TextColor);
            _textPaint.TextSize = Settings.FontSize;
            _textPaint.Typeface = SKTypeface.FromFamilyName(Settings.FontName);

            // Y축 레이블
            float stepY = GraphArea.Height / Settings.GridLineCount;
            double valueStep = (DisplayYMax - DisplayYMin) / Settings.GridLineCount;

            for (int i = 0; i <= Settings.GridLineCount; i++)
            {
                float y = GraphArea.Bottom - i * stepY;
                double value = DisplayYMin + i * valueStep;
                string label = value.ToString(Settings.ValueFormat);

                // Y축 값 레이블
                _textPaint.TextAlign = SKTextAlign.Right;
                canvas.DrawText(label, GraphArea.Left - 5, y + Settings.FontSize / 3, _textPaint);
            }

            // X축 레이블 (시간 간격)
            _textPaint.TextAlign = SKTextAlign.Center;

            int xDivisions = 5;
            float stepX = GraphArea.Width / xDivisions;
            TimeSpan timeRange = VisibleXMax - VisibleXMin;
            TimeSpan timeStep = new(timeRange.Ticks / xDivisions);

            for (int i = 0; i <= xDivisions; i++)
            {
                float x = GraphArea.Left + i * stepX;
                DateTime time = VisibleXMin.Add(timeStep * i);
                string label = FormatDateTime(time);

                canvas.DrawText(label, x, GraphArea.Bottom + Settings.FontSize + 5, _textPaint);
            }

            // 축 제목
            if (!string.IsNullOrEmpty(Settings.YAxisTitle))
            {
                _textPaint.TextAlign = SKTextAlign.Center;

                // Y축 제목을 세로로 그리기 위해 캔버스 회전
                canvas.Save();
                canvas.RotateDegrees(270, GraphArea.Left - 30, GraphArea.Top + GraphArea.Height / 2);
                canvas.DrawText(Settings.YAxisTitle, GraphArea.Left - 30, GraphArea.Top + GraphArea.Height / 2 + Settings.FontSize / 3, _textPaint);
                canvas.Restore();
            }

            if (!string.IsNullOrEmpty(Settings.XAxisTitle))
            {
                _textPaint.TextAlign = SKTextAlign.Center;
                canvas.DrawText(Settings.XAxisTitle, GraphArea.Left + GraphArea.Width / 2, GraphArea.Bottom + Settings.FontSize * 2 + 10, _textPaint);
            }
        }

        private void DrawLegend(SKCanvas canvas, GoTheme thm)
        {
            if (Series.Count == 0) return;

            float legendItemHeight = Settings.FontSize + 10;
            float padding = 10;
            float markerSize = 10;

            // 레전드 배경
            _legendPaint.Color = thm.ToColor(Settings.BgColor).WithAlpha(200);
            canvas.DrawRoundRect(LegendArea, 5, 5, _legendPaint);

            _legendTextPaint.Color = thm.ToColor(Settings.TextColor);
            _legendTextPaint.TextSize = Settings.FontSize;
            _legendTextPaint.Typeface = SKTypeface.FromFamilyName(Settings.FontName);

            // 각 시리즈에 대한 레전드 아이템 그리기
            for (int i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                var itemY = LegendArea.Top + padding + i * legendItemHeight;
                var markerX = LegendArea.Left + padding;
                var textX = markerX + markerSize + 5;

                // 마커 그리기
                _legendPaint.Color = GetSeriesColor(series.Name, thm);
                canvas.DrawRect(markerX, itemY, markerSize, markerSize, _legendPaint);

                // 텍스트 그리기
                canvas.DrawText(series.Name, textX, itemY + markerSize - 1, _legendTextPaint);
            }
        }

        private void DrawScrollBar(SKCanvas canvas, GoTheme thm)
        {
            // 스크롤바 배경
            _scrollBarPaint.Color = thm.ToColor(Settings.ScrollBarColor);
            canvas.DrawRoundRect(ScrollBarArea, 4, 4, _scrollBarPaint);

            // 스크롤 핸들 그리기
            _scrollHandlePaint.Color = thm.ToColor(Settings.ScrollHandleColor);
            canvas.DrawRoundRect(ScrollHandleX, ScrollBarArea.Top, ScrollHandleWidth, ScrollBarArea.Height, 4, 4, _scrollHandlePaint);
        }

        private void DrawHoveredPoint(SKCanvas canvas, TrendDataPoint point, GoTheme thm)
        {
            var series = FindSeriesForDataPoint(point);
            if (series == null) return;

            float x = MapXToCanvas(point.Timestamp);
            float y = MapYToCanvas(point.Value);

            // 뷰포트 밖에 있으면 그리지 않음
            if (x < GraphArea.Left || x > GraphArea.Right)
                return;

            // 강조 원 그리기
            _pointPaint.Color = GetSeriesColor(series.Name, thm);
            canvas.DrawCircle(x, y, Settings.DataPointRadius * 2, _pointPaint);

            // 테두리 그리기
            _pointPaint.Style = SKPaintStyle.Stroke;
            _pointPaint.StrokeWidth = 2;
            _pointPaint.Color = SKColors.White;
            canvas.DrawCircle(x, y, Settings.DataPointRadius * 2, _pointPaint);

            // 스타일 복원
            _pointPaint.Style = SKPaintStyle.Fill;

            // 툴팁 배경
            string tooltip = $"{series.Name}: {point.Value.ToString(Settings.ValueFormat)}\n{FormatDateTime(point.Timestamp)}";

            _textPaint.TextSize = Settings.FontSize;
            _textPaint.Color = thm.ToColor(Settings.TextColor);

            // 툴팁 크기 계산
            float tooltipWidth = 0;
            float tooltipHeight = Settings.FontSize * 2 + 15;  // 두 줄 + 여백

            string[] lines = tooltip.Split('\n');
            foreach (var line in lines)
            {
                float lineWidth = _textPaint.MeasureText(line);
                tooltipWidth = Math.Max(tooltipWidth, lineWidth);
            }

            tooltipWidth += 20;  // 여백 추가

            // 툴팁 위치 조정 (화면 밖으로 나가지 않도록)
            float tooltipX = x + 10;
            float tooltipY = y - tooltipHeight - 10;

            if (tooltipX + tooltipWidth > Settings.Bounds.Right)
                tooltipX = x - tooltipWidth - 10;

            if (tooltipY < Settings.Bounds.Top)
                tooltipY = y + 10;

            // 툴팁 배경 그리기
            SKRect tooltipRect = new(tooltipX, tooltipY, tooltipX + tooltipWidth, tooltipY + tooltipHeight);
            _legendPaint.Color = thm.ToColor(Settings.BgColor).WithAlpha(230);
            canvas.DrawRoundRect(tooltipRect, 5, 5, _legendPaint);

            // 툴팁 테두리
            _borderPaint.Color = thm.ToColor(Settings.BorderColor);
            canvas.DrawRoundRect(tooltipRect, 5, 5, _borderPaint);

            // 툴팁 텍스트 그리기
            _textPaint.TextAlign = SKTextAlign.Left;

            for (int i = 0; i < lines.Length; i++)
            {
                canvas.DrawText(lines[i], tooltipX + 10, tooltipY + Settings.FontSize + i * (Settings.FontSize + 5) + 5, _textPaint);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// X 좌표를 시간에서 캔버스 좌표로 변환
        /// </summary>
        public float MapXToCanvas(DateTime time)
        {
            if (VisibleXMax == VisibleXMin)
                return GraphArea.Left;

            double normalizedX = (time - VisibleXMin).TotalMilliseconds / (VisibleXMax - VisibleXMin).TotalMilliseconds;
            return (float)(GraphArea.Left + normalizedX * GraphArea.Width);
        }

        /// <summary>
        /// Y 값을 캔버스 좌표로 변환
        /// </summary>
        public float MapYToCanvas(double value)
        {
            if (DisplayYMax == DisplayYMin)
                return GraphArea.Bottom;

            double normalizedY = (value - DisplayYMin) / (DisplayYMax - DisplayYMin);
            return (float)(GraphArea.Bottom - normalizedY * GraphArea.Height);
        }

        /// <summary>
        /// 시간 형식 포맷
        /// </summary>
        private string FormatDateTime(DateTime time)
        {
            // 적절한 날짜/시간 포맷 선택
            TimeSpan range = VisibleXMax - VisibleXMin;

            if (range.TotalDays > 365)
                return time.ToString("yyyy-MM");
            else if (range.TotalDays > 30)
                return time.ToString("MM-dd");
            else if (range.TotalDays > 1)
                return time.ToString("MM-dd HH:mm");
            else
                return time.ToString("HH:mm");
        }

        /// <summary>
        /// 시리즈 색상 가져오기
        /// </summary>
        private SKColor GetSeriesColor(string seriesName, GoTheme thm)
        {
            var series = Series.FirstOrDefault(s => s.Name == seriesName);
            if (series == null || string.IsNullOrEmpty(series.Color))
                return thm.ToColor("Text");

            return thm.ToColor(series.Color);
        }

        /// <summary>
        /// 데이터 포인트에 해당하는 시리즈 찾기
        /// </summary>
        private TrendSeries? FindSeriesForDataPoint(TrendDataPoint point)
        {
            foreach (var series in Series)
            {
                if (series.DataPoints == null) continue;

                foreach (var p in series.DataPoints)
                {
                    // 타임스탬프와 값이 정확히 일치하는지 확인
                    if (p.Timestamp == point.Timestamp && Math.Abs(p.Value - point.Value) < 0.00001)
                    {
                        return series;
                    }
                }
            }

            return null;
        }

        #endregion

        #region IDisposable 구현

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 리소스 해제 구현
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                    _backgroundPaint.Dispose();
                    _borderPaint.Dispose();
                    _axisPaint.Dispose();
                    _gridPaint.Dispose();
                    _linePaint.Dispose();
                    _fillPaint.Dispose();
                    _pointPaint.Dispose();
                    _textPaint.Dispose();
                    _legendPaint.Dispose();
                    _legendTextPaint.Dispose();
                    _scrollBarPaint.Dispose();
                    _scrollHandlePaint.Dispose();
                }

                // 관리되지 않는 리소스 해제 (현재 없음)

                _disposed = true;
            }
        }

        /// <summary>
        /// 소멸자
        /// </summary>
        ~TrendRenderer()
        {
            Dispose(false);
        }
        
        #endregion
    }

    /// <summary>
    /// 트렌드 그래프 설정
    /// </summary>
    public class TrendGraphSettings
    {
        #region 기본 설정
        public SKRect Bounds { get; set; }
        public float Padding { get; set; } = 10f;
        #endregion

        #region 아이콘 설정
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;
        public GoDirectionHV IconDirection { get; set; }
        #endregion

        #region 그래프 라벨 설정
        public string Text { get; set; } = "label";
        public string TextFontName { get; set; } = "나눔고딕";
        public float TextFontSize { get; set; } = 12;
        #endregion

        #region 그래프 배경 설정
        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; }
        public string BgColor { get; set; } = "Base3";
        public string BorderColor { get; set; } = "Border";
        public GoRoundType Round { get; set; } = GoRoundType.All;
        #endregion

        #region 그래프 설정
        public string AxisColor { get; set; } = "Text";
        public string GridColor { get; set; } = "Base2";
        public bool ShowGrid { get; set; } = true;
        public bool ShowLegend { get; set; } = true;
        public int GridLineCount { get; set; } = 5;
        public bool EnableShadow { get; set; } = true;
        public bool EnableAnimation { get; set; } = true;
        public int AnimationDuration { get; set; } = 300; // 밀리초
        public float LineThickness { get; set; } = 2f;
        public bool FillArea { get; set; } = true;
        public int FillOpacity { get; set; } = 100; // 0-255
        public bool ShowDataPoints { get; set; }
        public float DataPointRadius { get; set; } = 4f;
        public bool ShowLabels { get; set; } = true;
        #endregion

        #region 축 레이블 설정
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;
        public string TextColor { get; set; } = "Text";
        public string ValueFormat { get; set; } = "F1";
        public string? XAxisTitle { get; set; }
        public string? YAxisTitle { get; set; }
        public bool AutoScale { get; set; } = true;
        public double YAxisMin { get; set; } = 0;
        public double YAxisMax { get; set; } = 100;
        #endregion

        #region 스크롤 설정
        public bool EnableScrolling { get; set; } = true;
        public bool EnableMouseWheelZoom { get; set; } = true;
        public double VisibleTimeRange { get; set; } = 60 * 60 * 24 * 7; // 기본 7일
        public double MinTimeRange { get; set; } = 60 * 60; // 1시간
        public double MaxTimeRange { get; set; } = 60 * 60 * 24 * 365; // 1년
        public float ScrollBarHeight { get; set; } = 15f;
        public string ScrollBarColor { get; set; } = "Base2";
        public string ScrollHandleColor { get; set; } = "Primary";
        public bool ShowScrollBar { get; set; } = true;
        #endregion
    }
}