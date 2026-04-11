using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Utils
{
    /// <summary>
    /// 비동기 반복 작업을 간편하게 관리하는 클래스.
    /// Loop 이벤트에 등록된 비동기 작업을 반복 실행하며, Start/Stop으로 제어한다.
    /// </summary>
    public class EasyTask
    {
        #region Properties
        /// <summary>작업 실행 중 여부</summary>
        public bool IsStart { get; private set; }
        #endregion

        #region Member Variable
        private Task? task;
        private CancellationTokenSource? cancel;
        #endregion

        #region Event
        /// <summary>반복 실행되는 비동기 작업 이벤트. CancellationToken을 통해 취소 요청을 수신한다.</summary>
        public event Func<CancellationToken, Task>? Loop;
        /// <summary>작업이 정지된 후 발생하는 이벤트</summary>
        public event Func<Task>? Stopped;
        /// <summary>작업이 시작된 직후 발생하는 이벤트</summary>
        public event Func<Task>? Started;
        #endregion

        #region Method
        /// <summary>비동기 반복 작업을 시작한다.</summary>
        public void Start()
        {
            if (task == null)
            {
                cancel = new CancellationTokenSource();
                task = Task.Run(async () => { try { await Process(cancel.Token); } catch { } });
            }
        }

        /// <summary>외부 CancellationToken과 연결하여 비동기 반복 작업을 시작한다.</summary>
        /// <param name="cancellationToken">외부 취소 토큰</param>
        public void Start(CancellationToken cancellationToken)
        {
            if (task == null)
            {
                cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                task = Task.Run(async () => { try { await Process(cancel.Token); } catch { } }, cancellationToken);
            }
        }

        /// <summary>실행 중인 반복 작업을 정지한다. 최대 3초간 작업 종료를 대기한다.</summary>
        public void Stop()
        {
            if (task != null)
            {
                var t = task;
                cancel?.Cancel();

                try { t.Wait(3000); }
                catch { }
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
            catch (OperationCanceledException) { }
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
