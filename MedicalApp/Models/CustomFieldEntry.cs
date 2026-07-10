using CommunityToolkit.Mvvm.ComponentModel;

namespace MedicalApp.Models
{
    public partial class CustomFieldEntry : ObservableObject
    {
        [ObservableProperty]
        private string _label = string.Empty;

        [ObservableProperty]
        private string _value = string.Empty;
    }
}
