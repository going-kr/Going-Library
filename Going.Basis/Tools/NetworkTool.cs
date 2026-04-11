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
    /// <summary>
    /// 네트워크 관련 유틸리티 클래스.
    /// IP 주소 검증, 로컬 IP 조회/설정, DHCP 설정, NIC 정보 조회, 소켓 연결 상태 확인 기능을 제공한다.
    /// </summary>
    public class NetworkTool
    {
        #region ValidIPv4
        /// <summary>문자열이 유효한 IPv4 주소인지 검증한다.</summary>
        /// <param name="address">검증할 IP 주소 문자열</param>
        /// <returns>유효한 IPv4 주소이면 true</returns>
        public static bool ValidIPv4(string address)
        {
            byte n;
            return Uri.CheckHostName(address) == UriHostNameType.IPv4 && address.Split('.').Where(x => byte.TryParse(x, out n)).Count() == 4;
        }
        #endregion
        #region ValidDomain
        /// <summary>문자열이 유효한 도메인 또는 IPv4 주소인지 검증한다. DNS 조회를 수행한다.</summary>
        /// <param name="address">검증할 도메인 또는 IP 주소 문자열</param>
        /// <returns>유효한 도메인/IP이면 true</returns>
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
        /// <summary>로컬 머신의 IPv4 주소를 반환한다.</summary>
        /// <returns>로컬 IPv4 주소 문자열. 찾지 못하면 "UNKNOWN"</returns>
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
        /// <summary>네트워크 인터페이스에 고정 IP를 설정한다. Windows와 Linux를 지원한다.</summary>
        /// <param name="description">네트워크 인터페이스 설명 (Windows) 또는 인터페이스 이름 (Linux)</param>
        /// <param name="ip">설정할 IP 주소</param>
        /// <param name="subnet">서브넷 마스크 (예: "255.255.255.0")</param>
        /// <param name="gateway">게이트웨이 주소</param>
        /// <returns>설정 성공 시 true</returns>
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
        /// <summary>네트워크 인터페이스를 DHCP 모드로 전환한다. Windows와 Linux를 지원한다.</summary>
        /// <param name="description">네트워크 인터페이스 설명 (Windows) 또는 인터페이스 이름 (Linux)</param>
        /// <returns>설정 성공 시 true</returns>
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
        /// <summary>
        /// 시스템의 네트워크 인터페이스 목록을 반환한다.
        /// Windows에서는 NIC Description, Linux에서는 인터페이스 이름을 반환한다.
        /// 루프백, 터널, Docker 브릿지 인터페이스는 제외된다.
        /// </summary>
        /// <returns>네트워크 인터페이스 설명/이름 배열</returns>
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
        /// <summary>소켓이 현재 연결 상태인지 확인한다.</summary>
        /// <param name="s">확인할 소켓</param>
        /// <param name="PollTimeMicros">폴링 대기 시간 (마이크로초, 1μs=0.001ms). 기본값: 100</param>
        /// <returns>연결 중이면 true</returns>
        public static bool IsSocketConnected(Socket s, int PollTimeMicros = 100)
        {
            try
            {
                if (s != null)
                {
                    return !((s.Poll(PollTimeMicros, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
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
