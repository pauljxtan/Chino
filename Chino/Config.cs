using System.IO;

namespace Chino
{
    public static class Config
    {
        public static string DefaultDatabasePath = @"c:\data\chino\chino.sqlite";
        public static string ImageFileRegex = @"^.+\.(" +
            "jpeg" +
            "|jpg" +
            "|jpg-large" +
            "|png" +
            "|png-large" +
            ")$";
    }
}
