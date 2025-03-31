using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Going.Basis.Tools
{
    public class NetworkTool
    {
        #region ValidIPv4
        public static bool ValidIPv4(string address)
        {
            byte n;
            return Uri.CheckHostName(address) == UriHostNameType.IPv4 && address.Split('.').Where(x => byte.TryParse(x, out n)).Count() == 4;
        }
        #endregion
        #region ValidDomain
        public static bool ValidDomain(string address)
        {
            bool ret = false;
            try
            {
                if (Uri.CheckHostName(address) == UriHostNameType.IPv4)
                {
                    ret = ValidIPv4(address);
                }
                else
                {
                    var r = Dns.GetHostEntry(address);
                    ret = r.AddressList.Count() > 0;
                }
            }
            catch { ret = false; }
            return ret;
        }
        #endregion
        #region GetLocalIP
        public static string GetLocalIP()
        {
            string localIP = "UNKNOWN";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        #endregion
        #region SetLocalIP
        public static bool SetLocalIP(string description, string ip, string subnet, string gateway)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return SetLocalIP_Windows(description, ip, subnet, gateway);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return SetLocalIP_Linux(description, ip, subnet, gateway);
            else
                throw new NotSupportedException("지원되지 않는 OS입니다.");
        }

        private static bool SetLocalIP_Windows(string description, string ip, string subnet, string gateway)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();

                    foreach (ManagementObject managementObject in managementObjectCollection)
                    {
                        var _description = managementObject["Description"] as string;
                        if (string.Compare(_description, description, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            ManagementBaseObject setGatewaysManagementBaseObject = managementObject.GetMethodParameters("SetGateways");
                            setGatewaysManagementBaseObject["DefaultIPGateway"] = new string[] { gateway };
                            setGatewaysManagementBaseObject["GatewayCostMetric"] = new int[] { 1 };

                            ManagementBaseObject enableStaticManagementBaseObject = managementObject.GetMethodParameters("EnableStatic");
                            enableStaticManagementBaseObject["IPAddress"] = new string[] { ip };
                            enableStaticManagementBaseObject["SubnetMask"] = new string[] { subnet };

                            managementObject.InvokeMethod("EnableStatic", enableStaticManagementBaseObject, null);
                            managementObject.InvokeMethod("SetGateways", setGatewaysManagementBaseObject, null);
                            return true;
                        }
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        private static bool SetLocalIP_Linux(string interfaceName, string ip, string subnet, string gateway)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    RunBashCommand($"sudo ip addr flush dev {interfaceName}");
                    RunBashCommand($"sudo ip addr add {ip}/{subnet} dev {interfaceName}");
                    RunBashCommand($"sudo ip route add default via {gateway} dev {interfaceName}");

                    return true;
                }
                catch (Exception ex) { return false; }
            }
            return false;
        }
        #endregion
        #region SetDHCP
        public static bool SetDHCP(string description)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SetDHCP_Windows(description);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return SetDHCP_Linux(description);
            }
            else
            {
                throw new NotSupportedException("지원되지 않는 OS입니다.");
            }
        }

        private static bool SetDHCP_Windows(string description)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();

                    foreach (ManagementObject managementObject in managementObjectCollection)
                    {
                        string _description = managementObject["Description"] as string;
                        bool dhcpEnabled = (bool)(managementObject["DHCPEnabled"] ?? false);

                        if (string.Compare(_description, description, StringComparison.InvariantCultureIgnoreCase) == 0 && !dhcpEnabled)
                        {
                            ManagementBaseObject enableDHCPManagementBaseObject = managementObject.InvokeMethod("EnableDHCP", null, null);
                            return enableDHCPManagementBaseObject != null;
                        }
                    }
                }
                catch (Exception ex) { }
            }
            return false;
        }

        private static bool SetDHCP_Linux(string interfaceName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    // 기존 설정 제거 (정적 IP를 제거하고 DHCP 활성화)
                    RunBashCommand($"sudo dhclient -r {interfaceName}");
                    RunBashCommand($"sudo dhclient {interfaceName}");
                    return true;
                }
                catch (Exception ex) { }
            }
            return false;
        }
        #endregion
        #region GetNicDescriptions
        public static string[] GetNicDescriptions()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetNicDescriptions_Windows();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetNicDescriptions_Linux();
            else
                throw new NotSupportedException("지원되지 않는 OS입니다.");
        }

        private static string[] GetNicDescriptions_Windows()
        {
            List<string> ls = [];

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();

                    foreach (ManagementObject managementObject in managementObjectCollection)
                    {
                        var desc = managementObject["Description"] as string;
                        if (!string.IsNullOrEmpty(desc))
                        {
                            ls.Add(desc);
                        }
                    }
                }
                catch (Exception ex) { }
            }
            return ls.ToArray();
        }

        private static string[] GetNicDescriptions_Linux()
        {
            List<string> interfaces = [];

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    string output = RunBashCommand("ip -o link show | awk -F': ' '{print $2}'");
                    if (!string.IsNullOrEmpty(output))
                    {
                        interfaces.AddRange(output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                catch (Exception ex) { }
            }

            return interfaces.ToArray();
        }

        #endregion
        #region IsSocketConnected
        public static bool IsSocketConnected(Socket s, int PollTime = 100)
        {
            try
            {
                if (s != null)
                {
                    return !((s.Poll(PollTime, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
                }
                else return false;
            }
            catch (SocketException)
            {
                return false;
            }
        }
        #endregion

        #region RunBashCommand
        private static string RunBashCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception("명령 실행 오류: " + error);
                }

                return output.Trim();
            }
        }
        #endregion
    }
}
