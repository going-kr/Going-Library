using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 키보드 보조 키(modifier key) 플래그를 정의하는 열거형입니다.
    /// </summary>
    [Flags]
    public enum GoKeyModifiers
    {
        /// <summary>
        /// Shift 키가 눌린 상태
        /// </summary>
        Shift = 0x0001,

        /// <summary>
        /// Control 키가 눌린 상태
        /// </summary>
        Control = 0x0002,

        /// <summary>
        /// Alt 키가 눌린 상태
        /// </summary>
        Alt = 0x0004,

        /// <summary>
        /// Super(Windows) 키가 눌린 상태
        /// </summary>
        Super = 0x0008,

        /// <summary>
        /// Caps Lock이 활성화된 상태
        /// </summary>
        CapsLock = 0x0010,

        /// <summary>
        /// Num Lock이 활성화된 상태
        /// </summary>
        NumLock = 0x0020,
    }

    /// <summary>
    /// 키보드 키 코드를 정의하는 열거형입니다.
    /// </summary>
    public enum GoKeys : int
    {
        /// <summary>
        /// 알 수 없는 키
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 스페이스바 키
        /// </summary>
        Space = 32,

        /// <summary>
        /// 아포스트로피(') 키
        /// </summary>
        Apostrophe = 39 /* ' */,

        /// <summary>
        /// 쉼표(,) 키
        /// </summary>
        Comma = 44 /* , */,

        /// <summary>
        /// 빼기(-) 키
        /// </summary>
        Minus = 45 /* - */,

        /// <summary>
        /// 마침표(.) 키
        /// </summary>
        Period = 46 /* . */,

        /// <summary>
        /// 슬래시(/) 키
        /// </summary>
        Slash = 47 /* / */,

        /// <summary>
        /// 숫자 0 키
        /// </summary>
        D0 = 48,

        /// <summary>
        /// 숫자 1 키
        /// </summary>
        D1 = 49,

        /// <summary>
        /// 숫자 2 키
        /// </summary>
        D2 = 50,

        /// <summary>
        /// 숫자 3 키
        /// </summary>
        D3 = 51,

        /// <summary>
        /// 숫자 4 키
        /// </summary>
        D4 = 52,

        /// <summary>
        /// 숫자 5 키
        /// </summary>
        D5 = 53,

        /// <summary>
        /// 숫자 6 키
        /// </summary>
        D6 = 54,

        /// <summary>
        /// 숫자 7 키
        /// </summary>
        D7 = 55,

        /// <summary>
        /// 숫자 8 키
        /// </summary>
        D8 = 56,

        /// <summary>
        /// 숫자 9 키
        /// </summary>
        D9 = 57,

        /// <summary>
        /// 세미콜론(;) 키
        /// </summary>
        Semicolon = 59 /* ; */,

        /// <summary>
        /// 등호(=) 키
        /// </summary>
        Equal = 61 /* = */,

        /// <summary>
        /// A 키
        /// </summary>
        A = 65,

        /// <summary>
        /// B 키
        /// </summary>
        B = 66,

        /// <summary>
        /// C 키
        /// </summary>
        C = 67,

        /// <summary>
        /// D 키
        /// </summary>
        D = 68,

        /// <summary>
        /// E 키
        /// </summary>
        E = 69,

        /// <summary>
        /// F 키
        /// </summary>
        F = 70,

        /// <summary>
        /// G 키
        /// </summary>
        G = 71,

        /// <summary>
        /// H 키
        /// </summary>
        H = 72,

        /// <summary>
        /// I 키
        /// </summary>
        I = 73,

        /// <summary>
        /// J 키
        /// </summary>
        J = 74,

        /// <summary>
        /// K 키
        /// </summary>
        K = 75,

        /// <summary>
        /// L 키
        /// </summary>
        L = 76,

        /// <summary>
        /// M 키
        /// </summary>
        M = 77,

        /// <summary>
        /// N 키
        /// </summary>
        N = 78,

        /// <summary>
        /// O 키
        /// </summary>
        O = 79,

        /// <summary>
        /// P 키
        /// </summary>
        P = 80,

        /// <summary>
        /// Q 키
        /// </summary>
        Q = 81,

        /// <summary>
        /// R 키
        /// </summary>
        R = 82,

        /// <summary>
        /// S 키
        /// </summary>
        S = 83,

        /// <summary>
        /// T 키
        /// </summary>
        T = 84,

        /// <summary>
        /// U 키
        /// </summary>
        U = 85,

        /// <summary>
        /// V 키
        /// </summary>
        V = 86,

        /// <summary>
        /// W 키
        /// </summary>
        W = 87,

        /// <summary>
        /// X 키
        /// </summary>
        X = 88,

        /// <summary>
        /// Y 키
        /// </summary>
        Y = 89,

        /// <summary>
        /// Z 키
        /// </summary>
        Z = 90,

        /// <summary>
        /// 왼쪽 대괄호([) 키
        /// </summary>
        LeftBracket = 91 /* [ */,

        /// <summary>
        /// 백슬래시(\) 키
        /// </summary>
        Backslash = 92 /* \ */,

        /// <summary>
        /// 오른쪽 대괄호(]) 키
        /// </summary>
        RightBracket = 93 /* ] */,

        /// <summary>
        /// 백틱(`) 키
        /// </summary>
        GraveAccent = 96 /* ` */,

        /// <summary>
        /// Escape 키
        /// </summary>
        Escape = 256,

        /// <summary>
        /// Enter 키
        /// </summary>
        Enter = 257,

        /// <summary>
        /// Tab 키
        /// </summary>
        Tab = 258,

        /// <summary>
        /// Backspace 키
        /// </summary>
        Backspace = 259,

        /// <summary>
        /// Insert 키
        /// </summary>
        Insert = 260,

        /// <summary>
        /// Delete 키
        /// </summary>
        Delete = 261,

        /// <summary>
        /// 오른쪽 화살표 키
        /// </summary>
        Right = 262,

        /// <summary>
        /// 왼쪽 화살표 키
        /// </summary>
        Left = 263,

        /// <summary>
        /// 아래쪽 화살표 키
        /// </summary>
        Down = 264,

        /// <summary>
        /// 위쪽 화살표 키
        /// </summary>
        Up = 265,

        /// <summary>
        /// Page Up 키
        /// </summary>
        PageUp = 266,

        /// <summary>
        /// Page Down 키
        /// </summary>
        PageDown = 267,

        /// <summary>
        /// Home 키
        /// </summary>
        Home = 268,

        /// <summary>
        /// End 키
        /// </summary>
        End = 269,

        /// <summary>
        /// Caps Lock 키
        /// </summary>
        CapsLock = 280,

        /// <summary>
        /// Scroll Lock 키
        /// </summary>
        ScrollLock = 281,

        /// <summary>
        /// Num Lock 키
        /// </summary>
        NumLock = 282,

        /// <summary>
        /// Print Screen 키
        /// </summary>
        PrintScreen = 283,

        /// <summary>
        /// Pause 키
        /// </summary>
        Pause = 284,

        /// <summary>
        /// F1 키
        /// </summary>
        F1 = 290,

        /// <summary>
        /// F2 키
        /// </summary>
        F2 = 291,

        /// <summary>
        /// F3 키
        /// </summary>
        F3 = 292,

        /// <summary>
        /// F4 키
        /// </summary>
        F4 = 293,

        /// <summary>
        /// F5 키
        /// </summary>
        F5 = 294,

        /// <summary>
        /// F6 키
        /// </summary>
        F6 = 295,

        /// <summary>
        /// F7 키
        /// </summary>
        F7 = 296,

        /// <summary>
        /// F8 키
        /// </summary>
        F8 = 297,

        /// <summary>
        /// F9 키
        /// </summary>
        F9 = 298,

        /// <summary>
        /// F10 키
        /// </summary>
        F10 = 299,

        /// <summary>
        /// F11 키
        /// </summary>
        F11 = 300,

        /// <summary>
        /// F12 키
        /// </summary>
        F12 = 301,

        /// <summary>
        /// F13 키
        /// </summary>
        F13 = 302,

        /// <summary>
        /// F14 키
        /// </summary>
        F14 = 303,

        /// <summary>
        /// F15 키
        /// </summary>
        F15 = 304,

        /// <summary>
        /// F16 키
        /// </summary>
        F16 = 305,

        /// <summary>
        /// F17 키
        /// </summary>
        F17 = 306,

        /// <summary>
        /// F18 키
        /// </summary>
        F18 = 307,

        /// <summary>
        /// F19 키
        /// </summary>
        F19 = 308,

        /// <summary>
        /// F20 키
        /// </summary>
        F20 = 309,

        /// <summary>
        /// F21 키
        /// </summary>
        F21 = 310,

        /// <summary>
        /// F22 키
        /// </summary>
        F22 = 311,

        /// <summary>
        /// F23 키
        /// </summary>
        F23 = 312,

        /// <summary>
        /// F24 키
        /// </summary>
        F24 = 313,

        /// <summary>
        /// F25 키
        /// </summary>
        F25 = 314,

        /// <summary>
        /// 키패드 0 키
        /// </summary>
        KeyPad0 = 320,

        /// <summary>
        /// 키패드 1 키
        /// </summary>
        KeyPad1 = 321,

        /// <summary>
        /// 키패드 2 키
        /// </summary>
        KeyPad2 = 322,

        /// <summary>
        /// 키패드 3 키
        /// </summary>
        KeyPad3 = 323,

        /// <summary>
        /// 키패드 4 키
        /// </summary>
        KeyPad4 = 324,

        /// <summary>
        /// 키패드 5 키
        /// </summary>
        KeyPad5 = 325,

        /// <summary>
        /// 키패드 6 키
        /// </summary>
        KeyPad6 = 326,

        /// <summary>
        /// 키패드 7 키
        /// </summary>
        KeyPad7 = 327,

        /// <summary>
        /// 키패드 8 키
        /// </summary>
        KeyPad8 = 328,

        /// <summary>
        /// 키패드 9 키
        /// </summary>
        KeyPad9 = 329,

        /// <summary>
        /// 키패드 소수점(.) 키
        /// </summary>
        KeyPadDecimal = 330,

        /// <summary>
        /// 키패드 나누기(/) 키
        /// </summary>
        KeyPadDivide = 331,

        /// <summary>
        /// 키패드 곱하기(*) 키
        /// </summary>
        KeyPadMultiply = 332,

        /// <summary>
        /// 키패드 빼기(-) 키
        /// </summary>
        KeyPadSubtract = 333,

        /// <summary>
        /// 키패드 더하기(+) 키
        /// </summary>
        KeyPadAdd = 334,

        /// <summary>
        /// 키패드 Enter 키
        /// </summary>
        KeyPadEnter = 335,

        /// <summary>
        /// 키패드 등호(=) 키
        /// </summary>
        KeyPadEqual = 336,

        /// <summary>
        /// 왼쪽 Shift 키
        /// </summary>
        LeftShift = 340,

        /// <summary>
        /// 왼쪽 Control 키
        /// </summary>
        LeftControl = 341,

        /// <summary>
        /// 왼쪽 Alt 키
        /// </summary>
        LeftAlt = 342,

        /// <summary>
        /// 왼쪽 Super(Windows) 키
        /// </summary>
        LeftSuper = 343,

        /// <summary>
        /// 오른쪽 Shift 키
        /// </summary>
        RightShift = 344,

        /// <summary>
        /// 오른쪽 Control 키
        /// </summary>
        RightControl = 345,

        /// <summary>
        /// 오른쪽 Alt 키
        /// </summary>
        RightAlt = 346,

        /// <summary>
        /// 오른쪽 Super(Windows) 키
        /// </summary>
        RightSuper = 347,

        /// <summary>
        /// 메뉴 키
        /// </summary>
        Menu = 348,

        /// <summary>
        /// 이 열거형의 마지막 유효 키
        /// </summary>
        LastKey = Menu
    }
}
