using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 이미지 크기 조정 모드를 정의하는 열거형입니다.
    /// </summary>
    public enum GoImageScaleMode
    {
        /// <summary>원본 크기 그대로 표시</summary>
        Real,
        /// <summary>원본 크기를 유지하면서 중앙에 배치</summary>
        CenterImage,
        /// <summary>컨트롤 크기에 맞게 늘려서 표시</summary>
        Stretch,
        /// <summary>비율을 유지하면서 컨트롤에 맞게 확대/축소</summary>
        Zoom
    }
}
