using Going.UI.Controls;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Going.UI.Managers
{
    /// <summary>
    /// 컨트롤의 텍스트/숫자 입력 이벤트를 관리하는 싱글톤 클래스입니다.
    /// </summary>
    public class GoInputEventer
    {
        private static readonly Lazy<GoInputEventer> _instance = new Lazy<GoInputEventer>(() => new GoInputEventer());
        /// <summary>
        /// GoInputEventer의 싱글톤 인스턴스를 가져옵니다.
        /// </summary>
        public static GoInputEventer Current => _instance.Value;
        /// <summary>
        /// 현재 입력 중인 컨트롤을 가져옵니다.
        /// </summary>
        public IGoControl? InputControl { get; private set; }

        private GoInputEventer() { }

        /// <summary>
        /// 문자열 입력이 필요할 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<IGoControl, SKRect, Action<string>, string?>? InputString;
        /// <summary>
        /// 숫자 입력이 필요할 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<IGoControl, SKRect, Action<string>, Type, object, object?, object?>? InputNumber;

        /// <summary>
        /// 문자열 입력 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="control">입력을 요청한 컨트롤</param>
        /// <param name="bounds">입력 영역</param>
        /// <param name="callback">입력 완료 콜백</param>
        /// <param name="value">현재 값</param>
        public void FireInputString(IGoControl control, SKRect bounds, Action<string> callback, string? value) => InputString?.Invoke(control, bounds, callback, value);
        /// <summary>
        /// 숫자 입력 이벤트를 발생시킵니다.
        /// </summary>
        /// <typeparam name="T">숫자 타입</typeparam>
        /// <param name="control">입력을 요청한 컨트롤</param>
        /// <param name="bounds">입력 영역</param>
        /// <param name="callback">입력 완료 콜백</param>
        /// <param name="value">현재 값</param>
        /// <param name="min">최솟값</param>
        /// <param name="max">최댓값</param>
        public void FireInputNumber<T>(IGoControl control, SKRect bounds, Action<string> callback, T value, T? min, T? max) where T : struct => InputNumber?.Invoke(control, bounds, callback, typeof(T), value, min, max);

        /// <summary>
        /// 현재 입력 컨트롤을 설정합니다.
        /// </summary>
        /// <param name="control">입력 중인 컨트롤</param>
        public void SetInputControl(IGoControl control) => InputControl = control;
        /// <summary>
        /// 현재 입력 컨트롤을 해제합니다.
        /// </summary>
        public void ClearInputControl() => InputControl = null;
    }

    /// <summary>
    /// 입력 타입을 정의합니다.
    /// </summary>
    public enum InputType
    {
        /// <summary>문자열 입력</summary>
        String,
        /// <summary>숫자 입력</summary>
        Number
    }

}
