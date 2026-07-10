using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MedicalApp.Services;

namespace MedicalApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThemeService _themeService;

        public bool IsDarkMode => _themeService.IsDarkMode;

        [RelayCommand]
        public void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        public HomeViewModel(IServiceProvider serviceProvider, IThemeService themeService)
        {
            _serviceProvider = serviceProvider;
            _themeService = themeService;
            System.Windows.WeakEventManager<IThemeService, EventArgs>.AddHandler(_themeService, nameof(IThemeService.ThemeChanged), (s, ev) => OnPropertyChanged(nameof(IsDarkMode)));
        }

        [RelayCommand]
        public void NavigateToRegistry()
        {
            var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
            mainVm.NavigateToPatientRegistration();
        }

        [RelayCommand]
        public void NavigateToExam()
        {
            var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
            mainVm.NavigateToClinicalExam();
        }
    }
}
