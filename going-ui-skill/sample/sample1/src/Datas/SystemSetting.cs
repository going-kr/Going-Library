namespace TspMonitor.Datas
{
    public class SystemSetting
    {
        public string PortName { get; set; } = "COM1";
        public int Baudrate { get; set; } = 115200;
        public string Parity { get; set; } = "None";
        public int Timeout { get; set; } = 1000;
        public int SlaveID { get; set; } = 1;
        public string InstallDate { get; set; } = "";
        public int ModelNo { get; set; } = 0;
    }
}
