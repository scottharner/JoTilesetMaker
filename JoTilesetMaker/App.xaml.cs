using System.Configuration;
using System.Data;
using System.Windows;

namespace JoTilesetMaker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(String.Format(Strings.UnhandledException, e.Exception.Message), Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }

}
