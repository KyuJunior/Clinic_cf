using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MedicalApp.Views
{
    public partial class ImagePreviewWindow : Window
    {
        public ImagePreviewWindow(string imagePath)
        {
            InitializeComponent();
            try
            {
                PreviewImage.Source = new BitmapImage(new Uri(imagePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load image: {ex.Message}", "Error | خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
