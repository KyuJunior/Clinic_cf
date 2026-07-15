using CommunityToolkit.Mvvm.ComponentModel;

namespace MedicalApp.Models
{
    public partial class PerformedProcedure : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private decimal _cost = 0;
    }
}
