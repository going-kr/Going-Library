using Going.UI.Controls;
using Going.UI.Gudx;
using Going.UI.Themes;
using SkiaSharp;
using System.Collections.Generic;

namespace Going.UI.ViewObjects
{
    /// <summary>
    /// VoObj 트리를 호스팅하는 <see cref="GoControl"/>입니다.
    /// <para>여러 VoObj가 모여 하나의 컨트롤이 된다는 개념의 진입점입니다.
    /// <see cref="Children"/>(보통 루트 컨테이너 1개)을 컨트롤의 콘텐츠 영역에 채워 그립니다.</para>
    /// <para>자식이 비-IGoControl인 VoObj이므로 P4 wrapper로 직렬화되어 폴리모픽 역직렬화가 됩니다.</para>
    /// </summary>
    public class VoControl : GoControl
    {
        /// <summary>이 컨트롤을 구성하는 VoObj 트리들. 보통 루트 컨테이너 하나입니다.</summary>
        [GoChildWrappers] public List<VoObj> Children { get; set; } = [];

        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rtContent = Areas()["Content"];
            foreach (var vo in Children)
                vo.Draw(canvas, rtContent, thm);

            base.OnDraw(canvas, thm);
        }
    }
}
