using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Tools
{
    /// <summary>
    /// 프로그램 실행 런처 유틸리티. Mutex를 이용한 중복 실행 방지 기능을 제공한다.
    /// </summary>
    internal class LauncherTool
    {
        #region Start
        /// <summary>프로그램을 실행한다. 중복 실행 방지 옵션을 지원한다.</summary>
        /// <param name="ProgramName">프로그램 이름 (Mutex 이름으로 사용)</param>
        /// <param name="run">실행할 액션</param>
        /// <param name="failed">중복 실행 시 호출되는 액션</param>
        /// <param name="UseDuplicate">중복 실행 방지 사용 여부</param>
        /// <param name="Delay">실행 전 지연 시간 (밀리초). 기본값: 0</param>
        public static void Start(string ProgramName, Action run, Action failed, bool UseDuplicate, int Delay = 0)
        {
            if (UseDuplicate)
            {
                #region Duplication
                bool bnew;
                Mutex mutex = new Mutex(true, ProgramName, out bnew);
                if (bnew)
                {
                    if (Delay != 0) System.Threading.Thread.Sleep(Delay);
                    run?.Invoke();
                    mutex.ReleaseMutex();
                }
                else
                {
                    failed?.Invoke();
                }
                #endregion
            }
            else
            {
                #region Run
                if (Delay != 0) System.Threading.Thread.Sleep(Delay);
                run?.Invoke();
                #endregion
            }
        }
        #endregion
    }
}
