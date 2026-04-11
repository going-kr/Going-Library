using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Utils
{
    /// <summary>
    /// 외부 프로그램의 실행 및 종료를 관리하는 클래스.
    /// 이미 실행 중인 동일 프로세스가 있으면 해당 프로세스에 연결하고, 없으면 새로 시작한다.
    /// </summary>
    public class ExternalProgram
    {
        /// <summary>현재 관리 중인 프로세스 인스턴스</summary>
        public Process? Process { get; private set; } = null;

        /// <summary>외부 프로그램의 실행 파일 경로</summary>
        public string Path { get; private set; }
        /// <summary>확장자를 제외한 프로세스 이름</summary>
        public string ProcessName { get; private set; }

        /// <summary>외부 프로그램 관리 인스턴스를 생성한다.</summary>
        /// <param name="PATH">실행 파일 경로</param>
        public ExternalProgram(string PATH)
        {
            this.Path = PATH;
            this.ProcessName = System.IO.Path.GetFileNameWithoutExtension(PATH);
        }

        /// <summary>
        /// 프로그램을 시작한다. 동일 이름의 프로세스가 이미 실행 중이면 해당 프로세스에 연결한다.
        /// 표준 입출력 리다이렉션이 활성화된 상태로 실행된다.
        /// </summary>
        public void Start()
        {
            var pss = Process.GetProcesses();

            var tps = pss.Where(x => string.Equals(x.ProcessName, ProcessName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (tps != null) Process = tps;
            else
            {
                Process = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Path;
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardInput = true;
                info.RedirectStandardError = true;

                Process.StartInfo = info;
                Process.Start();
            }
        }

        /// <summary>프로세스를 강제 종료하고 리소스를 해제한다.</summary>
        public void Stop()
        {
            if (Process != null)
            {
                if (!Process.HasExited) Process.Kill();
                Process.Close();
                Process.Dispose();
                Process = null;
            }
        }
    }
}
