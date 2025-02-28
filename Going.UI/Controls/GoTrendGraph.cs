using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;

namespace Going.UI.Controls
{
    /// <summary>
    /// 트렌드 그래프 컨트롤 - 시간에 따른 데이터 변화를 시각적으로 표현하는 컴포넌트
    /// </summary>
    public class GoTrendGraph : GoControl
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

        #region 시리즈 데이터
        private List<TrendSeries> _series = new List<TrendSeries>();
        public IReadOnlyList<TrendSeries> Series => _series.AsReadOnly();
        #endregion

        #region 내부 상태
        private SKRect graphArea;
        private SKRect legendArea;
        private bool needsLayoutUpdate = true;
        private Dictionary<string, SKColor> seriesColors = new Dictionary<string, SKColor>();
        private double actualYMin;
        private double actualYMax;
        private double displayYMin;
        private double displayYMax;
        private DateTime xMin = DateTime.MinValue;
        private DateTime xMax = DateTime.MaxValue;
        private float animationProgress = 1.0f; // 0.0 ~ 1.0
        private DateTime animationStartTime;
        public float Padding { get; set; } = 10f; // 패딩
        #endregion

        #region 성능 최적화를 위한 페인트 객체 재사용
        private readonly SKPaint backgroundPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint borderPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
        private readonly SKPaint axisPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
        private readonly SKPaint gridPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1, PathEffect = SKPathEffect.CreateDash([4, 4], 0) };
        private readonly SKPaint linePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        private readonly SKPaint fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint pointPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint textPaint = new SKPaint { IsAntialias = true };
        private readonly SKPaint legendPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPaint legendTextPaint = new SKPaint { IsAntialias = true };
        #endregion

        #endregion

        #region Event

        /// <summary>
        /// 시리즈 데이터가 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? DataChanged;

        /// <summary>
        /// 사용자가 데이터 포인트를 클릭했을 때 발생합니다.
        /// </summary>
        public event EventHandler<DataPointEventArgs>? DataPointSelected;

        #endregion

        #region Constructor
        public GoTrendGraph()
        {
            Selectable = true;
            InitializeDefaults();
        }
        #endregion

        #region Member Variable
        private bool isHovering;
        private TrendDataPoint? hoveredPoint;
        private float mx, my;   // 마우스 위치 추적
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
            mx = x;
            my = y;

            // 데이터 포인트 위에 마우스가 있는지 확인
            var prevHoveredPoint = hoveredPoint;
            hoveredPoint = FindNearestDataPoint(x, y);
            isHovering = hoveredPoint != null;

            if (hoveredPoint != prevHoveredPoint)
            {
                Invalidate?.Invoke(); // 다시 그리기 요청
            }

            base.OnMouseMove(x, y);
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            if (hoveredPoint != null)
            {
                // 데이터 포인트 클릭 이벤트 발생
                DataPointSelected?.Invoke(this, new DataPointEventArgs
                {
                    Series = FindSeriesForDataPoint(hoveredPoint),
                    DataPoint = hoveredPoint
                });
            }

            base.OnMouseDown(x, y, button);
        }
        #endregion

        #endregion

        #region Methods

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
                animationProgress = 0f;
                animationStartTime = DateTime.Now;
            }

            // 이벤트 발생 및 다시 그리기
            DataChanged?.Invoke(this, EventArgs.Empty);
            needsLayoutUpdate = true;
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
                    animationProgress = 0f;
                    animationStartTime = DateTime.Now;
                }

                DataChanged?.Invoke(this, EventArgs.Empty);
                Invalidate?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// X축 범위를 설정합니다.
        /// </summary>
        public void SetXRange(DateTime min, DateTime max)
        {
            if (min >= max) throw new ArgumentException("Min must be less than max");
            xMin = min;
            xMax = max;
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

        #endregion

        #region Private Drawing Methods

        private void DrawGraph(SKCanvas canvas)
        {
            var thm = GoTheme.Current;

            // 영역 계산
            if (needsLayoutUpdate)
            {
                CalculateAreas();
                needsLayoutUpdate = false;
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

            // 호버링된 데이터 포인트 그리기
            if (isHovering && hoveredPoint != null)
            {
                DrawHoveredPoint(canvas, hoveredPoint, thm);
            }
        }

        private void DrawBackground(SKCanvas canvas, GoTheme thm)
        {
            if (BackgroundDraw)
            {
                // 배경 그리기
                backgroundPaint.Color = thm.ToColor(BgColor);
                Util.DrawBox(canvas, Bounds, BorderOnly ? SKColors.Transparent : backgroundPaint.Color, Round, thm.Corner);

                // 테두리 그리기
                if (!BorderOnly)
                {
                    borderPaint.Color = thm.ToColor(BorderColor);
                    // canvas.DrawRoundRect(Util.FromRect(Bounds), thm.Corner, borderPaint);
                }
            }
        }

        private void DrawGrid(SKCanvas canvas, GoTheme thm)
        {
            gridPaint.Color = thm.ToColor(GridColor);

            // Y축 그리드 라인
            float stepY = graphArea.Height / GridLineCount;
            for (int i = 0; i <= GridLineCount; i++)
            {
                float y = graphArea.Bottom - i * stepY;
                canvas.DrawLine(graphArea.Left, y, graphArea.Right, y, gridPaint);
            }

            // X축 그리드 라인 (시간 간격)
            int xDivisions = 5; // X축 분할 개수
            float stepX = graphArea.Width / xDivisions;
            for (int i = 0; i <= xDivisions; i++)
            {
                float x = graphArea.Left + i * stepX;
                canvas.DrawLine(x, graphArea.Top, x, graphArea.Bottom, gridPaint);
            }
        }

        private void DrawAxes(SKCanvas canvas, GoTheme thm)
        {
            axisPaint.Color = thm.ToColor(AxisColor);

            // X축
            canvas.DrawLine(graphArea.Left, graphArea.Bottom, graphArea.Right, graphArea.Bottom, axisPaint);

            // Y축
            canvas.DrawLine(graphArea.Left, graphArea.Top, graphArea.Left, graphArea.Bottom, axisPaint);
        }

        private void DrawSeries(SKCanvas canvas, TrendSeries series, GoTheme thm)
        {
            if (series.DataPoints == null || series.DataPoints.Count < 2)
                return;

            var seriesColor = GetSeriesColor(series.Name, thm);
            linePaint.Color = seriesColor;
            linePaint.StrokeWidth = LineThickness;

            // 그라디언트 채우기 색상
            if (FillArea)
            {
                var fillColor = seriesColor.WithAlpha((byte)FillOpacity);
                fillPaint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(graphArea.Left, graphArea.Top),
                    new SKPoint(graphArea.Left, graphArea.Bottom),
                    new SKColor[] { fillColor, fillColor.WithAlpha(0) },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Clamp
                );
            }

            // 라인 경로 생성
            SKPath linePath = new SKPath();
            SKPath fillPath = null;

            if (FillArea)
            {
                fillPath = new SKPath();
                fillPath.MoveTo(graphArea.Left, graphArea.Bottom); // 시작점은 X축 위
            }

            bool first = true;
            float prevX = 0, prevY = 0;

            foreach (var point in series.DataPoints.OrderBy(p => p.Timestamp))
            {
                if (point.Timestamp < xMin || point.Timestamp > xMax)
                    continue;

                float x = MapXToCanvas(point.Timestamp);
                float y = MapYToCanvas(point.Value);

                // 애니메이션 적용
                if (EnableAnimation && animationProgress < 1.0f)
                {
                    if (first)
                    {
                        prevX = x;
                        prevY = y;
                    }
                    else
                    {
                        // 시작점에서 현재 진행 상태에 맞게 선형 보간
                        x = prevX + (x - prevX) * animationProgress;
                        y = prevY + (y - prevY) * animationProgress;
                    }
                }

                if (first)
                {
                    linePath.MoveTo(x, y);
                    if (FillArea)
                    {
                        fillPath.LineTo(x, y);
                    }
                    first = false;
                }
                else
                {
                    linePath.LineTo(x, y);
                    if (FillArea)
                    {
                        fillPath.LineTo(x, y);
                    }
                }

                prevX = x;
                prevY = y;
            }

            // 채우기 경로 완성
            if (FillArea && !first)
            {
                fillPath.LineTo(prevX, graphArea.Bottom);
                fillPath.Close();

                if (EnableShadow)
                {
                    using var shadow = SKImageFilter.CreateDropShadow(2, 2, 5, 5, new SKColor(0, 0, 0, 50));
                    fillPaint.ImageFilter = shadow;
                }

                canvas.DrawPath(fillPath, fillPaint);
                fillPaint.ImageFilter = null;
            }

            // 선 그리기
            if (!first)
            {
                if (EnableShadow)
                {
                    using var shadow = SKImageFilter.CreateDropShadow(1, 1, 3, 3, new SKColor(0, 0, 0, 80));
                    linePaint.ImageFilter = shadow;
                }

                canvas.DrawPath(linePath, linePaint);
                linePaint.ImageFilter = null;
            }

            // 정리
            linePath.Dispose();
            fillPath?.Dispose();
        }

        private void DrawDataPoints(SKCanvas canvas, TrendSeries series, GoTheme thm)
        {
            if (series.DataPoints == null)
                return;

            var seriesColor = GetSeriesColor(series.Name, thm);
            pointPaint.Color = seriesColor;

            foreach (var point in series.DataPoints)
            {
                if (point.Timestamp < xMin || point.Timestamp > xMax)
                    continue;

                float x = MapXToCanvas(point.Timestamp);
                float y = MapYToCanvas(point.Value);

                // 점 그리기
                canvas.DrawCircle(x, y, DataPointRadius, pointPaint);

                // 테두리 그리기
                pointPaint.Style = SKPaintStyle.Stroke;
                pointPaint.StrokeWidth = 1;
                pointPaint.Color = SKColors.White;
                canvas.DrawCircle(x, y, DataPointRadius, pointPaint);

                // 스타일 복원
                pointPaint.Style = SKPaintStyle.Fill;
                pointPaint.Color = seriesColor;
            }
        }

        private void DrawLabels(SKCanvas canvas, GoTheme thm)
        {
            textPaint.Color = thm.ToColor(TextColor);
            textPaint.TextSize = FontSize;
            textPaint.Typeface = SKTypeface.FromFamilyName(FontName);

            // Y축 레이블
            float stepY = graphArea.Height / GridLineCount;
            double valueStep = (displayYMax - displayYMin) / GridLineCount;

            for (int i = 0; i <= GridLineCount; i++)
            {
                float y = graphArea.Bottom - i * stepY;
                double value = displayYMin + i * valueStep;
                string label = value.ToString(ValueFormat);

                // Y축 값 레이블
                textPaint.TextAlign = SKTextAlign.Right;
                canvas.DrawText(label, graphArea.Left - 5, y + FontSize / 3, textPaint);
            }

            // X축 레이블 (시간 간격)
            textPaint.TextAlign = SKTextAlign.Center;

            int xDivisions = 5;
            float stepX = graphArea.Width / xDivisions;
            TimeSpan timeRange = xMax - xMin;
            TimeSpan timeStep = new TimeSpan(timeRange.Ticks / xDivisions);

            for (int i = 0; i <= xDivisions; i++)
            {
                float x = graphArea.Left + i * stepX;
                DateTime time = xMin.Add(timeStep * i);
                string label = FormatDateTime(time);

                canvas.DrawText(label, x, graphArea.Bottom + FontSize + 5, textPaint);
            }

            // 축 제목
            if (!string.IsNullOrEmpty(YAxisTitle))
            {
                textPaint.TextAlign = SKTextAlign.Center;

                // Y축 제목을 세로로 그리기 위해 캔버스 회전
                canvas.Save();
                canvas.RotateDegrees(270, graphArea.Left - 30, graphArea.Top + graphArea.Height / 2);
                canvas.DrawText(YAxisTitle, graphArea.Left - 30, graphArea.Top + graphArea.Height / 2 + FontSize / 3, textPaint);
                canvas.Restore();
            }

            if (!string.IsNullOrEmpty(XAxisTitle))
            {
                textPaint.TextAlign = SKTextAlign.Center;
                canvas.DrawText(XAxisTitle, graphArea.Left + graphArea.Width / 2, graphArea.Bottom + FontSize * 2 + 10, textPaint);
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
            legendPaint.Color = thm.ToColor(BgColor).WithAlpha(200);
            canvas.DrawRoundRect(legendArea, 5, 5, legendPaint);

            legendTextPaint.Color = thm.ToColor(TextColor);
            legendTextPaint.TextSize = FontSize;
            legendTextPaint.Typeface = SKTypeface.FromFamilyName(FontName);

            // 각 시리즈에 대한 레전드 아이템 그리기
            for (int i = 0; i < _series.Count; i++)
            {
                var series = _series[i];
                var itemY = legendArea.Top + padding + i * legendItemHeight;
                var markerX = legendArea.Left + padding;
                var textX = markerX + markerSize + 5;

                // 마커 그리기
                legendPaint.Color = GetSeriesColor(series.Name, thm);
                canvas.DrawRect(markerX, itemY, markerSize, markerSize, legendPaint);

                // 텍스트 그리기
                canvas.DrawText(series.Name, textX, itemY + markerSize - 1, legendTextPaint);
            }
        }

        private void DrawHoveredPoint(SKCanvas canvas, TrendDataPoint point, GoTheme thm)
        {
            var series = FindSeriesForDataPoint(point);
            if (series == null) return;

            float x = MapXToCanvas(point.Timestamp);
            float y = MapYToCanvas(point.Value);

            // 강조 원 그리기
            pointPaint.Color = GetSeriesColor(series.Name, thm);
            canvas.DrawCircle(x, y, DataPointRadius * 2, pointPaint);

            // 테두리 그리기
            pointPaint.Style = SKPaintStyle.Stroke;
            pointPaint.StrokeWidth = 2;
            pointPaint.Color = SKColors.White;
            canvas.DrawCircle(x, y, DataPointRadius * 2, pointPaint);

            // 스타일 복원
            pointPaint.Style = SKPaintStyle.Fill;

            // 툴팁 배경
            string tooltip = $"{series.Name}: {point.Value.ToString(ValueFormat)}\n{FormatDateTime(point.Timestamp)}";

            textPaint.TextSize = FontSize;
            textPaint.Color = thm.ToColor(TextColor);

            // 툴팁 크기 계산
            float tooltipWidth = 0;
            float tooltipHeight = FontSize * 2 + 15;  // 두 줄 + 여백

            string[] lines = tooltip.Split('\n');
            foreach (var line in lines)
            {
                float lineWidth = textPaint.MeasureText(line);
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
            SKRect tooltipRect = new SKRect(tooltipX, tooltipY, tooltipX + tooltipWidth, tooltipY + tooltipHeight);
            legendPaint.Color = thm.ToColor(BgColor).WithAlpha(230);
            canvas.DrawRoundRect(tooltipRect, 5, 5, legendPaint);

            // 툴팁 테두리
            borderPaint.Color = thm.ToColor(BorderColor);
            canvas.DrawRoundRect(tooltipRect, 5, 5, borderPaint);

            // 툴팁 텍스트 그리기
            textPaint.TextAlign = SKTextAlign.Left;

            for (int i = 0; i < lines.Length; i++)
            {
                canvas.DrawText(lines[i], tooltipX + 10, tooltipY + FontSize + i * (FontSize + 5) + 5, textPaint);
            }
        }

        #endregion

        #region Helper Methods

        private void InitializeDefaults()
        {
            xMin = DateTime.Now.AddDays(-7);
            xMax = DateTime.Now;

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
                DataPoints = new List<TrendDataPoint>()
            };

            var series2 = new TrendSeries
            {
                Name = "Humidity",
                Color = "primary",
                DataPoints = new List<TrendDataPoint>()
            };

            var random = new Random(42);
            double value1 = 25;
            double value2 = 50;

            for (int i = 0; i < 30; i++)
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
            series1.DataPoints = series1.DataPoints.OrderBy(p => p.Timestamp).ToList();
            series2.DataPoints = series2.DataPoints.OrderBy(p => p.Timestamp).ToList();

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

            // 그래프 영역 계산
            graphArea = new SKRect(
                Bounds.Left + yAxisWidth + Padding,
                Bounds.Top + Padding,
                Bounds.Right - Padding - (ShowLegend ? legendWidth + Padding : 0),
                Bounds.Bottom - xAxisHeight - Padding
            );

            // 레전드 영역 계산
            if (ShowLegend)
            {
                legendArea = new SKRect(
                    Bounds.Right - legendWidth - Padding / 2,
                    Bounds.Top + Padding,
                    Bounds.Right - Padding / 2,
                    Bounds.Top + Padding + legendHeight
                );
            }
        }

        private void UpdateSeriesColors()
        {
            seriesColors.Clear();
            foreach (var series in _series)
            {
                // 캐싱하지 않고 필요할 때 테마에서 색상 가져오기
            }
        }

        private void UpdateDataRange()
        {
            if (_series.Count == 0 || _series.All(s => s.DataPoints == null || s.DataPoints.Count == 0))
            {
                actualYMin = 0;
                actualYMax = 100;
                displayYMin = 0;
                displayYMax = 100;
                return;
            }

            // 데이터 최소/최대값 계산
            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (var series in _series)
            {
                if (series.DataPoints == null || series.DataPoints.Count == 0)
                    continue;

                min = Math.Min(min, series.DataPoints.Min(p => p.Value));
                max = Math.Max(max, series.DataPoints.Max(p => p.Value));
            }

            actualYMin = min;
            actualYMax = max;

            // 자동 스케일링인 경우 최소/최대값에 여유 공간 추가
            if (AutoScale)
            {
                double range = max - min;

                // 범위가 너무 작으면 기본값 사용
                if (range < 0.001)
                {
                    range = 100;
                    min = Math.Max(0, min - 50);
                }

                // 10% 여유 공간 추가
                double padding = range * 0.1;
                displayYMin = Math.Max(min - padding, 0); // 음수 방지
                displayYMax = max + padding;

                // 눈금 값을 깔끔하게 조정
                AdjustYAxisRange();
            }
            else
            {
                displayYMin = YAxisMin;
                displayYMax = YAxisMax;
            }
        }

        private void AdjustYAxisRange()
        {
            // 눈금 간격 계산
            double range = displayYMax - displayYMin;
            double step = CalculateNiceStep(range / GridLineCount);

            // 최소값을 내려서 눈금에 맞춤
            displayYMin = Math.Floor(displayYMin / step) * step;

            // 최대값을 올려서 눈금에 맞춤
            displayYMax = Math.Ceiling(displayYMax / step) * step;
        }

        private double CalculateNiceStep(double roughStep)
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
            if (!EnableAnimation || animationProgress >= 1.0f)
                return;

            DateTime now = DateTime.Now;
            double elapsed = (now - animationStartTime).TotalMilliseconds;

            animationProgress = Math.Min(1.0f, (float)(elapsed / AnimationDuration));

            if (animationProgress < 1.0f)
            {
                // 다음 프레임에도 다시 그리기
                Invalidate?.Invoke();
            }
        }

        private float MapXToCanvas(DateTime time)
        {
            if (xMax == xMin)
                return graphArea.Left;

            double normalizedX = (time - xMin).TotalMilliseconds / (xMax - xMin).TotalMilliseconds;
            return (float)(graphArea.Left + normalizedX * graphArea.Width);
        }

        private float MapYToCanvas(double value)
        {
            if (displayYMax == displayYMin)
                return graphArea.Bottom;

            double normalizedY = (value - displayYMin) / (displayYMax - displayYMin);
            return (float)(graphArea.Bottom - normalizedY * graphArea.Height);
        }

        private string FormatDateTime(DateTime time)
        {
            // 적절한 날짜/시간 포맷 선택
            TimeSpan range = xMax - xMin;
            
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
            string[] defaultColors = new[] { "primary", "success", "warning", "danger", "info" };
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
                
                foreach (var point in series.DataPoints)
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

        #endregion

        #region IDisposable

        public void Dispose()
        {
            backgroundPaint?.Dispose();
            borderPaint?.Dispose();
            axisPaint?.Dispose();
            gridPaint?.Dispose();
            linePaint?.Dispose();
            fillPaint?.Dispose();
            pointPaint?.Dispose();
            textPaint?.Dispose();
            legendPaint?.Dispose();
            legendTextPaint?.Dispose();
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// 트렌드 데이터 포인트를 나타냅니다.
    /// </summary>
    public class TrendDataPoint
    {
        /// <summary>
        /// 데이터 측정 시간
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 데이터 값
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// 트렌드 시리즈를 나타냅니다.
    /// </summary>
    public class TrendSeries
    {
        /// <summary>
        /// 시리즈 이름
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 데이터 포인트 목록
        /// </summary>
        public List<TrendDataPoint> DataPoints { get; set; }
        
        /// <summary>
        /// 시리즈 색상 (테마 색상 이름)
        /// </summary>
        public string Color { get; set; }
    }

    /// <summary>
    /// 데이터 포인트 선택 이벤트 인자
    /// </summary>
    public class DataPointEventArgs : EventArgs
    {
        /// <summary>
        /// 선택된 시리즈
        /// </summary>
        public TrendSeries Series { get; set; }
        
        /// <summary>
        /// 선택된 데이터 포인트
        /// </summary>
        public TrendDataPoint DataPoint { get; set; }
    }

    #endregion
}