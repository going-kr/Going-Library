using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 자식들을 CSS 그리드처럼 셀에 배치하는 레이아웃 컨테이너입니다 (Vo 패밀리).
    /// <para>열/행 크기는 <c>"100px"</c>, <c>"50%"</c>, <c>"*"</c> 토큰을 씁니다. 각 자식의 셀 위치/병합은
    /// <see cref="Childrens"/> 컬렉션의 Cell 인덱스(col,row,colSpan,rowSpan)로 지정합니다
    /// (<see cref="GoTableLayoutPanel"/>과 동일한 셀 모델 — 에디터가 셀 배치를 지원).</para>
    /// </summary>
    public class VoGrid : GoContainer
    {
        /// <summary>열 크기 정의 (예: ["50%","50%"], ["100px","*"]).</summary>
        [GoSizesProperty(PCategory.Control, 0)] public List<string> Columns { get; set; } = ["*"];
        /// <summary>행 크기 정의.</summary>
        [GoSizesProperty(PCategory.Control, 1)] public List<string> Rows { get; set; } = ["*"];

        /// <summary>셀에 배치된 자식 컨트롤 컬렉션.</summary>
        [GoChildCells]
        [JsonInclude] public override GoTableLayoutControlCollection Childrens { get; } = [];

        /// <summary>JSON 역직렬화용 생성자.</summary>
        [JsonConstructor] public VoGrid(GoTableLayoutControlCollection childrens) => Childrens = childrens ?? [];
        /// <summary><see cref="VoGrid"/>의 새 인스턴스를 초기화합니다.</summary>
        public VoGrid() { }

        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var rt = Areas()["Content"];
            var cols = Columns is { Count: > 0 } ? Columns.ToArray() : ["*"];
            var rows = Rows is { Count: > 0 } ? Rows.ToArray() : ["*"];
            var rts = Util.Grid(rt, cols, rows);
            foreach (var c in Childrens)
            {
                try
                {
                    var idx = Childrens.Indexes[c.Id];
                    if (idx != null)
                    {
                        var vrt = Util.Merge(rts, idx.Column, idx.Row, idx.ColSpan, idx.RowSpan);
                        c.Bounds = Util.FromRect(vrt, c.Margin);
                    }
                }
                catch { }
            }
        }
    }
}
