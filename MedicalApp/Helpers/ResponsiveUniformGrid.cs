using System;
using System.Windows;
using System.Windows.Controls;

namespace MedicalApp.Helpers
{
    public class ResponsiveUniformGrid : Panel
    {
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(ResponsiveUniformGrid),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            int visibleCount = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    visibleCount++;
                }
            }

            if (visibleCount == 0)
                return new Size(0, 0);

            int cols = Columns > 0 ? Columns : (int)Math.Ceiling(Math.Sqrt(visibleCount));
            int rows = (int)Math.Ceiling((double)visibleCount / cols);

            double cellWidth = constraint.Width / cols;
            double cellHeight = constraint.Height / rows;

            if (double.IsInfinity(cellWidth)) cellWidth = 200; // Fallback
            if (double.IsInfinity(cellHeight)) cellHeight = 150; // Fallback

            Size childConstraint = new Size(cellWidth, cellHeight);
            double maxChildWidth = 0;
            double maxChildHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    child.Measure(childConstraint);
                    maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
                    maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
                }
            }

            return new Size(
                double.IsInfinity(constraint.Width) ? maxChildWidth * cols : constraint.Width,
                double.IsInfinity(constraint.Height) ? maxChildHeight * rows : constraint.Height
            );
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            int visibleCount = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    visibleCount++;
                }
            }

            if (visibleCount == 0)
                return arrangeSize;

            int cols = Columns > 0 ? Columns : (int)Math.Ceiling(Math.Sqrt(visibleCount));
            int rows = (int)Math.Ceiling((double)visibleCount / cols);

            double cellWidth = arrangeSize.Width / cols;
            double cellHeight = arrangeSize.Height / rows;

            int index = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    int row = index / cols;
                    int col = index % cols;

                    Rect rect = new Rect(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                    child.Arrange(rect);
                    index++;
                }
            }

            return arrangeSize;
        }
    }
}
