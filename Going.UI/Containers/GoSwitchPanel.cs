using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
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
    public class GoSwitchPanel : GoContainer
    {
        #region Properties
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
                        selPage = value;
                        PageCnanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        [JsonIgnore] public override IEnumerable<IGoControl> Childrens => SelectedPage?.Childrens ?? [];
        [GoProperty(PCategory.Control, 0)] public List<GoSubPage> Pages { get; set; } = [];
        //[GoProperty(PCategory.Misc, 0), JsonInclude] public List<GoSubPage> Pages { get; } = [];
        #endregion

        #region Event
        public event EventHandler? PageCnanged;
        public event EventHandler<CancelEventArgs>? PageChanging;
        #endregion

        #region Member Variable
        GoSubPage? selPage;
        #endregion

        #region Constructor
        //[JsonConstructor]
        //public GoSwitchPanel(List<GoSubPage> pages) : this() => this.Pages = pages;
        public GoSwitchPanel() { }
        #endregion

        #region Override
        #region Init
        protected override void OnInit(GoDesign? design)
        {
            base.OnInit(design);

            foreach (var p in Pages)
                foreach (var c in p.Childrens)
                    c.FireInit(design);
        }
        #endregion

        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            if (FirstRender && SelectedPage == null) SelectedPage = Pages.FirstOrDefault();
            base.OnDraw(canvas);
        }
        #endregion
        #endregion

        #region Method
        #region SetPage
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
    public class GoSubPage
    {
        public string Name { get; set; }

        [JsonInclude] public List<IGoControl> Childrens { get; } = [];

        [JsonConstructor]
        public GoSubPage(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoSubPage() { }

        public override string ToString() => Name;
    }
    #endregion
}
