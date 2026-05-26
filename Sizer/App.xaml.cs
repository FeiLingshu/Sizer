using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Sizer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainPoint();
        }

        public readonly string GUID = "48B8FB63-615B-459B-B50B-97D6098CA02D";

        private EventWaitHandle ProgramStarted;

        private void MainPoint()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, GUID, out bool createnew);
            if (createnew)
            {
                ThreadPool.RegisterWaitForSingleObject(ProgramStarted, OnProgramStarted, null, -1, false);
            }
            else
            {
                ProgramStarted.Set();
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            window = new MainWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            window = null;
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private MainWindow window = null;

        private void OnProgramStarted(object state, bool timeout)
        {
            window?.Activate();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception_throw(e.ExceptionObject as Exception);
            if (!e.IsTerminating)
            {
                Environment.Exit(0);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception_throw(e.Exception);
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Exception_throw(e.Exception);
            e.SetObserved();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Exception_throw(Exception e)
        {
            string estd =
                $"[应用程序内部异常] [{DateTime.Now:yyyy/MM/dd HH:mm:ss}]"
                + $"\n\n根命名空间:{e.Source}"
                + $"\n方法体:{e.TargetSite}";
            if (e is AggregateException ae && ae.InnerException != null)
            {
                estd +=
                    $"\nInnerException:{e.InnerException.GetType().Name}"
                    + $"\n    根命名空间:{e.InnerException.Source}"
                    + $"\n    方法体:{e.InnerException.TargetSite}"
                    + $"\n    详细信息:\n        {e.InnerException.Message}"
                    + $"{(Regex.IsMatch(e.InnerException.Message, @"\n\z") ? string.Empty : "\n")}"
                    + $"    位置:";
                if (string.IsNullOrEmpty(e.InnerException.StackTrace)
                    || !e.InnerException.StackTrace.Contains("\n"))
                {
                    estd += $"\n        {e.InnerException.StackTrace.Trim()}";
                }
                else
                {
                    foreach (string st in e.InnerException.StackTrace.Split('\n'))
                    {
                        estd += $"\n        {st.Trim()}";
                    }
                }
                estd += "\n\nSizer - Exceptions Processed By FeiLingshu";
            }
            else
            {
                estd +=
                    $"\n详细信息:{e.GetType().Name}\n    {e.Message}"
                    + $"{(Regex.IsMatch(e.Message, @"\n\z") ? string.Empty : "\n")}"
                    + $"位置:";
                if (string.IsNullOrEmpty(e.StackTrace)
                    || !e.StackTrace.Contains("\n"))
                {
                    estd += $"\n    {e.StackTrace.Trim()}";
                }
                else
                {
                    foreach (string st in e.StackTrace.Split('\n'))
                    {
                        estd += $"\n    {st.Trim()}";
                    }
                }
                estd += "\n\nSizer - Exceptions Processed By FeiLingshu";
            }
            MessageBox.Show(
                estd,
                "Sizer",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.DefaultDesktopOnly);
            // 强制退出程序进程
            Environment.Exit(0);
        }
    }
}
