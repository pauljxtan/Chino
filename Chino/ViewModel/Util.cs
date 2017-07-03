using Microsoft.WindowsAPICodePack.Dialogs;

namespace Chino.ViewModel
{
    public static class Util
    {
        public static CommonOpenFileDialog GetOpenFolderDialog(string initialDirectory)
        {
            return new CommonOpenFileDialog()
            {
                EnsureReadOnly = true,
                IsFolderPicker = true,
                AllowNonFileSystemItems = false,
                Multiselect = false,
                InitialDirectory = initialDirectory,
                Title = "Select folder"
            };
        }
    }
}
