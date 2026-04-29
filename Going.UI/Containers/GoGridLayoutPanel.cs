using Going.UI.Controls;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    /// <summary>
    /// 각 행이 독립적인 열 정의를 가지는 유연한 그리드 레이아웃 패널입니다.
    /// </summary>
    public class GoGridLayoutPanel : GoContainer
    {
        #region Properties
        /// <summary>
        /// 그리드 행 정의 목록을 가져오거나 설정합니다. 각 행은 높이와 열 정의를 포함합니다.
        /// </summary>
        // [GoChilds] is handled by GoGudxConverter's GoGridLayoutPanel special case (Rows + Childrens dual interlock), not the generic dispatch loop.
        [GoChilds]
        [GoProperty(PCategory.Control, 0)] public List<GoGridLayoutPanelRow> Rows { get; set; } = [];

        /// <summary>
        /// 그리드 레이아웃에 배치된 자식 컨트롤 컬렉션을 가져옵니다.
        /// </summary>
        [JsonInclude] public override GoGridLayoutControlCollection Childrens { get; } = [];
        #endregion

        #region Constructor
        /// <summary>
        /// 자식 컨트롤 컬렉션을 사용하여 <see cref="GoGridLayoutPanel"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">그리드 레이아웃 컨트롤 컬렉션</param>
        [JsonConstructor]
        public GoGridLayoutPanel(GoGridLayoutControlCollection childrens) => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoGridLayoutPanel"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoGridLayoutPanel() { }
        #endregion

        #region Override
        #region OnDraw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            if (Design != null && Design.DesignMode)
            {
                var rt = Areas()["Content"];
                rt.Inflate(-0.5F, -0.5F);
                rt.Offset(0.5F, 0.5F);
                using var pe = SKPathEffect.CreateDash([1, 2], 2);
                using var p = new SKPaint { };

                var rts = GridBounds(rt);

                p.IsStroke = true;
                p.StrokeWidth = 1;
                p.Color = thm.Base3;
                p.IsAntialias = false;
                p.PathEffect = pe;
                
                for (int ir = 0; ir < Rows.Count; ir++)
                {
                    if (CellBounds(rts, 0, ir) is SKRect vrt1)
                    {
                        if (ir == 0) canvas.DrawLine(rt.Left, vrt1.Top, rt.Right, vrt1.Top, p);
                        canvas.DrawLine(rt.Left, vrt1.Bottom, rt.Right, vrt1.Bottom, p);
                    }

                    for (int ic = 0; ic < Rows[ir].Columns.Count; ic++)
                    {
                        if (CellBounds(rts, ic, ir) is SKRect vrt2)
                        {
                            if (ic == 0) canvas.DrawLine(vrt2.Left, vrt2.Top, vrt2.Left, vrt2.Bottom, p);
                            canvas.DrawLine(vrt2.Right, vrt2.Top, vrt2.Right, vrt2.Bottom, p);
                        }
                    }
                }
 
            }

            base.OnDraw(canvas, thm);
        }
        #endregion
        #region OnLayout
        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var b = Bounds;
            var rts = GridBounds();
            foreach (var c in Childrens)
            {
                try
                {
                    var idx = Childrens.Indexes[c.Id];
                    if (idx != null && CellBounds(rts, idx.Column, idx.Row) is SKRect vrt)
                    {
                        c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        //if (c.Fill) c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        //else c.Bounds = Util.Int(Util.FromRect(vrt.Left, vrt.Top, c.Bounds.Width, c.Bounds.Height));
                    }
                }
                catch { }
            }
        }
        #endregion
        #endregion

        #region Method
        /// <summary>
        /// 그리드에 행을 추가합니다.
        /// </summary>
        /// <param name="height">행 높이 문자열 (예: "100px", "50%")</param>
        /// <param name="columns">열 크기 정의 배열</param>
        public void AddRow(string height, string[] columns)
        {
            Rows.Add(new GoGridLayoutPanelRow { Height = height, Columns = [.. columns] });
        }

        /// <summary>
        /// 현재 콘텐츠 영역을 기준으로 모든 셀의 영역을 계산합니다.
        /// </summary>
        /// <returns>행 인덱스와 열 인덱스로 구성된 셀 영역 딕셔너리</returns>
        public Dictionary<int, Dictionary<int, SKRect>> GridBounds() => GridBounds(Areas()["Content"]);
        /// <summary>
        /// 지정된 영역을 기준으로 모든 셀의 영역을 계산합니다.
        /// </summary>
        /// <param name="rt">기준 영역</param>
        /// <returns>행 인덱스와 열 인덱스로 구성된 셀 영역 딕셔너리</returns>
        public Dictionary<int, Dictionary<int, SKRect>> GridBounds(SKRect rt)
        {
            var rows = Util.Rows(rt, Rows.Select(x => x.Height).ToArray());
            var rts = new Dictionary<int, Dictionary<int, SKRect>>();
            for (int i = 0; i < rows.Length; i++)
            {
                var vrt = rows[i];
                rts.Add(i, []);

                var cols = Util.Columns(vrt, Rows[i].Columns.ToArray());
                for (int j = 0; j < cols.Length; j++) rts[i].Add(j, cols[j]);
            }
            return rts;
        }


        /// <summary>
        /// 지정된 열과 행 위치의 셀 영역을 가져옵니다.
        /// </summary>
        /// <param name="rts">그리드 영역 딕셔너리</param>
        /// <param name="col">열 인덱스</param>
        /// <param name="row">행 인덱스</param>
        /// <returns>셀 영역. 해당 위치가 없으면 null</returns>
        public SKRect? CellBounds(Dictionary<int, Dictionary<int, SKRect>> rts, int col, int row)
        {
            SKRect? ret = null;
            if (rts.TryGetValue(row, out Dictionary<int, SKRect>? vrow) && vrow.TryGetValue(col, out SKRect vrt)) ret = vrt;
            return ret;
        }
        #endregion
    }

    #region class : GoGridLayoutPanelRow
    /// <summary>
    /// 그리드 레이아웃 패널의 행 정의 클래스입니다. 높이와 열 정의를 포함합니다.
    /// </summary>
    public class GoGridLayoutPanelRow
    {
        /// <summary>
        /// 행의 높이를 가져오거나 설정합니다 (예: "100px", "50%").
        /// </summary>
        [GoSizeProperty(PCategory.Control, 0)] public string Height { get; set; } = "100%";
        /// <summary>
        /// 이 행의 열 크기 정의 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoSizesProperty(PCategory.Control, 1)] public List<string> Columns { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString() => $"{Height} / Columns : {Columns.Count}";
    }
    #endregion
    #region class : GoGridIndex
    /// <summary>
    /// 그리드 레이아웃에서 컨트롤의 행, 열 위치를 정의하는 클래스입니다.
    /// </summary>
    public class GoGridIndex
    {
        /// <summary>
        /// 행 인덱스를 가져오거나 설정합니다.
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// 열 인덱스를 가져오거나 설정합니다.
        /// </summary>
        public int Column { get; set; }
    }
    #endregion

    /// <summary>
    /// 그리드 레이아웃 패널의 자식 컨트롤과 인덱스 정보를 관리하는 컬렉션 클래스입니다.
    /// </summary>
    public class GoGridLayoutControlCollection : IEnumerable<IGoControl>
    {
        #region Properties
        /// <summary>
        /// 컨트롤 ID와 그리드 인덱스의 매핑 딕셔너리를 가져옵니다.
        /// </summary>
        [JsonInclude] public Dictionary<Guid, GoGridIndex> Indexes { get; } = [];
        /// <summary>
        /// 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public List<IGoControl> Controls { get; } = [];
        #endregion

        #region Indexer
        /// <summary>
        /// 지정된 인덱스 위치의 컨트롤을 가져옵니다.
        /// </summary>
        /// <param name="index">목록 내 인덱스</param>
        /// <returns>해당 인덱스의 컨트롤</returns>
        public IGoControl? this[int index] => Controls[index];
        /// <summary>
        /// 지정된 열과 행 위치의 컨트롤을 가져옵니다.
        /// </summary>
        /// <param name="col">열 인덱스</param>
        /// <param name="row">행 인덱스</param>
        /// <returns>해당 위치의 컨트롤. 없으면 null</returns>
        public IGoControl? this[int col, int row] => Controls.FirstOrDefault(c => Indexes.TryGetValue(c.Id, out var index) && index.Column == col && index.Row == row);
        /// <summary>
        /// 지정된 컨트롤의 그리드 인덱스 정보를 가져옵니다.
        /// </summary>
        /// <param name="control">대상 컨트롤</param>
        /// <returns>그리드 인덱스 정보. 없으면 null</returns>
        public GoGridIndex? this[IGoControl? control] => control != null && Indexes.TryGetValue(control.Id, out var index) ? index : null;
        #endregion

        #region Method
        /// <summary>
        /// 컨트롤을 지정된 열, 행 위치에 추가합니다.
        /// </summary>
        /// <param name="control">추가할 컨트롤</param>
        /// <param name="col">열 인덱스</param>
        /// <param name="row">행 인덱스</param>
        public void Add(IGoControl control, int col, int row)
        {
            Controls.Add(control);
            Indexes.Add(control.Id, new GoGridIndex { Column = col, Row = row });
        }

        /// <summary>
        /// 컨트롤을 컬렉션에서 제거합니다.
        /// </summary>
        /// <param name="control">제거할 컨트롤</param>
        /// <returns>제거 성공 여부</returns>
        public bool Remove(IGoControl control)
        {
            if (Controls.Remove(control))
            {
                Indexes.Remove(control.Id);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 모든 컨트롤과 인덱스 정보를 제거합니다.
        /// </summary>
        public void Clear()
        {
            Controls.Clear();
            Indexes.Clear();
        }

        /// <summary>
        /// 컬렉션에 지정된 컨트롤이 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="control">확인할 컨트롤</param>
        /// <returns>포함 여부</returns>
        public bool Contains(IGoControl control) => control != null && Controls.Contains(control);

        /// <summary>
        /// 컨트롤 컬렉션의 열거자를 반환합니다.
        /// </summary>
        /// <returns>컨트롤 열거자</returns>
        public IEnumerator<IGoControl> GetEnumerator() => Controls.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Controls.GetEnumerator();
        #endregion
    }
}
