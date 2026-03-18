using TspMonitor;
using TspMonitor.Managers;

var dataMgr = Main.DataMgr = new DataManager();
var devMgr = Main.DevMgr = new DeviceManager();
var wnd = Main.Window = new MainWindow();

devMgr.Start();
wnd.Run();
devMgr.Stop();

wnd.Dispose();
