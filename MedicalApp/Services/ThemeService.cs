using System;
using System.IO;
using System.Windows;

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
            ResourceDictionary? existingTheme = null;
            foreach (var dict in mergedDicts)
            {
                if (dict.Source != null && (dict.Source.OriginalString.Contains("LightTheme.xaml") || dict.Source.OriginalString.Contains("DarkTheme.xaml")))
                {
                    existingTheme = dict;
                    break;
                }
            }

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
