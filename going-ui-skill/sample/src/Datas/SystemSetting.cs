namespace SenvasSample.Datas
{
    public class SystemSetting
    {
        public string PortName { get; set; } = "COM1";
        public int Baudrate { get; set; } = 115200;
        public int SlaveId { get; set; } = 1;
        public int Timeout { get; set; } = 1000;
        public string InstallDate { get; set; } = "";       // YYMMDD
        public int ModelNumber { get; set; } = 0;
    }
}
