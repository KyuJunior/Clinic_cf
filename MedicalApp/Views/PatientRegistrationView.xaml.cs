using System.Windows.Controls;

namespace MedicalApp.Views
{
    public partial class PatientRegistrationView : UserControl
    {
        public PatientRegistrationView()
        {
            InitializeComponent();
        }

        private void GearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.PatientRegistrationViewModel vm)
            {
                vm.ShowRegistrationModal = false;
            }
        }
    }
}
