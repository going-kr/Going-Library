using Going.UI.Controls;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoGridLayoutPanel : GoContainer
    {
        #region Properties
        [GoProperty(PCategory.Misc, 0)] public List<GoGridLayoutPanelRow> Rows { get; set; } = [];

        [JsonInclude]
        public override GoGridLayoutControlCollection Childrens { get; } = [];
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoGridLayoutPanel(GoGridLayoutControlCollection childrens) => Childrens = childrens;
        public GoGridLayoutPanel() { }
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            if (Design != null && Design.DesignMode)
            {
                var rt = Areas()["Content"];
                
                using var pe = SKPathEffect.CreateDash([1, 2], 2);
                using var p = new SKPaint { };

                var rts = GridBounds();

                for (int ir = 0; ir < Rows.Count; ir++)
                {
                    for (int ic = 0; ic < Rows[ir].Columns.Count; ic++)
                    {
                        if (CellBounds(rts, ic, ir) is SKRect vrt)
                        {
                            var mrt = vrt;
                            mrt.Inflate(-0.5F, -0.5F);

                            p.IsStroke = true;
                            p.StrokeWidth = 1;
                            p.Color = GoTheme.Current.Base3;
                            p.PathEffect = pe;
                            canvas.DrawRect(mrt, p);
                            p.PathEffect = null;
                        }
                    }
                }
 
            }

            base.OnDraw(canvas);
        }
        #endregion
        #region OnLayout
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
                        //c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        if (c.Fill) c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        else c.Bounds = Util.Int(Util.FromRect(vrt.Left, vrt.Top, c.Bounds.Width, c.Bounds.Height));
                    }
                }
                catch { }
            }
        }
        #endregion
        #endregion

        #region Method
        public void AddRow(string height, string[] columns)
        {
            Rows.Add(new GoGridLayoutPanelRow { Height = height, Columns = [.. columns] });
        }

        public Dictionary<int, Dictionary<int, SKRect>> GridBounds() => GridBounds(Areas()["Content"]);
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


        public SKRect? CellBounds(Dictionary<int, Dictionary<int, SKRect>> rts, int col, int row)
        {
            SKRect? ret = null;
            if (rts.TryGetValue(row, out Dictionary<int, SKRect>? vrow) && vrow.TryGetValue(col, out SKRect vrt)) ret = vrt;
            return ret;
        }
        #endregion
    }

    #region class : GoGridLayoutPanelRow
    public class GoGridLayoutPanelRow
    {
        [GoSizeProperty(PCategory.Misc, 0)] public string Height { get; set; } = "100%";
        [GoSizesProperty(PCategory.Misc, 1)] public List<string> Columns { get; set; } = [];

        public override string ToString() => $"{Height} / Columns : {Columns.Count}";
    }
    #endregion
    #region class : GoGridIndex
    public class GoGridIndex
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
    #endregion

    public class GoGridLayoutControlCollection : IEnumerable<IGoControl>
    {
        #region Properties
        [JsonInclude] public Dictionary<Guid, GoGridIndex> Indexes { get; } = [];
        [JsonInclude] public List<IGoControl> Controls { get; } = [];
        #endregion

        #region Indexer
        public IGoControl? this[int index] => Controls[index];
        public IGoControl? this[int col, int row] => Controls.FirstOrDefault(c => Indexes.TryGetValue(c.Id, out var index) && index.Column == col && index.Row == row);
        public GoGridIndex? this[IGoControl? control] => control != null && Indexes.TryGetValue(control.Id, out var index) ? index : null;
        #endregion

        #region Method
        public void Add(IGoControl control, int col, int row)
        {
            Controls.Add(control);
            Indexes.Add(control.Id, new GoGridIndex { Column = col, Row = row });
        }

        public bool Remove(IGoControl control)
        {
            if (Controls.Remove(control))
            {
                Indexes.Remove(control.Id);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            Controls.Clear();
            Indexes.Clear();
        }

        public bool Contains(IGoControl control) => control != null && Controls.Contains(control);

        public IEnumerator<IGoControl> GetEnumerator() => Controls.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Controls.GetEnumerator();
        #endregion
    }
}
