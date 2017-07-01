namespace Chino.Model.Util
{
    public class DirectoryInfo1
    {
        public string DirectoryName { get; set; }
        public int NumberOfSubdirs { get; set; }
        public int NumberOfImages { get; set; }

        public DirectoryInfo1()
        {
        }

        public DirectoryInfo1(string directoryName, int numberOfSubdirs, int numberOfImages)
        {
            DirectoryName = directoryName;
            NumberOfSubdirs = numberOfSubdirs;
            NumberOfImages = numberOfImages;
        }
    }
}
