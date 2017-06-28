using Chino.Database;
using System.Collections.Generic;

namespace Chino.Model
{
    public class Tag
    {
        private static List<Tag> _tagList;

        public string Name { get; set; }
        public static List<Tag> TagList { get { return _tagList; } }

        public Tag(string name)
        {
            Name = name;
        }

        static Tag()
        {
            Load();
        }

        public static void Load()
        {
            _tagList = ChinoDbHelper.GetAllTags();
        }

        public static List<Image> GetImagesByTag(string tagName)
        {
            return Image.ImageList.FindAll(i => i.Tags.Contains(tagName));
        }

        public static void Add(string tagName)
        {
            ChinoDbHelper.InsertTag(tagName);
        }

        public static void Remove(string tagName)
        {
            ChinoDbHelper.RemoveTag(tagName);
        }
    }
}