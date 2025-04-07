using Going.UI.Themes;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UI.Forms.Menus
{
    public class GoMenuStrip : MenuStrip
    {
        #region Member Variable
        GoTheme? thm;
        #endregion

        #region Constructor
        public GoMenuStrip()
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            SetRenderer();
        }
        #endregion

        #region Override
        protected override void OnHandleCreated(EventArgs e) { SetRenderer(); base.OnHandleCreated(e); }
        protected override void OnCreateControl() { SetRenderer(); base.OnCreateControl(); }
        protected override void OnParentChanged(EventArgs e) { SetRenderer(); base.OnParentChanged(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            SetRenderer();
            base.OnPaint(e);
        }
        #endregion

        #region Method
        #region SetRenderer
        private void SetRenderer()
        {
            if (GoTheme.Current != thm)
            {
                thm = GoTheme.Current;
                var c = new ThemeMenuColorTable(thm);
                Renderer = new GoToolStripProfessionalRenderer(c);
                this.BackColor = c.MenuStripColor;
                this.ForeColor = c.TextColor;
            }
        }
        #endregion
        #endregion
    }

    public class GoContextMenuStrip : ContextMenuStrip
    {
        #region Member Variable
        GoTheme? thm;
        #endregion

        #region Constructor
        public GoContextMenuStrip()
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            SetRenderer();
        }
        #endregion

        #region Override
        protected override void OnHandleCreated(EventArgs e) { SetRenderer(); base.OnHandleCreated(e); }
        protected override void OnCreateControl() { SetRenderer(); base.OnCreateControl(); }
        protected override void OnParentChanged(EventArgs e) { SetRenderer(); base.OnParentChanged(e); }
        protected override void OnPaint(PaintEventArgs e) { SetRenderer(); base.OnPaint(e); }
        #endregion

        #region Method
        #region SetRenderer
        private void SetRenderer()
        {
            if (GoTheme.Current != thm)
            {
                thm = GoTheme.Current;
                var c = new ThemeMenuColorTable(thm);
                Renderer = new GoToolStripProfessionalRenderer(c);
                this.BackColor = c.MenuStripColor;
                this.ForeColor = c.TextColor;
            }
        }
        #endregion
        #endregion
    }

    #region ThemeMenuColorTable
    public class ThemeMenuColorTable(GoTheme thm) : ProfessionalColorTable
    {
        #region Properties
        public Color TextColor { get; } = Util.FromArgb(thm.Base5);
        public Color MenuStripColor { get; } = Util.FromArgb(thm.Window);
        public Color MenuItemPressedColor { get; } = Util.FromArgb(thm.Window);
        public Color ToolStripDropDownBackgroundColor { get; } = Util.FromArgb(thm.Window);

        public Color MenuItemSelectedColor { get; } = Util.FromArgb(thm.Base1);
        public Color ImageMarginColor { get; } = Util.FromArgb(thm.Base2);

        public Color MenuItemBorderColor { get; } = Util.FromArgb(thm.Base3);
        public Color MenuBorderColor { get; } = Util.FromArgb(thm.Base3);
        public Color SeparatorColor { get; } = Util.FromArgb(thm.Base3);
        #endregion
        #region Override
        //메뉴바 색상
        public override Color MenuStripGradientBegin => MenuStripColor; 
        public override Color MenuStripGradientEnd => MenuStripColor; 

        //메뉴선택
        public override Color MenuItemSelectedGradientBegin => MenuItemSelectedColor;
        public override Color MenuItemSelectedGradientEnd =>MenuItemSelectedColor;
        public override Color MenuItemSelected => MenuItemSelectedColor;

        //메뉴아이템 보더
        public override Color MenuItemBorder => MenuItemBorderColor;

        //메뉴전체 보더
        public override Color MenuBorder => MenuBorderColor;

        //메뉴선택시 아이템색상
        public override Color MenuItemPressedGradientBegin => MenuItemPressedColor; 
        public override Color MenuItemPressedGradientMiddle => MenuItemPressedColor;
        public override Color MenuItemPressedGradientEnd => MenuItemPressedColor; 

        //메뉴펼쳐질시 배경색
        public override Color ToolStripDropDownBackground => ToolStripDropDownBackgroundColor;

        //이미지 아이콘 영역 배경색
        public override Color ImageMarginGradientBegin => ImageMarginColor;
        public override Color ImageMarginGradientEnd => ImageMarginColor;
        public override Color ImageMarginGradientMiddle => ImageMarginColor;

        public override Color SeparatorDark => SeparatorColor;
        public override Color SeparatorLight => SeparatorColor;

        //화살표
        /*
        public override Color ToolStripBorder { get { return Color.Black; } }
        public override Color ToolStripContentPanelGradientBegin { get { return Color.FromArgb(30, 30, 30); } }
        public override Color ToolStripContentPanelGradientEnd { get { return Color.FromArgb(30, 30, 30); } }
        public override Color ToolStripGradientBegin { get { return Color.Black; } }
        public override Color ToolStripGradientMiddle { get { return Color.Black; } }
        public override Color ToolStripGradientEnd { get { return Color.Black; } }
        public override Color ToolStripPanelGradientBegin { get { return Color.DarkSlateGray; } }
        public override Color ToolStripPanelGradientEnd { get { return Color.DarkSlateGray; } }
        public override Color RaftingContainerGradientBegin { get { return Color.DarkSlateGray; } }
        public override Color RaftingContainerGradientEnd { get { return Color.DarkSlateGray; } }

        public override Color ImageMarginGradientBegin { get { return Color.FromArgb(88, 104, 120); } }
        public override Color ImageMarginGradientEnd { get { return Color.FromArgb(88, 104, 120); } }
        public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(88, 104, 120); } }

        public override Color CheckSelectedBackground { get { return Color.Orange; } }
        public override Color CheckBackground { get { return Color.Orange; } }
        public override Color CheckPressedBackground { get { return Color.SlateGray; } }
        public override Color ButtonCheckedHighlightBorder { get { return Color.OrangeRed; } }
        */
        #endregion
    }
    #endregion

    #region GoToolStripProfessionalRenderer
    public class GoToolStripProfessionalRenderer : ToolStripProfessionalRenderer
    {
        public GoToolStripProfessionalRenderer(ProfessionalColorTable c) : base(c) { }
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            ProfessionalColorTable colorTable = this.ColorTable;
            if (colorTable != null)
            {
                e.ArrowColor = colorTable.MenuItemBorder;
            }
            base.OnRenderArrow(e);
        }
    }
    #endregion
}
