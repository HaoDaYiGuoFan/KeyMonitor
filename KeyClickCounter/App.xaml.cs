using System.Threading;
using System.Windows;

namespace KeyClickCounter;

public partial class App : System.Windows.Application
{
    private static EventWaitHandle? _activateEvent;
    private static Mutex? _instanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 单实例守护：第二个实例只唤醒已有窗口，自身立即退出，避免双实例互相覆盖数据文件
        _instanceMutex = new Mutex(true, @"Local\KeyClickCounter.SingleInstance", out bool isNew);   // 静态字段持有：进程存活期全程占住，退出由系统回收
        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\KeyClickCounter.Activate");
        if (!isNew)
        {
            _activateEvent.Set();
            Environment.Exit(0);
        }

        // 已运行实例监听唤醒信号：还原托盘里的主窗口
        var ui = Dispatcher;
        var listener = new Thread(() =>
        {
            while (_activateEvent.WaitOne())
            {
                ui.InvokeAsync(() => (Current.MainWindow as MainWindow)?.ShowFromTray());
            }
        })
        { IsBackground = true };
        listener.Start();

        base.OnStartup(e);
    }
}