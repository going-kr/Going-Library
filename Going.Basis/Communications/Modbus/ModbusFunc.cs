using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.Modbus
{
    /// <summary>
    /// Modbus 프로토콜의 함수 코드를 정의하는 열거형입니다.
    /// </summary>
    public enum ModbusFunction
    {
        /// <summary>코일 상태 읽기 (FC1)</summary>
        BITREAD_F1 = 1,
        /// <summary>이산 입력 상태 읽기 (FC2)</summary>
        BITREAD_F2 = 2,
        /// <summary>보유 레지스터 읽기 (FC3)</summary>
        WORDREAD_F3 = 3,
        /// <summary>입력 레지스터 읽기 (FC4)</summary>
        WORDREAD_F4 = 4,
        /// <summary>단일 코일 쓰기 (FC5)</summary>
        BITWRITE_F5 = 5,
        /// <summary>단일 레지스터 쓰기 (FC6)</summary>
        WORDWRITE_F6 = 6,
        /// <summary>다중 코일 쓰기 (FC15)</summary>
        MULTIBITWRITE_F15 = 15,
        /// <summary>다중 레지스터 쓰기 (FC16)</summary>
        MULTIWORDWRITE_F16 = 16,
        /// <summary>워드 내 특정 비트 설정 (FC26, 확장 함수)</summary>
        WORDBITSET_F26 = 26,
    }
}
