namespace Chino
{
    public static class Config
    {
        public static string DatabasePath = @"c:\data\chino\chino.sqlite";
        public static string ImageFileRegex = @"^.+\.(" +
            "jpeg" +
            "|jpg" +
            "|jpg-large" +
            "|png" +
            "|png-large" +
            ")$";
    }
}
