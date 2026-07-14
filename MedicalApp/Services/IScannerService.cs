namespace MedicalApp.Services
{
    public interface IScannerService
    {
        string? ScanFromDefaultDevice(string targetFolder);
    }
}
