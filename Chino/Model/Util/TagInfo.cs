namespace Chino.Model.Util
{
    public class TagInfo
    {
        public string TagName { get; set; }
        public int NumberOfImages { get; set; }

        public TagInfo()
        {
        }

        public TagInfo(string tagName, int numberOfImages)
        {
            TagName = tagName;
            NumberOfImages = numberOfImages;
        }
    }
}
