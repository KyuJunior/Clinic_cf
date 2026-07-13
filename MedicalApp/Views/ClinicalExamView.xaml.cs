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

        private void DrugSearchTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Down && DrugSuggestionsPopup.IsOpen)
            {
                DrugSuggestionsListBox.Focus();
                if (DrugSuggestionsListBox.Items.Count > 0)
                {
                    DrugSuggestionsListBox.SelectedIndex = 0;
                    // Focus the container item
                    var item = DrugSuggestionsListBox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                    item?.Focus();
                }
                e.Handled = true;
            }
        }

        private void DrugSuggestionsListBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (DrugSuggestionsListBox.SelectedItem is string selectedDrug)
                {
                    var vm = DataContext as ViewModels.ClinicalExamViewModel;
                    vm?.SelectSuggestedDrugCommand.Execute(selectedDrug);
                    DrugSearchTextBox.Focus();
                    e.Handled = true;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Up && DrugSuggestionsListBox.SelectedIndex == 0)
            {
                DrugSearchTextBox.Focus();
                e.Handled = true;
            }
        }

        private void ComboBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var comboBox = sender as ComboBox;
                if (comboBox != null)
                {
                    var expression = comboBox.GetBindingExpression(ComboBox.TextProperty);
                    expression?.UpdateSource();

                    var vm = DataContext as ViewModels.ClinicalExamViewModel;
                    if (comboBox.Name == "LabsComboBox")
                    {
                        vm?.AddInvestigationCommand.Execute(null);
                        e.Handled = true;
                    }
                    else if (comboBox.Name == "ImagingComboBox")
                    {
                        vm?.AddImagingCommand.Execute(null);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
