using System.Windows;

namespace MedicalApp.Helpers
{
    public static class BadgeHelper
    {
        public static readonly DependencyProperty HasDataProperty =
            DependencyProperty.RegisterAttached("HasData",
                typeof(bool), typeof(BadgeHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static void SetHasData(DependencyObject dp, bool value)
        {
            dp.SetValue(HasDataProperty, value);
        }

        public static bool GetHasData(DependencyObject dp)
        {
            return (bool)dp.GetValue(HasDataProperty);
        }
    }
}
