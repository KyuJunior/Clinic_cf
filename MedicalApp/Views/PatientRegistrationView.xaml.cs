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

    public class BindingProxy : System.Windows.Freezable
    {
        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly System.Windows.DependencyProperty DataProperty =
            System.Windows.DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new System.Windows.PropertyMetadata(null));
    }
}
