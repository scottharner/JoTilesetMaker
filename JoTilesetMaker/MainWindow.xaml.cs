using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace JoTilesetMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string LabelDefaultText = "N/A";
        const int MaxFileNameLength = 8;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddTileButton_Click(object sender, RoutedEventArgs e)
        {
            bool tileAdded = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Tile files (*.tga)|*.tga";
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    if (!TilesListBox.Items.Contains(filePath))
                    {
                        TilesListBox.Items.Add(filePath);
                        tileAdded = true;
                    }   
                }
            }

            if (tileAdded && TilesListBox.Items.Count > 0)
            { 
                var firstFilePath = TilesListBox.Items[0].ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(firstFilePath))
                {
                    var tileSize = GetTileDimensions(firstFilePath);
                    TileWidthLabel.Content = tileSize.Width;
                    TileHeightLabel.Content = tileSize.Height;

                    UpdateTilesetConfigText(tileSize);
                }
            }
        }

        private void UpdateTilesetConfigText(SixLabors.ImageSharp.Size tileSize)
        {
            if (TilesListBox.Items.Count == 0)
            {
                TilesetConfigTextBox.Text = LabelDefaultText;
                return;
            }

            var configString = "jo_tile game_tileset[] = {\n";

            for (var i = 0; i < TilesListBox.Items.Count; i++)
            {
                var filePath = TilesListBox.Items[i].ToString();
                if (i == TilesListBox.Items.Count - 1)
                {
                    configString += $"    {{ {i * tileSize.Width}, 0, {tileSize.Width}, {tileSize.Height} }}\n";
                }
                else
                {
                    configString += $"    {{ {i * tileSize.Width}, 0, {tileSize.Width}, {tileSize.Height} }},\n";
                }
            }

            configString += "};\n";

            TilesetConfigTextBox.Text = configString;
        }

        private SixLabors.ImageSharp.Size GetTileDimensions(string filePath)
        {
            using (var image = SixLabors.ImageSharp.Image.Load(filePath))
            {
                return new SixLabors.ImageSharp.Size(image.Width, image.Height);
            }
        }

        private void RemoveTileButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFilePaths = TilesListBox.SelectedItems.Cast<string>().ToList();
            foreach (var selectedFilePath in selectedFilePaths)
            {
                TilesListBox.Items.Remove(selectedFilePath);
            }

            if (TilesListBox.Items.Count == 0)
            {
                TileWidthLabel.Content = LabelDefaultText;
                TileHeightLabel.Content = LabelDefaultText;
                UpdateTilesetConfigText(new SixLabors.ImageSharp.Size(0, 0));
            }
            else
            { 
                UpdateTilesetConfigText(GetTileDimensions(TilesListBox.Items[0].ToString() ?? string.Empty));
            }
        }

        private void SetTilesetFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            // todo - if file path was already set, we should pre populate it
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".tga";
            saveFileDialog.Filter = "Tile files (*.tga)|*.tga";
            if (saveFileDialog.ShowDialog() == true)
            {
                TilesetFilePathTextBlock.Text = saveFileDialog.FileName;
                FileInfo saveFileInfo = new FileInfo(saveFileDialog.FileName);
                var extensionlessFileName = saveFileInfo.Name.Replace(saveFileInfo.Extension, "");
                var fileNameLengthError = $"The tileset file name exceeds {MaxFileNameLength} characters.";
                if (extensionlessFileName.Length > MaxFileNameLength)
                {
                    if (!ErrorsListBox.Items.Contains(fileNameLengthError))
                        ErrorsListBox.Items.Add(fileNameLengthError);
                }
                else
                {
                    if (ErrorsListBox.Items.Contains(fileNameLengthError))
                        ErrorsListBox.Items.Remove(fileNameLengthError);
                }
            }

        }

        private void CopyConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TilesetConfigTextBox.Text);

            MessageBox.Show("Tileset configuration copied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BuildTilesetButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(TilesetFilePathTextBlock.Text))
            {
                var result = MessageBox.Show(
                    "The file already exists. Are you sure you want to overwrite it with a new tileset file?",
                    "Overwrite Tileset File",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.No)
                {
                    return; // User chose not to overwrite the file, so we exit the method early
                }

            }

            var backgroundColor = Color.Transparent.ToPixel<Rgba32>();
            if (BackgroundColorPicker.SelectedColor != null)
                backgroundColor = new Rgba32(BackgroundColorPicker.SelectedColor.Value.R, BackgroundColorPicker.SelectedColor.Value.G, BackgroundColorPicker.SelectedColor.Value.B, BackgroundColorPicker.SelectedColor.Value.A);

            using Image<Rgba32> targetImage = new Image<Rgba32>(Convert.ToInt32(TileWidthLabel.Content) * TilesListBox.Items.Count, Convert.ToInt32(TileHeightLabel.Content), backgroundColor);
            
            int currentX = 0;
            foreach (var filePath in TilesListBox.Items)
            {
                using var sourceImage = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath.ToString() ?? string.Empty);
                targetImage.Mutate(ctx => ctx.DrawImage(sourceImage, new SixLabors.ImageSharp.Point(currentX, 0), 1f));
                currentX += sourceImage.Width;
            }

            var bitsPerPixelTag = Convert.ToByte(((ComboBoxItem)BitDepthComboBox.SelectedItem).Tag);
            TgaBitsPerPixel bitsPerPixel = bitsPerPixelTag switch
            {
                8 => TgaBitsPerPixel.Bit8,
                16 => TgaBitsPerPixel.Bit16,
                24 => TgaBitsPerPixel.Bit24,
                32 => TgaBitsPerPixel.Bit32,
                _ => throw new InvalidOperationException("Invalid bits per pixel value.")
            };

            targetImage.Save(TilesetFilePathTextBlock.Text, new TgaEncoder
            {
                BitsPerPixel = bitsPerPixel,
                Compression = TgaCompression.None
            });

            MessageBox.Show("Tileset built successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetTilesetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all tile data?",
                "Reset Tile Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                ResetTileData();
            }   
        }

        private void ResetTileData()
        {
            // clear tiles
            TilesListBox.Items.Clear();

            // reset tile properties
            TileWidthLabel.Content = LabelDefaultText;
            TileHeightLabel.Content = LabelDefaultText;

            // clear tileset file path
            TilesetFilePathTextBlock.Text = LabelDefaultText;

            // clear tileset config
            TilesetConfigTextBox.Text = LabelDefaultText;
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (TilesListBox.SelectedItems.Count == 1 && TilesListBox.SelectedIndex > 0)
            {
                var selectedIndex = TilesListBox.SelectedIndex;
                var selectedItem = TilesListBox.SelectedItem;
                TilesListBox.Items.RemoveAt(selectedIndex);
                TilesListBox.Items.Insert(selectedIndex - 1, selectedItem);
                TilesListBox.SelectedIndex = selectedIndex - 1;
            }
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (TilesListBox.SelectedItems.Count == 1 && TilesListBox.SelectedIndex < (TilesListBox.Items.Count - 1))
            {
                var selectedIndex = TilesListBox.SelectedIndex;
                var selectedItem = TilesListBox.SelectedItem;
                TilesListBox.Items.RemoveAt(selectedIndex);
                TilesListBox.Items.Insert(selectedIndex + 1, selectedItem);
                TilesListBox.SelectedIndex = selectedIndex + 1;
            }
        }
    }
}