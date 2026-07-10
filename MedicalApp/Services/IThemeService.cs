using System;

namespace MedicalApp.Services
{
    public interface IThemeService
    {
        bool IsDarkMode { get; set; }
        void ToggleTheme();
        event EventHandler? ThemeChanged;
    }
}
