using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    /// <summary>
    /// 여러 서브 페이지 중 하나를 선택하여 표시하는 전환(스위칭) 패널입니다.
    /// </summary>
    public class GoSwitchPanel : GoContainer
    {
        #region Properties
        /// <summary>
        /// 현재 선택된 페이지를 가져오거나 설정합니다. 변경 시 <see cref="PageChanging"/> 및 <see cref="PageChanged"/> 이벤트가 발생합니다.
        /// </summary>
        [JsonIgnore]
        public GoSubPage? SelectedPage
        {
            get => selPage;
            set
            {
                if ((value == null || (value != null && Pages.Contains(value))) && selPage != value)
                {
                    var args = new CancelEventArgs { Cancel = false };
                    PageChanging?.Invoke(this, args);

                    if (!args.Cancel)
                    {
                        if (selPage != null) foreach (var c in selPage.Childrens) c.FireHide();
                        selPage = value;
                        if (selPage != null) foreach (var c in selPage.Childrens) c.FireShow();

                        PageChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 선택된 페이지의 자식 컨트롤 컬렉션을 가져옵니다. 선택된 페이지가 없으면 빈 컬렉션을 반환합니다.
        /// </summary>
        [JsonIgnore] public override IEnumerable<IGoControl> Childrens => SelectedPage?.Childrens ?? [];
        /// <summary>
        /// 서브 페이지 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoChildWrappers]
        [GoProperty(PCategory.Control, 0)] public List<GoSubPage> Pages { get; set; } = [];
        //[GoProperty(PCategory.Misc, 0), JsonInclude] public List<GoSubPage> Pages { get; } = [];
        #endregion

        #region Event
        /// <summary>
        /// 페이지가 변경된 후 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler? PageChanged;
        /// <summary>
        /// 페이지가 변경되기 전에 발생하는 이벤트입니다. Cancel을 true로 설정하면 변경을 취소할 수 있습니다.
        /// </summary>
        public event EventHandler<CancelEventArgs>? PageChanging;
        #endregion

        #region Member Variable
        GoSubPage? selPage;
        #endregion

        #region Constructor
        //[JsonConstructor]
        //public GoSwitchPanel(List<GoSubPage> pages) : this() => this.Pages = pages;
        /// <summary>
        /// <see cref="GoSwitchPanel"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoSwitchPanel() { }
        #endregion

        #region Override
        #region Init
        /// <inheritdoc/>
        protected override void OnInit(GoDesign? design)
        {
            base.OnInit(design);

            foreach (var p in Pages)
                foreach (var c in p.Childrens)
                {
                    c.FireInit(design);
                    if (c is GoControl c2) c2.Parent = this;
                }
        }
        #endregion

        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            if (FirstRender && SelectedPage == null) SelectedPage = Pages.FirstOrDefault();
            base.OnDraw(canvas, thm);
        }
        #endregion

        #region OnDispose
        /// <inheritdoc/>
        protected override void OnDispose()
        {
            //base.OnDispose();

            foreach (var p in Pages)
                foreach (var c in p.Childrens)
                    c.Dispose();
        }
        #endregion
        #endregion

        #region Method
        #region SetPage
        /// <summary>
        /// 이름으로 페이지를 선택합니다.
        /// </summary>
        /// <param name="name">선택할 페이지의 이름</param>
        public void SetPage(string name)
        {
            if (name != null)
            {
                var page = Pages.FirstOrDefault(x => x.Name == name);
                if (page != null) SelectedPage = page;
            }
        }
        #endregion
        #endregion
    }

    #region class : GoSubPage
    /// <summary>
    /// GoSwitchPanel에서 사용되는 서브 페이지 클래스입니다. 이름과 자식 컨트롤을 포함합니다.
    /// </summary>
    public class GoSubPage
    {
        /// <summary>
        /// 페이지 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string Name { get; set; }

        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [GoChildList]
        [JsonInclude] public List<IGoControl> Childrens { get; } = [];

        /// <summary>
        /// 자식 컨트롤 목록을 사용하여 <see cref="GoSubPage"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public GoSubPage(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoSubPage"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoSubPage() { }

        /// <inheritdoc/>
        public override string ToString() => Name;
    }
    #endregion
}
