using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 글꼴 스타일을 정의하는 열거형입니다.
    /// </summary>
    public enum GoFontStyle
    {
        /// <summary>보통 스타일</summary>
        Normal,
        /// <summary>기울임꼴</summary>
        Italic,
        /// <summary>굵은 글꼴</summary>
        Bold,
        /// <summary>굵은 기울임꼴</summary>
        BoldItalic
    }

    /// <summary>
    /// 자동 글꼴 크기 프리셋을 정의하는 열거형입니다.
    /// </summary>
    public enum GoAutoFontSize
    {
        /// <summary>자동 크기 사용 안 함</summary>
        NotUsed,
        /// <summary>매우 작은 글꼴 크기</summary>
        XXS,
        /// <summary>아주 작은 글꼴 크기</summary>
        XS,
        /// <summary>작은 글꼴 크기</summary>
        S,
        /// <summary>중간 글꼴 크기</summary>
        M,
        /// <summary>큰 글꼴 크기</summary>
        L,
        /// <summary>아주 큰 글꼴 크기</summary>
        XL,
        /// <summary>매우 큰 글꼴 크기</summary>
        XXL,
    }
}
