using Going.UI.Controls.TrendGraph;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Going.UI.Controls
{
    /// <summary>
    /// 트렌드 그래프 컨트롤 - 시간에 따른 데이터 변화를 시각적으로 표현하는 컴포넌트
    /// </summary>
    public class GoTrendGraph : GoControl, IDisposable
    {
        #region Properties

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
        public bool ShowDataPoints { get; set; } = false;
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
        /// <summary>
        /// 스크롤 기능 활성화 여부
        /// </summary>
        public bool EnableScrolling { get; set; } = true;

        /// <summary>
        /// 마우스 휠로 줌 기능 활성화 여부
        /// </summary>
        public bool EnableMouseWheelZoom { get; set; } = true;

        /// <summary>
        /// 화면에 표시되는 시간 범위 (초 단위)
        /// </summary>
        public double VisibleTimeRange { get; set; } = 60 * 60 * 24 * 7; // 기본 7일

        /// <summary>
        /// 줌 최소 시간 범위 (초 단위)
        /// </summary>
        public double MinTimeRange { get; set; } = 60 * 60; // 1시간

        /// <summary>
        /// 줌 최대 시간 범위 (초 단위)
        /// </summary>
        public double MaxTimeRange { get; set; } = 60 * 60 * 24 * 365; // 1년

        /// <summary>
        /// 스크롤바 두께
        /// </summary>
        public float ScrollBarHeight { get; set; } = 15f;

        /// <summary>
        /// 스크롤바 색상
        /// </summary>
        public string ScrollBarColor { get; set; } = "Base2";

        /// <summary>
        /// 스크롤바 핸들 색상
        /// </summary>
        public string ScrollHandleColor { get; set; } = "Primary";

        /// <summary>
        /// 스크롤바 표시 여부
        /// </summary>
        public bool ShowScrollBar { get; set; } = true;
        #endregion

        #region 시리즈 데이터
        private readonly List<TrendSeries> _series = new();
        public IReadOnlyList<TrendSeries> Series => _series.AsReadOnly();
        #endregion

        #region 내부 상태
        private SKRect _graphArea;
        private SKRect _legendArea;
        private SKRect _scrollBarArea;
        private bool _needsLayoutUpdate = true;
        private readonly Dictionary<string, SKColor> _seriesColors = new();
        private double _actualYMin;
        private double _actualYMax;
        private double _displayYMin;
        private double _displayYMax;
        private DateTime _dataXMin = DateTime.MinValue;
        private DateTime _dataXMax = DateTime.MaxValue;
        private DateTime _visibleXMin;
        private DateTime _visibleXMax;
        private float _animationProgress = 1.0f; // 0.0 ~ 1.0
        private DateTime _animationStartTime;

        /// <summary>
        /// 그래프 패딩
        /// </summary>
        public float Padding { get; set; } = 10f;
        #endregion

        #region 스크롤 내부 상태
        private bool _isDraggingGraph;
        private bool _isDraggingScrollHandle;
        private float _dragStartX;
        private DateTime _dragStartMin;
        private DateTime _dragStartMax;
        private float _scrollHandleWidth;
        private float _scrollHandleX;
        #endregion

        #region 성능 최적화를 위한 페인트 객체 재사용
        private readonly SKPaint _backgroundPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint _borderPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
        private readonly SKPaint _axisPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
        private readonly SKPaint _gridPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1, PathEffect = SKPathEffect.CreateDash([4, 4], 0) };
        private readonly SKPaint _linePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
        private readonly SKPaint _fillPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint _pointPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint _textPaint = new() { IsAntialias = true };
        private readonly SKPaint _legendPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint _legendTextPaint = new() { IsAntialias = true };
        private readonly SKPaint _scrollBarPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint _scrollHandlePaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

        // 데이터 캐싱을 위한 객체
        private readonly List<(float X, float Y)> _cachedPoints = new();
        #endregion

        #endregion

        #region Events

        /// <summary>
        /// 시리즈 데이터가 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? DataChanged;

        /// <summary>
        /// 사용자가 데이터 포인트를 클릭했을 때 발생합니다.
        /// </summary>
        public event EventHandler<DataPointEventArgs>? DataPointSelected;

        /// <summary>
        /// 그래프의 보이는 영역이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler<VisibleRangeChangedEventArgs>? VisibleRangeChanged;

        #endregion

        #region Constructor
        public GoTrendGraph()
        {
            Selectable = true;
            InitializeDefaults();
        }
        #endregion

        #region Member Variable
        private bool _isHovering;
        private TrendDataPoint? _hoveredPoint;
        private float _mx, _my;   // 마우스 위치 추적
        #endregion

        #region Override

        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            UpdateAnimation();
            DrawGraph(canvas);
            base.OnDraw(canvas);
        }
        #endregion

        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            _mx = x;
            _my = y;

            if (_isDraggingScrollHandle)
            {
                HandleScrollDrag(x);
                Invalidate?.Invoke();
                return;
            }

            if (_isDraggingGraph)
            {
                HandleGraphDrag(x);
                Invalidate?.Invoke();
                return;
            }

            // 스크롤바 영역에 있는지 확인
            if (ShowScrollBar && _scrollBarArea.Contains(x, y))
            {
                if (_scrollHandleX <= x && x <= _scrollHandleX + _scrollHandleWidth)
                {
                    // 커서 변경 로직 (플랫폼에 따라 다를 수 있음)
                }
                return;
            }

            // 데이터 포인트 위에 마우스가 있는지 확인
            var prevHoveredPoint = _hoveredPoint;
            _hoveredPoint = FindNearestDataPoint(x, y);
            _isHovering = _hoveredPoint != null;

            if (_hoveredPoint != prevHoveredPoint)
            {
                Invalidate?.Invoke(); // 다시 그리기 요청
            }

            base.OnMouseMove(x, y);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            if (button == GoMouseButton.Left)
            {
                // 스크롤 핸들 드래그 시작
                if (ShowScrollBar && _scrollBarArea.Contains(x, y))
                {
                    if (_scrollHandleX <= x && x <= _scrollHandleX + _scrollHandleWidth)
                    {
                        _isDraggingScrollHandle = true;
                        _dragStartX = x;
                        return;
                    }
                    else
                    {
                        // 스크롤바 클릭 시 핸들 이동
                        ScrollToPosition(x);
                        Invalidate?.Invoke();
                        return;
                    }
                }

                // 그래프 영역 드래그 시작
                if (EnableScrolling && _graphArea.Contains(x, y))
                {
                    _isDraggingGraph = true;
                    _dragStartX = x;
                    _dragStartMin = _visibleXMin;
                    _dragStartMax = _visibleXMax;
                    return;
                }
            }

            if (_hoveredPoint != null)
            {
                // 데이터 포인트 클릭 이벤트 발생
                DataPointSelected?.Invoke(this, new DataPointEventArgs
                {
                    Series = FindSeriesForDataPoint(_hoveredPoint),
                    DataPoint = _hoveredPoint
                });
            }

            base.OnMouseDown(x, y, button);
        }
        #endregion

        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            _isDraggingScrollHandle = false;
            _isDraggingGraph = false;
            base.OnMouseUp(x, y, button);
        }
        #endregion

        #region OnMouseWheel
        protected override void OnMouseWheel(float x, float y, float delta)
        {
            if (EnableMouseWheelZoom && _graphArea.Contains(x, y))
            {
                // 마우스 위치를 중심으로 줌
                ZoomAtPoint(x, delta);
                Invalidate?.Invoke();
            }

            base.OnMouseWheel(x, y, delta);
        }
        #endregion

        #endregion

        #region Drawing Methods

        private void DrawGraph(SKCanvas canvas)
        {
            var thm = GoTheme.Current;

            // 영역 계산
            if (_needsLayoutUpdate)
            {
                CalculateAreas();
                _needsLayoutUpdate = false;
            }

            // 배경 그리기
            DrawBackground(canvas, thm);

            // 그리드 및 축 그리기
            if (ShowGrid)
            {
                DrawGrid(canvas, thm);
            }
            DrawAxes(canvas, thm);

            // 모든 시리즈 그리기
            foreach (var series in _series)
            {
                DrawSeries(canvas, series, thm);
            }

            // 데이터 포인트 그리기
            if (ShowDataPoints)
            {
                foreach (var series in _series)
                {
                    DrawDataPoints(canvas, series, thm);
                }
            }

            // 레이블 그리기
            if (ShowLabels)
            {
                DrawLabels(canvas, thm);
            }

            // 레전드 그리기
            if (ShowLegend && _series.Count > 0)
            {
                DrawLegend(canvas, thm);
            }

            // 스크롤바 그리기
            if (ShowScrollBar && EnableScrolling)
            {
                DrawScrollBar(canvas, thm);
            }

            // 호버링된 데이터 포인트 그리기
            if (_isHovering && _hoveredPoint != null)
            {
                DrawHoveredPoint(canvas, _hoveredPoint, thm);
            }
        }

        private void DrawBackground(SKCanvas canvas, GoTheme thm)
        {
            if (BackgroundDraw)
            {
                // 배경 그리기
                _backgroundPaint.Color = thm.ToColor(BgColor);
                Util.DrawBox(canvas, Bounds, BorderOnly ? SKColors.Transparent : _backgroundPaint.Color, Round, thm.Corner);

                // 테두리 그리기
                if (!BorderOnly)
                {
                    _borderPaint.Color = thm.ToColor(BorderColor);
                    // canvas.DrawRoundRect(Util.FromRect(Bounds), thm.Corner, borderPaint);
                }
            }
        }

        private void DrawGrid(SKCanvas canvas, GoTheme thm)
        {
            _gridPaint.Color = thm.ToColor(GridColor);

            // Y축 그리드 라인
            float stepY = _graphArea.Height / GridLineCount;
            for (int i = 0; i <= GridLineCount; i++)
            {
                float y = _graphArea.Bottom - i * stepY;
                canvas.DrawLine(_graphArea.Left, y, _graphArea.Right, y, _gridPaint);
            }

            // X축 그리드 라인 (시간 간격)
            int xDivisions = 5; // X축 분할 개수
            float stepX = _graphArea.Width / xDivisions;
            for (int i = 0; i <= xDivisions; i++)
            {
                float x = _graphArea.Left + i * stepX;
                canvas.DrawLine(x, _graphArea.Top, x, _graphArea.Bottom, _gridPaint);
            }
        }

        private void DrawAxes(SKCanvas canvas, GoTheme thm)
        {
            _axisPaint.Color = thm.ToColor(AxisColor);

            // X축
            canvas.DrawLine(_graphArea.Left, _graphArea.Bottom, _graphArea.Right, _graphArea.Bottom, _axisPaint);

            // Y축
            canvas.DrawLine(_graphArea.Left, _graphArea.Top, _graphArea.Left, _graphArea.Bottom, _axisPaint);
        }

        private void DrawSeries(SKCanvas canvas, TrendSeries series, GoTheme thm)
        {
            if (series.DataPoints == null || series.DataPoints.Count < 2)
                return;

            var seriesColor = GetSeriesColor(series.Name, thm);
            _linePaint.Color = seriesColor;
            _linePaint.StrokeWidth = LineThickness;

            // 그라디언트 채우기 색상
            if (FillArea)
            {
                var fillColor = seriesColor.WithAlpha((byte)FillOpacity);
                _fillPaint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(_graphArea.Left, _graphArea.Top),
                    new SKPoint(_graphArea.Left, _graphArea.Bottom),
                    [fillColor, fillColor.WithAlpha(0)],
                    [0, 1],
                    SKShaderTileMode.Clamp
                );
            }

            // 캐시 초기화
            _cachedPoints.Clear();

            // 보이는 범위에 있는 데이터 포인트 필터링
            var visiblePoints = series.DataPoints
                .Where(p => p.Timestamp >= _visibleXMin && p.Timestamp <= _visibleXMax)
                .OrderBy(p => p.Timestamp)
                .ToList();

            // 보이는 범위 바로 바깥의 포인트를 추가하여 부드러운 라인 보장
            var firstVisible = visiblePoints.FirstOrDefault();
            var lastVisible = visiblePoints.LastOrDefault();

            if (firstVisible != null)
            {
                var outsideLeft = series.DataPoints
                    .Where(p => p.Timestamp < _visibleXMin)
                    .OrderByDescending(p => p.Timestamp)
                    .FirstOrDefault();

                if (outsideLeft != null)
                {
                    visiblePoints.Insert(0, outsideLeft);
                }
            }

            if (lastVisible != null)
            {
                var outsideRight = series.DataPoints
                    .Where(p => p.Timestamp > _visibleXMax)
                    .OrderBy(p => p.Timestamp)
                    .FirstOrDefault();

                if (outsideRight != null)
                {
                    visiblePoints.Add(outsideRight);
                }
            }

            if (visiblePoints.Count < 2) return;

            // 라인 경로 생성
            using SKPath linePath = new();
            using SKPath? fillPath = FillArea ? new SKPath() : null;

            if (FillArea)
            {
                fillPath!.MoveTo(_graphArea.Left, _graphArea.Bottom); // 시작점은 X축 위
            }

            bool first = true;
            float prevX = 0, prevY = 0;

            foreach (var point in visiblePoints)
            {
                float x = MapXToCanvas(point.Timestamp);
                float y = MapYToCanvas(point.Value);

                // 애니메이션 적용
                if (EnableAnimation && _animationProgress < 1.0f)
                {
                    if (first)
                    {
                        prevX = x;
                        prevY = y;
                    }
                    else
                    {
                        // 시작점에서 현재 진행 상태에 맞게 선형 보간
                        x = prevX + (x - prevX) * _animationProgress;
                        y = prevY + (y - prevY) * _animationProgress;
                    }
                }

                // 점 캐싱
                _cachedPoints.Add((x, y));

                if (first)
                {
                    linePath.MoveTo(x, y);
                    if (FillArea)
                    {
                        fillPath!.LineTo(x, y);
                    }
                    first = false;
                }
                else
                {
                    linePath.LineTo(x, y);
                    if (FillArea)
                    {
                        fillPath!.LineTo(x, y);
                    }
                }

                prevX = x;
                prevY = y;
            }

            // 채우기 경로 완성
            if (FillArea && !first)
            {
                fillPath!.LineTo(prevX, _graphArea.Bottom);
                fillPath.Close();

                if (EnableShadow)
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
                if (EnableShadow)
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
                if (point.Timestamp < _visibleXMin || point.Timestamp > _visibleXMax)
                    continue;

                float x = MapXToCanvas(point.Timestamp);
                float y = MapYToCanvas(point.Value);

                // 뷰포트 밖에 있으면 그리지 않음
                if (x < _graphArea.Left || x > _graphArea.Right)
                    continue;

                // 점 그리기
                canvas.DrawCircle(x, y, DataPointRadius, _pointPaint);

                // 테두리 그리기
                _pointPaint.Style = SKPaintStyle.Stroke;
                _pointPaint.StrokeWidth = 1;
                _pointPaint.Color = SKColors.White;
                canvas.DrawCircle(x, y, DataPointRadius, _pointPaint);

                // 스타일 복원
                _pointPaint.Style = SKPaintStyle.Fill;
                _pointPaint.Color = seriesColor;
            }
        }

        private void DrawLabels(SKCanvas canvas, GoTheme thm)
        {
            _textPaint.Color = thm.ToColor(TextColor);
            _textPaint.TextSize = FontSize;
            _textPaint.Typeface = SKTypeface.FromFamilyName(FontName);

            // Y축 레이블
            float stepY = _graphArea.Height / GridLineCount;
            double valueStep = (_displayYMax - _displayYMin) / GridLineCount;

            for (int i = 0; i <= GridLineCount; i++)
            {
                float y = _graphArea.Bottom - i * stepY;
                double value = _displayYMin + i * valueStep;
                string label = value.ToString(ValueFormat);

                // Y축 값 레이블
                _textPaint.TextAlign = SKTextAlign.Right;
                canvas.DrawText(label, _graphArea.Left - 5, y + FontSize / 3, _textPaint);
            }

            // X축 레이블 (시간 간격)
            _textPaint.TextAlign = SKTextAlign.Center;

            int xDivisions = 5;
            float stepX = _graphArea.Width / xDivisions;
            TimeSpan timeRange = _visibleXMax - _visibleXMin;
            TimeSpan timeStep = new(timeRange.Ticks / xDivisions);

            for (int i = 0; i <= xDivisions; i++)
            {
                float x = _graphArea.Left + i * stepX;
                DateTime time = _visibleXMin.Add(timeStep * i);
                string label = FormatDateTime(time);

                canvas.DrawText(label, x, _graphArea.Bottom + FontSize + 5, _textPaint);
            }

            // 축 제목
            if (!string.IsNullOrEmpty(YAxisTitle))
            {
                _textPaint.TextAlign = SKTextAlign.Center;

                // Y축 제목을 세로로 그리기 위해 캔버스 회전
                canvas.Save();
                canvas.RotateDegrees(270, _graphArea.Left - 30, _graphArea.Top + _graphArea.Height / 2);
                canvas.DrawText(YAxisTitle, _graphArea.Left - 30, _graphArea.Top + _graphArea.Height / 2 + FontSize / 3, _textPaint);
                canvas.Restore();
            }

            if (!string.IsNullOrEmpty(XAxisTitle))
            {
                _textPaint.TextAlign = SKTextAlign.Center;
                canvas.DrawText(XAxisTitle, _graphArea.Left + _graphArea.Width / 2, _graphArea.Bottom + FontSize * 2 + 10, _textPaint);
            }
        }

        private void DrawLegend(SKCanvas canvas, GoTheme thm)
        {
            if (_series.Count == 0) return;

            float legendItemHeight = FontSize + 10;
            float legendItemWidth = 100;
            float padding = 10;
            float markerSize = 10;

            // 레전드 배경
            _legendPaint.Color = thm.ToColor(BgColor).WithAlpha(200);
            canvas.DrawRoundRect(_legendArea, 5, 5, _legendPaint);

            _legendTextPaint.Color = thm.ToColor(TextColor);
            _legendTextPaint.TextSize = FontSize;
            _legendTextPaint.Typeface = SKTypeface.FromFamilyName(FontName);

            // 각 시리즈에 대한 레전드 아이템 그리기
            for (int i = 0; i < _series.Count; i++)
            {
                var series = _series[i];
                var itemY = _legendArea.Top + padding + i * legendItemHeight;
                var markerX = _legendArea.Left + padding;
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
            _scrollBarPaint.Color = thm.ToColor(ScrollBarColor);
            canvas.DrawRoundRect(_scrollBarArea, 4, 4, _scrollBarPaint);

            // 스크롤 핸들 위치 및 크기 계산
            TimeSpan totalRange = _dataXMax - _dataXMin;
            if (totalRange.TotalSeconds <= 0) return;

            float totalWidth = _scrollBarArea.Width;

            // 스크롤 핸들 너비 계산 (전체 데이터 중 현재 보이는 비율)
            TimeSpan visibleRange = _visibleXMax - _visibleXMin;
            float visibleRatio = (float)(visibleRange.TotalSeconds / totalRange.TotalSeconds);
            _scrollHandleWidth = Math.Max(totalWidth * visibleRatio, 30); // 최소 핸들 너비 보장

            // 핸들 위치 계산
            TimeSpan startOffset = _visibleXMin - _dataXMin;
            float positionRatio = (float)(startOffset.TotalSeconds / totalRange.TotalSeconds);
            _scrollHandleX = _scrollBarArea.Left + positionRatio * (totalWidth - _scrollHandleWidth);

            // 스크롤 핸들 그리기
            _scrollHandlePaint.Color = thm.ToColor(ScrollHandleColor);
            canvas.DrawRoundRect(_scrollHandleX, _scrollBarArea.Top, _scrollHandleWidth, _scrollBarArea.Height, 4, 4, _scrollHandlePaint);
        }

        private void DrawHoveredPoint(SKCanvas canvas, TrendDataPoint point, GoTheme thm)
        {
            var series = FindSeriesForDataPoint(point);
            if (series == null) return;

            float x = MapXToCanvas(point.Timestamp);
            float y = MapYToCanvas(point.Value);

            // 뷰포트 밖에 있으면 그리지 않음
            if (x < _graphArea.Left || x > _graphArea.Right)
                return;

            // 강조 원 그리기
            _pointPaint.Color = GetSeriesColor(series.Name, thm);
            canvas.DrawCircle(x, y, DataPointRadius * 2, _pointPaint);

            // 테두리 그리기
            _pointPaint.Style = SKPaintStyle.Stroke;
            _pointPaint.StrokeWidth = 2;
            _pointPaint.Color = SKColors.White;
            canvas.DrawCircle(x, y, DataPointRadius * 2, _pointPaint);

            // 스타일 복원
            _pointPaint.Style = SKPaintStyle.Fill;

            // 툴팁 배경
            string tooltip = $"{series.Name}: {point.Value.ToString(ValueFormat)}\n{FormatDateTime(point.Timestamp)}";

            _textPaint.TextSize = FontSize;
            _textPaint.Color = thm.ToColor(TextColor);

            // 툴팁 크기 계산
            float tooltipWidth = 0;
            float tooltipHeight = FontSize * 2 + 15;  // 두 줄 + 여백

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

            if (tooltipX + tooltipWidth > Bounds.Right)
                tooltipX = x - tooltipWidth - 10;

            if (tooltipY < Bounds.Top)
                tooltipY = y + 10;

            // 툴팁 배경 그리기
            SKRect tooltipRect = new(tooltipX, tooltipY, tooltipX + tooltipWidth, tooltipY + tooltipHeight);
            _legendPaint.Color = thm.ToColor(BgColor).WithAlpha(230);
            canvas.DrawRoundRect(tooltipRect, 5, 5, _legendPaint);

            // 툴팁 테두리
            _borderPaint.Color = thm.ToColor(BorderColor);
            canvas.DrawRoundRect(tooltipRect, 5, 5, _borderPaint);

            // 툴팁 텍스트 그리기
            _textPaint.TextAlign = SKTextAlign.Left;

            for (int i = 0; i < lines.Length; i++)
            {
                canvas.DrawText(lines[i], tooltipX + 10, tooltipY + FontSize + i * (FontSize + 5) + 5, _textPaint);
            }
        }

        #endregion

        #region Scrolling Methods

        private void HandleScrollDrag(float x)
        {
            // 스크롤바 드래그 처리
            float delta = x - _dragStartX;
            float totalWidth = _scrollBarArea.Width - _scrollHandleWidth;
            if (totalWidth <= 0) return;

            float newX = Math.Clamp(_scrollHandleX + delta, _scrollBarArea.Left, _scrollBarArea.Right - _scrollHandleWidth);
            float positionRatio = (newX - _scrollBarArea.Left) / totalWidth;

            // 위치 비율을 기반으로 보이는 시간 범위 조정
            TimeSpan totalRange = _dataXMax - _dataXMin;
            TimeSpan visibleRange = _visibleXMax - _visibleXMin;

            DateTime newMin = _dataXMin.AddTicks((long)(totalRange.Ticks * positionRatio));
            DateTime newMax = newMin.AddTicks(visibleRange.Ticks);

            // 범위 초과 방지
            if (newMax > _dataXMax)
            {
                newMax = _dataXMax;
                newMin = newMax.AddTicks(-visibleRange.Ticks);
            }

            UpdateVisibleRange(newMin, newMax);
            _dragStartX = x;
        }

        private void HandleGraphDrag(float x)
        {
            float delta = _dragStartX - x;
            if (Math.Abs(delta) < 1) return;

            // 픽셀 변화량을 시간 변화량으로 변환
            TimeSpan timePerPixel = new((int)((_visibleXMax - _visibleXMin).Ticks / _graphArea.Width));
            TimeSpan timeChange = new((long)(timePerPixel.Ticks * delta));

            DateTime newMin = _dragStartMin.Add(timeChange);
            DateTime newMax = _dragStartMax.Add(timeChange);

            // 범위 초과 방지
            if (newMin < _dataXMin)
            {
                newMin = _dataXMin;
                newMax = newMin.AddTicks((_visibleXMax - _visibleXMin).Ticks);
            }
            else if (newMax > _dataXMax)
            {
                newMax = _dataXMax;
                newMin = newMax.AddTicks(-(_visibleXMax - _visibleXMin).Ticks);
            }

            UpdateVisibleRange(newMin, newMax);
        }

        private void ZoomAtPoint(float x, float delta)
        {
            // 현재 보이는 시간 범위
            TimeSpan currentRange = _visibleXMax - _visibleXMin;

            // 확대/축소 비율 계산 (델타가 양수면 확대, 음수면 축소)
            float zoomFactor = delta > 0 ? 0.8f : 1.25f;
            double newRangeSecs = currentRange.TotalSeconds * zoomFactor;

            // 최소/최대 범위 제한
            newRangeSecs = Math.Clamp(newRangeSecs, MinTimeRange, MaxTimeRange);

            // 마우스 위치의 시간 계산
            float relativeX = (x - _graphArea.Left) / _graphArea.Width;
            TimeSpan offsetFromMin = new((long)(currentRange.Ticks * relativeX));
            DateTime pivotTime = _visibleXMin.Add(offsetFromMin);

            // 새 범위 계산
            TimeSpan halfNewRange = new((long)(newRangeSecs * TimeSpan.TicksPerSecond / 2));
            DateTime newMin = pivotTime.AddTicks(-halfNewRange.Ticks);
            DateTime newMax = pivotTime.AddTicks(halfNewRange.Ticks);

            // 범위 초과 방지
            if (newMin < _dataXMin)
            {
                newMin = _dataXMin;
            }
            if (newMax > _dataXMax)
            {
                newMax = _dataXMax;
            }

            // 범위가 너무 좁아지면 조정
            if ((newMax - newMin).TotalSeconds < MinTimeRange)
            {
                // 축소하려는 방향에 따라 조정
                if (pivotTime - newMin < newMax - pivotTime)
                {
                    // 왼쪽이 더 가까우면 오른쪽 조정
                    newMax = newMin.AddSeconds(MinTimeRange);
                }
                else
                {
                    // 오른쪽이 더 가까우면 왼쪽 조정
                    newMin = newMax.AddSeconds(-MinTimeRange);
                }
            }

            UpdateVisibleRange(newMin, newMax);
        }

        private void ScrollToPosition(float x)
        {
            float handleCenter = Math.Clamp(x, _scrollBarArea.Left + _scrollHandleWidth / 2,
                _scrollBarArea.Right - _scrollHandleWidth / 2);

            float positionRatio = (handleCenter - _scrollBarArea.Left - _scrollHandleWidth / 2) /
                (_scrollBarArea.Width - _scrollHandleWidth);

            // 위치 비율을 기반으로 보이는 시간 범위 조정
            TimeSpan totalRange = _dataXMax - _dataXMin;
            TimeSpan visibleRange = _visibleXMax - _visibleXMin;

            DateTime newMin = _dataXMin.AddTicks((long)(totalRange.Ticks * positionRatio));
            DateTime newMax = newMin.AddTicks(visibleRange.Ticks);

            // 범위 초과 방지
            if (newMax > _dataXMax)
            {
                newMax = _dataXMax;
                newMin = newMax.AddTicks(-visibleRange.Ticks);
            }

            UpdateVisibleRange(newMin, newMax);
        }

        private void UpdateVisibleRange(DateTime newMin, DateTime newMax)
        {
            // 범위가 변경되었는지 확인
            if (newMin == _visibleXMin && newMax == _visibleXMax)
                return;

            _visibleXMin = newMin;
            _visibleXMax = newMax;

            // 이벤트 발생
            VisibleRangeChanged?.Invoke(this, new VisibleRangeChangedEventArgs
            {
                VisibleMinimum = _visibleXMin,
                VisibleMaximum = _visibleXMax
            });

            // 레이아웃 업데이트 필요
            _needsLayoutUpdate = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 트렌드 시리즈를 추가합니다.
        /// </summary>
        public void AddSeries(TrendSeries series)
        {
            if (series == null) throw new ArgumentNullException(nameof(series));
            if (string.IsNullOrEmpty(series.Name)) throw new ArgumentException("Series name cannot be empty", nameof(series));

            // 기존 시리즈가 있는지 확인
            var existingSeries = _series.FirstOrDefault(s => s.Name == series.Name);
            if (existingSeries != null)
            {
                // 기존 시리즈 업데이트
                existingSeries.DataPoints = series.DataPoints;
                existingSeries.Color = series.Color;
            }
            else
            {
                // 새 시리즈 추가
                _series.Add(series);

                // 색상이 지정되지 않은 경우 자동 할당
                if (string.IsNullOrEmpty(series.Color))
                {
                    series.Color = GetNextColor();
                }
            }

            // 캐싱된 색상 업데이트
            UpdateSeriesColors();

            // 데이터 범위 업데이트
            UpdateDataRange();

            // 애니메이션 시작
            if (EnableAnimation)
            {
                _animationProgress = 0f;
                _animationStartTime = DateTime.Now;
            }

            // 이벤트 발생 및 다시 그리기
            DataChanged?.Invoke(this, EventArgs.Empty);
            _needsLayoutUpdate = true;
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 이름으로 시리즈를 제거합니다.
        /// </summary>
        public bool RemoveSeries(string seriesName)
        {
            var series = _series.FirstOrDefault(s => s.Name == seriesName);
            if (series != null)
            {
                _series.Remove(series);
                UpdateDataRange();
                DataChanged?.Invoke(this, EventArgs.Empty);
                Invalidate?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 모든 시리즈를 제거합니다.
        /// </summary>
        public void ClearSeries()
        {
            _series.Clear();
            DataChanged?.Invoke(this, EventArgs.Empty);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 특정 시리즈의 데이터를 업데이트합니다.
        /// </summary>
        public bool UpdateSeriesData(string seriesName, List<TrendDataPoint> newData)
        {
            var series = _series.FirstOrDefault(s => s.Name == seriesName);
            if (series != null)
            {
                series.DataPoints = newData;
                UpdateDataRange();

                // 애니메이션 시작
                if (EnableAnimation)
                {
                    _animationProgress = 0f;
                    _animationStartTime = DateTime.Now;
                }

                DataChanged?.Invoke(this, EventArgs.Empty);
                Invalidate?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 데이터 X축 범위를 설정합니다. (전체 데이터 범위)
        /// </summary>
        public void SetDataXRange(DateTime min, DateTime max)
        {
            if (min >= max) throw new ArgumentException("Min must be less than max");
            _dataXMin = min;
            _dataXMax = max;

            // 보이는 범위가 데이터 범위를 벗어나지 않도록 조정
            AdjustVisibleRange();
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 보이는 X축 범위를 설정합니다.
        /// </summary>
        public void SetVisibleXRange(DateTime min, DateTime max)
        {
            if (min >= max) throw new ArgumentException("Min must be less than max");

            // 시간 범위가 최소/최대 범위 내에 있는지 확인
            TimeSpan range = max - min;
            if (range.TotalSeconds < MinTimeRange)
            {
                max = min.AddSeconds(MinTimeRange);
            }
            else if (range.TotalSeconds > MaxTimeRange)
            {
                max = min.AddSeconds(MaxTimeRange);
            }

            // 데이터 범위를 벗어나지 않도록 조정
            if (min < _dataXMin)
            {
                min = _dataXMin;
                max = min.Add(range);
            }
            if (max > _dataXMax)
            {
                max = _dataXMax;
                min = max.Subtract(range);
            }

            UpdateVisibleRange(min, max);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// Y축 범위를 수동으로 설정합니다. AutoScale = false로 설정됩니다.
        /// </summary>
        public void SetYRange(double min, double max)
        {
            if (min >= max) throw new ArgumentException("Min must be less than max");
            YAxisMin = min;
            YAxisMax = max;
            AutoScale = false;
            UpdateDataRange();
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 자동 스케일링을 활성화합니다.
        /// </summary>
        public void EnableAutoScale()
        {
            AutoScale = true;
            UpdateDataRange();
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 전체 데이터 범위를 표시합니다.
        /// </summary>
        public void ShowFullRange()
        {
            SetVisibleXRange(_dataXMin, _dataXMax);
        }

        /// <summary>
        /// 주어진 시간만큼 스크롤합니다. (양수: 오른쪽, 음수: 왼쪽)
        /// </summary>
        public void ScrollByTime(TimeSpan amount)
        {
            DateTime newMin = _visibleXMin.Add(amount);
            DateTime newMax = _visibleXMax.Add(amount);

            // 범위 초과 방지
            if (newMin < _dataXMin)
            {
                newMin = _dataXMin;
                newMax = newMin.AddTicks((_visibleXMax - _visibleXMin).Ticks);
            }
            else if (newMax > _dataXMax)
            {
                newMax = _dataXMax;
                newMin = newMax.AddTicks(-(_visibleXMax - _visibleXMin).Ticks);
            }

            UpdateVisibleRange(newMin, newMax);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 일정 비율로 확대/축소합니다. (factor > 1: 축소, factor < 1: 확대)
        /// </summary>
        public void Zoom(float factor)
        {
            // 현재 보이는 시간 범위
            TimeSpan currentRange = _visibleXMax - _visibleXMin;
            TimeSpan newRange = new((long)(currentRange.Ticks * factor));

            // 최소/최대 범위 제한
            double newRangeSecs = newRange.TotalSeconds;
            newRangeSecs = Math.Clamp(newRangeSecs, MinTimeRange, MaxTimeRange);
            newRange = TimeSpan.FromSeconds(newRangeSecs);

            // 중앙 기준 확대/축소
            DateTime center = _visibleXMin.AddTicks(currentRange.Ticks / 2);
            DateTime newMin = center.AddTicks(-newRange.Ticks / 2);
            DateTime newMax = center.AddTicks(newRange.Ticks / 2);

            // 범위 초과 방지
            if (newMin < _dataXMin)
            {
                newMin = _dataXMin;
                newMax = newMin.AddTicks(newRange.Ticks);
            }
            if (newMax > _dataXMax)
            {
                newMax = _dataXMax;
                newMin = newMax.AddTicks(-newRange.Ticks);
            }

            UpdateVisibleRange(newMin, newMax);
            Invalidate?.Invoke();
        }

        #endregion

        #region Helper Methods

        private void InitializeDefaults()
        {
            _dataXMin = DateTime.Now.AddDays(-30);
            _dataXMax = DateTime.Now;
            _visibleXMin = DateTime.Now.AddDays(-7);
            _visibleXMax = DateTime.Now;

            // 테스트 데이터 생성
            GenerateSampleData();
        }

        private void GenerateSampleData()
        {
            // 예시 데이터 생성
            var now = DateTime.Now;
            var series1 = new TrendSeries
            {
                Name = "Temperature",
                Color = "danger",
                DataPoints = []
            };

            var series2 = new TrendSeries
            {
                Name = "Humidity",
                Color = "primary",
                DataPoints = []
            };

            var random = new Random(42);
            double value1 = 25;
            double value2 = 50;

            for (int i = 0; i < 100; i++) // 더 많은 데이터 포인트 생성
            {
                var time = now.AddHours(-i * 8);

                // 랜덤 변동 추가
                value1 += (random.NextDouble() - 0.5) * 3;
                value2 += (random.NextDouble() - 0.5) * 5;

                // 범위 제한
                value1 = Math.Max(15, Math.Min(35, value1));
                value2 = Math.Max(30, Math.Min(70, value2));

                series1.DataPoints.Add(new TrendDataPoint { Timestamp = time, Value = value1 });
                series2.DataPoints.Add(new TrendDataPoint { Timestamp = time, Value = value2 });
            }

            // 역순으로 정렬
            series1.DataPoints = [.. series1.DataPoints.OrderBy(p => p.Timestamp)];
            series2.DataPoints = [.. series2.DataPoints.OrderBy(p => p.Timestamp)];

            _series.Add(series1);
            _series.Add(series2);

            UpdateSeriesColors();
            UpdateDataRange();
        }

        private void CalculateAreas()
        {
            float xAxisHeight = FontSize * 2 + 15;  // X축 레이블 및 제목 공간
            float yAxisWidth = 60;  // Y축 레이블 및 제목 공간
            float legendWidth = 120;  // 레전드 영역 너비
            float legendHeight = _series.Count * (FontSize + 10) + 20;  // 레전드 영역 높이
            float scrollBarBottom = Bounds.Bottom - Padding;
            float scrollBarTop = scrollBarBottom - ScrollBarHeight;

            // 스크롤바 영역 계산
            if (ShowScrollBar && EnableScrolling)
            {
                _scrollBarArea = new SKRect(
                    Bounds.Left + yAxisWidth + Padding,
                    scrollBarTop,
                    Bounds.Right - Padding - (ShowLegend ? legendWidth + Padding : 0),
                    scrollBarBottom
                );
            }

            // 그래프 영역 계산
            _graphArea = new SKRect(
                Bounds.Left + yAxisWidth + Padding,
                Bounds.Top + Padding,
                Bounds.Right - Padding - (ShowLegend ? legendWidth + Padding : 0),
                scrollBarTop - xAxisHeight - (ShowScrollBar ? Padding : 0)
            );

            // 레전드 영역 계산
            if (ShowLegend)
            {
                _legendArea = new SKRect(
                    Bounds.Right - legendWidth - Padding / 2,
                    Bounds.Top + Padding,
                    Bounds.Right - Padding / 2,
                    Bounds.Top + Padding + legendHeight
                );
            }
        }

        private void UpdateSeriesColors()
        {
            _seriesColors.Clear();
            // 색상은 필요할 때 테마에서 가져오므로 여기서는 아무 작업도 하지 않음
        }

        private void UpdateDataRange()
        {
            if (_series.Count == 0 || _series.All(s => s.DataPoints == null || s.DataPoints.Count == 0))
            {
                _actualYMin = 0;
                _actualYMax = 100;
                _displayYMin = 0;
                _displayYMax = 100;
                _dataXMin = DateTime.Now.AddDays(-7);
                _dataXMax = DateTime.Now;
                return;
            }

            // 데이터 X 범위 계산
            DateTime minDate = DateTime.MaxValue;
            DateTime maxDate = DateTime.MinValue;

            // 데이터 Y 최소/최대값 계산
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            foreach (var series in _series)
            {
                if (series.DataPoints == null || series.DataPoints.Count == 0)
                    continue;

                var timestamps = series.DataPoints.Select(p => p.Timestamp);
                minDate = timestamps.Min() < minDate ? timestamps.Min() : minDate;
                maxDate = timestamps.Max() > maxDate ? timestamps.Max() : maxDate;

                minValue = Math.Min(minValue, series.DataPoints.Min(p => p.Value));
                maxValue = Math.Max(maxValue, series.DataPoints.Max(p => p.Value));
            }

            _dataXMin = minDate;
            _dataXMax = maxDate;
            _actualYMin = minValue;
            _actualYMax = maxValue;

            // 초기 보이는 범위 설정 (전체 데이터 중에서 마지막 7일)
            if (_visibleXMin == DateTime.MinValue || _visibleXMax == DateTime.MaxValue)
            {
                // 전체 데이터 범위가 7일보다 작으면 전체 표시
                if ((maxDate - minDate).TotalDays <= 7)
                {
                    _visibleXMin = minDate;
                    _visibleXMax = maxDate;
                }
                else
                {
                    // 마지막 7일 표시
                    _visibleXMax = maxDate;
                    _visibleXMin = maxDate.AddDays(-7);
                }
            }
            else
            {
                // 보이는 범위가 데이터 범위를 벗어나지 않도록 조정
                AdjustVisibleRange();
            }

            // 자동 스케일링인 경우 Y 최소/최대값에 여유 공간 추가
            if (AutoScale)
            {
                double range = maxValue - minValue;

                // 범위가 너무 작으면 기본값 사용
                if (range < 0.001)
                {
                    range = 100;
                    minValue = Math.Max(0, minValue - 50);
                }

                // 10% 여유 공간 추가
                double padding = range * 0.1;
                _displayYMin = Math.Max(minValue - padding, 0); // 음수 방지
                _displayYMax = maxValue + padding;

                // 눈금 값을 깔끔하게 조정
                AdjustYAxisRange();
            }
            else
            {
                _displayYMin = YAxisMin;
                _displayYMax = YAxisMax;
            }
        }

        private void AdjustVisibleRange()
        {
            // 보이는 범위가 데이터 범위 내에 있도록 조정
            if (_visibleXMin < _dataXMin)
            {
                TimeSpan offset = _dataXMin - _visibleXMin;
                _visibleXMin = _dataXMin;
                _visibleXMax = _visibleXMax.Add(offset);
            }

            if (_visibleXMax > _dataXMax)
            {
                TimeSpan offset = _visibleXMax - _dataXMax;
                _visibleXMax = _dataXMax;
                _visibleXMin = _visibleXMin.Subtract(offset);
            }

            // 보이는 범위가 데이터 범위보다 크면 데이터 범위로 설정
            if (_visibleXMax - _visibleXMin > _dataXMax - _dataXMin)
            {
                _visibleXMin = _dataXMin;
                _visibleXMax = _dataXMax;
            }
        }

        private void AdjustYAxisRange()
        {
            // 눈금 간격 계산
            double range = _displayYMax - _displayYMin;
            double step = CalculateNiceStep(range / GridLineCount);

            // 최소값을 내려서 눈금에 맞춤
            _displayYMin = Math.Floor(_displayYMin / step) * step;

            // 최대값을 올려서 눈금에 맞춤
            _displayYMax = Math.Ceiling(_displayYMax / step) * step;
        }

        private static double CalculateNiceStep(double roughStep)
        {
            // 1, 2, 5, 10, 20, 50, ... 형태의 눈금 간격 계산
            double exponent = Math.Floor(Math.Log10(roughStep));
            double fraction = roughStep / Math.Pow(10, exponent);

            double niceFraction;
            if (fraction < 1.5)
                niceFraction = 1;
            else if (fraction < 3)
                niceFraction = 2;
            else if (fraction < 7)
                niceFraction = 5;
            else
                niceFraction = 10;

            return niceFraction * Math.Pow(10, exponent);
        }

        private void UpdateAnimation()
        {
            if (!EnableAnimation || _animationProgress >= 1.0f)
                return;

            DateTime now = DateTime.Now;
            double elapsed = (now - _animationStartTime).TotalMilliseconds;

            _animationProgress = Math.Min(1.0f, (float)(elapsed / AnimationDuration));

            if (_animationProgress < 1.0f)
            {
                // 다음 프레임에도 다시 그리기
                Invalidate?.Invoke();
            }
        }

        private float MapXToCanvas(DateTime time)
        {
            if (_visibleXMax == _visibleXMin)
                return _graphArea.Left;

            double normalizedX = (time - _visibleXMin).TotalMilliseconds / (_visibleXMax - _visibleXMin).TotalMilliseconds;
            return (float)(_graphArea.Left + normalizedX * _graphArea.Width);
        }

        private float MapYToCanvas(double value)
        {
            if (_displayYMax == _displayYMin)
                return _graphArea.Bottom;

            double normalizedY = (value - _displayYMin) / (_displayYMax - _displayYMin);
            return (float)(_graphArea.Bottom - normalizedY * _graphArea.Height);
        }

        private string FormatDateTime(DateTime time)
        {
            // 적절한 날짜/시간 포맷 선택
            TimeSpan range = _visibleXMax - _visibleXMin;

            if (range.TotalDays > 365)
                return time.ToString("yyyy-MM");
            else if (range.TotalDays > 30)
                return time.ToString("MM-dd");
            else if (range.TotalDays > 1)
                return time.ToString("MM-dd HH:mm");
            else
                return time.ToString("HH:mm");
        }

        private string GetNextColor()
        {
            string[] defaultColors = ["primary", "success", "warning", "danger", "info"];
            int index = _series.Count % defaultColors.Length;
            return defaultColors[index];
        }

        private SKColor GetSeriesColor(string seriesName, GoTheme thm)
        {
            var series = _series.FirstOrDefault(s => s.Name == seriesName);
            if (series == null || string.IsNullOrEmpty(series.Color))
                return thm.ToColor("Text");

            return thm.ToColor(series.Color);
        }

        private TrendDataPoint? FindNearestDataPoint(float x, float y)
        {
            const float HitDistance = 20f; // 픽셀 단위 검색 거리

            TrendDataPoint? closestPoint = null;
            float minDistance = float.MaxValue;

            foreach (var series in _series)
            {
                if (series.DataPoints == null) continue;

                // 보이는 범위 내의 포인트만 검사
                foreach (var point in series.DataPoints.Where(p => p.Timestamp >= _visibleXMin && p.Timestamp <= _visibleXMax))
                {
                    float px = MapXToCanvas(point.Timestamp);
                    float py = MapYToCanvas(point.Value);

                    float distance = (float)Math.Sqrt(Math.Pow(px - x, 2) + Math.Pow(py - y, 2));

                    if (distance < HitDistance && distance < minDistance)
                    {
                        minDistance = distance;
                        closestPoint = point;
                    }
                }
            }

            return closestPoint;
        }

        private TrendSeries? FindSeriesForDataPoint(TrendDataPoint point)
        {
            foreach (var series in _series)
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

        #region IDisposable

        public void Dispose()
        {
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
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}