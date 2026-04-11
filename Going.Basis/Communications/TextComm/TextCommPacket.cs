using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Communications.TextComm
{
    /// <summary>
    /// STX(0x02)/ETX(0x03)/DLE(0x10) 기반 텍스트 통신 패킷의 인코딩 및 디코딩을 제공하는 유틸리티 클래스
    /// </summary>
    public class TextCommPacket
    {
        /// <summary>
        /// 슬레이브 ID, 명령 코드, 메시지를 사용하여 STX/ETX 형식의 통신 패킷을 생성합니다.
        /// DLE 이스케이프 처리와 체크섬을 포함합니다.
        /// </summary>
        /// <param name="enc">메시지 문자열을 바이트로 변환할 인코딩</param>
        /// <param name="id">슬레이브 ID</param>
        /// <param name="cmd">명령 코드</param>
        /// <param name="message">전송할 메시지 문자열</param>
        /// <returns>STX/ETX로 감싸고 DLE 이스케이프 처리된 패킷 바이트 배열</returns>
        public static byte[] MakePacket(Encoding enc, byte id, byte cmd, string message)
        {
            List<byte> ls = [id, cmd];
            if (!string.IsNullOrEmpty(message)) ls.AddRange(enc.GetBytes(message));
            ls.Add(Convert.ToByte(ls.Select(x => (int)x).Sum() & 0xFF));

            List<byte> ret = [0x02];
            foreach (var v in ls)
                if (v == 0x02 || v == 0x03 || v == 0x10) { ret.Add(0x10); ret.Add(Convert.ToByte(v + 0x10)); }
                else ret.Add(v);
            ret.Add(0x03);

            return ret.ToArray();
        }

        /// <summary>
        /// STX/ETX 형식의 수신 패킷을 파싱하여 DLE 이스케이프를 복원한 원본 데이터를 추출합니다.
        /// </summary>
        /// <param name="data">수신된 원시 바이트 배열</param>
        /// <param name="len">파싱할 데이터 길이. null이면 전체 배열을 사용합니다.</param>
        /// <returns>파싱된 페이로드 바이트 배열. 패킷이 불완전하거나 유효하지 않으면 null을 반환합니다.</returns>
        public static byte[]? ParsePacket(byte[] data, int? len = null)
        {
            var bDLE = false;
            var bValid = true;
            bool bComplete = false;

            List<byte> ls = [];
            for (int i = 0; i < (len ?? data.Length); i++)
            {
                byte d = data[i];
                byte v = d;
                if (bDLE)
                {
                    bDLE = false;
                    if (v >= 0x10) v -= 0x10;
                    else bValid = false;
                }

                switch (d)
                {
                    case 0x02: ls.Clear(); bValid = true; break;
                    case 0x03: bComplete = true; break;
                    case 0x10: bDLE = true; break;
                    default: ls.Add(v); break;
                }
                if (bComplete) break;
            }

            if (bComplete && bValid) return ls.ToArray();
            else return null;
        }
    }
}
