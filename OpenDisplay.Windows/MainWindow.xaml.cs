using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenDisplay.Windows.Video;

namespace OpenDisplay.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private WriteableBitmap? _bitmap;
    public MainWindow()
    {
        InitializeComponent();
    }

    public void DisplayFrame(VideoFrame frame)
    {
        Dispatcher.Invoke(() =>
        {
            if (
                _bitmap == null ||
                _bitmap.PixelWidth != frame.Width ||
                _bitmap.PixelHeight != frame.Height
            )
            {
                _bitmap = new WriteableBitmap(
                    frame.Width,
                    frame.Height,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null
                );

                VideoImage.Source = _bitmap;

                StatusText.Visibility =
                    Visibility.Collapsed;
            }

            _bitmap.WritePixels(
                new Int32Rect(
                    0,
                    0,
                    frame.Width,
                    frame.Height
                ),
                frame.Data,
                frame.Stride,
                0
            );
        });
    }
}