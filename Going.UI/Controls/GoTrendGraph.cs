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
    /// 매 프레임 렌더링에 최적화됨
    /// </summary>
    public class GoTrendGraph : GoControl, IDisposable
    {
        #region Private Fields

        private readonly TrendRenderer _renderer;
        private readonly TrendScrollManager _scrollManager;
        private readonly TrendGraphSettings _settings;
        private readonly List<TrendSeries> _series;

        private bool _isHovering;
        private TrendDataPoint? _hoveredPoint;
        private float _mx, _my;   // 마우스 위치 추적
        private bool _isDraggingGraph;
        private bool _isDraggingScrollHandle;
        private DateTime _animationStartTime;
        private float _animationProgress = 1.0f;
        private bool _needsLayoutUpdate = true;
        private bool _disposed;

        // 애니메이션 최적화
        private long _lastFrameTimeTicks;
        private float _targetAnimationProgress = 1.0f;

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
                if (_settings.IconString != value)
                {
                    _settings.IconString = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.IconSize != value)
                {
                    _settings.IconSize = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.IconGap != value)
                {
                    _settings.IconGap = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.IconDirection != value)
                {
                    _settings.IconDirection = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.Text != value)
                {
                    _settings.Text = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.TextFontName != value)
                {
                    _settings.TextFontName = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.TextFontSize != value)
                {
                    _settings.TextFontSize = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.BackgroundDraw != value)
                {
                    _settings.BackgroundDraw = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.BorderOnly != value)
                {
                    _settings.BorderOnly = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.BgColor != value)
                {
                    _settings.BgColor = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.BorderColor != value)
                {
                    _settings.BorderColor = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.Round != value)
                {
                    _settings.Round = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.AxisColor != value)
                {
                    _settings.AxisColor = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.GridColor != value)
                {
                    _settings.GridColor = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ShowGrid != value)
                {
                    _settings.ShowGrid = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ShowLegend != value)
                {
                    _settings.ShowLegend = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
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
                if (_settings.GridLineCount != value)
                {
                    _settings.GridLineCount = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.EnableShadow != value)
                {
                    _settings.EnableShadow = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.EnableAnimation != value)
                {
                    _settings.EnableAnimation = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.AnimationDuration != value)
                {
                    _settings.AnimationDuration = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.LineThickness != value)
                {
                    _settings.LineThickness = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.FillArea != value)
                {
                    _settings.FillArea = value;
                    Invalidate?.Invoke();
                }
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
                int clampedValue = Math.Clamp(value, 0, 255);
                if (_settings.FillOpacity != clampedValue)
                {
                    _settings.FillOpacity = clampedValue;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ShowDataPoints != value)
                {
                    _settings.ShowDataPoints = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.DataPointRadius != value)
                {
                    _settings.DataPointRadius = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ShowLabels != value)
                {
                    _settings.ShowLabels = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.Padding != value)
                {
                    _settings.Padding = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
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
                if (_settings.FontName != value)
                {
                    _settings.FontName = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.FontSize != value)
                {
                    _settings.FontSize = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
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
                if (_settings.TextColor != value)
                {
                    _settings.TextColor = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ValueFormat != value)
                {
                    _settings.ValueFormat = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.XAxisTitle != value)
                {
                    _settings.XAxisTitle = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
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
                if (_settings.YAxisTitle != value)
                {
                    _settings.YAxisTitle = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
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
                if (_settings.AutoScale != value)
                {
                    _settings.AutoScale = value;
                    UpdateDataRange();
                    Invalidate?.Invoke();
                }
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
                if (_settings.YAxisMin != value)
                {
                    _settings.YAxisMin = value;
                    if (!AutoScale)
                    {
                        UpdateDataRange();
                        Invalidate?.Invoke();
                    }
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
                if (_settings.YAxisMax != value)
                {
                    _settings.YAxisMax = value;
                    if (!AutoScale)
                    {
                        UpdateDataRange();
                        Invalidate?.Invoke();
                    }
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
                if (_settings.EnableScrolling != value)
                {
                    _settings.EnableScrolling = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
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
                if (_settings.EnableMouseWheelZoom != value)
                {
                    _settings.EnableMouseWheelZoom = value;
                    Invalidate?.Invoke();
                }
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
                double clampedValue = Math.Clamp(value, _settings.MinTimeRange, _settings.MaxTimeRange);
                if (_settings.VisibleTimeRange != clampedValue)
                {
                    _settings.VisibleTimeRange = clampedValue;
                    Invalidate?.Invoke();
                }
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
                if (_settings.MinTimeRange != value)
                {
                    _settings.MinTimeRange = value;
                    _scrollManager.MinTimeRange = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.MaxTimeRange != value)
                {
                    _settings.MaxTimeRange = value;
                    _scrollManager.MaxTimeRange = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ScrollBarHeight != value)
                {
                    _settings.ScrollBarHeight = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ScrollBarColor != value)
                {
                    _settings.ScrollBarColor = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ScrollHandleColor != value)
                {
                    _settings.ScrollHandleColor = value;
                    Invalidate?.Invoke();
                }
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
                if (_settings.ShowScrollBar != value)
                {
                    _settings.ShowScrollBar = value;
                    _needsLayoutUpdate = true;
                    Invalidate?.Invoke();
                }
            }
        }
        #endregion

        #region 데이터 속성

        /// <summary>
        /// 시리즈 데이터 목록
        /// </summary>
        public IReadOnlyList<TrendSeries> Series => _series.AsReadOnly();

        /// <summary>
        /// 현재 보이는 시간 범위의 최소값
        /// </summary>
        public DateTime VisibleMinimum => _scrollManager.VisibleMinimum;

        /// <summary>
        /// 현재 보이는 시간 범위의 최대값
        /// </summary>
        public DateTime VisibleMaximum => _scrollManager.VisibleMaximum;

        /// <summary>
        /// 전체 데이터 범위의 최소값
        /// </summary>
        public DateTime DataMinimum => _scrollManager.DataMinimum;

        /// <summary>
        /// 전체 데이터 범위의 최대값
        /// </summary>
        public DateTime DataMaximum => _scrollManager.DataMaximum;

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

            _settings = new TrendGraphSettings();
            _renderer = new TrendRenderer();
            _scrollManager = new TrendScrollManager();
            _series = new List<TrendSeries>();

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
            try
            {
                if (canvas == null)
                    return;

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
            try
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
            try
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
                    var series = FindSeriesForDataPoint(_hoveredPoint.Value);
                    DataPointSelected?.Invoke(this, new DataPointEventArgs(series, _hoveredPoint.Value));
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
            _isDraggingScrollHandle = false;
            _isDraggingGraph = false;
            base.OnMouseUp(x, y, button);
        }
        #endregion

        #region OnMouseWheel
        protected override void OnMouseWheel(float x, float y, float delta)
        {
            try
            {
                if (EnableMouseWheelZoom && _renderer.GraphArea.Contains(x, y))
                {
                    // 마우스 위치를 중심으로 줌
                    _scrollManager.ZoomAtPoint(x, delta);
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
                    StartAnimation();
                }

                // 이벤트 발생 및 다시 그리기
                DataChanged?.Invoke(this, EventArgs.Empty);
                _needsLayoutUpdate = true;
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

            var series = _series.FirstOrDefault(s => s.Name == seriesName);
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

                _scrollManager.UpdateVisibleRange(min, max);
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
            if (factor <= 0) throw new ArgumentException("Factor must be positive", nameof(factor));

            _scrollManager.Zoom(factor);
            Invalidate?.Invoke();
        }

        /// <summary>
        /// 현재 보이는 시간 범위를 가져옵니다.
        /// </summary>
        public TimeSpan GetVisibleTimeSpan()
        {
            return _scrollManager.VisibleMaximum - _scrollManager.VisibleMinimum;
        }

        /// <summary>
        /// 애니메이션 시작
        /// </summary>
        public void StartAnimation(float targetProgress = 1.0f)
        {
            if (!EnableAnimation)
                return;

            _animationProgress = 0f;
            _targetAnimationProgress = targetProgress;
            _animationStartTime = DateTime.Now;
            _lastFrameTimeTicks = _animationStartTime.Ticks;
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

            foreach (var series in _series)
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

            _scrollManager.DataMinimum = min;
            _scrollManager.DataMaximum = max;
            _scrollManager.UpdateVisibleRange(now.AddDays(-7), now);

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

                _series.Add(series1);
                _series.Add(series2);

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
                float legendHeight = _series.Count * (FontSize + 10) + 20;  // 레전드 영역 높이

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
                _renderer.GraphArea = graphArea;
                _renderer.LegendArea = legendArea;
                _renderer.ScrollBarArea = scrollBarArea;

                _scrollManager.GraphArea = graphArea;
                _scrollManager.ScrollBarArea = scrollBarArea;

                // 스크롤 핸들 메트릭 업데이트
                _scrollManager.UpdateScrollHandleMetrics();
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
            _renderer.Settings = _settings;
            _renderer.Series = _series;
            _renderer.AnimationProgress = _animationProgress;
            _renderer.VisibleXMin = _scrollManager.VisibleMinimum;
            _renderer.VisibleXMax = _scrollManager.VisibleMaximum;
            _renderer.DisplayYMin = _settings.YAxisMin;
            _renderer.DisplayYMax = _settings.YAxisMax;
            _renderer.ScrollHandleX = _scrollManager.HandleX;
            _renderer.ScrollHandleWidth = _scrollManager.HandleWidth;
            _renderer.HoveredPoint = _hoveredPoint;

            // 매핑 캐시 무효화 (Y축 범위 변경 시)
            _renderer.InvalidateYMappingCache();
        }

        private void UpdateDataRange()
        {
            try
            {
                // 아무 데이터도 없는 경우 기본값 사용
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

                _scrollManager.DataMinimum = minDate;
                _scrollManager.DataMaximum = maxDate;

                // 초기 보이는 범위 설정 (전체 데이터 중에서 마지막 7일)
                if (_scrollManager.VisibleMinimum == DateTime.MinValue || _scrollManager.VisibleMaximum == DateTime.MaxValue)
                {
                    // 전체 데이터 범위가 7일보다 작으면 전체 표시
                    if ((maxDate - minDate).TotalDays <= 7)
                    {
                        _scrollManager.UpdateVisibleRange(minDate, maxDate);
                    }
                    else
                    {
                        // 마지막 7일 표시
                        _scrollManager.UpdateVisibleRange(maxDate.AddDays(-7), maxDate);
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
            catch (Exception ex)
            {
                // 데이터 범위 업데이트 오류 - 기본값 설정
                _settings.YAxisMin = 0;
                _settings.YAxisMax = 100;
                System.Diagnostics.Debug.WriteLine($"데이터 범위 업데이트 오류: {ex.Message}");
            }
        }

        private void AdjustYAxisRange()
        {
            // 눈금 간격 계산
            double range = _settings.YAxisMax - _settings.YAxisMin;
            double step = CalculateNiceStep(range / Math.Max(1, GridLineCount));

            // 최소값을 내려서 눈금에 맞춤
            _settings.YAxisMin = Math.Floor(_settings.YAxisMin / step) * step;

            // 최대값을 올려서 눈금에 맞춤
            _settings.YAxisMax = Math.Ceiling(_settings.YAxisMax / step) * step;
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
            if (!EnableAnimation || _animationProgress >= _targetAnimationProgress)
                return;

            try
            {
                // 현재 시간
                DateTime now = DateTime.Now;
                long currentTimeTicks = now.Ticks;

                // 애니메이션 정지된 경우 (창 최소화 등)
                if (_lastFrameTimeTicks == 0)
                {
                    _lastFrameTimeTicks = currentTimeTicks;
                    return;
                }

                // 마지막 프레임 이후 경과 시간 (밀리초)
                double elapsed = (currentTimeTicks - _lastFrameTimeTicks) / TimeSpan.TicksPerMillisecond;

                // 너무 큰 시간 간격 제한 (창 최소화 후 복원 등의 경우)
                elapsed = Math.Min(elapsed, AnimationDuration / 2.0);

                // 진행률 업데이트
                float progressDelta = (float)(elapsed / AnimationDuration);
                _animationProgress = Math.Min(_targetAnimationProgress, _animationProgress + progressDelta);

                // 마지막 프레임 시간 업데이트
                _lastFrameTimeTicks = currentTimeTicks;

                // 애니메이션이 완료되지 않았으면 다시 그리기 요청
                if (_animationProgress < _targetAnimationProgress)
                {
                    Invalidate?.Invoke();
                }
            }
            catch (Exception ex)
            {
                // 애니메이션 오류 - 진행률을 최대로 설정하고 계속 진행
                _animationProgress = _targetAnimationProgress;
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
                if (!_renderer.GraphArea.Contains(x, y))
                    return null;

                // 보이는 범위의 데이터만 검색
                foreach (var series in _series)
                {
                    if (series.DataPoints == null || series.DataPoints.Count == 0)
                        continue;

                    // 성능 최적화: 보이는 범위의 데이터만 검색
                    foreach (var point in series.GetVisiblePoints(_scrollManager.VisibleMinimum, _scrollManager.VisibleMaximum))
                    {
                        float px = _scrollManager.TimeToCanvasX(point.Timestamp);
                        float py = _renderer.MapYToCanvas(point.Value);

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
            foreach (var series in _series)
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
            int index = _series.Count % defaultColors.Length;
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
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                    _renderer.Dispose();
                    _scrollManager.Dispose();

                    // 이벤트 핸들러 해제
                    DataChanged = null;
                    DataPointSelected = null;
                    VisibleRangeChanged = null;
                }

                _disposed = true;
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
}