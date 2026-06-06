using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Utils;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Going.UI.Containers
{
    /// <summary>
    /// 자식들을 한 방향(가로/세로)으로 <b>각자의 크기 그대로</b> 순차 적층하는 레이아웃 컨테이너입니다.
    /// 주축(main-axis) 크기는 자식의 Bounds 크기를 쓰고 <see cref="Spacing"/>만큼 띄웁니다.
    /// 교차축(cross-axis)은 <see cref="Alignment"/>로 정렬하거나 가득 채웁니다. 자신은 그리지 않고 배치만 합니다.
    /// </summary>
    public class GoStackLayout : GoContainer
    {
        /// <summary>적층 방향. Vertical이면 세로(위→아래), Horizon이면 가로(좌→우).</summary>
        [GoProperty(PCategory.Control, 0)] public GoDirectionHV Direction { get; set; } = GoDirectionHV.Vertical;
        /// <summary>자식 사이 간격.</summary>
        [GoProperty(PCategory.Control, 1)] public float Spacing { get; set; } = 0F;
        /// <summary>교차축 정렬(자식 크기 유지) 또는 채움.</summary>
        [GoProperty(PCategory.Control, 2)] public GoStackAlignment Alignment { get; set; } = GoStackAlignment.Near;

        /// <summary>자식 컨트롤 목록.</summary>
        [GoChildList]
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        /// <summary>JSON 역직렬화용 생성자.</summary>
        [JsonConstructor] public GoStackLayout(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary><see cref="GoStackLayout"/>의 새 인스턴스를 초기화합니다.</summary>
        public GoStackLayout() { }

        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var rt = Areas()["Content"];
            bool vert = Direction == GoDirectionHV.Vertical;
            float pos = vert ? rt.Top : rt.Left;

            foreach (var c in Childrens)
            {
                var m = c.Margin;

                if (vert)
                {
                    float h = c.Height;                                   // 자연 높이(주축)
                    float availW = rt.Width - m.Left - m.Right;
                    float w = Alignment == GoStackAlignment.Fill ? availW : c.Width;
                    float x = Alignment switch
                    {
                        GoStackAlignment.Center => rt.Left + (rt.Width - w) / 2F,
                        GoStackAlignment.Far => rt.Right - m.Right - w,
                        _ => rt.Left + m.Left,                            // Near / Fill
                    };
                    c.Left = x; c.Top = pos + m.Top; c.Width = w; c.Height = h;
                    pos += m.Top + h + m.Bottom + Spacing;
                }
                else
                {
                    float w = c.Width;                                    // 자연 너비(주축)
                    float availH = rt.Height - m.Top - m.Bottom;
                    float h = Alignment == GoStackAlignment.Fill ? availH : c.Height;
                    float y = Alignment switch
                    {
                        GoStackAlignment.Center => rt.Top + (rt.Height - h) / 2F,
                        GoStackAlignment.Far => rt.Bottom - m.Bottom - h,
                        _ => rt.Top + m.Top,                              // Near / Fill
                    };
                    c.Left = pos + m.Left; c.Top = y; c.Width = w; c.Height = h;
                    pos += m.Left + w + m.Right + Spacing;
                }
            }
        }
    }
}
