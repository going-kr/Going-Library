using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Dialogs
{
    /// <summary>
    /// 메시지 박스 다이얼로그. OK, OK/Cancel, Yes/No, Yes/No/Cancel 등 다양한 버튼 조합을 지원합니다.
    /// </summary>
    public class GoMessageBox : GoWindow
    {
        #region Properties
        /// <summary>
        /// '예' 버튼의 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string YesText { get => btnYes.Text; set => btnYes.Text = value; }
        /// <summary>
        /// '아니요' 버튼의 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string NoText { get => btnNo.Text; set => btnNo.Text = value; }
        /// <summary>
        /// '확인' 버튼의 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string OkText { get => btnOK.Text; set => btnOK.Text = value; }
        /// <summary>
        /// '취소' 버튼의 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string CancelText { get => btnCancel.Text; set => btnCancel.Text = value; }
        #endregion

        #region Member Variable
        GoTableLayoutPanel tpnl;
        GoLabel lbl;
        GoButton btnOK, btnCancel, btnYes, btnNo;

        Action<GoDialogResult>? callback;
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="GoMessageBox"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoMessageBox()
        {
            IconString = "fa-comment-dots";
            IconSize = 18;
            IconGap = 10;

            tpnl = new GoTableLayoutPanel { Dock = GoDockStyle.Fill, Margin = new GoPadding(10), Columns = ["16.66%", "16.66%", "16.66%", "16.66%", "16.66%", "16.67%",], Rows = ["100%", "40px"] };
            lbl = new GoLabel { Dock = GoDockStyle.Fill, BackgroundDraw = false };
            btnOK = new GoButton { Dock = GoDockStyle.Fill, Text="확인" };
            btnCancel = new GoButton { Dock = GoDockStyle.Fill, Text = "취소" };
            btnYes = new GoButton { Dock = GoDockStyle.Fill, Text = "예" };
            btnNo = new GoButton { Dock = GoDockStyle.Fill, Text = "아니요" };
            Childrens.Add(tpnl);

            btnOK.ButtonClicked += (o, s) => { Close(); callback?.Invoke(GoDialogResult.Ok); };
            btnCancel.ButtonClicked += (o, s) => { Close(); callback?.Invoke(GoDialogResult.Cancel); };
            btnYes.ButtonClicked += (o, s) => { Close(); callback?.Invoke(GoDialogResult.Yes); };
            btnNo.ButtonClicked += (o, s) => { Close(); callback?.Invoke(GoDialogResult.No); };
        }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnCloseButtonClick()
        {
            base.OnCloseButtonClick();
            callback?.Invoke(GoDialogResult.Cancel);
        }
        #endregion

        #region Method
        (float w, float h) show(string title, string message, Action<GoDialogResult> result)
        {
            this.Text = title;
            this.callback = result;
            lbl.Text = message;

            var sz = Util.MeasureText(message, lbl.FontName, lbl.FontStyle, lbl.FontSize);
            var w = Math.Max(180, sz.Width + 40);
            var h = Math.Max(120, sz.Height + TitleHeight + 40 + 40);

            return (w, h);
        }

        /// <summary>
        /// 확인 버튼만 있는 메시지 박스를 표시합니다.
        /// </summary>
        /// <param name="title">메시지 박스 제목</param>
        /// <param name="message">표시할 메시지</param>
        /// <param name="result">다이얼로그 결과 콜백</param>
        public void ShowMessageBoxOk(string title, string message, Action<GoDialogResult> result)
        {
            var (w, h) = show(title, message, result);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnOK, 1, 1, 4, 1);

            Show(w, h);
        }

        /// <summary>
        /// 확인/취소 버튼이 있는 메시지 박스를 표시합니다.
        /// </summary>
        /// <param name="title">메시지 박스 제목</param>
        /// <param name="message">표시할 메시지</param>
        /// <param name="result">다이얼로그 결과 콜백</param>
        public void ShowMessageBoxOkCancel(string title, string message, Action<GoDialogResult> result)
        {
            var (w, h) = show(title, message, result);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnOK, 0, 1, 3, 1);
            tpnl.Childrens.Add(btnCancel, 3, 1, 3, 1);

            Show(w, h);
        }

        /// <summary>
        /// 예/아니요 버튼이 있는 메시지 박스를 표시합니다.
        /// </summary>
        /// <param name="title">메시지 박스 제목</param>
        /// <param name="message">표시할 메시지</param>
        /// <param name="result">다이얼로그 결과 콜백</param>
        public void ShowMessageBoxYesNo(string title, string message, Action<GoDialogResult> result)
        {
            var (w, h) = show(title, message, result);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnYes, 0, 1, 3, 1);
            tpnl.Childrens.Add(btnNo, 3, 1, 3, 1);

            Show(w, h);
        }

        /// <summary>
        /// 예/아니요/취소 버튼이 있는 메시지 박스를 표시합니다.
        /// </summary>
        /// <param name="title">메시지 박스 제목</param>
        /// <param name="message">표시할 메시지</param>
        /// <param name="result">다이얼로그 결과 콜백</param>
        public void ShowMessageBoxYesNoCancel(string title, string message, Action<GoDialogResult> result)
        {
            var (w, h) = show(title, message, result);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnYes, 0, 1, 2, 1);
            tpnl.Childrens.Add(btnNo, 2, 1, 2, 1);
            tpnl.Childrens.Add(btnCancel, 4, 1, 2, 1);

            Show(w, h);
        }
        #endregion
    }
}
