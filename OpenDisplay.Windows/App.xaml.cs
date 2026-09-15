using System.Configuration;
using System.Data;
using System.Windows;
using OpenDisplay.Windows.Protocol;

namespace OpenDisplay.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private OpenDisplayServer? _server;
    private CancellationTokenSource? _cancellationTokenSource;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _cancellationTokenSource = new CancellationTokenSource();

        var display = new DisplayConfiguration();

        var identity = new ReceiverIdentity();

        _server = new OpenDisplayServer(9000, display, identity);

        _ = RunServerAsync(_cancellationTokenSource.Token);
    }

    private async Task RunServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _server!.StartAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Receiver error:\n\n{ex.Message}",
                    "OpenDisplay",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _cancellationTokenSource?.Cancel();

        base.OnExit(e);
    }
}

