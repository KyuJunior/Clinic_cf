using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MedicalApp.Services
{
    public class ScannerService : IScannerService
    {
        public string? ScanFromDefaultDevice(string targetFolder)
        {
            try
            {
                var wiaCommonDialogType = Type.GetTypeFromProgID("WIA.CommonDialog");
                if (wiaCommonDialogType == null)
                {
                    throw new InvalidOperationException("Windows Image Acquisition (WIA) is not registered or supported on this system.");
                }

                dynamic commonDialog = Activator.CreateInstance(wiaCommonDialogType)!;

                // Show the default Windows scan dialog
                // 1: ScannerDeviceType, 1: ColorIntent, 1: MaximizeQuality
                // "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}": FormatID.wiaFormatJPEG
                dynamic scannedImage = commonDialog.ShowAcquireImage(
                    1,     // ScannerDeviceType
                    1,     // ColorIntent
                    1,     // MaximizeQuality
                    "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}", // JPEG Format GUID
                    false, // ShowDeviceUI: Show device selection UI if multiple scanners found
                    true,  // UseCommonUI: Use standard progress/settings dialog
                    false  // CancelError: Cancel error handling behavior
                );

                if (scannedImage == null)
                {
                    return null; // User cancelled
                }

                // Generate a unique file name
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }
                
                string uniqueName = $"scan_{Guid.NewGuid()}.jpg";
                string filePath = Path.Combine(targetFolder, uniqueName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Save the scanned WIA Image file to disk
                scannedImage.SaveFile(filePath);
                return filePath;
            }
            catch (COMException ex)
            {
                throw new InvalidOperationException("Scanner communication failed. Verify that your scanner is powered on and connected.", ex);
            }
        }
    }
}
