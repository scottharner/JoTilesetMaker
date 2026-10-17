using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JoTilesetMaker
{
    public static class UtilityDialog
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

            await DialogHost.Show(content, "UtilityDialog");
        }

        public static async Task<bool> ConfirmAsync(string message, string title)
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

            var buttonPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            var noButton = new Button
            {
                Content = Strings.No,
                Padding = new System.Windows.Thickness(16, 8, 16, 8),
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = false
            };

            var yesButton = new Button
            {
                Content = Strings.Yes,
                Padding = new System.Windows.Thickness(16, 8, 16, 8),
                Margin = new System.Windows.Thickness(0, 0, 8, 0),
                IsDefault = true,
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = true
            };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
            content.Children.Add(buttonPanel);

            var result = await DialogHost.Show(content, "UtilityDialog");

            return result is bool confirmed && confirmed;
        }
    }
}
