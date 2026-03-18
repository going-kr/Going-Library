using System.Text.Json;
using TspMonitor.Datas;

namespace TspMonitor.Managers
{
    public class DataManager
    {
        const string PATH = "setting.json";
        public SystemSetting Setting { get; set; }

        public DataManager()
        {
            Setting = new SystemSetting();
            if (File.Exists(PATH))
            {
                var obj = JsonSerializer.Deserialize<SystemSetting>(File.ReadAllText(PATH));
                if (obj != null) Setting = obj;
            }
        }

        public void Save()
        {
            File.WriteAllText(PATH, JsonSerializer.Serialize(Setting));
        }
    }
}
