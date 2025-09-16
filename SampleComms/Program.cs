//#define Modbus
//#define TextComm
//#define Mqtt
#define Test

using Going.Basis.Datas;
using uPLibrary.Networking.M2Mqtt;
using System.Numerics;




#if Modbus
using Going.Basis.Communications.Modbus.RTU;
using Going.Basis.Communications.Modbus.TCP;

BitMemories P = new BitMemories("P", 512);
BitMemories M = new BitMemories("M", 4096);
WordMemories T = new WordMemories("T", 128);
WordMemories C = new WordMemories("C", 128);
WordMemories D = new WordMemories("D", 1024);

var slave = new SlaveRTU { Port = "COM5", Baudrate = 115200, Slave = 1 };
//var slave = new SlaveTCP {  Slave = 1 };
slave.BitAreas.Add(0x0000, P);
slave.BitAreas.Add(0x1000, M);
slave.WordAreas.Add(0x5000, T);
slave.WordAreas.Add(0x6000, C);
slave.WordAreas.Add(0x7000, D);
slave.Start();

var master = new MasterRTU { Port = "COM6", Baudrate = 115200 };
//var master = new MasterTCP { RemoteIP = "127.0.0.1" };
master.BitAreas.Add(0x0000, "P");
master.BitAreas.Add(0x1000, "M");
master.WordAreas.Add(0x5000, "T");
master.WordAreas.Add(0x6000, "C");
master.WordAreas.Add(0x7000, "D");
master.MonitorWord_F3(1, 0x7000, 12);
master.Start();

master.SetWord(1, "D10", 100);

int sec = 0;
while (true)
{
    var now = DateTime.Now;
    D[0] = (ushort)now.Year;
    D[1] = (ushort)now.Month;
    D[2] = (ushort)now.Day;
    D[3] = (ushort)now.Hour;
    D[4] = (ushort)now.Minute;
    D[5] = (ushort)now.Second;
    D[6] = (ushort)now.Millisecond;

    if(now.Second != sec)
    {
        sec = now.Second;
        for (int i = 0; i <= 10; i++) Console.Write((master.GetWord(1, $"D{i}") ?? 0).ToString().PadLeft(5));
        Console.WriteLine();
    }

    Thread.Sleep(1);
}
#elif TextComm
using Going.Basis.Communications.TextComm.RTU;
using Going.Basis.Communications.TextComm.TCP;

//var slave = new TextCommRTUSlave { Port = "COM5", Baudrate = 115200 };
var slave = new TextCommTCPSlave { };
slave.MessageRequest += (o, s) => { if (s.Slave == 1 && s.Command == 1) s.ResponseMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); };
slave.Start();

//var master = new TextCommRTUMaster { Port = "COM6", Baudrate = 115200 };
var master = new TextCommTCPMaster { RemoteIP = "127.0.0.1" };
master.AutoSend(1, 1, 1, "");
master.MessageReceived += (o, s) => { Console.WriteLine(s.Message); };
master.Start();

while (true)
{
    Thread.Sleep(1000);
}
#elif Mqtt
using Going.Basis.Communications.Mqtt;
using System.Text;

var mq = new MQClient();
mq.Subscribe("/time");
mq.Start();

mq.Received += (o, s) => Console.WriteLine($"{s.Topic} : {Encoding.UTF8.GetString(s.Datas)}");

while (true)
{
    mq.Publish("/time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
    Thread.Sleep(1000);
}
#elif Test
using Going.Basis.Communications.Modbus.RTU;

BitMemories P = new BitMemories("P", 512);
BitMemories M = new BitMemories("M", 4096);
WordMemories T = new WordMemories("T", 512);
WordMemories C = new WordMemories("C", 512);
WordMemories D = new WordMemories("D", 4096);

var slave = new SlaveRTU { Port = "COM6", Baudrate = 115200, Slave = 1 };

slave.BitAreas.Add(0x0000, P);
slave.BitAreas.Add(0x1000, M);
slave.WordAreas.Add(0x5000, T);
slave.WordAreas.Add(0x6000, C);
slave.WordAreas.Add(0x7000, D);
slave.Start();

while(true)
{

}
#endif

