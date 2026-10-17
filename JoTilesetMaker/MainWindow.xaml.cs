using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JoTilesetMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MaxFileNameLength = 8;
        const string DefaultBitDepth = "32"; // Default to 32 bits per pixel    

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddTileButton_Click(object sender, RoutedEventArgs e)
        {
            bool tileAdded = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = Strings.TgaFilter;
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

                    UpdateTilesetCodeText(tileSize);
                    CheckTileSizeMismatch(tileSize.Width, tileSize.Height);
                }

                UpdateBuildTilesetButtonEnabledState();
                UpdateRemoveTileButtonEnabledState();
                UpdateMoveDownButtonEnabledState();
                UpdateMoveUpButtonEnabledState();  
            }
        }

        private void CheckTileSizeMismatch(int width, int height)
        {
            bool mismatchFound = false;
            foreach (var tile in TilesListBox.Items)
            {
                var tileSize = GetTileDimensions(tile.ToString() ?? string.Empty);
                if (tileSize.Width != width || tileSize.Height != height)
                {
                    mismatchFound = true;
                    break;
                }
            }

            if (mismatchFound)
            {
                if (!ErrorsListBox.Items.Contains(Strings.TileSizeMismatch))
                {
                    ErrorsListBox.Items.Add(Strings.TileSizeMismatch);
                    UpdateBuildTilesetButtonEnabledState();
                }
            }
            else
            {
                if (ErrorsListBox.Items.Contains(Strings.TileSizeMismatch))
                {
                    ErrorsListBox.Items.Remove(Strings.TileSizeMismatch);
                    UpdateBuildTilesetButtonEnabledState();
                }
            }   
        }

        private void UpdateBuildTilesetButtonEnabledState()
        {
            var isEnabled = true;
            if (ErrorsListBox.Items.Count > 0 || 
                TilesListBox.Items.Count == 0 || 
                TilesetFilePathTextBlock.Text == Strings.NotApplicable)
                isEnabled = false;

            BuildTilesetButton.IsEnabled = isEnabled;
        }

        private void UpdateCopyDefinitionButtonEnabledState()
        {
            CopyDefinitionButton.IsEnabled = (TilesetDefinitionTextBox.Text != Strings.NotApplicable);
        }

        private void UpdateCopyInitializationButtonEnabledState()
        {
            CopyInitializationButton.IsEnabled = (TilesetInitializationTextBox.Text != Strings.NotApplicable);
        }

        private void UpdateRemoveTileButtonEnabledState()
        {
            RemoveTileButton.IsEnabled = TilesListBox.SelectedItems.Count > 0;
        }

        private void UpdateMoveUpButtonEnabledState()
        {
            MoveUpButton.IsEnabled = TilesListBox.SelectedItems.Count == 1 && TilesListBox.SelectedIndex > 0;
        }

        private void UpdateMoveDownButtonEnabledState()
        {
            MoveDownButton.IsEnabled = TilesListBox.SelectedItems.Count == 1 && TilesListBox.SelectedIndex < (TilesListBox.Items.Count - 1);
        }

        private void UpdateTilesetCodeText(SixLabors.ImageSharp.Size tileSize)
        {
            if (TilesListBox.Items.Count == 0)
            {
                TilesetDefinitionTextBox.Text = Strings.NotApplicable;
                TilesetInitializationTextBox.Text = Strings.NotApplicable;
                UpdateCopyDefinitionButtonEnabledState();
                UpdateCopyInitializationButtonEnabledState();
                return;
            }

            UpdateTilesetDefinitionText(tileSize);
            UpdateTilesetInitializationText();
        }

        private void UpdateTilesetInitializationText()
        {
            if (TilesetInitializationTextBox != null)
            {
                var initializationString = String.Empty;

                for (var i = 0; i < TilesListBox.Items.Count; i++)
                {
                    if (i == 0)
                    {
                        var colorString = BackgroundColorPicker.SelectedColor == System.Windows.Media.Colors.Transparent || BackgroundColorPicker.SelectedColor == null ? "JO_COLOR_Transparent" : $"JO_COLOR_RGB({BackgroundColorPicker.SelectedColor.Value.R},{BackgroundColorPicker.SelectedColor.Value.G},{BackgroundColorPicker.SelectedColor.Value.B})";
                        var tilesetFileName = TilesetFilePathTextBlock.Text == Strings.NotApplicable ? Strings.NotApplicable : new FileInfo(TilesetFilePathTextBlock.Text).Name;
                        initializationString += $"game{i + 1}_sprite_id = jo_sprite_add_tga_tileset(JO_ROOT_DIR, \"{tilesetFileName}\", {colorString}, game_tileset, GAME_TILE_COUNT);\n";
                    }
                    else
                    {
                        initializationString += $"game{i + 1}_sprite_id = game1_sprite_id + {i};\n";
                    }
                }

                TilesetInitializationTextBox.Text = initializationString;
                UpdateCopyInitializationButtonEnabledState();
            }
        }

        private void UpdateTilesetDefinitionText(SixLabors.ImageSharp.Size tileSize)
        {
            var tileVarDefinitionsString = String.Empty;
            var definitionString = "jo_tile game_tileset[] = {\n";

            for (var i = 0; i < TilesListBox.Items.Count; i++)
            {
                tileVarDefinitionsString += $"int game{i + 1}_sprite_id;\n";
                var filePath = TilesListBox.Items[i].ToString();
                if (i == TilesListBox.Items.Count - 1)
                {
                    definitionString += $"    {{ {i * tileSize.Width}, 0, {tileSize.Width}, {tileSize.Height} }}\n";
                }
                else
                {
                    definitionString += $"    {{ {i * tileSize.Width}, 0, {tileSize.Width}, {tileSize.Height} }},\n";
                }
            }

            definitionString += "};\n\n";
            definitionString += "#define GAME_TILE_COUNT JO_TILE_COUNT(game_tileset)\n";

            TilesetDefinitionTextBox.Text = tileVarDefinitionsString + "\n" + definitionString;
            UpdateCopyDefinitionButtonEnabledState();
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
                TileWidthLabel.Content = Strings.NotApplicable;
                TileHeightLabel.Content = Strings.NotApplicable;
                UpdateTilesetCodeText(new SixLabors.ImageSharp.Size(0, 0));
                if (ErrorsListBox.Items.Contains(Strings.TileSizeMismatch))
                {
                    ErrorsListBox.Items.Remove(Strings.TileSizeMismatch);
                    UpdateBuildTilesetButtonEnabledState();
                }
            }
            else
            {
                var firstFilePath = TilesListBox.Items[0].ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(firstFilePath))
                {
                    var tileSize = GetTileDimensions(firstFilePath);
                    TileWidthLabel.Content = tileSize.Width;
                    TileHeightLabel.Content = tileSize.Height;
                    CheckTileSizeMismatch(tileSize.Width, tileSize.Height);
                }

                UpdateTilesetCodeText(GetTileDimensions(TilesListBox.Items[0].ToString() ?? string.Empty));
            }

            UpdateBuildTilesetButtonEnabledState();
            UpdateRemoveTileButtonEnabledState();
            UpdateMoveUpButtonEnabledState();
            UpdateMoveDownButtonEnabledState();
        }

        private void SetTilesetFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".tga";
            saveFileDialog.Filter = Strings.TgaFilter;
            if (TilesetFilePathTextBlock.Text != Strings.NotApplicable)
                saveFileDialog.FileName = TilesetFilePathTextBlock.Text; // Pre-populate with the current file path if it's not the default text

            if (saveFileDialog.ShowDialog() == true)
            {
                TilesetFilePathTextBlock.Text = saveFileDialog.FileName;
                FileInfo saveFileInfo = new FileInfo(saveFileDialog.FileName);
                var extensionlessFileName = saveFileInfo.Name.Replace(saveFileInfo.Extension, "");
                var fileNameLengthError = String.Format(Strings.FilenameMaxCharacters, MaxFileNameLength);
                if (extensionlessFileName.Length > MaxFileNameLength)
                {
                    if (!ErrorsListBox.Items.Contains(fileNameLengthError))
                    {
                        ErrorsListBox.Items.Add(fileNameLengthError);
                        UpdateBuildTilesetButtonEnabledState();
                    }
                }
                else
                {
                    if (ErrorsListBox.Items.Contains(fileNameLengthError))
                    {
                        ErrorsListBox.Items.Remove(fileNameLengthError);
                        UpdateBuildTilesetButtonEnabledState();
                    }
                }

                UpdateTilesetInitializationText();
                UpdateBuildTilesetButtonEnabledState();
            }

        }

        private async void CopyInitializationButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TilesetInitializationTextBox.Text);

            await UtilityDialog.ShowAsync(Strings.TilesetInitializationCopied, Strings.Success);
        }

        private async void CopyDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TilesetDefinitionTextBox.Text);

            await UtilityDialog.ShowAsync(Strings.TilesetDefinitionCopied, Strings.Success);
        }

        private async void BuildTilesetButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(TilesetFilePathTextBlock.Text))
            {
                var result = await UtilityDialog.ConfirmAsync(Strings.OverwriteConfirmation, Strings.OverwriteTilesetFile);

                if (!result)
                {
                    return; // User chose not to overwrite the file, so we exit the method early
                }

            }

            var backgroundColor = SixLabors.ImageSharp.Color.Transparent.ToPixel<Rgba32>();
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
                _ => throw new InvalidOperationException(Strings.InvalidBits)
            };

            targetImage.Save(TilesetFilePathTextBlock.Text, new TgaEncoder
            {
                BitsPerPixel = bitsPerPixel,
                Compression = TgaCompression.None
            });


            await UtilityDialog.ShowAsync(Strings.TilesetSuccess, Strings.Success);
        }

        private async void ResetTilesetButton_Click(object sender, RoutedEventArgs e)
        {

            var result = await UtilityDialog.ConfirmAsync(Strings.AreYouSure, Strings.ResetTileData);

            if (result)
            {
                ResetTileData();
            }   
        }

        private void ResetTileData()
        {
            // clear tiles
            TilesListBox.Items.Clear();

            // reset tile properties
            TileWidthLabel.Content = Strings.NotApplicable;
            TileHeightLabel.Content = Strings.NotApplicable;

            // clear tileset file path
            TilesetFilePathTextBlock.Text = Strings.NotApplicable;

            // clear tileset code
            TilesetInitializationTextBox.Text = Strings.NotApplicable;
            TilesetDefinitionTextBox.Text = Strings.NotApplicable;

            // clear errors
            ErrorsListBox.Items.Clear();

            BackgroundColorPicker.SelectedColor = System.Windows.Media.Colors.Transparent;
            BitDepthComboBox.SelectedItem = BitDepthComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag.ToString() == DefaultBitDepth); // Default to 32 bits per pixel

            UpdateBuildTilesetButtonEnabledState();
            UpdateRemoveTileButtonEnabledState();
            UpdateMoveUpButtonEnabledState();
            UpdateMoveDownButtonEnabledState();
            UpdateCopyDefinitionButtonEnabledState();
            UpdateCopyInitializationButtonEnabledState();
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

        private void BackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            UpdateTilesetInitializationText();
        }

        private void TilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRemoveTileButtonEnabledState();
            UpdateMoveUpButtonEnabledState();
            UpdateMoveDownButtonEnabledState();
        }
    }
}