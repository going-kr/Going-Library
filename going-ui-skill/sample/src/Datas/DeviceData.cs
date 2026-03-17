using Going.Basis.Communications.Modbus.RTU;

namespace SenvasSample.Datas
{
    public class DeviceData
    {
        public int ID { get; set; }  // slaveId

        private MasterRTU _rtu;

        public DeviceData(MasterRTU rtu, int slaveId)
        {
            _rtu = rtu;
            ID = slaveId;
        }

        #region Helper
        /// <summary>
        /// Temperature encoding: bit15=sign, bit14~0=absolute value
        /// Result = (sign ? -1 : 1) * (abs) / 10.0
        /// </summary>
        private double DecodeTemp(int? raw)
        {
            if (raw == null) return 0;
            int v = raw.Value;
            bool negative = (v & 0x8000) != 0;
            int abs = v & 0x7FFF;
            return (negative ? -1.0 : 1.0) * abs / 10.0;
        }

        /// <summary>
        /// Encode temperature for writing: bit15=sign, bit14~0=abs*10
        /// </summary>
        private int EncodeTemp(double temp)
        {
            bool negative = temp < 0;
            int abs = (int)(Math.Abs(temp) * 10);
            return negative ? (abs | 0x8000) : abs;
        }
        #endregion

        #region Read — Status (0x7001~0x7004)
        public int BoardStatus => _rtu.GetWord(ID, "D0") ?? 0;        // 0x7001: 0=wait, 1=run
        public int AlarmStatus => _rtu.GetWord(ID, "D1") ?? 0;        // 0x7002: alarm bitmap
        public int InputStatus => _rtu.GetWord(ID, "D2") ?? 0;        // 0x7003: input status
        public int OutputStatus => _rtu.GetWord(ID, "D3") ?? 0;       // 0x7004: output status
        #endregion

        #region Read — Current (0x7005~0x7007)
        public double R_Current => (_rtu.GetWord(ID, "D4") ?? 0) / 10.0;   // 0x7005: 0.1A
        public double T_Current => (_rtu.GetWord(ID, "D5") ?? 0) / 10.0;   // 0x7006: 0.1A
        public double S_Current => (_rtu.GetWord(ID, "D6") ?? 0) / 10.0;   // 0x7007: 0.1A
        #endregion

        #region Read — Temperature (0x7008~0x700D)
        public double SCR_Temp => DecodeTemp(_rtu.GetWord(ID, "D7"));          // 0x7008
        public double IGBT_Temp => DecodeTemp(_rtu.GetWord(ID, "D8"));         // 0x7009
        public double Trans_Temp => DecodeTemp(_rtu.GetWord(ID, "D9"));        // 0x700A
        public double InBreaker_Temp => DecodeTemp(_rtu.GetWord(ID, "D10"));   // 0x700B
        public double OutBreaker_Temp => DecodeTemp(_rtu.GetWord(ID, "D11"));  // 0x700C
        public double EDLC_Temp => DecodeTemp(_rtu.GetWord(ID, "D12"));        // 0x700D
        #endregion

        #region Read — Smoke (0x7010~0x7012)
        public int Smoke1 => _rtu.GetWord(ID, "D15") ?? 0;    // 0x7010: %
        public int Smoke2 => _rtu.GetWord(ID, "D16") ?? 0;    // 0x7011: %
        public int Smoke3 => _rtu.GetWord(ID, "D17") ?? 0;    // 0x7012: %
        #endregion

        #region Read — Warning Set1 (0x7015~0x701F)
        public double SCR_TempSet1 => DecodeTemp(_rtu.GetWord(ID, "D20"));          // 0x7015
        public double IGBT_TempSet1 => DecodeTemp(_rtu.GetWord(ID, "D21"));         // 0x7016
        public double Trans_TempSet1 => DecodeTemp(_rtu.GetWord(ID, "D22"));        // 0x7017
        public double InBreaker_TempSet1 => DecodeTemp(_rtu.GetWord(ID, "D23"));    // 0x7018
        public double OutBreaker_TempSet1 => DecodeTemp(_rtu.GetWord(ID, "D24"));   // 0x7019
        public double EDLC_TempSet1 => DecodeTemp(_rtu.GetWord(ID, "D25"));         // 0x701A
        public int Smoke1Set1 => _rtu.GetWord(ID, "D28") ?? 0;                      // 0x701D
        #endregion

        #region Read — Warning Set2 (0x7021~0x702B)
        public double SCR_TempSet2 => DecodeTemp(_rtu.GetWord(ID, "D32"));          // 0x7021
        public double IGBT_TempSet2 => DecodeTemp(_rtu.GetWord(ID, "D33"));         // 0x7022
        public double Trans_TempSet2 => DecodeTemp(_rtu.GetWord(ID, "D34"));        // 0x7023
        public double InBreaker_TempSet2 => DecodeTemp(_rtu.GetWord(ID, "D35"));    // 0x7024
        public double OutBreaker_TempSet2 => DecodeTemp(_rtu.GetWord(ID, "D36"));   // 0x7025
        public double EDLC_TempSet2 => DecodeTemp(_rtu.GetWord(ID, "D37"));         // 0x7026
        public int Smoke1Set2 => _rtu.GetWord(ID, "D40") ?? 0;                      // 0x7029
        #endregion

        #region Read — Correction (0x702D~0x703A)
        public double CorrectionCurrent1 => DecodeTemp(_rtu.GetWord(ID, "D44"));    // 0x702D: R
        public double CorrectionCurrent2 => DecodeTemp(_rtu.GetWord(ID, "D45"));    // 0x702E: S
        public double CorrectionCurrent3 => DecodeTemp(_rtu.GetWord(ID, "D46"));    // 0x702F: T
        public double CorrectionTemp1 => DecodeTemp(_rtu.GetWord(ID, "D47"));       // 0x7030: SCR
        public double CorrectionTemp2 => DecodeTemp(_rtu.GetWord(ID, "D48"));       // 0x7031: IGBT
        public double CorrectionTemp3 => DecodeTemp(_rtu.GetWord(ID, "D49"));       // 0x7032: Trans
        public double CorrectionTemp4 => DecodeTemp(_rtu.GetWord(ID, "D50"));       // 0x7033: InBreaker
        public double CorrectionTemp5 => DecodeTemp(_rtu.GetWord(ID, "D51"));       // 0x7034: OutBreaker
        public double CorrectionTemp6 => DecodeTemp(_rtu.GetWord(ID, "D52"));       // 0x7035: EDLC
        public int CorrectionSmoke1 => _rtu.GetWord(ID, "D55") ?? 0;               // 0x7038
        #endregion

        #region Read — Input status bits
        public bool IsLeak => (InputStatus & 0x0001) != 0;
        public bool IsFuse => (InputStatus & 0x0002) != 0;
        public bool IsEMO => (InputStatus & 0x0004) != 0;
        public bool IsFan1 => (InputStatus & 0x0008) != 0;
        public bool IsFan2 => (InputStatus & 0x0010) != 0;
        #endregion

        #region Read — Output status bits
        public bool IsInverter => (OutputStatus & 0x0001) != 0;
        public bool IsInput => (OutputStatus & 0x0002) != 0;
        public bool IsOutput => (OutputStatus & 0x0004) != 0;
        #endregion

        #region Read — Alarm bits
        public bool AlarmLeak => (AlarmStatus & 0x0001) != 0;
        public bool AlarmIGBT => (AlarmStatus & 0x0002) != 0;
        public bool AlarmOverCurrent => (AlarmStatus & 0x0004) != 0;
        public bool AlarmFuse => (AlarmStatus & 0x0008) != 0;
        public bool AlarmSuperCapWarn => (AlarmStatus & 0x0010) != 0;
        public bool AlarmSuperCapAlarm => (AlarmStatus & 0x0020) != 0;
        public bool AlarmChargeEmpty => (AlarmStatus & 0x0040) != 0;
        public bool AlarmBackupEnd => (AlarmStatus & 0x0080) != 0;
        public bool AlarmOverVoltage => (AlarmStatus & 0x0100) != 0;
        public bool AlarmSensorOut => (AlarmStatus & 0x0200) != 0;
        public bool AlarmFanMotion => (AlarmStatus & 0x0400) != 0;
        public bool AlarmOverload => (AlarmStatus & 0x0800) != 0;
        #endregion

        #region Write — Correction Temps (FC6)
        public void SetCorrectionTemp1(double value) => _rtu.SetWord(ID, "D47", EncodeTemp(value));  // 0x7030
        public void SetCorrectionTemp2(double value) => _rtu.SetWord(ID, "D48", EncodeTemp(value));  // 0x7031
        public void SetCorrectionTemp3(double value) => _rtu.SetWord(ID, "D49", EncodeTemp(value));  // 0x7032
        public void SetCorrectionTemp4(double value) => _rtu.SetWord(ID, "D50", EncodeTemp(value));  // 0x7033
        public void SetCorrectionTemp5(double value) => _rtu.SetWord(ID, "D51", EncodeTemp(value));  // 0x7034
        public void SetCorrectionTemp6(double value) => _rtu.SetWord(ID, "D52", EncodeTemp(value));  // 0x7035
        #endregion

        #region Write — Correction Current (FC6)
        public void SetCorrectionCurrent1(double value) => _rtu.SetWord(ID, "D44", EncodeTemp(value));  // 0x702D
        public void SetCorrectionCurrent2(double value) => _rtu.SetWord(ID, "D45", EncodeTemp(value));  // 0x702E
        public void SetCorrectionCurrent3(double value) => _rtu.SetWord(ID, "D46", EncodeTemp(value));  // 0x702F
        #endregion

        #region Write — Correction Smoke (FC6)
        public void SetCorrectionSmoke1(int value) => _rtu.SetWord(ID, "D55", value);  // 0x7038
        #endregion

        #region Write — Board Command
        public void SetBoardCommand(int value) => _rtu.SetWord(ID, "D59", value);  // 0x703C
        #endregion

        #region Alarm text helper
        public string GetAlarmText()
        {
            var alarms = new List<string>();
            if (AlarmLeak) alarms.Add("Leak");
            if (AlarmIGBT) alarms.Add("IGBT ERR");
            if (AlarmOverCurrent) alarms.Add("Over Current");
            if (AlarmFuse) alarms.Add("Fuse Broken");
            if (AlarmSuperCapWarn) alarms.Add("SuperCap Warn");
            if (AlarmSuperCapAlarm) alarms.Add("SuperCap Alarm");
            if (AlarmChargeEmpty) alarms.Add("Charge Empty");
            if (AlarmBackupEnd) alarms.Add("Backup End ERR");
            if (AlarmOverVoltage) alarms.Add("Over Voltage");
            if (AlarmSensorOut) alarms.Add("Sensor Out");
            if (AlarmFanMotion) alarms.Add("Fan Motion");
            if (AlarmOverload) alarms.Add("Overload");
            return alarms.Count > 0 ? string.Join(", ", alarms) : "Normal";
        }
        #endregion
    }
}
