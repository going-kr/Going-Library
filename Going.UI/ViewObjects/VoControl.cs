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

        // Name → VoObj 인덱스. 최초 Node 호출 시 1회만 빌드하여 캐시한다.
        private Dictionary<string, VoObj>? _index;

        /// <summary>
        /// 트리에서 <paramref name="name"/> 노드를 찾아 <typeparamref name="T"/>로 반환합니다.
        /// 이름 인덱스는 1회만 빌드/캐시되므로, 결과 참조를 보관해두면 OnUpdate에서 반복 호출 없이
        /// 값만 바꿔도 됩니다 (권장: <c>_gauge = vo.Node&lt;VoArc&gt;("Gauge")</c> 후 <c>_gauge.Value = x</c>).
        /// </summary>
        /// <returns>해당 노드. 없거나 타입이 다르면 null.</returns>
        public T? Node<T>(string name) where T : VoObj
        {
            _index ??= BuildIndex();
            return _index.TryGetValue(name, out var vo) ? vo as T : null;
        }

        /// <summary>트리 구조를 바꾼 뒤 이름 인덱스를 무효화합니다.</summary>
        public void InvalidateIndex() => _index = null;

        private Dictionary<string, VoObj> BuildIndex()
        {
            var map = new Dictionary<string, VoObj>();
            void Walk(VoObj vo)
            {
                if (!string.IsNullOrEmpty(vo.Name)) map[vo.Name!] = vo;
                foreach (var c in vo.Children) Walk(c);
            }
            foreach (var vo in Children) Walk(vo);
            return map;
        }

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
