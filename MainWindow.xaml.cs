using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace converter
{
    public partial class MainWindow : Window
    {
        private BitmapImage? _selectedImage;
        private string _originalFormat = "";
        private string _selectedFormat = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image files (*.bmp, *.gif, *.jpg, *.jpeg, *.png, *.tiff) | *.bmp; *.gif; *.jpg; *.jpeg; *.png; *.tiff"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;
                _selectedImage = new(new Uri(imagePath));
                _originalFormat = System.IO.Path.GetExtension(imagePath).ToLower();
                imageNameTextBlock.Text = System.IO.Path.GetFileName(imagePath);

                radioBmp.IsEnabled = _originalFormat != ".bmp";
                radioGif.IsEnabled = _originalFormat != ".gif";
                radioJpeg.IsEnabled = _originalFormat != ".jpg" && _originalFormat != ".jpeg";
                radioPng.IsEnabled = _originalFormat != ".png"; 
                radioTiff.IsEnabled = _originalFormat != ".tiff";

                UpdateConvertButtonState();
            }
        }
        
        private void ConverButton_Click(object sender, RoutedEventArgs e)
        {
            convertButton.Content = "Wait";
            convertButton.IsEnabled = false;
            ConvertImage();
            convertButton.Content = "Convert";
            convertButton.IsEnabled = true;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            _selectedFormat = radioButton.Content?.ToString() ?? string.Empty;
            UpdateConvertButtonState();
        }

        private void UpdateConvertButtonState()
        {
            convertButton.IsEnabled = _selectedImage != null && !string.IsNullOrEmpty(_selectedFormat);
        }

        private void ConvertImage()
        {
            ImageFormat? format = GetImageFormat(_selectedFormat);
            if (format == null)
            {
                MessageBox.Show("Format not allowed.");
                return;
            }

            SaveFileDialog saveFileDialog = new()
            {
                Filter = $"{_selectedFormat} files|*.{_selectedFormat.ToLower()}",
                DefaultExt = _selectedFormat.ToLower()
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string outputPath = saveFileDialog.FileName;
                using (MemoryStream memoryStream = new())
                {
                    BitmapEncoder? encoder = GetBitmapEncoder(_selectedFormat);
                    if (encoder == null)
                    {
                        MessageBox.Show("Encoder not found.");
                        return;
                    }
                    encoder.Frames.Add(BitmapFrame.Create(_selectedImage));
                    encoder.Save(memoryStream);

                    using (Bitmap bitmap = new(memoryStream))
                    {
                        bitmap.Save(outputPath, format);
                    }
                }

                MessageBox.Show("Done.");
            }
        }

        private static ImageFormat? GetImageFormat(string format)
        {
            return format.ToLower() switch
            {
                "bmp" => ImageFormat.Bmp,
                "gif" => ImageFormat.Gif,
                "jpeg" => ImageFormat.Jpeg,
                "png" => ImageFormat.Png,
                "tiff" => ImageFormat.Tiff,
                _ => null,
            };
        }

        private static BitmapEncoder? GetBitmapEncoder(string format)
        {
            return format.ToLower() switch
            {
                "bmp" => new BmpBitmapEncoder(),
                "gif" => new GifBitmapEncoder(),
                "jpeg" => new JpegBitmapEncoder(),
                "png" => new PngBitmapEncoder(),
                "tiff" => new TiffBitmapEncoder(),
                _ => null,
            };
        }
    }
}