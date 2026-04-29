using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Managers;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Going.UI.Datas
{
    #region Base
    #region class : GoDataGridCell
    /// <summary>
    /// 데이터 그리드 셀의 추상 기본 클래스입니다.
    /// </summary>
    public abstract class GoDataGridCell
    {
        #region Const
        /// <summary>입력 영역의 가로 Inflate 값.</summary>
        protected const float InputInflateW = -1;
        /// <summary>입력 영역의 세로 Inflate 값.</summary>
        protected const float InputInflateH = -1;
        #endregion

        #region Properties
        /// <summary>셀의 이름 (데이터 바인딩 프로퍼티명)</summary>
        public string? Name { get; set; }
        /// <summary>키보드 입력을 사용하는 셀인지 여부</summary>
        public virtual bool IsKeyboardInput { get; } = false;

        /// <summary>열 병합 수</summary>
        public int ColSpan { get; set; } = 1;
        /// <summary>행 병합 수</summary>
        public int RowSpan { get; set; } = 1;
        /// <summary>셀이 속한 열 인덱스</summary>
        [JsonIgnore] public int ColumnIndex => Row.Cells.IndexOf(this);
        /// <summary>셀이 속한 행 인덱스</summary>
        [JsonIgnore] public int RowIndex => Row.RowIndex;

        /// <summary>셀 표시 여부</summary>
        public bool Visible { get; set; } = true;
        /// <summary>셀 활성화 여부</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>사용자 정의 데이터를 저장하기 위한 태그 객체</summary>
        [JsonIgnore] public object? Tag { get; set; }

        /// <summary>셀 배경색</summary>
        public string CellBackColor { get; set; }
        /// <summary>선택 시 셀 배경색</summary>
        public string SelectedCellBackColor { get; set; }
        /// <summary>셀 텍스트 색상</summary>
        public string CellTextColor { get; set; }

        /// <summary>셀이 속한 데이터 그리드</summary>
        [JsonIgnore] public GoDataGrid Grid { get; private set; }
        /// <summary>셀이 속한 행</summary>
        [JsonIgnore] public GoDataGridRow Row { get; private set; }
        /// <summary>셀이 속한 열</summary>
        [JsonIgnore] public GoDataGridColumn Column { get; private set; }

        /// <summary>셀 값 바인딩에 사용되는 프로퍼티 정보.</summary>
        [JsonIgnore] protected PropertyInfo? ValueInfo { get; }
        /// <summary>셀에 바인딩된 데이터 값</summary>
        [JsonIgnore]
        public object? Value
        {
            get => Row.Source != null && ValueInfo != null ? ValueInfo.GetValue(Row.Source) : null;
            set
            {
                if (ValueInfo != null && ValueInfo.CanWrite)
                    ValueInfo.SetValue(Row.Source, value);
            }
        }

        /// <summary>셀의 화면 영역</summary>
        [JsonIgnore]
        public SKRect Bounds
        {
            get
            {
                var ci = ColumnIndex;
                var l = Grid.Columns[ci].Bounds.Left;
                var r = Grid.Columns[ci + ColSpan - 1].Bounds.Right;

                var ri = RowIndex;
                var t = Grid.ViewRows[ri].Bounds.Top;
                var b = Grid.ViewRows[ri + RowSpan- 1].Bounds.Bottom;

                return new SKRect(l, t, r, b);
            }
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        #endregion

        #region Constructor
        /// <summary>
        /// 데이터 그리드 셀을 초기화합니다.
        /// </summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column)
        {
            this.Grid = Grid;
            this.Row = Row;
            this.Column = Column;

            if (Grid.DataType != null && !(Column is GoDataGridButtonColumn) && Column?.Name != null)
                ValueInfo = Grid.DataType.GetProperty(Column.Name);

            CellBackColor = Grid.RowColor;
            SelectedCellBackColor = Grid.SelectedRowColor;
            CellTextColor = Grid.TextColor;
        }
        #endregion

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas, GoTheme thm)
        {
            if (Grid != null)
            {
                var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
                var cBack = thm.ToColor(CellBackColor ?? Grid.RowColor);
                var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
                var cF = Row.Selected ? cSel : cBack;
                var cV = cF.BrightnessTransmit(RowIndex % 2 == 0 ? 0.05F : -0.05F);
                var cB = cV.BrightnessTransmit(thm.Dark ? GoDataGrid.BorderBright : -GoDataGrid.BorderBright);
                
                Util.DrawBox(canvas, Bounds, cV, GoRoundType.Rect, thm.Corner);

                using var pe = SKPathEffect.CreateDash([2, 2,], 3);
                using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1, Color = cB, PathEffect = pe };
                var l = Convert.ToInt32(Bounds.Left) + 0.5F;
                var r = Convert.ToInt32(Bounds.Right) + 0.5F;
                var ci = ColumnIndex;
                if (ci > 0) canvas.DrawLine(l, Bounds.Top, l, Bounds.Bottom, p);
                if (ci < Grid.Columns.Count-1) canvas.DrawLine(r, Bounds.Top, r, Bounds.Bottom, p);
                p.PathEffect = null;
                OnDraw(canvas, thm);

                if(!Enabled)
                {
                    Util.DrawBox(canvas, Bounds, Util.FromArgb(GoTheme.DisableAlpha, cBack), GoRoundType.Rect, thm.Corner);
                }
            }
        }
        #endregion

        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button)
        {
            if (CollisionTool.Check(Bounds, x, y) && Enabled)
            {
                bDown = true;
                OnMouseDown(x, y, button);
            }
        }

        internal void MouseUp(float x, float y, GoMouseButton button)
        {
            if (bDown && Enabled)
            {
                bDown = false;
                OnMouseUp(x, y, button);
            }
        }

        internal void MouseClick(float x, float y, GoMouseButton button)
        {
            if (Enabled)
                OnMouseClick(x, y, button);
        }

        internal void MouseMove(float x, float y)
        {
            if (Enabled)
                OnMouseMove(x, y);
        }
        #endregion
        #endregion

        #region Virtual
        /// <summary>셀을 그릴 때 호출되는 가상 메서드입니다.</summary>
        /// <param name="canvas">그리기에 사용되는 캔버스</param>
        /// <param name="thm">현재 테마</param>
        protected virtual void OnDraw(SKCanvas canvas, GoTheme thm) { }
        /// <summary>마우스 버튼 누름 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">눌러진 마우스 버튼</param>
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { }
        /// <summary>마우스 버튼 놓음 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">놓아진 마우스 버튼</param>
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { }
        /// <summary>마우스 이동 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        protected virtual void OnMouseMove(float x, float y) { }
        /// <summary>마우스 클릭 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭된 마우스 버튼</param>
        protected virtual void OnMouseClick(float x, float y, GoMouseButton button) { }
        #endregion
        #endregion
    }
    #endregion
    #region class : GoDataGridColumn
    /// <summary>
    /// 데이터 그리드 열의 추상 기본 클래스입니다.
    /// </summary>
    public abstract class GoDataGridColumn
    {
        #region Const
        /// <summary>입력 영역의 가로 Inflate 값.</summary>
        protected const float InputInflateW = -1;
        /// <summary>입력 영역의 세로 Inflate 값.</summary>
        protected const float InputInflateH = -1;
        #endregion

        #region Properties
        /// <summary>열의 이름 (데이터 바인딩 프로퍼티명)</summary>
        [GoProperty(PCategory.Basic, 0)] public string? Name { get; set; }
        /// <summary>열 그룹 이름 (헤더 그룹화에 사용)</summary>
        public string? GroupName { get; set; }
        /// <summary>열 헤더에 표시할 텍스트</summary>
        [GoProperty(PCategory.Basic, 1)] public string? HeaderText { get; set; }
        /// <summary>열 너비 (퍼센트 또는 픽셀)</summary>
        [GoSizeProperty(PCategory.Basic, 2)] public string? Size { get; set; } = "100%";

        /// <summary>필터 사용 여부</summary>
        [GoProperty(PCategory.Control, 0)] public bool UseFilter { get; set; }
        /// <summary>필터 텍스트</summary>
        public string? FilterText { get; set; }

        /// <summary>정렬 사용 여부</summary>
        [GoProperty(PCategory.Control, 1)] public bool UseSort { get; set; }
        /// <summary>현재 정렬 상태</summary>
        public GoDataGridColumnSortState SortState { get; set; } = GoDataGridColumnSortState.None;
        /// <summary>정렬 우선순위</summary>
        public int SortOrder { get; }

        /// <summary>열 텍스트 색상</summary>
        public string? TextColor { get; set; }
        /// <summary>열 고정 여부</summary>
        [GoProperty(PCategory.Control, 2)] public bool Fixed { get; set; }

        /// <summary>이 열에 사용되는 셀 타입</summary>
        [JsonIgnore] public Type? CellType { get; set; }
        /// <summary>열이 속한 데이터 그리드</summary>
        [JsonIgnore] public GoDataGrid? Grid { get; internal set; }
        /// <summary>열 헤더의 화면 영역</summary>
        [JsonIgnore] public SKRect Bounds { get; internal set; }
        /// <summary>필터 영역의 화면 영역</summary>
        [JsonIgnore] public SKRect FilterBounds { get; internal set; }
        [JsonIgnore] internal int Depth { get; set; }
        #endregion

        #region Member Variable
        private bool bDown = false;
        #endregion

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas, GoTheme thm)
        {
            if (Grid != null)
            {
                var cText = thm.ToColor(TextColor ?? Grid.TextColor);
                var cBack = thm.ToColor(Grid.ColumnColor);
                var cBorder = cBack.BrightnessTransmit(thm.Dark ? GoDataGrid.BorderBright : -GoDataGrid.BorderBright);
                var cIF = cBack.BrightnessTransmit(thm.Dark ? GoDataGrid.InputBright : -GoDataGrid.InputBright);
                var cIB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Highlight : cBorder;
                var isCol = Grid.Columns.Contains(this);
                var (rtText, rtSort) = bounds();

                Util.DrawBox(canvas, Bounds, SKColors.Transparent, cBorder, GoRoundType.Rect, thm.Corner);
                Util.DrawText(canvas, HeaderText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtText, cText);

                if (isCol)
                {
                    if (FilterBounds.Height > 0) Util.DrawBox(canvas, FilterBounds, SKColors.Transparent, cBorder, GoRoundType.Rect, thm.Corner);
                    if (UseFilter)
                    {
                        var rt = FilterBounds; rt.Inflate(InputInflateW, InputInflateH);
                        Util.DrawBox(canvas, rt, cIF, cIB, GoRoundType.Rect, thm.Corner);
                        Util.DrawText(canvas, FilterText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rt, cText);
                    }

                    if (UseSort)
                    {
                        Util.DrawIcon(canvas, "fa-sort", 12, rtSort, cText.WithAlpha(60));

                        if (SortState == GoDataGridColumnSortState.Asc) Util.DrawIcon(canvas, "fa-sort-up", 12, rtSort, cText);
                        else if (SortState == GoDataGridColumnSortState.Desc) Util.DrawIcon(canvas, "fa-sort-down", 12, rtSort, cText);
                    }
                }

                OnDraw(canvas);
            }
        }
        #endregion

        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button)
        {
            var (rtText, rtSort) = bounds();
            var isCol = Grid?.Columns.Contains(this) ?? false;

            if (CollisionTool.Check(Bounds, x, y))
            {
                bDown = true;
                OnMouseDown(x, y, button);
            }

            if (isCol)
            {
                if (UseFilter && CollisionTool.Check(FilterBounds, x, y) && Grid != null)
                {
                    Grid.InputObject = this;
                    GoInputEventer.Current.FireInputString(Grid, FilterBounds, (s) => { FilterText = s; Grid?.RefreshRows(); }, FilterText);
                }

                if (CollisionTool.Check(rtSort, x, y))
                {
                    if (SortState == GoDataGridColumnSortState.None) SortState = GoDataGridColumnSortState.Asc;
                    else if (SortState == GoDataGridColumnSortState.Asc) SortState = GoDataGridColumnSortState.Desc;
                    else if (SortState == GoDataGridColumnSortState.Desc) SortState = GoDataGridColumnSortState.None;

                    Grid?.RefreshRows();
                }
            }
        }

        internal void MouseUp(float x, float y, GoMouseButton button)
        {
            if (bDown)
            {
                bDown = false;
                OnMouseUp(x, y, button);
            }
        }

        internal void MouseMove(float x, float y)
        {
            if (bDown)
            {
                OnMouseMove(x, y);
            }
        }
        #endregion
        #endregion

        #region Virtual
        /// <summary>열 헤더를 그릴 때 호출되는 가상 메서드입니다.</summary>
        /// <param name="canvas">그리기에 사용되는 캔버스</param>
        protected virtual void OnDraw(SKCanvas canvas) { }
        /// <summary>마우스 버튼 누름 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">눌러진 마우스 버튼</param>
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { }
        /// <summary>마우스 버튼 놓음 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">놓아진 마우스 버튼</param>
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { }
        /// <summary>마우스 이동 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        protected virtual void OnMouseMove(float x, float y) { }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtSort) bounds()
        {
            var isCol = Grid?.Columns.Contains(this) ?? false;
            var rts = Util.Columns(Bounds, ["100%", $"{(UseSort && isCol ? 24 : 0)}px"]);
            if (UseSort) rts[0].Offset(6, 0);
            return (rts[0], rts[1]);
        }
        #endregion
        #endregion
    }
    #endregion
    #region class : GoDataGridRow
    /// <summary>
    /// 데이터 그리드의 행을 나타내는 클래스입니다.
    /// </summary>
    public class GoDataGridRow
    {
        #region Properties
        /// <summary>행 인덱스</summary>
        public int RowIndex { get; internal set; }
        /// <summary>행 높이</summary>
        public float RowHeight { get; set; }
        /// <summary>행에 포함된 셀 목록</summary>
        [JsonIgnore] public List<GoDataGridCell> Cells { get; private set; } = [];
        /// <summary>행에 바인딩된 원본 데이터 객체</summary>
        [JsonIgnore] public object? Source { get; internal set; }
        /// <summary>사용자 정의 데이터를 저장하기 위한 태그 객체</summary>
        [JsonIgnore] public object? Tag { get; set; }
        /// <summary>행 선택 상태</summary>
        [JsonIgnore] public bool Selected { get; set; }
        /// <summary>행이 속한 데이터 그리드</summary>
        [JsonIgnore] public GoDataGrid? Grid { get; internal set; }
        /// <summary>행의 화면 영역</summary>
        [JsonIgnore] public SKRect Bounds { get; internal set; }
        #endregion
    }
    #endregion
    #region class : GoDataGridSummaryCell
    /// <summary>
    /// 데이터 그리드 요약 셀의 추상 기본 클래스입니다.
    /// </summary>
    public abstract class GoDataGridSummaryCell
    {
        #region Properties
        /// <summary>셀의 이름 (데이터 바인딩 프로퍼티명)</summary>
        public string? Name { get; set; }

        /// <summary>열 병합 수</summary>
        public int ColSpan { get; set; } = 1;
        /// <summary>행 병합 수</summary>
        public int RowSpan { get; set; } = 1;
        /// <summary>셀이 속한 열 인덱스</summary>
        [JsonIgnore] public int ColumnIndex => Row.Cells.IndexOf(this);
        /// <summary>셀이 속한 요약 행 인덱스</summary>
        [JsonIgnore] public int RowIndex => Grid.SummaryRows.IndexOf(Row);

        /// <summary>셀 표시 여부</summary>
        public bool Visible { get; set; } = true;
        /// <summary>셀 활성화 여부</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>사용자 정의 데이터를 저장하기 위한 태그 객체</summary>
        [JsonIgnore] public object? Tag { get; set; }

        /// <summary>셀 배경색</summary>
        public string CellBackColor { get; set; }
        /// <summary>셀 텍스트 색상</summary>
        public string CellTextColor { get; set; }

        /// <summary>셀이 속한 데이터 그리드</summary>
        [JsonIgnore] public GoDataGrid Grid { get; private set; }
        /// <summary>셀이 속한 요약 행</summary>
        [JsonIgnore] public GoDataGridSummaryRow Row { get; private set; }
        /// <summary>셀이 속한 열</summary>
        [JsonIgnore] public GoDataGridColumn Column { get; private set; }
        [JsonIgnore]
        internal SKRect Bounds
        {
            get
            {
                var ci = ColumnIndex;
                var l = Grid.Columns[ci].Bounds.Left;
                var r = Grid.Columns[ci + ColSpan - 1].Bounds.Right;

                var ri = RowIndex;
                var t = Grid.SummaryRows[ri].Bounds.Top;
                var b = Grid.SummaryRows[ri + RowSpan - 1].Bounds.Bottom;

                return new SKRect(l, t, r, b);
            }
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        #endregion

        #region Constructor
        /// <summary>
        /// 데이터 그리드 요약 셀을 초기화합니다.
        /// </summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 요약 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column)
        {
            this.Grid = Grid;
            this.Row = Row;
            this.Column = Column;

            CellBackColor = Grid.SummaryRowColor;
            CellTextColor = Grid.TextColor;
        }
        #endregion

        #region Method
        #region Fire
        #region Draw
        internal void Draw(SKCanvas canvas, GoTheme thm)
        {
            if (Grid != null)
            {
                var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
                var cBack = thm.ToColor(CellBackColor ?? Grid.SummaryRowColor);
                var cF = cBack;
                var cV = cF;
                var cB = cV.BrightnessTransmit(thm.Dark ? GoDataGrid.BorderBright : -GoDataGrid.BorderBright);

                Util.DrawBox(canvas, Bounds, cV, GoRoundType.Rect, thm.Corner);

                using var p = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1, Color = cB };
                var l = Convert.ToInt32(Bounds.Left) + 0.5F;
                var r = Convert.ToInt32(Bounds.Right) + 0.5F;
                canvas.DrawLine(l, Bounds.Top, l, Bounds.Bottom, p);
                canvas.DrawLine(r, Bounds.Top, r, Bounds.Bottom, p);
                OnDraw(canvas, thm);
            }
        }
        #endregion

        #region Mouse
        internal void MouseDown(float x, float y, GoMouseButton button)
        {
            if (CollisionTool.Check(Bounds, x, y))
            {
                bDown = true;
                OnMouseDown(x, y, button);
            }
        }

        internal void MouseUp(float x, float y, GoMouseButton button)
        {
            if (bDown)
            {
                bDown = false;
                OnMouseUp(x, y, button);
            }
        }

        internal void MouseMove(float x, float y)
        {
            if (bDown)
            {
                OnMouseMove(x, y);
            }
        }
        #endregion
        #endregion

        #region Virtual
        /// <summary>요약 셀을 그릴 때 호출되는 가상 메서드입니다.</summary>
        /// <param name="canvas">그리기에 사용되는 캔버스</param>
        /// <param name="thm">현재 테마</param>
        protected virtual void OnDraw(SKCanvas canvas, GoTheme thm) { }
        /// <summary>마우스 버튼 누름 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">눌러진 마우스 버튼</param>
        protected virtual void OnMouseDown(float x, float y, GoMouseButton button) { }
        /// <summary>마우스 버튼 놓음 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">놓아진 마우스 버튼</param>
        protected virtual void OnMouseUp(float x, float y, GoMouseButton button) { }
        /// <summary>마우스 이동 시 호출되는 가상 메서드입니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        protected virtual void OnMouseMove(float x, float y) { }

        /// <summary>요약 값을 계산합니다.</summary>
        public virtual void Calculate() { }
        #endregion
        #endregion
    }
    #endregion
    #region class : GoDataGridSummaryRow
    /// <summary>
    /// 데이터 그리드 요약 행의 추상 기본 클래스입니다.
    /// </summary>
    public abstract class GoDataGridSummaryRow
    {
        #region Properties
        /// <summary>요약 행 인덱스</summary>
        public int RowIndex { get; internal set; }
        /// <summary>요약 행 높이</summary>
        public int RowHeight { get; set; }
        /// <summary>요약 행에 포함된 셀 목록</summary>
        [JsonIgnore] public List<GoDataGridSummaryCell> Cells { get; private set; } = [];
        /// <summary>요약 행이 속한 데이터 그리드</summary>
        [JsonIgnore] public GoDataGrid? Grid { get; internal set; }
        /// <summary>제목 표시 시작 열 인덱스</summary>
        public int TitleColumnIndex { get; set; } = 0;
        /// <summary>제목이 차지하는 열 수</summary>
        public int TitleColSpan { get; set; } = 1;
        /// <summary>요약 행 제목 텍스트</summary>
        public string Title { get; set; } = "";

        [JsonIgnore] internal SKRect Bounds { get; set; }
        #endregion

        #region Method
        /// <summary>요약 행의 모든 셀 값을 계산합니다.</summary>
        public void Calculate()
        {
            foreach (var c in Cells) c.Calculate();
        }
        #endregion
    }
    #endregion
    #endregion

    #region Summary 
    #region Label
    /// <summary>
    /// 텍스트 레이블을 표시하는 요약 셀 클래스입니다.
    /// </summary>
    public class GoDataGridLabelSummaryCell : GoDataGridSummaryCell
    {
        #region Properties
        /// <summary>표시할 텍스트</summary>
        public string Text { get; internal set; }
        #endregion

        #region Constructor
        /// <summary>레이블 요약 셀을 초기화합니다.</summary>
        public GoDataGridLabelSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            Util.DrawText(canvas, Text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion
    }
    #endregion
    #region Sum
    /// <summary>합계 요약 행 클래스입니다.</summary>
    public class GoDataGridSumSummaryRow : GoDataGridSummaryRow { }
    /// <summary>합계를 계산하여 표시하는 요약 셀 클래스입니다.</summary>
    public class GoDataGridSumSummaryCell : GoDataGridSummaryCell
    {
        #region Properties
        /// <summary>계산된 합계 값</summary>
        public decimal Value { get; private set; }
        /// <summary>값 표시 형식 문자열</summary>
        public string? FormatString { get; internal set; }
        #endregion

        #region Constructor
        /// <summary>합계 요약 셀을 초기화합니다.</summary>
        public GoDataGridSumSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = ValueTool.ToString(Value, FormatString);
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion

        #region Calculate
        /// <inheritdoc/>
        public override void Calculate()
        {
            var ci = ColumnIndex;
            Value = Grid.ViewRows.Count == 0 ? 0 : Grid.ViewRows.Sum(x => Convert.ToDecimal(x.Cells[ci].Value));
        }
        #endregion
    }
    #endregion
    #region Average
    /// <summary>평균 요약 행 클래스입니다.</summary>
    public class GoDataGridAverageSummaryRow : GoDataGridSummaryRow { }
    /// <summary>평균을 계산하여 표시하는 요약 셀 클래스입니다.</summary>
    public class GoDataGridAverageSummaryCell : GoDataGridSummaryCell
    {
        #region Properties
        /// <summary>계산된 평균 값</summary>
        public decimal Value { get; private set; }
        /// <summary>값 표시 형식 문자열</summary>
        public string? FormatString { get; internal set; }
        #endregion

        #region Constructor
        /// <summary>평균 요약 셀을 초기화합니다.</summary>
        public GoDataGridAverageSummaryCell(GoDataGrid Grid, GoDataGridSummaryRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion
         
        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = ValueTool.ToString(Value, FormatString);
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion

        #region Calculate
        /// <inheritdoc/>
        public override void Calculate()
        {
            var ci = ColumnIndex;
            Value = Grid.ViewRows.Count == 0 ? 0 : Grid.ViewRows.Average(x => Convert.ToDecimal(x.Cells[ci].Value));
        }
        #endregion
    }
    #endregion
    #endregion

    #region Rows
    #region Label
    /// <summary>
    /// 읽기 전용 텍스트 레이블 셀 클래스입니다.
    /// </summary>
    public class GoDataGridLabelCell : GoDataGridCell
    {
        #region Constructor
        /// <summary>레이블 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridLabelCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = "";
            if (Column is GoDataGridLabelColumn col)
            {
                if (col.TextConverter != null) text = col.TextConverter(Value);
                else text = ValueTool.ToString(Value, col.FormatString);
            }
            else text = ValueTool.ToString(Value, null);

            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion
    }

    /// <summary>
    /// 읽기 전용 텍스트 레이블 열 클래스입니다.
    /// </summary>
    public class GoDataGridLabelColumn : GoDataGridColumn
    {
        #region Properties
        /// <summary>값을 표시 텍스트로 변환하는 함수</summary>
        public Func<object?, string?>? TextConverter { get; set; }
        /// <summary>값 표시 형식 문자열</summary>
        public string? FormatString { get; set; }
        #endregion

        #region Constructor
        /// <summary>레이블 열을 초기화합니다.</summary>
        public GoDataGridLabelColumn()
        {
            CellType = typeof(GoDataGridLabelCell);
        }
        #endregion
    }
    #endregion
    #region Number
    /// <summary>
    /// 읽기 전용 숫자 표시 셀 클래스입니다.
    /// </summary>
    /// <typeparam name="T">숫자 값 타입</typeparam>
    public class GoDataGridNumberCell<T> : GoDataGridCell where T : struct
    {
        #region Constructor
        /// <summary>숫자 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridNumberCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var text = ValueTool.ToString(Value, (Column is GoDataGridNumberColumn<T> col ? col.FormatString : null));
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, Bounds, cText);
        }
        #endregion
    }

    /// <summary>
    /// 읽기 전용 숫자 표시 열 클래스입니다.
    /// </summary>
    /// <typeparam name="T">숫자 값 타입</typeparam>
    public class GoDataGridNumberColumn<T> : GoDataGridColumn where T : struct
    {
        /// <summary>값 표시 형식 문자열</summary>
        public string? FormatString { get; set; }
        #region Constructor
        /// <summary>숫자 표시 열을 초기화합니다.</summary>
        public GoDataGridNumberColumn()
        {
            CellType = typeof(GoDataGridNumberCell<T>);
        }
        #endregion
    }
    #endregion
    #region Button
    /// <summary>
    /// 버튼을 포함하는 셀 클래스입니다.
    /// </summary>
    public class GoDataGridButtonCell : GoDataGridCell
    {
        #region Properties
        /// <summary>버튼 배경색</summary>
        public string ButtonColor { get; set; } = "Base3";
        /// <summary>행 선택 시 버튼 배경색</summary>
        public string SelectButtonColor { get; set; } = "Select";
        /// <summary>버튼에 표시할 텍스트</summary>
        public string? Text { get; set; }
        /// <summary>버튼에 표시할 아이콘 문자열</summary>
        public string? IconString { get; set; }
        /// <summary>아이콘 크기</summary>
        public int IconSize { get; set; } = 12;
        /// <summary>아이콘과 텍스트 사이 간격</summary>
        public int IconGap { get; set; } = 5;
        #endregion

        #region Member Variable
        bool bDown, bHover;
        #endregion

        #region Constructor
        /// <summary>버튼 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridButtonCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if(Column is GoDataGridButtonColumn col)
            {
                this.ButtonColor = col.ButtonColor;
                this.SelectButtonColor = col.SelectButtonColor;
                this.Text = col.Text;
                this.IconString = col.IconString;
                this.IconSize = col.IconSize;
                this.IconGap = col.IconGap;
            }
        }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var cButton = thm.ToColor(Row.Selected ? SelectButtonColor : ButtonColor).BrightnessTransmit(RowIndex % 2 == 0 ? 0.05F : -0.05F).BrightnessTransmit(bDown ? thm.DownBrightness : 0);

            var rt = Bounds; rt.Bottom -= 1; rt.Right -= 1;// rt.Inflate(-1, -1);
            Util.DrawBox(canvas, rt, cButton.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cButton.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0), GoRoundType.Rect, thm.Corner);
            if (bDown) rt.Offset(0, 1);
            Util.DrawTextIcon(canvas, Text, Grid.FontName, Grid.FontStyle, Grid.FontSize, IconString, Grid.FontSize, GoDirectionHV.Horizon, 5, rt, cText, GoContentAlignment.MiddleCenter);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(-1, -1);
            if (CollisionTool.Check(rt, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(-1, -1);
            if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rt, x, y)) Grid.InvokeButtonClick(this);
            }

            base.OnMouseUp(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            var rt = Bounds; rt.Inflate(-1, -1);
            bHover = CollisionTool.Check(rt, x, y);
            base.OnMouseMove(x, y);
        }
        #endregion
    }

    /// <summary>
    /// 버튼을 포함하는 열 클래스입니다.
    /// </summary>
    public class GoDataGridButtonColumn : GoDataGridColumn
    {
        #region Properties
        /// <summary>버튼 배경색</summary>
        public string ButtonColor { get; set; } = "Base3";
        /// <summary>행 선택 시 버튼 배경색</summary>
        public string SelectButtonColor { get; set; } = "Select-light";
        /// <summary>버튼에 표시할 텍스트</summary>
        public string? Text { get; set; }
        /// <summary>버튼에 표시할 아이콘 문자열</summary>
        public string? IconString { get; set; }
        /// <summary>아이콘 크기</summary>
        public int IconSize { get; set; } = 12;
        /// <summary>아이콘과 텍스트 사이 간격</summary>
        public int IconGap { get; set; } = 5;
        #endregion

        #region Constructor
        /// <summary>버튼 열을 초기화합니다.</summary>
        public GoDataGridButtonColumn()
        {
            CellType = typeof(GoDataGridButtonCell);
        }
        #endregion
    }
    #endregion
    #region Lamp
    /// <summary>
    /// 불리언 값을 램프(표시등)로 표시하는 셀 클래스입니다.
    /// </summary>
    public class GoDataGridLampCell : GoDataGridCell
    {
        #region Properties
        /// <summary>활성화 시 램프 색상</summary>
        public string OnColor { get; set; } = "Good";
        #endregion 

        #region Constructor
        /// <summary>램프 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridLampCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridLampColumn col)
            {
                this.OnColor = col.OnColor;
            }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var val = Value is bool b ? b : false;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cOn = thm.ToColor(OnColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(Grid.SelectedRowColor);
            var cOff = (Row.Selected ? cSel : cRow).BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);

            var sz = Math.Min(Convert.ToInt32(Grid.RowHeight * 0.75), 24);
            var rt = MathTool.MakeRectangle(Bounds, new SKSize(sz, sz));

            if(Column.Name =="IsAlarm" && val)
            {

            }

            Util.DrawLamp(canvas, thm, rt, cOn, cOff, val);
        }
        #endregion
    }

    /// <summary>
    /// 불리언 값을 램프(표시등)로 표시하는 열 클래스입니다.
    /// </summary>
    public class GoDataGridLampColumn : GoDataGridColumn
    {
        #region Properties
        /// <summary>활성화 시 램프 색상</summary>
        public string OnColor { get; set; } = "Good";
        #endregion 

        #region Constructor
        /// <summary>램프 열을 초기화합니다.</summary>
        public GoDataGridLampColumn()
        {
            CellType = typeof(GoDataGridLampCell);
        }
        #endregion
    }
    #endregion
    #region CheckBox
    /// <summary>
    /// 체크박스를 포함하는 셀 클래스입니다.
    /// </summary>
    public class GoDataGridCheckBoxCell : GoDataGridCell
    {
        #region Properties
        #endregion 

        #region Constructor
        /// <summary>체크박스 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridCheckBoxCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridCheckBoxColumn col)
            {
            }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var br = thm.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rtChk = MathTool.MakeRectangle(Bounds, new SKSize(20, 20));

            Util.DrawBox(canvas, rtChk, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);
            if (Value is bool v && v) Util.DrawIcon(canvas, "fa-check", 12, rtChk, cText);
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rtChk = MathTool.MakeRectangle(Bounds, new SKSize(20, 20));
            if(CollisionTool.Check(rtChk, x, y) && Value is bool v)
            {
                var old = Value;
                Value = !v;
                Grid?.InvokeValueChange(this, old, Value);
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    /// <summary>
    /// 체크박스를 포함하는 열 클래스입니다.
    /// </summary>
    public class GoDataGridCheckBoxColumn : GoDataGridColumn
    {
        #region Properties
        #endregion 

        #region Constructor
        /// <summary>체크박스 열을 초기화합니다.</summary>
        public GoDataGridCheckBoxColumn()
        {
            CellType = typeof(GoDataGridCheckBoxCell);
        }
        #endregion
    }
    #endregion
    #region InputText
    /// <summary>
    /// 텍스트 입력 셀 클래스입니다.
    /// </summary>
    public class GoDataGridInputTextCell : GoDataGridCell
    {
        #region Properties
        /// <inheritdoc/>
        public override bool IsKeyboardInput => true;
        #endregion

        #region Member Variable
        bool bDown = false;
        #endregion

        #region Constructor
        /// <summary>텍스트 입력 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridInputTextCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridCheckBoxColumn col)
            {
            }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var br = thm.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Highlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            if (!(Grid._InputModeInvisibleText_ && Grid.InputObject == this))
            {
                var text = Value is string s ? s : null;
                Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rt, cText);
            }
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (CollisionTool.Check(rt, x, y)) Grid?.InvokeEditText(this, Value as string);

            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    /// <summary>
    /// 텍스트 입력 열 클래스입니다.
    /// </summary>
    public class GoDataGridInputTextColumn : GoDataGridColumn
    {
        #region Constructor
        /// <summary>텍스트 입력 열을 초기화합니다.</summary>
        public GoDataGridInputTextColumn()
        {
            CellType = typeof(GoDataGridInputTextCell);
        }
        #endregion
    }
    #endregion
    #region InputNumber
    /// <summary>
    /// 숫자 입력 셀 클래스입니다.
    /// </summary>
    /// <typeparam name="T">숫자 값 타입</typeparam>
    public class GoDataGridInputNumberCell<T> : GoDataGridCell where T : struct
    {
        #region Properties
        /// <summary>입력 가능한 최솟값</summary>
        public T? Minimum { get; set; }
        /// <summary>입력 가능한 최댓값</summary>
        public T? Maximum { get; set; }
        /// <summary>값 표시 형식 문자열</summary>
        public string? FormatString { get; set; }
        /// <inheritdoc/>
        public override bool IsKeyboardInput => true;
        #endregion

        #region Member Variable
        bool bDown = false;
        #endregion

        #region Constructor
        /// <summary>숫자 입력 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridInputNumberCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridInputNumberColumn<T> col)
            {
                this.Minimum = col.Minimum;
                this.Maximum = col.Maximum;
                this.FormatString = col.FormatString;
            }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var br = thm.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Highlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (Convert.ToInt32(rt.Right) + 0.5F == Bounds.Right) rt.Right -= 1;

            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            if (!(Grid._InputModeInvisibleText_ && Grid.InputObject == this))
            {
                var text = Value is T val ? ValueTool.ToString<T>(val, FormatString) : null;
                Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rt, cText);
            }
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (CollisionTool.Check(rt, x, y) && Value is T v && Column is GoDataGridInputNumberColumn<T> col) Grid?.InvokeEditNumber<T>(this, v, col.Minimum, col.Maximum);
            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    /// <summary>
    /// 숫자 입력 열 클래스입니다.
    /// </summary>
    /// <typeparam name="T">숫자 값 타입</typeparam>
    public class GoDataGridInputNumberColumn<T>: GoDataGridColumn where T : struct
    {
        #region Properties
        /// <summary>입력 가능한 최솟값</summary>
        public T? Minimum { get; set; }
        /// <summary>입력 가능한 최댓값</summary>
        public T? Maximum { get; set; }
        /// <summary>값 표시 형식 문자열</summary>
        public string? FormatString { get; set; }
        #endregion 

        #region Constructor
        /// <summary>숫자 입력 열을 초기화합니다.</summary>
        public GoDataGridInputNumberColumn()
        {
            CellType = typeof(GoDataGridInputNumberCell<T>);
        }
        #endregion
    }
    #endregion
    #region InputBool
    /// <summary>
    /// 불리언 값 입력 셀 클래스입니다. On/Off 텍스트를 표시합니다.
    /// </summary>
    public class GoDataGridInputBoolCell : GoDataGridCell
    {
        #region Properties
        /// <summary>true일 때 표시할 텍스트</summary>
        public string? OnText { get; set; } = "On";
        /// <summary>false일 때 표시할 텍스트</summary>
        public string? OffText { get; set; } = "Off";
        #endregion 

        #region Constructor
        /// <summary>불리언 입력 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridInputBoolCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridInputBoolColumn col)
            {
                this.OnText = col.OnText;
                this.OffText = col.OffText;
            }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var br = thm.Dark ? 1F : -1F;
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = GoInputEventer.Current.InputControl == Grid && Grid.InputObject == this ? thm.Highlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            var rts = Util.Columns(rt, ["50%", "50%"]);
            var rtOff = rts[0];
            var rtOn = rts[1];
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            if (Value is bool v)
            {
                var cTextL = cText;
                var cTextD = Util.FromArgb(80, cTextL);
                Util.DrawText(canvas, OnText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtOn, v ? cTextL : cTextD);
                Util.DrawText(canvas, OffText, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtOff, v ? cTextD : cTextL);

                using var p = new SKPaint { IsAntialias = false };
                using var pe = SKPathEffect.CreateDash([2, 2], 2);
                p.StrokeWidth = 1;
                p.IsStroke = false;
                p.Color = Util.FromArgb(128, cB);
                p.PathEffect = pe;

                canvas.DrawLine(rt.MidX, rt.Top + 5, rt.MidX, rt.Bottom - 5, p);
            }
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            var rts = Util.Columns(rt, ["50%", "50%"]);
            var rtOff = rts[0];
            var rtOn = rts[1];

            if (Value is bool v)
            {
                var old = Value;
                if (CollisionTool.Check(rtOn, x, y) && !v) { Value = true; Grid?.InvokeValueChange(this, old, Value); }
                if (CollisionTool.Check(rtOff, x, y) && v) { Value = false; Grid?.InvokeValueChange(this, old, Value); }
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion
    }

    /// <summary>
    /// 불리언 값 입력 열 클래스입니다.
    /// </summary>
    public class GoDataGridInputBoolColumn : GoDataGridColumn
    {
        #region Properties
        /// <summary>true일 때 표시할 텍스트</summary>
        public string? OnText { get; set; } = "On";
        /// <summary>false일 때 표시할 텍스트</summary>
        public string? OffText { get; set; } = "Off";
        #endregion

        #region Constructor
        /// <summary>불리언 입력 열을 초기화합니다.</summary>
        public GoDataGridInputBoolColumn()
        {
            CellType = typeof(GoDataGridInputBoolCell);
        }
        #endregion
    }
    #endregion
    #region InputTime
    /// <summary>
    /// 날짜/시간 입력 셀 클래스입니다.
    /// </summary>
    public class GoDataGridInputTimeCell : GoDataGridCell
    {
        #region Properties
        /// <summary>날짜/시간 표시 유형</summary>
        public GoDateTimeKind DateTimeStyle { get; set; } = GoDateTimeKind.DateTime;
        /// <summary>날짜 표시 형식</summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        /// <summary>시간 표시 형식</summary>
        public string TimeFormat { get; set; } = "HH:mm:ss";
        #endregion

        #region Constructor
        /// <summary>날짜/시간 입력 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridInputTimeCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridInputTimeColumn col)
            {
                this.DateFormat = col.DateFormat;
                this.TimeFormat = col.TimeFormat;
                this.DateTimeStyle = col.DateTimeStyle;
    }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var br = thm.Dark ? 1F : -1F;
            var dwndVisible = Grid.DateTimeDropDownVisible(this);
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = dwndVisible ? thm.Highlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            var (rtText, rtArrow) = bounds(rt);

            #region item
            var format = $"{DateFormat} {TimeFormat}";
            switch (DateTimeStyle)
            {
                case GoDateTimeKind.DateTime: format = $"{DateFormat} {TimeFormat}"; break;
                case GoDateTimeKind.Time: format = TimeFormat; break;
                case GoDateTimeKind.Date: format = DateFormat; break;
            }
            
            var text = ValueTool.ToString(Value, format);
            Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtText, cText);
            #endregion
            #region sep
            using var p = new SKPaint { IsAntialias = false };
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cB);
            p.PathEffect = pe;
            canvas.DrawLine(rtArrow.Left, rt.Top + 5, rtArrow.Left, rt.Bottom - 5, p);
            #endregion
            #region arrow
            var ico = "fa-calendar";
            switch (DateTimeStyle)
            {
                case GoDateTimeKind.DateTime: ico = "fa-calendar"; break;
                case GoDateTimeKind.Time: ico = "fa-clock"; break;
                case GoDateTimeKind.Date: ico = "fa-calendar"; break;
            }

            if (dwndVisible) Util.DrawIcon(canvas, ico, 14, rtArrow, SKColors.Transparent, cText);
            else Util.DrawIcon(canvas, ico, 14, rtArrow, cText);
            #endregion
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (Value is DateTime dt && CollisionTool.Check(rt, x, y))
            {
                Grid.DateTimeDropDownOpen(this, rt, dt, DateTimeStyle, (time) =>
                {
                    if (time.HasValue)
                    {
                        var old = Value;
                        Value = time.Value;
                        try
                        {
                            if ((DateTime?)old != (DateTime?)Value) Grid.InvokeValueChange(this, old, Value);
                        }
                        catch { }
                    }
                });
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtArrow) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", $"{Math.Min(40, Bounds.Height)}px"]);
            return (rts[0], rts[1]);
        }
        #endregion
    }

    /// <summary>
    /// 날짜/시간 입력 열 클래스입니다.
    /// </summary>
    public class GoDataGridInputTimeColumn : GoDataGridColumn
    {
        #region Properties
        /// <summary>날짜/시간 표시 유형</summary>
        public GoDateTimeKind DateTimeStyle { get; set; } = GoDateTimeKind.DateTime;
        /// <summary>날짜 표시 형식</summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        /// <summary>시간 표시 형식</summary>
        public string TimeFormat { get; set; } = "HH:mm:ss";
        #endregion 

        #region Constructor
        /// <summary>날짜/시간 입력 열을 초기화합니다.</summary>
        public GoDataGridInputTimeColumn()
        {
            CellType = typeof(GoDataGridInputTimeCell);
        }
        #endregion
    }
    #endregion
    #region InputColor
    /// <summary>
    /// 색상 입력 셀 클래스입니다.
    /// </summary>
    public class GoDataGridInputColorCell : GoDataGridCell
    {
        #region Properties
        #endregion

        #region Constructor
        /// <summary>색상 입력 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridInputColorCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridInputTimeColumn col)
            {
            }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var br = thm.Dark ? 1F : -1F;
            var dwndVisible = Grid.ColorDropDownVisible(this);
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = dwndVisible ? thm.Highlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            var (rtText, rtArrow) = bounds(rt);

            #region Text
            if (Value is SKColor c)
            {
                var isz = Grid.FontSize;
                var text = $"#{c.Red:X2}{c.Green:X2}{c.Blue:X2}";

                var (rtIco, rtVal) = Util.TextIconBounds(text, Grid.FontName, Grid.FontStyle, Grid.FontSize, new SKSize(isz, isz), GoDirectionHV.Horizon, 5, rtText, GoContentAlignment.MiddleCenter);

                Util.DrawBox(canvas, rtIco, c, GoRoundType.Rect, thm.Corner);
                Util.DrawText(canvas, text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtVal, cText);
            }
            #endregion
            #region sep
            using var p = new SKPaint { IsAntialias = false };
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cB);
            p.PathEffect = pe;
            canvas.DrawLine(rtArrow.Left, rt.Top + 5, rtArrow.Left, rt.Bottom - 5, p);
            #endregion
            #region arrow
            if (dwndVisible) Util.DrawIcon(canvas, "fa-palette", 14, rtArrow, SKColors.Transparent, cText);
            else Util.DrawIcon(canvas, "fa-palette", 14, rtArrow, cText);
            #endregion
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (Value is SKColor c && CollisionTool.Check(rt, x, y))
            {
                Grid.ColorDropDownOpen(this, rt, c, (color) =>
                {
                    if (color.HasValue)
                    {
                        var old = Value;
                        Value = color.Value;
                        try
                        {
                            if ((SKColor?)old != (SKColor?)Value) Grid.InvokeValueChange(this, old, Value);
                        }
                        catch { }
                    }
                });
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtArrow) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", $"{Math.Min(40, Bounds.Height)}px"]);
            return (rts[0], rts[1]);
        }
        #endregion
    }

    /// <summary>
    /// 색상 입력 열 클래스입니다.
    /// </summary>
    public class GoDataGridInputColorColumn : GoDataGridColumn
    {
        #region Properties
        #endregion 

        #region Constructor
        /// <summary>색상 입력 열을 초기화합니다.</summary>
        public GoDataGridInputColorColumn()
        {
            CellType = typeof(GoDataGridInputColorCell);
        }
        #endregion
    }
    #endregion
    #region InputCombo
    /// <summary>
    /// 콤보박스(드롭다운) 입력 셀 클래스입니다.
    /// </summary>
    public class GoDataGridInputComboCell : GoDataGridCell
    {
        #region Properties
        #endregion

        #region Constructor
        /// <summary>콤보박스 입력 셀을 초기화합니다.</summary>
        /// <param name="Grid">소속 데이터 그리드</param>
        /// <param name="Row">소속 행</param>
        /// <param name="Column">소속 열</param>
        public GoDataGridInputComboCell(GoDataGrid Grid, GoDataGridRow Row, GoDataGridColumn Column) : base(Grid, Row, Column)
        {
            if (Column is GoDataGridInputComboColumn col)
            {
            }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            base.OnDraw(canvas, thm);

            var br = thm.Dark ? 1F : -1F;
            var dwndVisible = Grid.ComboDropDownVisible(this);
            var cText = thm.ToColor(CellTextColor ?? Grid.TextColor);
            var cRow = thm.ToColor(CellBackColor ?? Grid.RowColor);
            var cSel = thm.ToColor(SelectedCellBackColor ?? Grid.SelectedRowColor);
            var cF = Row.Selected ? cSel : cRow;
            var cV = cF.BrightnessTransmit(Row.RowIndex % 2 == 0 ? 0.05F : -0.05F);
            var cB = dwndVisible ? thm.Highlight : cV.BrightnessTransmit(GoDataGrid.BorderBright * br);

            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            Util.DrawBox(canvas, rt, cV.BrightnessTransmit(GoDataGrid.InputBright * br), cB, GoRoundType.Rect, thm.Corner);

            var (rtText, rtArrow) = bounds(rt);
            #region Text
            if (Column is GoDataGridInputComboColumn col)
            {
                var v = col.Items.FirstOrDefault(x => x.Value != null ? x.Value.Equals(Value) : false);
                if (v != null) Util.DrawText(canvas, v.Text, Grid.FontName, Grid.FontStyle, Grid.FontSize, rtText, cText);
            }
            #endregion
            #region sep
            using var p = new SKPaint { IsAntialias = false };
            using var pe = SKPathEffect.CreateDash([2, 2], 2);
            p.StrokeWidth = 1;
            p.IsStroke = false;
            p.Color = Util.FromArgb(128, cB);
            p.PathEffect = pe;
            canvas.DrawLine(rtArrow.Left, rt.Top + 5, rtArrow.Left, rt.Bottom - 5, p);
            #endregion
            #region arrow
            Util.DrawIcon(canvas, dwndVisible ? "fa-angle-up" : "fa-angle-down", 14, rtArrow, cText);
            #endregion
        }
        #endregion

        #region Mouse
        /// <inheritdoc/>
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rt = Bounds; rt.Inflate(InputInflateW, InputInflateH);
            if (Column is GoDataGridInputComboColumn col && CollisionTool.Check(rt, x, y))
            {
                var v = col.Items.FirstOrDefault(x => x.Value != null ? x.Value.Equals(Value) : false);

                var items = col.Filter != null ? col.Items.Where(x => col.Filter(x)).ToList() : col.Items;

                Grid.ComboDropDownOpen(this, rt, col.ItemHeight, col.MaximumViewCount, items, v, (item) =>
                {
                    if (item != null)
                    {
                        var old = Value;
                        Value = item.Value;
                        try
                        {
                            if ((GoDataGridInputComboItem?)old != (GoDataGridInputComboItem?)Value) Grid.InvokeValueChange(this, old, Value);
                        }
                        catch { }
                    }
                });
            }
            base.OnMouseClick(x, y, button);
        }
        #endregion

        #region bounds
        (SKRect rtText, SKRect rtArrow) bounds(SKRect rtValue)
        {
            var rts = Util.Columns(rtValue, ["100%", $"{Math.Min(40, Bounds.Height)}px"]);
            return (rts[0], rts[1]);
        }
        #endregion
    }

    /// <summary>
    /// 콤보박스(드롭다운) 입력 열 클래스입니다.
    /// </summary>
    public class GoDataGridInputComboColumn : GoDataGridColumn
    {
        #region Properties
        /// <summary>콤보박스에 표시할 항목 목록</summary>
        public List<GoDataGridInputComboItem> Items { get; set; } = [];
        /// <summary>항목 필터링 함수</summary>
        public Func<GoDataGridInputComboItem, bool>? Filter { get; set; }
        /// <summary>드롭다운에 표시할 최대 항목 수</summary>
        public int MaximumViewCount { get; set; } = 8;
        /// <summary>드롭다운 항목의 높이</summary>
        public int ItemHeight { get; set; } = 30;
        #endregion 

        #region Constructor
        /// <summary>콤보박스 입력 열을 초기화합니다.</summary>
        public GoDataGridInputComboColumn()
        {
            CellType = typeof(GoDataGridInputComboCell);
        }
        #endregion
    }

    /// <summary>
    /// 콤보박스 항목 클래스입니다. 표시 텍스트와 실제 값을 분리하여 관리합니다.
    /// </summary>
    public class GoDataGridInputComboItem : GoListItem
    {
        /// <summary>항목에 연결된 실제 값</summary>
        public object? Value { get; set; }
    }
    #endregion
    #endregion

    #region EventArgs
    /// <summary>
    /// 셀 내 버튼 클릭 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="cell">클릭된 셀</param>
    public class GoDataGridCellButtonClickEventArgs(GoDataGridCell cell) : EventArgs
    {
        /// <summary>클릭된 셀</summary>
        public GoDataGridCell Cell { get; private set; } = cell;
    }

    /// <summary>
    /// 열 헤더 마우스 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="column">이벤트가 발생한 열</param>
    public class GoDataGridColumnMouseEventArgs(GoDataGridColumn column) : EventArgs
    {
        /// <summary>이벤트가 발생한 열</summary>
        public GoDataGridColumn Column { get; private set; } = column;
    }

    /// <summary>
    /// 셀 마우스 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="cell">이벤트가 발생한 셀</param>
    public class GoDataGridCellMouseEventArgs(GoDataGridCell cell) : EventArgs
    {
        /// <summary>이벤트가 발생한 셀</summary>
        public GoDataGridCell Cell { get; private set; } = cell;
    }

    /// <summary>
    /// 셀 값 변경 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="cell">값이 변경된 셀</param>
    /// <param name="oldValue">이전 값</param>
    /// <param name="newValue">새 값</param>
    public class GoDataGridCellValueChangedEventArgs(GoDataGridCell cell, object? oldValue, object? newValue) : EventArgs
    {
        /// <summary>값이 변경된 셀</summary>
        public GoDataGridCell Cell { get; private set; } = cell;
        /// <summary>이전 값</summary>
        public object? OldValue { get; private set; } = oldValue;
        /// <summary>새 값</summary>
        public object? NewValue { get; private set; } = newValue;
    }

    /// <summary>
    /// 행 더블클릭 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="row">더블클릭된 행</param>
    public class GoDataGridRowDoubleClickEventArgs(GoDataGridRow row) : EventArgs
    {
        /// <summary>더블클릭된 행</summary>
        public GoDataGridRow Row { get; private set; } = row;
    }

    /// <summary>
    /// 날짜/시간 드롭다운 열림 이벤트 인자 클래스입니다. 취소 가능합니다.
    /// </summary>
    /// <param name="cell">대상 셀</param>
    /// <param name="rt">드롭다운 표시 영역</param>
    /// <param name="value">현재 날짜/시간 값</param>
    /// <param name="style">날짜/시간 표시 유형</param>
    /// <param name="action">선택 완료 시 호출할 콜백</param>
    public class GoDataGridDateTimeDropDownOpeningEventArgs(GoDataGridCell cell, SKRect rt, DateTime value, GoDateTimeKind style, Action<DateTime?> action) : GoCancelableEventArgs
    {
        /// <summary>대상 셀</summary>
        public GoDataGridCell Cell { get; } = cell;
        /// <summary>드롭다운 표시 영역</summary>
        public SKRect Bounds { get; } = rt;
        /// <summary>현재 날짜/시간 값</summary>
        public DateTime Value { get; } = value;
        /// <summary>날짜/시간 표시 유형</summary>
        public GoDateTimeKind Style { get; } = style;
        /// <summary>선택 완료 시 호출할 콜백</summary>
        public Action<DateTime?> Action { get; } = action;
    }

    /// <summary>
    /// 색상 드롭다운 열림 이벤트 인자 클래스입니다. 취소 가능합니다.
    /// </summary>
    /// <param name="cell">대상 셀</param>
    /// <param name="rt">드롭다운 표시 영역</param>
    /// <param name="value">현재 색상 값</param>
    /// <param name="action">선택 완료 시 호출할 콜백</param>
    public class GoDataGridColorDropDownOpeningEventArgs(GoDataGridCell cell, SKRect rt, SKColor value, Action<SKColor?> action) : GoCancelableEventArgs
    {
        /// <summary>대상 셀</summary>
        public GoDataGridCell Cell { get; } = cell;
        /// <summary>드롭다운 표시 영역</summary>
        public SKRect Bounds { get; } = rt;
        /// <summary>현재 색상 값</summary>
        public SKColor Value { get; } = value;
        /// <summary>선택 완료 시 호출할 콜백</summary>
        public Action<SKColor?> Action { get; } = action;
    }

    /// <summary>
    /// 콤보박스 드롭다운 열림 이벤트 인자 클래스입니다. 취소 가능합니다.
    /// </summary>
    /// <param name="cell">대상 셀</param>
    /// <param name="rt">드롭다운 표시 영역</param>
    /// <param name="itemHeight">항목 높이</param>
    /// <param name="maximumViewCount">최대 표시 항목 수</param>
    /// <param name="items">콤보박스 항목 목록</param>
    /// <param name="selectedItem">현재 선택된 항목</param>
    /// <param name="action">선택 완료 시 호출할 콜백</param>
    public class GoDataGridComboDropDownOpeningEventArgs(GoDataGridCell cell, SKRect rt, float itemHeight, int maximumViewCount, List<GoDataGridInputComboItem> items, GoDataGridInputComboItem? selectedItem, Action<GoDataGridInputComboItem?> action) : GoCancelableEventArgs
    {
        /// <summary>대상 셀</summary>
        public GoDataGridCell Cell { get; } = cell;
        /// <summary>드롭다운 표시 영역</summary>
        public SKRect Bounds { get; } = rt;
        /// <summary>항목 높이</summary>
        public float ItemHeight { get; } = itemHeight;
        /// <summary>최대 표시 항목 수</summary>
        public int MaximumViewCount { get; } = maximumViewCount;
        /// <summary>콤보박스 항목 목록</summary>
        public List<GoDataGridInputComboItem> Items { get; } = items;
        /// <summary>현재 선택된 항목</summary>
        public GoDataGridInputComboItem? SelectedItem { get; } = selectedItem;
        /// <summary>선택 완료 시 호출할 콜백</summary>
        public Action<GoDataGridInputComboItem?> Action { get; } = action;
    }

    #endregion
}
