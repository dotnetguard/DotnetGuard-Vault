using System.Windows;
using DotnetGuard.KeyBox.App.Views;
using DotnetGuard.KeyBox.Data;

namespace DotnetGuard.KeyBox.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Window startupWindow = AppSettings.Exists() ? new LoginWindow() : new SetupWindow();
        startupWindow.Show();
    }
}
