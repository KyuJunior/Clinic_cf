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



        private readonly System.Collections.Generic.Dictionary<string, FrameworkElement> _elementCache = new();

        private void HighlightElement(FrameworkElement element)
        {
            var (setBrush, setThickness, originalBrush, originalThickness) = element switch
            {
                Control control => (new Action<System.Windows.Media.Brush?>(b => control.BorderBrush = b),
                                    new Action<Thickness>(t => control.BorderThickness = t),
                                    control.BorderBrush,
                                    control.BorderThickness),
                Border border => (new Action<System.Windows.Media.Brush?>(b => border.BorderBrush = b),
                                  new Action<Thickness>(t => border.BorderThickness = t),
                                  border.BorderBrush,
                                  border.BorderThickness),
                _ => (null, null, null, default)
            };

            if (setBrush != null && setThickness != null)
            {
                // Fluent cyan/teal theme highlight color
                var highlightColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#06B6D4");
                var animBrush = new System.Windows.Media.SolidColorBrush(highlightColor);

                // Set visible border highlight thickness (slightly thicker for prominence)
                setThickness(new Thickness(2));
                setBrush(animBrush);

                var targetColor = originalBrush is System.Windows.Media.SolidColorBrush scb
                    ? scb.Color
                    : (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2D2D30");

                var animation = new System.Windows.Media.Animation.ColorAnimation
                {
                    From = highlightColor,
                    To = targetColor,
                    Duration = new Duration(TimeSpan.FromSeconds(1.2)),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };

                animation.Completed += (s, args) =>
                {
                    if (element.IsLoaded)
                    {
                        setBrush(originalBrush);
                        setThickness(originalThickness);
                    }
                };

                animBrush.BeginAnimation(System.Windows.Media.SolidColorBrush.ColorProperty, animation);
            }
        }

        private void SidebarNav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ButtonBase button && button.CommandParameter is string targetName)
            {
                if (!_elementCache.TryGetValue(targetName, out var targetElement))
                {
                    targetElement = CenterScrollViewer.FindName(targetName) as FrameworkElement;
                    if (targetElement != null)
                    {
                        _elementCache[targetName] = targetElement;
                    }
                }

                if (targetElement != null)
                {
                    targetElement.BringIntoView();
                    HighlightElement(targetElement);
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
            if (e.Key == System.Windows.Input.Key.Enter && DrugSuggestionsListBox.SelectedItem is string selectedDrug)
            {
                if (DataContext is ViewModels.ClinicalExamViewModel vm)
                {
                    vm.SelectSuggestedDrugCommand.Execute(selectedDrug);
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
            if (e.Key == System.Windows.Input.Key.Enter && sender is ComboBox comboBox)
            {
                var expression = comboBox.GetBindingExpression(ComboBox.TextProperty);
                expression?.UpdateSource();

                if (DataContext is ViewModels.ClinicalExamViewModel vm)
                {
                    if (comboBox.Name == "LabsComboBox")
                    {
                        vm.AddInvestigationCommand.Execute(null);
                        e.Handled = true;
                    }
                    else if (comboBox.Name == "ImagingComboBox")
                    {
                        vm.AddImagingCommand.Execute(null);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
