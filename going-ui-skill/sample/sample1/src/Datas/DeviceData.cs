using Going.Basis.Communications.Modbus.RTU;

namespace TspMonitor.Datas
{
    public class DeviceData
    {
        public int ID { get; set; }

        private MasterRTU _rtu;

        public DeviceData(MasterRTU rtu, int slaveId)
        {
            _rtu = rtu;
            ID = slaveId;
        }

        #region Helper
        private double ToSigned(int? raw)
        {
            if (raw == null) return 0;
            int v = raw.Value;
            if ((v & 0x8000) != 0)
                return -(v & 0x7FFF) / 10.0;
            return v / 10.0;
        }
        #endregion

        #region Data Read (FC3, 0x7001~)
        // 보드 상태
        public int RunState => _rtu.GetWord(ID, "D0") ?? 0;           // 0x7001
        public int AlarmState => _rtu.GetWord(ID, "D1") ?? 0;         // 0x7002
        public int SensorInput => _rtu.GetWord(ID, "D2") ?? 0;        // 0x7003
        public int MccbOutput => _rtu.GetWord(ID, "D3") ?? 0;         // 0x7004

        // 전류 (0.1A)
        public double RCurrent => (_rtu.GetWord(ID, "D4") ?? 0) / 10.0;   // 0x7005
        public double TCurrent => (_rtu.GetWord(ID, "D5") ?? 0) / 10.0;   // 0x7006
        public double SCurrent => (_rtu.GetWord(ID, "D6") ?? 0) / 10.0;   // 0x7007

        // 온도 (signed, 0.1℃)
        public double ScrTemp => ToSigned(_rtu.GetWord(ID, "D7"));         // 0x7008
        public double IgbtTemp => ToSigned(_rtu.GetWord(ID, "D8"));        // 0x7009
        public double TransTemp => ToSigned(_rtu.GetWord(ID, "D9"));       // 0x700A
        public double InBreakerTemp => ToSigned(_rtu.GetWord(ID, "D10"));  // 0x700B
        public double OutBreakerTemp => ToSigned(_rtu.GetWord(ID, "D11")); // 0x700C
        public double EdlcTemp => ToSigned(_rtu.GetWord(ID, "D12"));       // 0x700D

        // 연기 농도 (%)
        public double Smoke1 => (_rtu.GetWord(ID, "D15") ?? 0) / 1.0;     // 0x7010

        // DSP 상태
        public int DspBoardState => _rtu.GetWord(ID, "D58") ?? 0;         // 0x703B
        public int BoardStateChange => _rtu.GetWord(ID, "D59") ?? 0;      // 0x703C

        // 센서 비트 분해
        public bool SensorLeak => (SensorInput & 0x01) != 0;
        public bool SensorFuse => (SensorInput & 0x02) != 0;
        public bool SensorFan1 => (SensorInput & 0x08) != 0;
        public bool SensorFan2 => (SensorInput & 0x10) != 0;

        // MCCB 비트 분해
        public bool MccbInverter => (MccbOutput & 0x01) != 0;
        public bool MccbInput => (MccbOutput & 0x02) != 0;
        public bool MccbOutputBit => (MccbOutput & 0x04) != 0;
        #endregion

        #region Warning 설정값 (FC3, 0x7015~)
        // 1차 Warning
        public double ScrTempSet1 => ToSigned(_rtu.GetWord(ID, "D20"));        // 0x7015
        public double IgbtTempSet1 => ToSigned(_rtu.GetWord(ID, "D21"));       // 0x7016
        public double TransTempSet1 => ToSigned(_rtu.GetWord(ID, "D22"));      // 0x7017
        public double InBreakerTempSet1 => ToSigned(_rtu.GetWord(ID, "D23"));  // 0x7018
        public double OutBreakerTempSet1 => ToSigned(_rtu.GetWord(ID, "D24")); // 0x7019
        public double EdlcTempSet1 => ToSigned(_rtu.GetWord(ID, "D25"));       // 0x701A
        public double Smoke1Set1 => (_rtu.GetWord(ID, "D28") ?? 0) / 1.0;     // 0x701D

        // 2차 Warning
        public double ScrTempSet2 => ToSigned(_rtu.GetWord(ID, "D32"));        // 0x7021
        public double IgbtTempSet2 => ToSigned(_rtu.GetWord(ID, "D33"));       // 0x7022
        public double TransTempSet2 => ToSigned(_rtu.GetWord(ID, "D34"));      // 0x7023
        public double InBreakerTempSet2 => ToSigned(_rtu.GetWord(ID, "D35"));  // 0x7024
        public double OutBreakerTempSet2 => ToSigned(_rtu.GetWord(ID, "D36")); // 0x7025
        public double EdlcTempSet2 => ToSigned(_rtu.GetWord(ID, "D37"));       // 0x7026
        public double Smoke1Set2 => (_rtu.GetWord(ID, "D40") ?? 0) / 1.0;     // 0x7029

        // 보정 전류
        public double CorrectionR => ToSigned(_rtu.GetWord(ID, "D44"));        // 0x702D
        public double CorrectionS => ToSigned(_rtu.GetWord(ID, "D45"));        // 0x702E
        public double CorrectionT => ToSigned(_rtu.GetWord(ID, "D46"));        // 0x702F
        #endregion
    }
}
