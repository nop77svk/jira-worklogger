namespace jwl.gui.desktop;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;

public class MainWindowViewModel
{
    public ICommand MenuFileExitCommand { get; }

    public MainWindowViewModel()
    {
        MenuFileExitCommand = new RelayCommand(ExitApplication);
    }

    private void ExitApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow?.Close();
        }
    }
}
