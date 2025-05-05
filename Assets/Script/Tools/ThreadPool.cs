using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class ThreadPool
{
    // 私有静态变量用于存储单例实例
    private static readonly ThreadPool instance = new ThreadPool(2); // 设定最大线程数为4
    // 线程池相关成员
    private readonly int maxThreads;
    private int activeThreads;
    private readonly BlockingCollection<Action> taskQueue = new BlockingCollection<Action>();
    public object[] locks = new object[23];
    // 私有构造函数，防止外部实例化
    public object GetLock(int x, int y = 0, int z = 0)
    {
        int k = x + y + z;
        k = (k % 23 + 23) % 23;
        return locks[k];
    }
    private ThreadPool(int maxThreads)
    {

        for (int i = 0; i < 23; ++i)
        {
            locks[i] = new object();
        }
        this.maxThreads = maxThreads;
        for (int i = 0; i < maxThreads; i++)
        {
            var thread = new Thread(ConsumeTasks);
            thread.Start();
        }
    }

    // 公共属性，提供对单例实例的访问
    public static ThreadPool Instance
    {
        get
        {
            return instance;
        }
    }

    // 方法：添加任务到队列
    public void QueueTask(Action task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        taskQueue.Add(task);
    }

    // 消费者方法：每个线程执行的任务
    private void ConsumeTasks()
    {
        while (!taskQueue.IsCompleted)
        {
            try
            {
                Interlocked.Increment(ref activeThreads);
                Action task = taskQueue.Take(); // 如果队列为空会阻塞当前线程
                task.Invoke();
            }
            catch (Exception e)
            {
                // Debug.Log($"Error executing task: {e.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref activeThreads);
            }
        }
    }

    // 完成添加任务，允许线程安全退出
    public void Shutdown()
    {
        taskQueue.CompleteAdding();
    }

    // 判断线程池是否空闲
    public bool IsIdle => activeThreads == 0 && taskQueue.Count == 0;

    public void MainThreadRun(SendOrPostCallback func)
    {
        Task.Run(() =>
        {
            DontDestroy.MainThreadSyncContext.Post(func, null);
        });
    }
}