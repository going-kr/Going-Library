using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
            try
            {
                string interfaceName = GetInterfaceNameFromDescription(description);
                if (string.IsNullOrEmpty(interfaceName)) return false;
                return ExecuteNetshCommand($"interface ip set address \"{interfaceName}\" static {ip} {subnet} {gateway}");
            }
            catch
            {
                return false;
            }
        }

        private static bool SetLocalIP_Linux(string interfaceName, string ip, string subnet, string gateway)
        {
            try
            {
                int prefixLength = SubnetToCIDR(subnet);

                var hasNmcli = ExecuteLinuxCommand("which", "nmcli");
                if (hasNmcli)
                {
                    var cmd = $"connection modify \"{interfaceName}\" " +
                             $"ipv4.method manual " +
                             $"ipv4.addresses {ip}/{prefixLength} " +
                             $"ipv4.gateway {gateway}";

                    return ExecuteLinuxCommand("nmcli", cmd);
                }

                var result1 = ExecuteLinuxCommand("ip", $"addr add {ip}/{prefixLength} dev {interfaceName}");
                var result2 = ExecuteLinuxCommand("ip", $"route add default via {gateway} dev {interfaceName}");
                return result1 && result2;
            }
            catch
            {
                return false;
            }
        }

        private static string GetInterfaceNameFromDescription(string description)
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.Description == description);
            return nic?.Name ?? string.Empty;
        }

        private static int SubnetToCIDR(string subnet)
        {
            var parts = subnet.Split('.');
            if (parts.Length != 4) return 24;

            int cidr = 0;
            foreach (var part in parts)
            {
                if (byte.TryParse(part, out byte b))
                {
                    while (b > 0)
                    {
                        cidr += b & 1;
                        b >>= 1;
                    }
                }
            }
            return cidr;
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
            try
            {
                string interfaceName = GetInterfaceNameFromDescription(description);
                if (string.IsNullOrEmpty(interfaceName)) return false;

                var result1 = ExecuteNetshCommand($"interface ip set address \"{interfaceName}\" dhcp");
                var result2 = ExecuteNetshCommand($"interface ip set dns \"{interfaceName}\" dhcp");

                return result1 && result2;
            }
            catch
            {
                return false;
            }
        }

        private static bool SetDHCP_Linux(string interfaceName)
        {
            try
            {
                var result = ExecuteLinuxCommand("which", "nmcli");
                if (result) return ExecuteLinuxCommand("nmcli", $"connection modify \"{interfaceName}\" ipv4.method auto");
                return ExecuteLinuxCommand("dhclient", interfaceName);
            }
            catch
            {
                return false;
            }
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
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .Select(nic => nic.Description)
                .ToArray();
        }

        private static string[] GetNicDescriptions_Linux()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && !nic.Name.StartsWith("docker") && !nic.Name.StartsWith("br-"))
                .Select(nic => nic.Name)
                .ToArray();
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

        #region Command
        private static bool ExecuteNetshCommand(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas" // 관리자 권한
                };

                using var process = Process.Start(psi);
                if (process == null)
                    return false;

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool ExecuteLinuxCommand(string command, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(psi);
                if (process == null)
                    return false;

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
