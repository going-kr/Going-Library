using Going.UI.Controls.TrendGraph;
using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Going.UI.Controls
{
    /// <summary>
    /// 트렌드 그래프 컨트롤 - 시간에 따른 데이터 변화를 시각적으로 표현하고 좌우 스크롤이 가능한 컴포넌트
    /// </summary>
    public class GoTrendGraph : GoControl, IDisposable
    {
        #region Private Fields

        private readonly TrendRenderer _renderer = new();
        private readonly TrendScrollManager _scrollManager = new();
        private readonly TrendGraphSettings _settings = new();
        private readonly List<TrendSeries> _series = new();

        private bool _isHovering;
        private TrendDataPoint? _hoveredPoint;
        private float _mx, _my;   // 마우스 위치 추적
        private bool _isDraggingGraph;
        private bool _isDraggingScrollHandle;
        private DateTime _animationStartTime;
        private float _animationProgress = 1.0f;
        private bool _needsLayoutUpdate = true;

        #endregion

        #region Properties

        #region 아이콘 설정
        /// <summary>
        /// 아이콘 문자열
        /// </summary>
        public string? IconString
        {
            get => _settings.IconString;
            set
            {
                _settings.IconString = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 아이콘 크기
        /// </summary>
        public float IconSize
        {
            get => _settings.IconSize;
            set
            {
                _settings.IconSize = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 아이콘과 텍스트 사이 간격
        /// </summary>
        public float IconGap
        {
            get => _settings.IconGap;
            set
            {
                _settings.IconGap = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 아이콘 방향
        /// </summary>
        public GoDirectionHV IconDirection
        {
            get => _settings.IconDirection;
            set
            {
                _settings.IconDirection = value;
                Invalidate?.Invoke();
            }
        }
        #endregion

        #region 그래프 라벨 설정
        /// <summary>
        /// 그래프 라벨 텍스트
        /// </summary>
        public string Text
        {
            get => _settings.Text;
            set
            {
                _settings.Text = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 텍스트 폰트 이름
        /// </summary>
        public string TextFontName
        {
            get => _settings.TextFontName;
            set
            {
                _settings.TextFontName = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 텍스트 폰트 크기
        /// </summary>
        public float TextFontSize
        {
            get => _settings.TextFontSize;
            set
            {
                _settings.TextFontSize = value;
                Invalidate?.Invoke();
            }
        }
        #endregion

        #region 그래프 배경 설정
        /// <summary>
        /// 배경 그리기 여부
        /// </summary>
        public bool BackgroundDraw
        {
            get => _settings.BackgroundDraw;
            set
            {
                _settings.BackgroundDraw = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 테두리만 그리기 여부
        /// </summary>
        public bool BorderOnly
        {
            get => _settings.BorderOnly;
            set
            {
                _settings.BorderOnly = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 배경 색상
        /// </summary>
        public string BgColor
        {
            get => _settings.BgColor;
            set
            {
                _settings.BgColor = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 테두리 색상
        /// </summary>
        public string BorderColor
        {
            get => _settings.BorderColor;
            set
            {
                _settings.BorderColor = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 모서리 둥글기 타입
        /// </summary>
        public GoRoundType Round
        {
            get => _settings.Round;
            set
            {
                _settings.Round = value;
                Invalidate?.Invoke();
            }
        }
        #endregion

        #region 그래프 설정
        /// <summary>
        /// 축 색상
        /// </summary>
        public string AxisColor
        {
            get => _settings.AxisColor;
            set
            {
                _settings.AxisColor = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 그리드 색상
        /// </summary>
        public string GridColor
        {
            get => _settings.GridColor;
            set
            {
                _settings.GridColor = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 그리드 표시 여부
        /// </summary>
        public bool ShowGrid
        {
            get => _settings.ShowGrid;
            set
            {
                _settings.ShowGrid = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 레전드 표시 여부
        /// </summary>
        public bool ShowLegend
        {
            get => _settings.ShowLegend;
            set
            {
                _settings.ShowLegend = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 그리드 선 개수
        /// </summary>
        public int GridLineCount
        {
            get => _settings.GridLineCount;
            set
            {
                _settings.GridLineCount = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 그림자 효과 활성화 여부
        /// </summary>
        public bool EnableShadow
        {
            get => _settings.EnableShadow;
            set
            {
                _settings.EnableShadow = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 애니메이션 활성화 여부
        /// </summary>
        public bool EnableAnimation
        {
            get => _settings.EnableAnimation;
            set
            {
                _settings.EnableAnimation = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 애니메이션 지속 시간 (밀리초)
        /// </summary>
        public int AnimationDuration
        {
            get => _settings.AnimationDuration;
            set
            {
                _settings.AnimationDuration = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 선 두께
        /// </summary>
        public float LineThickness
        {
            get => _settings.LineThickness;
            set
            {
                _settings.LineThickness = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 영역 채우기 여부
        /// </summary>
        public bool FillArea
        {
            get => _settings.FillArea;
            set
            {
                _settings.FillArea = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 채우기 투명도 (0-255)
        /// </summary>
        public int FillOpacity
        {
            get => _settings.FillOpacity;
            set
            {
                _settings.FillOpacity = Math.Clamp(value, 0, 255);
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 데이터 포인트 표시 여부
        /// </summary>
        public bool ShowDataPoints
        {
            get => _settings.ShowDataPoints;
            set
            {
                _settings.ShowDataPoints = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 데이터 포인트 반지름
        /// </summary>
        public float DataPointRadius
        {
            get => _settings.DataPointRadius;
            set
            {
                _settings.DataPointRadius = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 레이블 표시 여부
        /// </summary>
        public bool ShowLabels
        {
            get => _settings.ShowLabels;
            set
            {
                _settings.ShowLabels = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 패딩
        /// </summary>
        public float Padding
        {
            get => _settings.Padding;
            set
            {
                _settings.Padding = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }
        #endregion

        #region 축 레이블 설정
        /// <summary>
        /// 폰트 이름
        /// </summary>
        public string FontName
        {
            get => _settings.FontName;
            set
            {
                _settings.FontName = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 폰트 크기
        /// </summary>
        public float FontSize
        {
            get => _settings.FontSize;
            set
            {
                _settings.FontSize = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 텍스트 색상
        /// </summary>
        public string TextColor
        {
            get => _settings.TextColor;
            set
            {
                _settings.TextColor = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 값 포맷 문자열
        /// </summary>
        public string ValueFormat
        {
            get => _settings.ValueFormat;
            set
            {
                _settings.ValueFormat = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// X축 제목
        /// </summary>
        public string? XAxisTitle
        {
            get => _settings.XAxisTitle;
            set
            {
                _settings.XAxisTitle = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// Y축 제목
        /// </summary>
        public string? YAxisTitle
        {
            get => _settings.YAxisTitle;
            set
            {
                _settings.YAxisTitle = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 자동 스케일링 여부
        /// </summary>
        public bool AutoScale
        {
            get => _settings.AutoScale;
            set
            {
                _settings.AutoScale = value;
                UpdateDataRange();
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// Y축 최소값 (자동 스케일링이 꺼진 경우)
        /// </summary>
        public double YAxisMin
        {
            get => _settings.YAxisMin;
            set
            {
                _settings.YAxisMin = value;
                if (!AutoScale)
                {
                    UpdateDataRange();
                    Invalidate?.Invoke();
                }
            }
        }

        /// <summary>
        /// Y축 최대값 (자동 스케일링이 꺼진 경우)
        /// </summary>
        public double YAxisMax
        {
            get => _settings.YAxisMax;
            set
            {
                _settings.YAxisMax = value;
                if (!AutoScale)
                {
                    UpdateDataRange();
                    Invalidate?.Invoke();
                }
            }
        }
        #endregion

        #region 스크롤 설정
        /// <summary>
        /// 스크롤 기능 활성화 여부
        /// </summary>
        public bool EnableScrolling
        {
            get => _settings.EnableScrolling;
            set
            {
                _settings.EnableScrolling = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 마우스 휠로 줌 기능 활성화 여부
        /// </summary>
        public bool EnableMouseWheelZoom
        {
            get => _settings.EnableMouseWheelZoom;
            set
            {
                _settings.EnableMouseWheelZoom = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 화면에 표시되는 시간 범위 (초 단위)
        /// </summary>
        public double VisibleTimeRange
        {
            get => _settings.VisibleTimeRange;
            set
            {
                _settings.VisibleTimeRange = Math.Clamp(value, _settings.MinTimeRange, _settings.MaxTimeRange);
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 줌 최소 시간 범위 (초 단위)
        /// </summary>
        public double MinTimeRange
        {
            get => _settings.MinTimeRange;
            set
            {
                _settings.MinTimeRange = value;
                _scrollManager.MinTimeRange = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 줌 최대 시간 범위 (초 단위)
        /// </summary>
        public double MaxTimeRange
        {
            get => _settings.MaxTimeRange;
            set
            {
                _settings.MaxTimeRange = value;
                _scrollManager.MaxTimeRange = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 스크롤바 높이
        /// </summary>
        public float ScrollBarHeight
        {
            get => _settings.ScrollBarHeight;
            set
            {
                _settings.ScrollBarHeight = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 스크롤바 색상
        /// </summary>
        public string ScrollBarColor
        {
            get => _settings.ScrollBarColor;
            set
            {
                _settings.ScrollBarColor = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 스크롤 핸들 색상
        /// </summary>
        public string ScrollHandleColor
        {
            get => _settings.ScrollHandleColor;
            set
            {
                _settings.ScrollHandleColor = value;
                Invalidate?.Invoke();
            }
        }

        /// <summary>
        /// 스크롤바 표시 여부
        /// </summary>
        public bool ShowScrollBar
        {
            get => _settings.ShowScrollBar;
            set
            {
                _settings.ShowScrollBar = value;
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            }
        }
        #endregion

        #region 데이터 속성

        /// <summary>
        /// 시리즈 데이터 목록
        /// </summary>
        public IReadOnlyList<TrendSeries> Series => _series.AsReadOnly();

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

        /// <summary>
        /// 트렌드 그래프 컴포넌트를 초기화합니다.
        /// </summary>
        public GoTrendGraph()
        {
            Selectable = true;

            // 스크롤 이벤트 연결
            _scrollManager.VisibleRangeChanged += (s, e) =>
            {
                VisibleRangeChanged?.Invoke(this, e);
                _needsLayoutUpdate = true;
                Invalidate?.Invoke();
            };

            InitializeDefaults();
        }

        #endregion

        #region Override

        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            _settings.Bounds = Bounds;
            UpdateAnimation();

            // 영역 계산
            if (_needsLayoutUpdate)
            {
                CalculateAreas();
                _needsLayoutUpdate = false;
            }

            // 렌더러에 필요한 속성 업데이트
            UpdateRendererProperties();

            // 그래프 그리기
            _renderer.DrawGraph(canvas, GoTheme.Current);

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
                _scrollManager.HandleScrollDrag(x);
                return;
            }

            if (_isDraggingGraph)
            {
                _scrollManager.HandleGraphDrag(x);
                return;
            }

            // 스크롤바 영역에 있는지 확인
            SKRect scrollBarArea = _renderer.ScrollBarArea;
            if (ShowScrollBar && EnableScrolling && scrollBarArea.Contains(x, y))
            {
                // 커서 변경 로직 (플랫폼에 따라 다를 수 있음)
                return;
            }

            // 데이터 포인트 위에 마우스가 있는지 확인
            var prevHoveredPoint = _hoveredPoint;
            _hoveredPoint = FindNearestDataPoint(x, y);
            _isHovering = _hoveredPoint != null;

            if (_hoveredPoint != prevHoveredPoint)
            {
                _renderer.HoveredPoint = _hoveredPoint;
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
                SKRect scrollBarArea = _renderer.ScrollBarArea;
                float scrollHandleX = _renderer.ScrollHandleX;
                float scrollHandleWidth = _renderer.ScrollHandleWidth;

                if (ShowScrollBar && EnableScrolling && scrollBarArea.Contains(x, y))
                {
                    if (scrollHandleX <= x && x <= scrollHandleX + scrollHandleWidth)
                    {
                        _isDraggingScrollHandle = true;
                        _scrollManager.DragStartX = x;
                        return;
                    }
                    else
                    {
                        // 스크롤바 클릭 시 핸들 이동
                        _scrollManager.ScrollToPosition(x);
                        return;
                    }
                }

                // 그래프 영역 드래그 시작
                SKRect graphArea = _renderer.GraphArea;
                if (EnableScrolling && graphArea.Contains(x, y))
                {
                    _isDraggingGraph = true;
                    _scrollManager.DragStartX = x;
                    _scrollManager.DragStartMin = _scrollManager.VisibleMinimum;
                    _scrollManager.DragStartMax = _scrollManager.VisibleMaximum;
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
            if (EnableMouseWheelZoom && _renderer.GraphArea.Contains(x, y))
            {
                // 마우스 위치를 중심으로 줌
                _scrollManager.ZoomAtPoint(x, delta);
            }

            base.OnMouseWheel(x, y, delta);
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
            _scrollManager.DataMinimum = min;
            _scrollManager.DataMaximum = max;

            // 보이는 범위가 데이터 범위를 벗어나지 않도록 조정
            _scrollManager.AdjustVisibleRange();
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

            _scrollManager.UpdateVisibleRange(min, max);
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
            SetVisibleXRange(_scrollManager.DataMinimum, _scrollManager.DataMaximum);
        }

        /// <summary>
        /// 주어진 시간만큼 스크롤합니다. (양수: 오른쪽, 음수: 왼쪽)
        /// </summary>
        public void ScrollByTime(TimeSpan amount)
        {
            _scrollManager.ScrollByTime(amount);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 일정 비율로 확대/축소합니다. (factor > 1: 축소, factor < 1: 확대)
        /// </summary>
        public void Zoom(float factor)
        {
            _scrollManager.Zoom(factor);
            Invalidate?.Invoke();
        }

        #endregion

        #region Private Methods

        private void InitializeDefaults()
        {
            DateTime now = DateTime.Now;
            DateTime min = now.AddDays(-30);
            DateTime max = now;

            _scrollManager.DataMinimum = min;
            _scrollManager.DataMaximum = max;
            _scrollManager.VisibleMinimum = now.AddDays(-7);
            _scrollManager.VisibleMaximum = now;

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
            SKRect scrollBarArea;
            if (ShowScrollBar && EnableScrolling)
            {
                scrollBarArea = new SKRect(
                    Bounds.Left + yAxisWidth + Padding,
                    scrollBarTop,
                    Bounds.Right - Padding - (ShowLegend ? legendWidth + Padding : 0),
                    scrollBarBottom
                );
            }
            else
            {
                scrollBarArea = SKRect.Empty;
            }

            // 그래프 영역 계산
            SKRect graphArea = new(
                Bounds.Left + yAxisWidth + Padding,
                Bounds.Top + Padding,
                Bounds.Right - Padding - (ShowLegend ? legendWidth + Padding : 0),
                scrollBarTop - xAxisHeight - (ShowScrollBar ? Padding : 0)
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
            _renderer.GraphArea = graphArea;
            _renderer.LegendArea = legendArea;
            _renderer.ScrollBarArea = scrollBarArea;

            _scrollManager.GraphArea = graphArea;
            _scrollManager.ScrollBarArea = scrollBarArea;

            // 스크롤 핸들 메트릭 업데이트
            _scrollManager.UpdateScrollHandleMetrics();
        }

        private void UpdateRendererProperties()
        {
            // 렌더러에 속성 업데이트
            _renderer.Settings = _settings;
            _renderer.Series = _series;
            _renderer.AnimationProgress = _animationProgress;
            _renderer.VisibleXMin = _scrollManager.VisibleMinimum;
            _renderer.VisibleXMax = _scrollManager.VisibleMaximum;
            _renderer.DisplayYMin = _scrollManager.VisibleMinimum == DateTime.MinValue ? 0 : _settings.YAxisMin;
            _renderer.DisplayYMax = _scrollManager.VisibleMaximum == DateTime.MaxValue ? 100 : _settings.YAxisMax;
            _renderer.ScrollHandleX = _scrollManager.HandleX;
            _renderer.ScrollHandleWidth = _scrollManager.HandleWidth;
            _renderer.HoveredPoint = _hoveredPoint;
        }

        private void UpdateDataRange()
        {
            if (_series.Count == 0 || _series.All(s => s.DataPoints == null || s.DataPoints.Count == 0))
            {
                _settings.YAxisMin = 0;
                _settings.YAxisMax = 100;
                _scrollManager.DataMinimum = DateTime.Now.AddDays(-7);
                _scrollManager.DataMaximum = DateTime.Now;
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

            _scrollManager.DataMinimum = minDate;
            _scrollManager.DataMaximum = maxDate;

            // 초기 보이는 범위 설정 (전체 데이터 중에서 마지막 7일)
            if (_scrollManager.VisibleMinimum == DateTime.MinValue || _scrollManager.VisibleMaximum == DateTime.MaxValue)
            {
                // 전체 데이터 범위가 7일보다 작으면 전체 표시
                if ((maxDate - minDate).TotalDays <= 7)
                {
                    _scrollManager.VisibleMinimum = minDate;
                    _scrollManager.VisibleMaximum = maxDate;
                }
                else
                {
                    // 마지막 7일 표시
                    _scrollManager.VisibleMaximum = maxDate;
                    _scrollManager.VisibleMinimum = maxDate.AddDays(-7);
                }
            }
            else
            {
                // 보이는 범위가 데이터 범위를 벗어나지 않도록 조정
                _scrollManager.AdjustVisibleRange();
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
                _settings.YAxisMin = Math.Max(minValue - padding, 0); // 음수 방지
                _settings.YAxisMax = maxValue + padding;

                // 눈금 값을 깔끔하게 조정
                AdjustYAxisRange();
            }
        }

        private void AdjustYAxisRange()
        {
            // 눈금 간격 계산
            double range = _settings.YAxisMax - _settings.YAxisMin;
            double step = CalculateNiceStep(range / GridLineCount);

            // 최소값을 내려서 눈금에 맞춤
            _settings.YAxisMin = Math.Floor(_settings.YAxisMin / step) * step;

            // 최대값을 올려서 눈금에 맞춤
            _settings.YAxisMax = Math.Ceiling(_settings.YAxisMax / step) * step;
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

        private TrendDataPoint? FindNearestDataPoint(float x, float y)
        {
            const float HitDistance = 20f; // 픽셀 단위 검색 거리

            TrendDataPoint? closestPoint = null;
            float minDistance = float.MaxValue;

            foreach (var series in _series)
            {
                if (series.DataPoints == null) continue;

                // 보이는 범위 내의 포인트만 검사
                foreach (var point in series.DataPoints.Where(p =>
                    p.Timestamp >= _scrollManager.VisibleMinimum &&
                    p.Timestamp <= _scrollManager.VisibleMaximum))
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

        private float MapXToCanvas(DateTime time)
        {
            DateTime min = _scrollManager.VisibleMinimum;
            DateTime max = _scrollManager.VisibleMaximum;

            if (max == min)
                return _renderer.GraphArea.Left;

            double normalizedX = (time - min).TotalMilliseconds / (max - min).TotalMilliseconds;
            return (float)(_renderer.GraphArea.Left + normalizedX * _renderer.GraphArea.Width);
        }

        private float MapYToCanvas(double value)
        {
            double min = _settings.YAxisMin;
            double max = _settings.YAxisMax;

            if (max == min)
                return _renderer.GraphArea.Bottom;

            double normalizedY = (value - min) / (max - min);
            return (float)(_renderer.GraphArea.Bottom - normalizedY * _renderer.GraphArea.Height);
        }

        private string GetNextColor()
        {
            string[] defaultColors = ["primary", "success", "warning", "danger", "info"];
            int index = _series.Count % defaultColors.Length;
            return defaultColors[index];
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 리소스를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            _renderer.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}