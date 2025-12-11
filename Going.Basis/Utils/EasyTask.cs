using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Utils
{
    public class EasyTask
    {
        #region Properties
        public bool IsStart { get; private set; }
        #endregion

        #region Member Variable
        private Task? task;
        private CancellationTokenSource? cancel;
        #endregion

        #region Event
        public event Func<CancellationToken, Task>? Loop;
        public event Func<Task>? Stopped;
        public event Func<Task>? Started;
        #endregion

        #region Method
        public void Start()
        {
            if (task == null)
            {
                cancel = new CancellationTokenSource();
                task = Task.Run(async () => { try { await Process(cancel.Token); } catch { } });
            }
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (task == null)
            {
                cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                task = Task.Run(async () => { try { await Process(cancel.Token); } catch { } }, cancellationToken);
            }
        }

        public void Stop()
        {
            if (task != null)
            {
                var t = task;
                cancel?.Cancel();

                try { Task.WhenAny(t); }
                catch (OperationCanceledException) { }
            }
        }

        private async Task Process(CancellationToken cancellationToken)
        {
            IsStart = true;

            if (Started != null) await Started();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                    if (Loop != null) 
                        await Loop(cancellationToken);
            }
            catch (OperationCanceledException ex) {  }
            finally
            {
                if (Stopped != null) await Stopped();

                IsStart = false;
                task = null;
                cancel = null;
            }
        }
        #endregion
    }
}
