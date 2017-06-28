using Chino.Database;
using System;
using System.Collections.Generic;

namespace Chino.Model
{
    public class Image
    {
        private static List<Image> _imageList;

        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public static List<Image> ImageList { get { return _imageList; } }

        public Image(string name)
        {
            Name = name;
            Tags = new List<string>();
        }

        public Image(string name, List<string> tags)
        {
            Name = name;
            Tags = tags;
        }

        static Image()
        {
            Load();
        }

        public static void Load()
        {
            _imageList = ChinoDbHelper.GetAllImages();
        }

        public static void Add(string imageName)
        {
            ChinoDbHelper.InsertImage(imageName);
        }

        public static void Remove(string imageName)
        {
            ChinoDbHelper.RemoveImage(imageName);
        }

        public static List<string> GetTags(string imageName)
        {
            var image = _imageList.Find(i => i.Name == imageName);
            if (image == null) return new List<string>() { "TAGME" };
            return image.Tags;
        }

        public static void AddTag(string imageName, string tagName)
        {
            // TODO: Move these guards into ChinoDbHelper
            // Create new tag if not existing
            if (!ChinoDbHelper.TagExists(tagName))
                Tag.Add(tagName);
            // Associate tag with image
            if (!ChinoDbHelper.ImageTagRelationExists(imageName, tagName))
                ChinoDbHelper.AddImageTagRelation(imageName, tagName);
            Load();
            Tag.Load();
        }

        public static void RemoveTag(string imageName, string tagName)
        {
            // TODO: Move these guards into ChinoDbHelper
            if (ChinoDbHelper.TagExists(tagName))
                Tag.Remove(tagName);
            if (ChinoDbHelper.ImageTagRelationExists(imageName, tagName))
                ChinoDbHelper.RemoveImageTagRelation(imageName, tagName);
            Load();
            Tag.Load();
        }
    }
}