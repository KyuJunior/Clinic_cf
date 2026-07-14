using System;
using System.IO;
using System.Windows;
using System.Linq;

namespace MedicalApp.Services
{
    public class ThemeService : IThemeService
    {
        private bool _isDarkMode;
        public event EventHandler? ThemeChanged;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    ApplyTheme(_isDarkMode);
                    SaveThemePreference(_isDarkMode);
                    ThemeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ThemeService()
        {
            _isDarkMode = LoadThemePreference();
            // We defer ApplyTheme slightly or call it immediately. Call it immediately.
            ApplyTheme(_isDarkMode);
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        private void ApplyTheme(bool isDark)
        {
            var app = Application.Current;
            if (app == null) return;

            var mergedDicts = app.Resources.MergedDictionaries;
            
            // Find and remove existing theme dictionary (LightTheme.xaml or DarkTheme.xaml)
            var existingTheme = mergedDicts.FirstOrDefault(dict => 
                dict.Source != null && 
                (dict.Source.OriginalString.Contains("LightTheme.xaml", StringComparison.OrdinalIgnoreCase) || 
                 dict.Source.OriginalString.Contains("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase)));

            if (existingTheme != null)
            {
                mergedDicts.Remove(existingTheme);
            }

            // Load and insert the new theme dictionary at the correct position
            var newThemeSource = isDark 
                ? "Styles/DarkTheme.xaml" 
                : "Styles/LightTheme.xaml";

            var newTheme = new ResourceDictionary 
            { 
                Source = new Uri(newThemeSource, UriKind.Relative) 
            };

            // Maintain order: insert right after MaterialDesign defaults (usually index 0)
            if (mergedDicts.Count >= 1)
            {
                mergedDicts.Insert(1, newTheme);
            }
            else
            {
                mergedDicts.Add(newTheme);
            }

            // Sync wpf-ui theme
            try
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    isDark ? Wpf.Ui.Appearance.ApplicationTheme.Dark : Wpf.Ui.Appearance.ApplicationTheme.Light
                );
            }
            catch { }
        }

        private bool LoadThemePreference()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_preference.txt");
                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path).Trim();
                    return text.Equals("dark", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { }
            return false; // Default to Light mode
        }

        private void SaveThemePreference(bool isDark)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_preference.txt");
                File.WriteAllText(path, isDark ? "dark" : "light");
            }
            catch { }
        }
    }
}
