using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class TaskRunner: IDisposable
{
    // 单例实例
    private static TaskRunner instance;
    public static TaskRunner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TaskRunner();
            }
            return instance;
        }
    }
    private long activeTaskCount = 0;
    private long mainTaskCount = 0;
    private TaskRunner() { }
    private readonly SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(2);
    private CancellationTokenSource cts = new CancellationTokenSource();
    public void RunTask(Action taskAction)
    {
        Task.Run(async () =>
        {
            try
            {
                cts.Token.ThrowIfCancellationRequested();
                // 非阻塞等待获取信号量（可理解为“许可”）
                await concurrencySemaphore.WaitAsync();

                Interlocked.Increment(ref activeTaskCount);

                Stopwatch stopwatch = Stopwatch.StartNew();
                
                taskAction();

                stopwatch.Stop();
                // UnityEngine.Debug.Log("线程运行时间: " + stopwatch.ElapsedMilliseconds + " 毫秒");
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.Log("任务被取消");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            finally
            {
                Interlocked.Decrement(ref activeTaskCount);
                concurrencySemaphore.Release(); // 释放信号量
            }
        });
    }
    public long GetActiveTaskCount()
    {
        return Interlocked.Read(ref activeTaskCount);
    }

    public void TaskRunMain(Action func)
    {
        RunTask(() =>
        {
            Interlocked.Increment(ref mainTaskCount);
            DontDestroy.MainThreadSyncContext.Post(_ =>
            {
                func();
                Interlocked.Decrement(ref mainTaskCount);
            }, null);
        });
    }
    public long GetMainTaskCount()
    {
        return Interlocked.Read(ref mainTaskCount);
    }
    
    public void RequestStop()
    {
        cts.Cancel();
    }
    public void Dispose()
    {
        RequestStop();
        // 等待所有任务完成（可选）
        while (GetActiveTaskCount() > 0)
        {
            Thread.Sleep(10); // 注意：这里用了短暂的sleep作为示例，实际应用中应避免长时间阻塞
        }
        cts.Dispose();
    }
}