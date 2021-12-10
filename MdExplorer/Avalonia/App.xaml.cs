using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SampleWebView.Avalonia {
    public class DesktopApp : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow();
                var test = desktop.MainWindow.DataContext as MainWindowViewModel;
                test.CurrentAddress = desktop.Args[0];
                
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
