using System.Windows;

namespace Mine.Wpf.Controls.Gallery;

public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
            System.IO.File.WriteAllText(@"G:\Documents\Rider\crash.txt", ex.ExceptionObject.ToString());
        DispatcherUnhandledException += (_, ex) =>
        {
            System.IO.File.WriteAllText(@"G:\Documents\Rider\crash.txt", ex.Exception.ToString());
            ex.Handled = true;
            Shutdown(1);
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
    }
}
