using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    /// <summary>
    /// 행과 열을 기반으로 자식 컨트롤을 테이블 형태로 배치하는 레이아웃 패널입니다.
    /// </summary>
    public class GoTableLayoutPanel : GoContainer
    {
        #region Properties
        /// <summary>
        /// 열 크기 정의 목록을 가져오거나 설정합니다. 퍼센트("50%"), 픽셀("100px") 등의 문자열을 사용합니다.
        /// </summary>
        [GoSizesProperty(PCategory.Control, 0)] public List<string> Columns { get; set; } = [];
        /// <summary>
        /// 행 크기 정의 목록을 가져오거나 설정합니다. 퍼센트("50%"), 픽셀("100px") 등의 문자열을 사용합니다.
        /// </summary>
        [GoSizesProperty(PCategory.Control, 1)] public List<string> Rows { get; set; } = [];

        /// <summary>
        /// 테이블 레이아웃에 배치된 자식 컨트롤 컬렉션을 가져옵니다.
        /// </summary>
        [GoChilds]
        [JsonInclude] public override GoTableLayoutControlCollection Childrens { get; } = [];
        #endregion

        #region Constructor
        /// <summary>
        /// 자식 컨트롤 컬렉션을 사용하여 <see cref="GoTableLayoutPanel"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">테이블 레이아웃 컨트롤 컬렉션</param>
        [JsonConstructor]
        public GoTableLayoutPanel(GoTableLayoutControlCollection childrens) => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoTableLayoutPanel"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoTableLayoutPanel() { }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            if (Design != null && Design.DesignMode)
            {
                var rt = Areas()["Content"];
                rt.Inflate(-0.5F, -0.5F);
                rt.Offset(0.5F, 0.5F);
                var rts = Util.Grid(rt, Columns.ToArray(), Rows.ToArray());

                using var pe = SKPathEffect.CreateDash([1, 2], 2);
                using var p = new SKPaint { };

                p.IsStroke = true;
                p.StrokeWidth = 1;
                p.Color = thm.Base3;
                p.PathEffect = pe;
                p.IsAntialias = false;

                for (int i = 0; i < rts.GetLength(1); i++)
                {
                    var l = rts[0, i].Left;
                    var r = rts[0, i].Right;
                    canvas.DrawLine(r, rt.Top, r, rt.Bottom, p);
                    if (i == 0) canvas.DrawLine(l, rt.Top, l, rt.Bottom, p);
                }

                for (int i = 0; i < rts.GetLength(0); i++)
                {
                    var t = rts[i, 0].Top;
                    var b = rts[i, 0].Bottom;
                    canvas.DrawLine(rt.Left, b, rt.Right, b, p);
                    if (i == 0) canvas.DrawLine(rt.Left, t, rt.Right, t, p);
                }

                p.PathEffect = null;
            }

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var rt = Areas()["Content"];
            var rts = Util.Grid(rt, Columns.ToArray(), Rows.ToArray());
            foreach (var c in Childrens)
            {
                try
                {
                    var idx = Childrens.Indexes[c.Id];
                    if (idx != null)
                    {
                        var vrt = Util.Merge(rts, idx.Column, idx.Row, idx.ColSpan, idx.RowSpan);
                        c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        //if (c.Fill) c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        //else c.Bounds = Util.Int(Util.FromRect(vrt.Left, vrt.Top, Math.Min(vrt.Width, c.Bounds.Width), Math.Min(vrt.Height, c.Bounds.Height)));
                    }
                }
                catch { }
            }
        }
        #endregion

    }

    #region class : GoTableIndex
    /// <summary>
    /// 테이블 레이아웃에서 컨트롤의 행, 열 위치 및 병합 범위를 정의하는 클래스입니다.
    /// </summary>
    public class GoTableIndex
    {
        /// <summary>
        /// 행 인덱스를 가져오거나 설정합니다.
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// 열 인덱스를 가져오거나 설정합니다.
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// 행 병합 수를 가져오거나 설정합니다.
        /// </summary>
        public int RowSpan { get; set; } = 1;
        /// <summary>
        /// 열 병합 수를 가져오거나 설정합니다.
        /// </summary>
        public int ColSpan { get; set; } = 1;
    }
    #endregion

    /// <summary>
    /// 테이블 레이아웃 패널의 자식 컨트롤과 인덱스 정보를 관리하는 컬렉션 클래스입니다.
    /// </summary>
    public class GoTableLayoutControlCollection : IEnumerable<IGoControl>
    {
        #region Properties
        /// <summary>
        /// 컨트롤 ID와 테이블 인덱스의 매핑 딕셔너리를 가져옵니다.
        /// </summary>
        public Dictionary<Guid, GoTableIndex> Indexes { get; } = [];
        /// <summary>
        /// 컨트롤 목록을 가져옵니다.
        /// </summary>
        public List<IGoControl> Controls { get; } = [];
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
        /// 지정된 컨트롤의 테이블 인덱스 정보를 가져옵니다.
        /// </summary>
        /// <param name="control">대상 컨트롤</param>
        /// <returns>테이블 인덱스 정보. 없으면 null</returns>
        public GoTableIndex? this[IGoControl? control] => control != null && Indexes.TryGetValue(control.Id, out var index) ? index : null;
        #endregion

        #region Method
        /// <summary>
        /// 컨트롤을 지정된 열, 행 위치에 추가합니다.
        /// </summary>
        /// <param name="control">추가할 컨트롤</param>
        /// <param name="col">열 인덱스</param>
        /// <param name="row">행 인덱스</param>
        /// <param name="colspan">열 병합 수</param>
        /// <param name="rowspan">행 병합 수</param>
        public void Add(IGoControl control, int col, int row, int colspan = 1, int rowspan = 1)
        {
            Controls.Add(control);
            Indexes.Add(control.Id, new GoTableIndex { Column = col, Row = row, ColSpan = colspan, RowSpan = rowspan });
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
