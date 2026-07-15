using CommunityToolkit.Mvvm.ComponentModel;

namespace MedicalApp.ViewModels
{
    public partial class SectionOrderItem : ObservableObject
    {
        public string SectionKey { get; set; } = string.Empty;
        public string DisplayNameEn { get; set; } = string.Empty;
        public string DisplayNameAr { get; set; } = string.Empty;

        [ObservableProperty]
        private bool _isVisible = true;

        [ObservableProperty]
        private int _displayOrder;
    }
}
