using System;

namespace Chino.Model.Util
{
    public class ImageInfo
    {
        public Uri ImageUri { get; set; }

        public ImageInfo(Uri imageUri)
        {
            ImageUri = imageUri;
        }
    }
}
