using System;
using SkiaSharp;

namespace Going.UI.Controls.TrendGraph
{
    /// <summary>
    /// 트렌드 그래프 표시 및 동작 설정을 관리하는 클래스
    /// </summary>
    public class TrendGraphProperties
    {
        #region 그래프 외관 설정

        /// <summary>
        /// 그래프 배경색
        /// </summary>
        /// <remarks>
        /// 그래프 영역 전체의 배경색으로, 가독성과 UI 일관성을 위해 설정
        /// </remarks>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// 그래프 테두리색
        /// </summary>
        /// <remarks>
        /// 그래프 영역을 구분하기 위한 테두리 색상
        /// </remarks>
        public SKColor BorderColor { get; set; } = SKColors.LightGray;

        /// <summary>
        /// 그래프 테두리 두께
        /// </summary>
        /// <remarks>
        /// 그래프 영역의 테두리 두께(픽셀 단위)
        /// </remarks>
        public float BorderWidth { get; set; } = 1.0f;

        /// <summary>
        /// 그래프 영역 여백
        /// </summary>
        /// <remarks>
        /// 그래프 데이터가 표시되는 영역의 내부 여백으로, 데이터가 테두리에 너무 붙지 않도록 함
        /// </remarks>
        public float Padding { get; set; } = 10.0f;

        #endregion

        #region 그리드 설정

        /// <summary>
        /// 그리드 표시 여부
        /// </summary>
        /// <remarks>
        /// 그래프 내 수평/수직 그리드 라인 표시 여부
        /// </remarks>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// 수평 그리드 라인 색상
        /// </summary>
        /// <remarks>
        /// Y축 값을 참조하기 위한 수평 그리드 라인의 색상
        /// </remarks>
        public SKColor HorizontalGridColor { get; set; } = new SKColor(230, 230, 230);

        /// <summary>
        /// 수직 그리드 라인 색상
        /// </summary>
        /// <remarks>
        /// X축 값을 참조하기 위한 수직 그리드 라인의 색상
        /// </remarks>
        public SKColor VerticalGridColor { get; set; } = new SKColor(230, 230, 230);

        /// <summary>
        /// 그리드 라인 두께
        /// </summary>
        /// <remarks>
        /// 그리드 라인의 두께(픽셀 단위)로, 너무 두꺼우면 데이터 가시성을 해칠 수 있음
        /// </remarks>
        public float GridLineWidth { get; set; } = 0.5f;

        /// <summary>
        /// 수평 그리드 라인 간격 (값 단위)
        /// </summary>
        /// <remarks>
        /// Y축 방향으로 그리드 라인 간의 값 간격, 0이면 자동 계산
        /// </remarks>
        public float HorizontalGridInterval { get; set; } = 0;

        /// <summary>
        /// 수직 그리드 라인 간격 (시간 단위, 초)
        /// </summary>
        /// <remarks>
        /// X축 방향으로 그리드 라인 간의 시간 간격(초), 0이면 자동 계산
        /// </remarks>
        public double VerticalGridInterval { get; set; } = 0;

        /// <summary>
        /// 그리드 대시 패턴 설정
        /// </summary>
        /// <remarks>
        /// 점선 그리드 라인을 위한 패턴 설정, null이면 실선
        /// </remarks>
        public float[] GridDashPattern { get; set; } = new float[] { 4, 2 };

        #endregion

        #region 축 설정

        // X축 표시 여부 : 시간축(X축) 표시 여부
        public bool ShowXAxis { get; set; } = true;

        // Y축 표시 여부 : 값축(Y축) 표시 여부
        public bool ShowYAxis { get; set; } = true;

        // X축 색상 : 시간축(X축)의 선 색상
        public SKColor XAxisColor { get; set; } = SKColors.Black;

        // Y축 색상 : 값축(Y축)의 선 색상
        public SKColor YAxisColor { get; set; } = SKColors.Black;

        // X축 선 두께 : 시간축(X축)의 선 두께
        public float AxisLineWidth { get; set; } = 1.0f;

        // X축 라벨 글꼴 크기 : 시간축(X축) 라벨의 글꼴 크기
        public float AxisLabelFontSize { get; set; } = 12.0f;

        // X축 라벨 색상 : 시간축(X축) 라벨의 텍스트 색상
        public SKColor AxisLabelColor { get; set; } = SKColors.Gray;

        // X축 시간 형식 : 시간축(X축) 라벨의 날짜/시간 형식 문자열 (예: "HH:mm:ss", "yyyy-MM-dd")
        public string TimeFormat { get; set; } = "HH:mm:ss";

        /// <summary>
        /// Y축 값 형식
        /// </summary>
        /// <remarks>
        /// 값축(Y축) 라벨의 숫자 형식 문자열 (예: "0.00", "N2")
        /// </remarks>
        public string ValueFormat { get; set; } = "0.##";

        /// <summary>
        /// 줌 레벨에 따른 X축 형식 자동 변경 사용 여부
        /// </summary>
        /// <remarks>
        /// 활성화하면 줌 레벨에 따라 시간 형식이 자동으로 조정됨 (초/분/시간/일/월)
        /// </remarks>
        public bool UseAdaptiveTimeFormat { get; set; } = true;

        #endregion

        #region 데이터 표시 설정

        /// <summary>
        /// 기본 라인 두께
        /// </summary>
        /// <remarks>
        /// 데이터 시리즈 라인의 기본 두께(픽셀 단위)
        /// </remarks>
        public float DefaultLineWidth { get; set; } = 2.0f;

        /// <summary>
        /// 기본 라인 색상
        /// </summary>
        /// <remarks>
        /// 데이터 시리즈 라인의 기본 색상, 시리즈별로 다른 색상을 지정하지 않은 경우 사용
        /// </remarks>
        public SKColor DefaultLineColor { get; set; } = SKColors.Blue;

        /// <summary>
        /// 데이터 포인트 표시 여부
        /// </summary>
        /// <remarks>
        /// 각 데이터 포인트에 마커를 표시할지 여부
        /// </remarks>
        public bool ShowDataPoints { get; set; } = false;

        /// <summary>
        /// 데이터 포인트 크기
        /// </summary>
        /// <remarks>
        /// 데이터 포인트 마커의 크기(픽셀 단위)
        /// </remarks>
        public float DataPointSize { get; set; } = 4.0f;

        /// <summary>
        /// 선택된 데이터 포인트 표시 여부
        /// </summary>
        /// <remarks>
        /// 사용자가 선택한 데이터 포인트에 강조 표시할지 여부
        /// </remarks>
        public bool HighlightSelectedPoint { get; set; } = true;

        /// <summary>
        /// 선택된 데이터 포인트 색상
        /// </summary>
        /// <remarks>
        /// 선택된 데이터 포인트의 강조 표시 색상
        /// </remarks>
        public SKColor SelectedPointColor { get; set; } = SKColors.Red;

        /// <summary>
        /// 선택된 데이터 포인트 크기
        /// </summary>
        /// <remarks>
        /// 선택된 데이터 포인트 마커의 크기(픽셀 단위)
        /// </remarks>
        public float SelectedPointSize { get; set; } = 8.0f;

        /// <summary>
        /// 영역 채우기 표시 여부
        /// </summary>
        /// <remarks>
        /// 라인 그래프 아래 영역을 색상으로 채울지 여부
        /// </remarks>
        public bool FillArea { get; set; } = false;

        /// <summary>
        /// 영역 채우기 알파값
        /// </summary>
        /// <remarks>
        /// 영역 채우기 색상의 투명도 (0~255)
        /// </remarks>
        public byte FillAreaAlpha { get; set; } = 80;

        /// <summary>
        /// 데이터가 없는 구간 연결 여부
        /// </summary>
        /// <remarks>
        /// 데이터가 없는 구간을 직선으로 연결할지 여부. false면 구간이 끊어짐
        /// </remarks>
        public bool ConnectNullData { get; set; } = true;

        #endregion

        #region 범례 설정

        /// <summary>
        /// 범례 표시 여부
        /// </summary>
        /// <remarks>
        /// 여러 데이터 시리즈를 구분하는 범례 표시 여부
        /// </remarks>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// 범례 위치
        /// </summary>
        /// <remarks>
        /// 범례가 표시될 위치 (Top, Bottom, Left, Right)
        /// </remarks>
        public LegendPosition LegendPosition { get; set; } = LegendPosition.Top;

        /// <summary>
        /// 범례 글꼴 크기
        /// </summary>
        /// <remarks>
        /// 범례 텍스트의 글꼴 크기(픽셀 단위)
        /// </remarks>
        public float LegendFontSize { get; set; } = 12.0f;

        /// <summary>
        /// 범례 텍스트 색상
        /// </summary>
        /// <remarks>
        /// 범례 텍스트의 색상
        /// </remarks>
        public SKColor LegendTextColor { get; set; } = SKColors.Black;

        /// <summary>
        /// 범례 배경색
        /// </summary>
        /// <remarks>
        /// 범례 배경 영역의 색상
        /// </remarks>
        public SKColor LegendBackgroundColor { get; set; } = new SKColor(240, 240, 240, 200);

        #endregion

        #region 상호작용 설정

        /// <summary>
        /// 툴팁 표시 여부
        /// </summary>
        /// <remarks>
        /// 마우스 오버 또는 탭 시 데이터 포인트 정보 툴팁 표시 여부
        /// </remarks>
        public bool ShowTooltip { get; set; } = true;

        /// <summary>
        /// 툴팁 배경색
        /// </summary>
        /// <remarks>
        /// 툴팁 배경 영역의 색상
        /// </remarks>
        public SKColor TooltipBackgroundColor { get; set; } = new SKColor(50, 50, 50, 220);

        /// <summary>
        /// 툴팁 텍스트 색상
        /// </summary>
        /// <remarks>
        /// 툴팁 내 텍스트의 색상
        /// </remarks>
        public SKColor TooltipTextColor { get; set; } = SKColors.White;

        /// <summary>
        /// 툴팁 글꼴 크기
        /// </summary>
        /// <remarks>
        /// 툴팁 텍스트의 글꼴 크기(픽셀 단위)
        /// </remarks>
        public float TooltipFontSize { get; set; } = 12.0f;

        /// <summary>
        /// 터치/클릭 감지 반경
        /// </summary>
        /// <remarks>
        /// 데이터 포인트 선택 시 허용되는 터치/클릭 오차 범위(픽셀 단위)
        /// </remarks>
        public float TouchDetectionRadius { get; set; } = 15.0f;

        /// <summary>
        /// 스크롤 민감도
        /// </summary>
        /// <remarks>
        /// 스와이프/드래그 시 스크롤 속도 계수, 높을수록 빠르게 스크롤됨
        /// </remarks>
        public float ScrollSensitivity { get; set; } = 1.0f;

        /// <summary>
        /// 줌 민감도
        /// </summary>
        /// <remarks>
        /// 핀치/휠 확대/축소 시 줌 속도 계수, 높을수록 빠르게 확대/축소됨
        /// </remarks>
        public float ZoomSensitivity { get; set; } = 1.0f;

        /// <summary>
        /// 크로스헤어 표시 여부
        /// </summary>
        /// <remarks>
        /// 선택한 데이터 포인트에 수직/수평 참조선 표시 여부
        /// </remarks>
        public bool ShowCrosshair { get; set; } = false;

        /// <summary>
        /// 크로스헤어 색상
        /// </summary>
        /// <remarks>
        /// 크로스헤어 참조선의 색상
        /// </remarks>
        public SKColor CrosshairColor { get; set; } = new SKColor(100, 100, 100, 150);

        /// <summary>
        /// 크로스헤어 선 두께
        /// </summary>
        /// <remarks>
        /// 크로스헤어 참조선의 두께(픽셀 단위)
        /// </remarks>
        public float CrosshairWidth { get; set; } = 1.0f;

        /// <summary>
        /// 크로스헤어 대시 패턴
        /// </summary>
        /// <remarks>
        /// 크로스헤어 참조선의 점선 패턴, null이면 실선
        /// </remarks>
        public float[] CrosshairDashPattern { get; set; } = new float[] { 4, 2 };

        #endregion

        #region 성능 최적화 설정

        /// <summary>
        /// 데이터 다운샘플링 사용 여부
        /// </summary>
        /// <remarks>
        /// 너무 많은 데이터 포인트가 있을 때 성능 최적화를 위해 다운샘플링 활성화
        /// </remarks>
        public bool UseDownsampling { get; set; } = true;

        /// <summary>
        /// 다운샘플링 임계값
        /// </summary>
        /// <remarks>
        /// 화면 픽셀당 이 이상의 데이터 포인트가 있을 때 다운샘플링 시작
        /// </remarks>
        public float DownsamplingThreshold { get; set; } = 0.5f;

        /// <summary>
        /// 렌더링 품질
        /// </summary>
        /// <remarks>
        /// 그래픽 렌더링 품질 설정 (Low, Medium, High), 성능과 품질 간 균형을 조절
        /// </remarks>
        public RenderQuality RenderingQuality { get; set; } = RenderQuality.Medium;

        /// <summary>
        /// 안티앨리어싱 사용 여부
        /// </summary>
        /// <remarks>
        /// 선과 텍스트의 가장자리 부드럽게 처리, 비활성화하면 성능 향상
        /// </remarks>
        public bool UseAntialiasing { get; set; } = true;

        #endregion

        #region 생성자

        /// <summary>
        /// 기본 설정으로 TrendGraphSettings 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public TrendGraphProperties()
        {
            // 기본 값으로 초기화됨
        }

        #endregion
    }

    /// <summary>
    /// 범례 위치 열거형
    /// </summary>
    public enum LegendPosition
    {
        /// <summary>
        /// 상단에 표시
        /// </summary>
        Top,

        /// <summary>
        /// 하단에 표시
        /// </summary>
        Bottom,

        /// <summary>
        /// 좌측에 표시
        /// </summary>
        Left,

        /// <summary>
        /// 우측에 표시
        /// </summary>
        Right
    }

    /// <summary>
    /// 렌더링 품질 열거형
    /// </summary>
    public enum RenderQuality
    {
        /// <summary>
        /// 저품질 (성능 최적화)
        /// </summary>
        Low,

        /// <summary>
        /// 중간 품질 (균형잡힌 설정)
        /// </summary>
        Medium,

        /// <summary>
        /// 고품질 (최상의 시각적 품질)
        /// </summary>
        High
    }
}