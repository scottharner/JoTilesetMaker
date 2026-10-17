using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JoTilesetMaker
{
    public static class InfoDialog
    {
        public static async Task ShowAsync( string message, string title)
        {
            var content = new StackPanel
            {
                Margin = new System.Windows.Thickness(24),
                MinWidth = 280
            };

            content.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 20,
                Margin = new System.Windows.Thickness(0, 0, 0, 12)
            });

            content.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                Margin = new System.Windows.Thickness(0, 0, 0, 24)
            });

            var okButton = new Button
            {
                Content = Strings.Ok,
                MinWidth = 64,
                Padding = new System.Windows.Thickness(16, 8, 16, 8),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                IsDefault = true,
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = true
            };

            content.Children.Add(okButton);

            await DialogHost.Show(content, "InfoDialog");
        }

    }
}
