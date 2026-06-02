using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// 자식들을 가로 또는 세로로 균등 분할하여 배치하는 레이아웃 컨테이너입니다 (Vo 패밀리, flexbox-lite).
    /// 자신은 그리지 않고 배치만 담당합니다.
    /// </summary>
    public class VoStack : GoContainer
    {
        /// <summary>배치 방향. Horizon이면 가로, Vertical이면 세로.</summary>
        [GoProperty(PCategory.Control, 0)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Vertical;
        /// <summary>자식 사이 간격.</summary>
        [GoProperty(PCategory.Control, 1)] public float Spacing { get; set; } = 0F;

        /// <summary>자식 컨트롤 목록.</summary>
        [GoChildList]
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        /// <summary>JSON 역직렬화용 생성자.</summary>
        [JsonConstructor] public VoStack(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary><see cref="VoStack"/>의 새 인스턴스를 초기화합니다.</summary>
        public VoStack() { }

        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var rt = Areas()["Content"];
            var items = Childrens.ToList();
            int n = items.Count;
            if (n == 0) return;

            bool horizon = Direction == GoDirectionHV.Horizon;
            float total = (horizon ? rt.Width : rt.Height) - Spacing * (n - 1);
            float each = total / n;

            float pos = horizon ? rt.Left : rt.Top;
            foreach (var c in items)
            {
                var cell = horizon
                    ? new SKRect(pos, rt.Top, pos + each, rt.Bottom)
                    : new SKRect(rt.Left, pos, rt.Right, pos + each);
                c.Bounds = Util.FromRect(cell, c.Margin);
                pos += each + Spacing;
            }
        }
    }
}
