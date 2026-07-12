using System;
using System.Windows;
using System.Windows.Controls;

namespace MedicalApp.Views
{
    public partial class ClinicalExamView : UserControl
    {
        public ClinicalExamView()
        {
            InitializeComponent();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new HelpDocsWindow
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void SidebarNav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ButtonBase button && button.CommandParameter is string targetName)
            {
                var targetElement = CenterScrollViewer.FindName(targetName) as FrameworkElement;
                if (targetElement != null)
                {
                    targetElement.BringIntoView();
                }
            }
        }
    }
}
