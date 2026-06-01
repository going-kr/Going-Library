using Going.UI.Datas;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// ViewObject 계열의 경량 추상 기반 클래스입니다.
    /// <para><see cref="Going.UI.Controls.GoControl"/>을 상속하지 <b>않습니다</b> — 라이프사이클/이벤트/직렬화
    /// 무게를 지지 않는 "순수 뷰 노드"입니다. Skia로 자신을 그리고, 자식 VoObj를 가질 수 있습니다.</para>
    /// <para>여러 VoObj를 조립하여 하나의 <see cref="VoControl"/>(= GoControl)을 구성합니다.</para>
    /// </summary>
    public abstract class VoObj
    {
        /// <summary>노드 이름. 호스트가 히트테스트/참조에 사용합니다.</summary>
        [VoProperty("Layout", 0)] public string? Name { get; set; }

        /// <summary>표시 여부.</summary>
        [VoProperty("Layout", 1)] public bool Visible { get; set; } = true;

        /// <summary>바깥 여백. 할당된 경계에서 이만큼 안쪽으로 들어가 자신을 그립니다.</summary>
        [VoProperty("Layout", 2)] public GoPadding Margin { get; set; } = new(0);

        /// <summary>안쪽 여백. 자식 배치 영역을 이만큼 줄입니다.</summary>
        [VoProperty("Layout", 3)] public GoPadding Padding { get; set; } = new(0);

        /// <summary>VoGrid 안에서의 열 위치.</summary>
        [VoProperty("Cell", 0)] public int Col { get; set; } = 0;
        /// <summary>VoGrid 안에서의 행 위치.</summary>
        [VoProperty("Cell", 1)] public int Row { get; set; } = 0;
        /// <summary>VoGrid 안에서의 열 병합 수.</summary>
        [VoProperty("Cell", 2)] public int ColSpan { get; set; } = 1;
        /// <summary>VoGrid 안에서의 행 병합 수.</summary>
        [VoProperty("Cell", 3)] public int RowSpan { get; set; } = 1;

        /// <summary>자식 노드 목록 (Composite). P4 wrapper로 직렬화됩니다.</summary>
        [GoChildWrappers] public List<VoObj> Children { get; set; } = [];

        /// <summary>레이아웃이 계산해 넣어주는 현재 경계(컨트롤 로컬 좌표). 런타임 전용.</summary>
        [JsonIgnore] public SKRect Bounds { get; private set; }

        /// <summary>
        /// 주어진 경계에 자신을 그리고, 자식들을 배치하여 재귀적으로 그립니다.
        /// </summary>
        /// <param name="canvas">대상 캔버스 (컨트롤 로컬 좌표)</param>
        /// <param name="bounds">이 노드에 할당된 경계</param>
        /// <param name="thm">현재 테마 (색 해석에 사용)</param>
        public void Draw(SKCanvas canvas, SKRect bounds, GoTheme thm)
        {
            if (!Visible) return;

            var box = Deflate(bounds, Margin);
            Bounds = box;
            OnDraw(canvas, box, thm);

            var inner = Deflate(box, Padding);
            var rects = ArrangeChildren(inner);
            for (int i = 0; i < Children.Count; i++)
            {
                if (i < rects.Length) Children[i].Draw(canvas, rects[i], thm);
            }
        }

        /// <summary>사각형을 패딩만큼 안쪽으로 줄입니다.</summary>
        protected static SKRect Deflate(SKRect rt, GoPadding p)
            => new(rt.Left + p.Left, rt.Top + p.Top, rt.Right - p.Right, rt.Bottom - p.Bottom);

        /// <summary>이 노드 자신의 그림을 그립니다. (자식 제외)</summary>
        protected abstract void OnDraw(SKCanvas canvas, SKRect bounds, GoTheme thm);

        /// <summary>
        /// 자식들의 경계를 계산합니다. 기본 동작은 모든 자식이 부모 영역을 채우는 것(겹침)입니다.
        /// 레이아웃 컨테이너(VoStack/VoGrid 등)가 이 메서드를 재정의합니다.
        /// </summary>
        protected virtual SKRect[] ArrangeChildren(SKRect bounds)
        {
            var rects = new SKRect[Children.Count];
            for (int i = 0; i < rects.Length; i++) rects[i] = bounds;
            return rects;
        }
    }
}
