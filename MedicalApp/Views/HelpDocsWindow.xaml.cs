using System.Windows;

namespace MedicalApp.Views
{
    public partial class HelpDocsWindow : Window
    {
        public HelpDocsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
