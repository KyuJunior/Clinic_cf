using System;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MedicalApp.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private readonly FlowDocument _document;
        private readonly string _documentTitle;
        private readonly bool _printBackground;
        private readonly Image? _bgImage;

        public PrintPreviewWindow(FlowDocument document, string documentTitle, bool printBackground = true, Image? bgImage = null)
        {
            InitializeComponent();
            _document = document;
            _documentTitle = documentTitle;
            _printBackground = printBackground;
            _bgImage = bgImage;
            DocViewer.Document = _document;

            LoadInstalledPrinters();
        }

        private void LoadInstalledPrinters()
        {
            try
            {
                var printServer = new LocalPrintServer();
                var printQueues = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

                foreach (var queue in printQueues)
                {
                    PrinterComboBox.Items.Add(queue.Name);
                }

                // Try to select the default printer
                var defaultQueue = LocalPrintServer.GetDefaultPrintQueue();
                if (defaultQueue != null && PrinterComboBox.Items.Contains(defaultQueue.Name))
                {
                    PrinterComboBox.SelectedItem = defaultQueue.Name;
                }
                else if (PrinterComboBox.Items.Count > 0)
                {
                    PrinterComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load installed printers: {ex.Message}", "Printer Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (PrinterComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a printer first.", "No Printer Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedPrinterName = PrinterComboBox.SelectedItem.ToString()!;
            
            try
            {
                var printServer = new LocalPrintServer();
                var queue = printServer.GetPrintQueue(selectedPrinterName);
                
                var printDialog = new PrintDialog
                {
                    PrintQueue = queue
                };

                // Get a print ticket and configure properties
                var ticket = queue.DefaultPrintTicket;

                // Configure Orientation
                if (OrientationComboBox.SelectedItem is ComboBoxItem selectedOrientationItem)
                {
                    string orientation = selectedOrientationItem.Content.ToString()!;
                    ticket.PageOrientation = orientation == "Landscape" 
                        ? PageOrientation.Landscape 
                        : PageOrientation.Portrait;
                }

                // Configure Copies
                int copies = 1;
                if (CopiesComboBox.Text != null)
                {
                    if (int.TryParse(CopiesComboBox.Text, out int parsedCopies))
                    {
                        copies = Math.Max(1, parsedCopies);
                    }
                }
                ticket.CopyCount = copies;

                // Configure Color Mode
                if (ColorComboBox.SelectedItem is ComboBoxItem selectedColorItem)
                {
                    string colorMode = selectedColorItem.Content.ToString()!;
                    ticket.OutputColor = colorMode == "Black & White" 
                        ? OutputColor.Monochrome 
                        : OutputColor.Color;
                }

                // Force A5 Page Size if applicable
                if (_documentTitle.Contains("Rx") || _document.PageWidth == 560)
                {
                    ticket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
                }

                printDialog.PrintTicket = ticket;

                // Collapse background image if we should NOT print it on physical paper
                if (!_printBackground && _bgImage != null)
                {
                    _bgImage.Visibility = Visibility.Collapsed;
                }

                try
                {
                    printDialog.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, _documentTitle);
                }
                finally
                {
                    // Restore background visibility for screen preview
                    if (_bgImage != null)
                    {
                        _bgImage.Visibility = Visibility.Visible;
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to print document: {ex.Message}", "Print Spooling Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
