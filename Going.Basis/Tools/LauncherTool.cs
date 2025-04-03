using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Tools
{
    internal class LauncherTool
    {
        #region Start
        public static void Start(string ProgramName, Action run, Action faild, bool UseDuplicate, int Delay = 0)
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
                    faild?.Invoke();
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
