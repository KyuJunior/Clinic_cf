using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MedicalApp.Models
{
    public partial class ClinicalAttachment : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AttachmentName))]
        private string _name = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AttachmentName))]
        [NotifyPropertyChangedFor(nameof(IsImage))]
        [NotifyPropertyChangedFor(nameof(IsNonImageFile))]
        private string _attachmentPath = string.Empty;

        public string AttachmentName => string.IsNullOrWhiteSpace(AttachmentPath) 
            ? string.Empty 
            : Path.GetFileName(AttachmentPath);

        public bool IsImage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AttachmentPath)) return false;
                string ext = Path.GetExtension(AttachmentPath).ToLower();
                return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp";
            }
        }

        public bool IsNonImageFile => !string.IsNullOrWhiteSpace(AttachmentPath) && !IsImage;
    }
}
