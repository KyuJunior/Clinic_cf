using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MedicalApp.Models;
using MedicalApp.ViewModels;

namespace MedicalApp.Views
{
    public partial class ClinicalExamView : UserControl
    {
        private Point _startPoint;
        private bool _isDragging;
        private FrameworkElement? _draggedElement;
        private QueueEntry? _draggedEntry;
        private Point _dragOffset;

        public ClinicalExamView()
        {
            InitializeComponent();
        }

        private void GearButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ClearNotifications_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Notifications cleared successfully. | تم مسح الإشعارات بنجاح.", "Notifications | الإشعارات", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new HelpDocsWindow
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void PatientList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject depObj)
            {
                if (HasParentOfType<Button>(depObj))
                {
                    return;
                }

                _draggedElement = FindElementWithQueueEntryDataContext(depObj);
                if (_draggedElement != null)
                {
                    _draggedEntry = _draggedElement.DataContext as QueueEntry;
                    _startPoint = e.GetPosition(null);
                }
            }
        }

        private void PatientList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                if (!_isDragging && 
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _isDragging = true;
                    _dragOffset = e.GetPosition(_draggedElement);

                    // Setup the visual preview card using VisualBrush
                    DragPreviewBrush.Visual = _draggedElement;
                    DragPreviewRect.Width = _draggedElement.ActualWidth;
                    DragPreviewRect.Height = _draggedElement.ActualHeight;
                    DragPreviewCanvas.Visibility = Visibility.Visible;

                    // Capture mouse to this control to track events globally
                    this.CaptureMouse();
                }

                if (_isDragging)
                {
                    // Move the visual preview card
                    Point canvasPos = e.GetPosition(DragPreviewCanvas);
                    Canvas.SetLeft(DragPreviewBorder, canvasPos.X - _dragOffset.X);
                    Canvas.SetTop(DragPreviewBorder, canvasPos.Y - _dragOffset.Y);

                    // Update visual feedback highlights on targets
                    Point mouseInUserControl = e.GetPosition(this);
                    UpdateDragHighlight(mouseInUserControl);
                    
                    e.Handled = true;
                }
            }
        }

        private async void PatientList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.ReleaseMouseCapture();
                DragPreviewCanvas.Visibility = Visibility.Collapsed;

                // Reset all highlights
                ResetDragHighlight(Card1Border);
                ResetDragHighlight(Card2Border);
                ResetDragHighlight(WaitingPatientsListBox);
                ResetDragHighlight(NotFinishedPatientsListBox);

                Point mouseInUserControl = e.GetPosition(this);
                
                if (_draggedEntry != null && this.DataContext is ClinicalExamViewModel vm)
                {
                    if (IsMouseOverElement(Card1Border, mouseInUserControl) || IsMouseOverElement(WaitingPatientsListBox, mouseInUserControl))
                    {
                        await vm.MoveToQueueStatusAsync(_draggedEntry, "Pending");
                    }
                    else if (IsMouseOverElement(Card2Border, mouseInUserControl) || IsMouseOverElement(NotFinishedPatientsListBox, mouseInUserControl))
                    {
                        await vm.MoveToQueueStatusAsync(_draggedEntry, "InExam");
                    }
                }
            }

            _draggedElement = null;
            _draggedEntry = null;
        }

        private void UpdateDragHighlight(Point mouseInUserControl)
        {
            ResetDragHighlight(Card1Border);
            ResetDragHighlight(Card2Border);
            ResetDragHighlight(WaitingPatientsListBox);
            ResetDragHighlight(NotFinishedPatientsListBox);

            var primaryBrush = TryFindResource("PrimaryColor") as SolidColorBrush;
            var primaryColor = primaryBrush?.Color ?? (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0EA5E9");

            var highlightBg = new SolidColorBrush(primaryColor) { Opacity = 0.08 };
            var highlightBorder = new SolidColorBrush(primaryColor) { Opacity = 0.7 };

            if (IsMouseOverElement(Card1Border, mouseInUserControl))
            {
                Card1Border.Background = highlightBg;
                Card1Border.BorderBrush = highlightBorder;
                Card1Border.BorderThickness = new Thickness(1.5);
            }
            else if (IsMouseOverElement(WaitingPatientsListBox, mouseInUserControl))
            {
                WaitingPatientsListBox.Background = highlightBg;
            }
            else if (IsMouseOverElement(Card2Border, mouseInUserControl))
            {
                Card2Border.Background = highlightBg;
                Card2Border.BorderBrush = highlightBorder;
                Card2Border.BorderThickness = new Thickness(1.5);
            }
            else if (IsMouseOverElement(NotFinishedPatientsListBox, mouseInUserControl))
            {
                NotFinishedPatientsListBox.Background = highlightBg;
            }
        }

        private bool IsMouseOverElement(FrameworkElement element, Point mouseInUserControl)
        {
            if (element == null || !element.IsVisible) return false;
            try
            {
                GeneralTransform transform = this.TransformToVisual(element);
                Point localPos = transform.Transform(mouseInUserControl);
                return localPos.X >= 0 && localPos.X <= element.ActualWidth &&
                       localPos.Y >= 0 && localPos.Y <= element.ActualHeight;
            }
            catch
            {
                return false;
            }
        }

        private void ResetDragHighlight(object sender)
        {
            if (sender is Border border)
            {
                border.ClearValue(Border.BackgroundProperty);
                border.ClearValue(Border.BorderBrushProperty);
                border.ClearValue(Border.BorderThicknessProperty);
            }
            else if (sender is ListBox listBox)
            {
                listBox.ClearValue(ListBox.BackgroundProperty);
            }
        }

        private bool HasParentOfType<T>(DependencyObject element) where T : DependencyObject
        {
            while (element != null)
            {
                if (element is T)
                    return true;
                if (element is FrameworkElement fe)
                {
                    element = fe.Parent ?? System.Windows.Media.VisualTreeHelper.GetParent(fe);
                }
                else
                {
                    element = System.Windows.Media.VisualTreeHelper.GetParent(element);
                }
            }
            return false;
        }

        private FrameworkElement? FindElementWithQueueEntryDataContext(DependencyObject child)
        {
            FrameworkElement? highestBorder = null;
            DependencyObject parentObject = child;
            
            while (parentObject != null)
            {
                if (parentObject is FrameworkElement fe && fe.DataContext is QueueEntry)
                {
                    // Find the main card border wrapping the data.
                    if (fe is Border)
                    {
                        highestBorder = fe;
                    }
                }

                if (parentObject is FrameworkElement frameworkEl)
                {
                    parentObject = frameworkEl.Parent ?? System.Windows.Media.VisualTreeHelper.GetParent(frameworkEl);
                }
                else
                {
                    parentObject = System.Windows.Media.VisualTreeHelper.GetParent(parentObject);
                }
            }
            return highestBorder;
        }

        // Empty dummy drop handlers kept to avoid XAML compilation warnings if wired elsewhere
        private void List_DragOver(object sender, DragEventArgs e) { }
        private void List_DragLeave(object sender, DragEventArgs e) { }
        private void ScheduleList_Drop(object sender, DragEventArgs e) { }
        private void PendingList_Drop(object sender, DragEventArgs e) { }
    }
}
