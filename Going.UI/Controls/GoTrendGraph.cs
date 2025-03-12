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
    /*
    /// <summary>
    /// 트렌드 그래프 컨트롤 - 시간에 따른 데이터 변화를 시각적으로 표현하고 좌우 스크롤이 가능한 컴포넌트
    /// 매 프레임 렌더링에 최적화됨
    /// </summary>
    public class GoTrendGraph : GoControl, IDisposable
    {
        
        #region Properties

        #region 아이콘 설정

        public string? IconString { get; set; }
        public float IconSize { get; set; }
        public float IconGap { get; set; }
        public GoDirectionHV IconPosition { get; set; }

        #endregion

        #region 그래프 라벨 설정

        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        #endregion

        #region 그래프 상태 설정

        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; }
        public bool ShowGrid { get; set; }
        public bool LegendDraw { get; set; }    // 범례 표시 : 일단 사용 안함
        public bool ShadowDraw { get; set; }    // 그림자 효과 : 일단 사용 안함
        public bool DataPointDraw { get; set; } // 데이터 포인트 표시 : 일단 사용 안함
        public bool IsStart { get; set; }       // 실시간 데이터 시작 여부
        public bool Pause { get; set; }         // 실시간 데이터 일시정지 여부

        #endregion

        #region 그래프 모양 설정

        public string TextColor { get; set; } = "Fore";
        public string BgColor { get; set; } = "Base3";
        public string BorderColor { get; set; } = "Base2";
        public string AxisColor { get; set; } = "Base2";
        public string GridColor { get; set; } = "Base2";
        public float Border { get; set; } = 1;
        public float Radius { get; set; } = 3;
        public GoRoundType Round { get; set; }
        public int GridLineCount { get; set; } = 5;
        public float DataPointRadius { get; set; } = 5; // 데이터 포인트 반지름 : 일단 사용 안함
        public string ValueFormatString { get; set; } = "0.0"; // 데이터 포인트 값 포맷
        public string TimeFormatString { get; set; } = "HH:mm"; // 데이터 포인트 시간 포맷
        public TimeSpan XAxisGraduation { get; set; } = new TimeSpan(0, 1, 0);
        public int YAxisGraduationCount { get; set; } = 10;
        public TimeSpan XScale { get; set; } = new TimeSpan(0, 10, 0);
        public TimeSpan MaximumXScale { get; set; } = new TimeSpan(1, 0, 0);
        public int Interval { get; set; } = 1000;

        #endregion

        #endregion

        #region Member Variable

        private Scroll scroll = new Scroll { Direction = ScrollDirection.Horizon };
        private DateTime vst;
        private DateTime ved;
        private DateTime vstarttime;
        private float mx, my;
        private bool bSettingDown;
        private double? nGraphWidth;
        private bool bDetail;
        private bool bDetailDown;
        private bool bGraphDown;
        private bool bPauseDown;
        private bool bPause;

        private Task? taskData;
        private CancellationTokenSource? cancelData;

        #endregion

        #region Events
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


            // 스크롤 이벤트 연결
            scrollManager.VisibleRangeChanged += (s, e) =>
            {
                VisibleRangeChanged?.Invoke(this, e);
                needsLayoutUpdate = true;
                Invalidate?.Invoke();
            };

            InitializeDefaults();
        }

        #endregion

        #region Override

        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            try
            {
                if (canvas == null)
                    return;

                settings.Bounds = Bounds;
                UpdateAnimation();

                // 영역 계산
                if (needsLayoutUpdate)
                {
                    CalculateAreas();
                    needsLayoutUpdate = false;
                }

                // 렌더러에 필요한 속성 업데이트
                UpdateRendererProperties();

                // 그래프 그리기
                renderer.DrawGraph(canvas, GoTheme.Current);

                base.OnDraw(canvas);
            }
            catch (Exception ex)
            {
                // 예외 발생 시 그리기 중단
                System.Diagnostics.Debug.WriteLine($"그래프 그리기 오류: {ex.Message}");
            }
        }
        #endregion

        #region OnMouseMove
        protected override void OnMouseMove(float x, float y)
        {
            mx = x;
            my = y;

            try
            {
                mx = x;
                my = y;

                if (isDraggingScrollHandle)
                {
                    scrollManager.HandleScrollDrag(x);
                    return;
                }

                if (isDraggingGraph)
                {
                    scrollManager.HandleGraphDrag(x);
                    return;
                }

                // 스크롤바 영역에 있는지 확인
                SKRect scrollBarArea = renderer.ScrollBarArea;
                if (ShowScrollBar && EnableScrolling && scrollBarArea.Contains(x, y))
                {
                    // 커서 변경 로직 (플랫폼에 따라 다를 수 있음)
                    return;
                }

                // 데이터 포인트 위에 마우스가 있는지 확인
                var prevHoveredPoint = hoveredPoint;
                hoveredPoint = FindNearestDataPoint(x, y);
                isHovering = hoveredPoint != null;

                if (hoveredPoint != prevHoveredPoint)
                {
                    renderer.HoveredPoint = hoveredPoint;
                    Invalidate?.Invoke(); // 다시 그리기 요청
                }

                base.OnMouseMove(x, y);
            }
            catch (Exception ex)
            {
                // 예외 처리 - 마우스 이동 무시
                System.Diagnostics.Debug.WriteLine($"마우스 이동 오류: {ex.Message}");
            }
        }
        #endregion

        #region OnMouseDown
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            mx = x;
            my = y;

            try
            {
                if (button == GoMouseButton.Left)
                {
                    // 스크롤 핸들 드래그 시작
                    SKRect scrollBarArea = renderer.ScrollBarArea;
                    float scrollHandleX = renderer.ScrollHandleX;
                    float scrollHandleWidth = renderer.ScrollHandleWidth;

                    if (ShowScrollBar && EnableScrolling && scrollBarArea.Contains(x, y))
                    {
                        if (scrollHandleX <= x && x <= scrollHandleX + scrollHandleWidth)
                        {
                            isDraggingScrollHandle = true;
                            scrollManager.DragStartX = x;
                            return;
                        }
                        else
                        {
                            // 스크롤바 클릭 시 핸들 이동
                            scrollManager.ScrollToPosition(x);
                            return;
                        }
                    }

                    // 그래프 영역 드래그 시작
                    SKRect graphArea = renderer.GraphArea;
                    if (EnableScrolling && graphArea.Contains(x, y))
                    {
                        isDraggingGraph = true;
                        scrollManager.DragStartX = x;
                        scrollManager.DragStartMin = scrollManager.VisibleMinimum;
                        scrollManager.DragStartMax = scrollManager.VisibleMaximum;
                        return;
                    }
                }

                if (hoveredPoint != null)
                {
                    // 데이터 포인트 클릭 이벤트 발생
                    var series = FindSeriesForDataPoint(hoveredPoint.Value);
                    DataPointSelected?.Invoke(this, new DataPointEventArgs(series, hoveredPoint.Value));
                }

                base.OnMouseDown(x, y, button);
            }
            catch (Exception ex)
            {
                // 예외 처리 - 마우스 다운 무시
                System.Diagnostics.Debug.WriteLine($"마우스 다운 오류: {ex.Message}");
            }
        }
        #endregion

        #region OnMouseUp
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            mx = x;
            my = y;
            isDraggingScrollHandle = false;
            isDraggingGraph = false;
            base.OnMouseUp(x, y, button);
        }
        #endregion

        #region OnMouseWheel
        protected override void OnMouseWheel(float x, float y, float delta)
        {
            try
            {
                if (EnableMouseWheelZoom && renderer.GraphArea.Contains(x, y))
                {
                    // 마우스 위치를 중심으로 줌
                    scrollManager.ZoomAtPoint(x, delta);
                }

                base.OnMouseWheel(x, y, delta);
            }
            catch (Exception ex)
            {
                // 예외 처리 - 마우스 휠 무시
                System.Diagnostics.Debug.WriteLine($"마우스 휠 오류: {ex.Message}");
            }
        }
        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// 트렌드 시리즈를 추가합니다.
        /// </summary>
        public void AddSeries(TrendSeries series)
        {
            if (series == null) throw new ArgumentNullException(nameof(series));
            if (string.IsNullOrEmpty(series.Name)) throw new ArgumentException("Series name cannot be empty", nameof(series));

            try
            {
                // 기존 시리즈가 있는지 확인
                var existingSeries = this.series.FirstOrDefault(s => s.Name == series.Name);
                if (existingSeries != null)
                {
                    // 기존 시리즈 업데이트
                    existingSeries.DataPoints = series.DataPoints;
                    existingSeries.Color = series.Color;
                }
                else
                {
                    // 새 시리즈 추가
                    this.series.Add(series);

                    // 색상이 지정되지 않은 경우 자동 할당
                    if (string.IsNullOrEmpty(series.Color))
                    {
                        series.Color = GetNextColor();
                    }
                }

                // 데이터 범위 업데이트
                UpdateDataRange();

                // 애니메이션 시작
                if (EnableAnimation)
                {
                    StartAnimation();
                }

                // 이벤트 발생 및 다시 그리기
                DataChanged?.Invoke(this, EventArgs.Empty);
                needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
            catch (Exception ex)
            {
                // 예외 로깅
                System.Diagnostics.Debug.WriteLine($"시리즈 추가 오류: {ex.Message}");
                throw; // 심각한 오류이므로 다시 던짐
            }
        }

        /// <summary>
        /// 이름으로 시리즈를 제거합니다.
        /// </summary>
        public bool RemoveSeries(string seriesName)
        {
            if (string.IsNullOrEmpty(seriesName))
                return false;

            var series = this.series.FirstOrDefault(s => s.Name == seriesName);
            if (series != null)
            {
                this.series.Remove(series);
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
            series.Clear();
            UpdateDataRange();
            DataChanged?.Invoke(this, EventArgs.Empty);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 특정 시리즈의 데이터를 업데이트합니다.
        /// </summary>
        public bool UpdateSeriesData(string seriesName, List<TrendDataPoint> newData)
        {
            if (string.IsNullOrEmpty(seriesName) || newData == null)
                return false;

            var series = this.series.FirstOrDefault(s => s.Name == seriesName);
            if (series != null)
            {
                series.DataPoints = newData;
                UpdateDataRange();

                // 애니메이션 시작
                if (EnableAnimation)
                {
                    StartAnimation();
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

            scrollManager.DataMinimum = min;
            scrollManager.DataMaximum = max;

            // 보이는 범위가 데이터 범위를 벗어나지 않도록 조정
            scrollManager.AdjustVisibleRange();
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 보이는 X축 범위를 설정합니다.
        /// </summary>
        public void SetVisibleXRange(DateTime min, DateTime max)
        {
            if (min >= max) throw new ArgumentException("Min must be less than max");

            try
            {
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

                scrollManager.UpdateVisibleRange(min, max);
                Invalidate?.Invoke();
            }
            catch (Exception ex)
            {
                // 예외 로깅
                System.Diagnostics.Debug.WriteLine($"X축 범위 설정 오류: {ex.Message}");
                throw; // 심각한 오류이므로 다시 던짐
            }
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
            SetVisibleXRange(scrollManager.DataMinimum, scrollManager.DataMaximum);
        }

        /// <summary>
        /// 주어진 시간만큼 스크롤합니다. (양수: 오른쪽, 음수: 왼쪽)
        /// </summary>
        public void ScrollByTime(TimeSpan amount)
        {
            scrollManager.ScrollByTime(amount);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 일정 비율로 확대/축소합니다. (factor > 1: 축소, factor < 1: 확대)
        /// </summary>
        public void Zoom(float factor)
        {
            if (factor <= 0) throw new ArgumentException("Factor must be positive", nameof(factor));

            scrollManager.Zoom(factor);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 현재 보이는 시간 범위를 가져옵니다.
        /// </summary>
        public TimeSpan GetVisibleTimeSpan()
        {
            return scrollManager.VisibleMaximum - scrollManager.VisibleMinimum;
        }

        /// <summary>
        /// 애니메이션 시작
        /// </summary>
        public void StartAnimation(float targetProgress = 1.0f)
        {
            if (!EnableAnimation)
                return;

            animationProgress = 0f;
            targetAnimationProgress = targetProgress;
            animationStartTime = DateTime.Now;
            lastFrameTimeTicks = animationStartTime.Ticks;
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 오래된 데이터를 정리합니다.
        /// </summary>
        /// <param name="maxAge">보관할 최대 기간</param>
        /// <returns>제거된 데이터 포인트 수</returns>
        public int CleanupOldData(TimeSpan maxAge)
        {
            int totalRemoved = 0;
            DateTime cutoffTime = DateTime.Now.Subtract(maxAge);

            foreach (var series in series)
            {
                if (series.DataPoints == null)
                    continue;

                int initialCount = series.DataPoints.Count;
                series.DataPoints = series.DataPoints
                    .Where(p => p.Timestamp >= cutoffTime)
                    .ToList();

                totalRemoved += initialCount - series.DataPoints.Count;
            }

            if (totalRemoved > 0)
            {
                UpdateDataRange();
                Invalidate?.Invoke();
            }

            return totalRemoved;
        }

        #endregion

        #region Private Methods

        private void InitializeDefaults()
        {
            DateTime now = DateTime.Now;
            DateTime min = now.AddDays(-30);
            DateTime max = now;

            scrollManager.DataMinimum = min;
            scrollManager.DataMaximum = max;
            scrollManager.UpdateVisibleRange(now.AddDays(-7), now);

            // 테스트 데이터 생성
            GenerateSampleData();
        }

        private void GenerateSampleData()
        {
            try
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

                for (int i = 0; i < 100; i++)
                {
                    var time = now.AddHours(-i * 8);

                    // 랜덤 변동 추가
                    value1 += (random.NextDouble() - 0.5) * 3;
                    value2 += (random.NextDouble() - 0.5) * 5;

                    // 범위 제한
                    value1 = Math.Max(15, Math.Min(35, value1));
                    value2 = Math.Max(30, Math.Min(70, value2));

                    series1.AddDataPoint(time, value1);
                    series2.AddDataPoint(time, value2);
                }

                // 시간별로 정렬
                series1.SortData();
                series2.SortData();

                series.Add(series1);
                series.Add(series2);

                UpdateDataRange();
            }
            catch (Exception ex)
            {
                // 샘플 데이터 생성 오류 - 무시하고 계속 진행
                System.Diagnostics.Debug.WriteLine($"샘플 데이터 생성 오류: {ex.Message}");
            }
        }

        private void CalculateAreas()
        {
            try
            {
                float xAxisHeight = FontSize * 2 + 15;  // X축 레이블 및 제목 공간
                float yAxisWidth = 60;  // Y축 레이블 및 제목 공간
                float legendWidth = 120;  // 레전드 영역 너비
                float legendHeight = series.Count * (FontSize + 10) + 20;  // 레전드 영역 높이

                // 경계 확인
                float graphBottom = Math.Max(Bounds.Top + Padding, Bounds.Bottom - Padding);
                if (ShowScrollBar && EnableScrolling)
                {
                    graphBottom -= ScrollBarHeight + Padding;
                }

                // 영역이 너무 작은 경우 최소 크기 보장
                float graphWidth = Math.Max(10, Bounds.Width - 2 * Padding - yAxisWidth - (ShowLegend ? legendWidth + Padding : 0));
                float graphHeight = Math.Max(10, graphBottom - (Bounds.Top + Padding));

                // 스크롤바 영역 계산
                SKRect scrollBarArea;
                if (ShowScrollBar && EnableScrolling)
                {
                    scrollBarArea = new SKRect(
                        Bounds.Left + yAxisWidth + Padding,
                        graphBottom,
                        Bounds.Left + yAxisWidth + Padding + graphWidth,
                        graphBottom + ScrollBarHeight
                    );
                }
                else
                {
                    scrollBarArea = SKRect.Empty;
                }

                // 그래프 영역 계산
                SKRect graphArea = new SKRect(
                    Bounds.Left + yAxisWidth + Padding,
                    Bounds.Top + Padding,
                    Bounds.Left + yAxisWidth + Padding + graphWidth,
                    graphBottom - xAxisHeight
                );

                // 레전드 영역 계산
                SKRect legendArea;
                if (ShowLegend)
                {
                    legendArea = new SKRect(
                        Bounds.Right - legendWidth - Padding / 2,
                        Bounds.Top + Padding,
                        Bounds.Right - Padding / 2,
                        Bounds.Top + Padding + legendHeight
                    );
                }
                else
                {
                    legendArea = SKRect.Empty;
                }

                // 영역 설정
                renderer.GraphArea = graphArea;
                renderer.LegendArea = legendArea;
                renderer.ScrollBarArea = scrollBarArea;

                scrollManager.GraphArea = graphArea;
                scrollManager.ScrollBarArea = scrollBarArea;

                // 스크롤 핸들 메트릭 업데이트
                scrollManager.UpdateScrollHandleMetrics();
            }
            catch (Exception ex)
            {
                // 영역 계산 오류 - 무시하고 계속 진행
                System.Diagnostics.Debug.WriteLine($"영역 계산 오류: {ex.Message}");
            }
        }

        private void UpdateRendererProperties()
        {
            // 렌더러에 속성 업데이트
            renderer.Settings = settings;
            renderer.Series = series;
            renderer.AnimationProgress = animationProgress;
            renderer.VisibleXMin = scrollManager.VisibleMinimum;
            renderer.VisibleXMax = scrollManager.VisibleMaximum;
            renderer.DisplayYMin = settings.YAxisMin;
            renderer.DisplayYMax = settings.YAxisMax;
            renderer.ScrollHandleX = scrollManager.HandleX;
            renderer.ScrollHandleWidth = scrollManager.HandleWidth;
            renderer.HoveredPoint = hoveredPoint;

            // 매핑 캐시 무효화 (Y축 범위 변경 시)
            renderer.InvalidateYMappingCache();
        }

        private void UpdateDataRange()
        {
            try
            {
                // 아무 데이터도 없는 경우 기본값 사용
                if (series.Count == 0 || series.All(s => s.DataPoints == null || s.DataPoints.Count == 0))
                {
                    settings.YAxisMin = 0;
                    settings.YAxisMax = 100;
                    scrollManager.DataMinimum = DateTime.Now.AddDays(-7);
                    scrollManager.DataMaximum = DateTime.Now;
                    return;
                }

                // 데이터 X 범위 계산
                DateTime minDate = DateTime.MaxValue;
                DateTime maxDate = DateTime.MinValue;

                // 데이터 Y 최소/최대값 계산
                double minValue = double.MaxValue;
                double maxValue = double.MinValue;

                foreach (var series in series)
                {
                    if (series.DataPoints == null || series.DataPoints.Count == 0)
                        continue;

                    var timeRange = series.GetTimeRange();
                    minDate = timeRange.Min < minDate ? timeRange.Min : minDate;
                    maxDate = timeRange.Max > maxDate ? timeRange.Max : maxDate;

                    var valueRange = series.GetValueRange();
                    minValue = Math.Min(minValue, valueRange.Min);
                    maxValue = Math.Max(maxValue, valueRange.Max);
                }

                // 데이터 범위 유효성 검사
                if (minDate >= maxDate)
                {
                    minDate = DateTime.Now.AddDays(-7);
                    maxDate = DateTime.Now;
                }

                if (minValue >= maxValue)
                {
                    minValue = 0;
                    maxValue = 100;
                }

                scrollManager.DataMinimum = minDate;
                scrollManager.DataMaximum = maxDate;

                // 초기 보이는 범위 설정 (전체 데이터 중에서 마지막 7일)
                if (scrollManager.VisibleMinimum == DateTime.MinValue || scrollManager.VisibleMaximum == DateTime.MaxValue)
                {
                    // 전체 데이터 범위가 7일보다 작으면 전체 표시
                    if ((maxDate - minDate).TotalDays <= 7)
                    {
                        scrollManager.UpdateVisibleRange(minDate, maxDate);
                    }
                    else
                    {
                        // 마지막 7일 표시
                        scrollManager.UpdateVisibleRange(maxDate.AddDays(-7), maxDate);
                    }
                }
                else
                {
                    // 보이는 범위가 데이터 범위를 벗어나지 않도록 조정
                    scrollManager.AdjustVisibleRange();
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
                    settings.YAxisMin = Math.Max(minValue - padding, 0); // 음수 방지
                    settings.YAxisMax = maxValue + padding;

                    // 눈금 값을 깔끔하게 조정
                    AdjustYAxisRange();
                }
            }
            catch (Exception ex)
            {
                // 데이터 범위 업데이트 오류 - 기본값 설정
                settings.YAxisMin = 0;
                settings.YAxisMax = 100;
                System.Diagnostics.Debug.WriteLine($"데이터 범위 업데이트 오류: {ex.Message}");
            }
        }

        private void AdjustYAxisRange()
        {
            // 눈금 간격 계산
            double range = settings.YAxisMax - settings.YAxisMin;
            double step = CalculateNiceStep(range / Math.Max(1, GridLineCount));

            // 최소값을 내려서 눈금에 맞춤
            settings.YAxisMin = Math.Floor(settings.YAxisMin / step) * step;

            // 최대값을 올려서 눈금에 맞춤
            settings.YAxisMax = Math.Ceiling(settings.YAxisMax / step) * step;
        }

        private static double CalculateNiceStep(double roughStep)
        {
            // 0이나 음수 값 처리
            if (roughStep <= 0)
                return 1;

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
            if (!EnableAnimation || animationProgress >= targetAnimationProgress)
                return;

            try
            {
                // 현재 시간
                DateTime now = DateTime.Now;
                long currentTimeTicks = now.Ticks;

                // 애니메이션 정지된 경우 (창 최소화 등)
                if (lastFrameTimeTicks == 0)
                {
                    lastFrameTimeTicks = currentTimeTicks;
                    return;
                }

                // 마지막 프레임 이후 경과 시간 (밀리초)
                double elapsed = (currentTimeTicks - lastFrameTimeTicks) / TimeSpan.TicksPerMillisecond;

                // 너무 큰 시간 간격 제한 (창 최소화 후 복원 등의 경우)
                elapsed = Math.Min(elapsed, AnimationDuration / 2.0);

                // 진행률 업데이트
                float progressDelta = (float)(elapsed / AnimationDuration);
                animationProgress = Math.Min(targetAnimationProgress, animationProgress + progressDelta);

                // 마지막 프레임 시간 업데이트
                lastFrameTimeTicks = currentTimeTicks;

                // 애니메이션이 완료되지 않았으면 다시 그리기 요청
                if (animationProgress < targetAnimationProgress)
                {
                    Invalidate?.Invoke();
                }
            }
            catch (Exception ex)
            {
                // 애니메이션 오류 - 진행률을 최대로 설정하고 계속 진행
                animationProgress = targetAnimationProgress;
                System.Diagnostics.Debug.WriteLine($"애니메이션 업데이트 오류: {ex.Message}");
            }
        }

        private TrendDataPoint? FindNearestDataPoint(float x, float y)
        {
            try
            {
                const float HitDistance = 20f; // 픽셀 단위 검색 거리

                TrendDataPoint? closestPoint = null;
                float minDistance = float.MaxValue;

                // 마우스가 그래프 영역 밖이면 검색 안 함
                if (!renderer.GraphArea.Contains(x, y))
                    return null;

                // 보이는 범위의 데이터만 검색
                foreach (var series in series)
                {
                    if (series.DataPoints == null || series.DataPoints.Count == 0)
                        continue;

                    // 성능 최적화: 보이는 범위의 데이터만 검색
                    foreach (var point in series.GetVisiblePoints(scrollManager.VisibleMinimum, scrollManager.VisibleMaximum))
                    {
                        float px = scrollManager.TimeToCanvasX(point.Timestamp);
                        float py = renderer.MapYToCanvas(point.Value);

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
            catch (Exception ex)
            {
                // 예외 발생 시 null 반환
                System.Diagnostics.Debug.WriteLine($"데이터 포인트 검색 오류: {ex.Message}");
                return null;
            }
        }

        private TrendSeries? FindSeriesForDataPoint(TrendDataPoint point)
        {
            foreach (var series in series)
            {
                if (series.DataPoints == null || series.DataPoints.Count == 0)
                    continue;

                // 직접 Equals 검사 (값 타입 최적화)
                foreach (var p in series.DataPoints)
                {
                    if (p.Equals(point))
                    {
                        return series;
                    }
                }
            }

            return null;
        }

        private string GetNextColor()
        {
            string[] defaultColors = { "primary", "success", "warning", "danger", "info" };
            int index = series.Count % defaultColors.Length;
            return defaultColors[index];
        }

        #endregion

        #region IDisposable 구현

        /// <summary>
        /// 리소스를 해제합니다.
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
            if (!disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                    renderer.Dispose();
                    scrollManager.Dispose();

                    // 이벤트 핸들러 해제
                    DataChanged = null;
                    DataPointSelected = null;
                    VisibleRangeChanged = null;
                }

                disposed = true;
            }
        }

        /// <summary>
        /// 소멸자
        /// </summary>
        ~GoTrendGraph()
        {
            Dispose(false);
        }

        #endregion
        
    }
    */
}