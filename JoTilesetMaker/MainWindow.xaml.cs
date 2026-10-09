using Microsoft.Win32;
using SixLabors.ImageSharp;
using System.Windows;

namespace JoTilesetMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string LabelDefaultText = "N/A";

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
                }
            }
        }

        private SixLabors.ImageSharp.Size GetTileDimensions(string filePath)
        {
            using (var image = Image.Load(filePath))
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
            }
        }

        private void CopyConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TilesetConfigTextBox.Text);
        }

        private void BuildTilesetButton_Click(object sender, RoutedEventArgs e)
        {

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
    }
}