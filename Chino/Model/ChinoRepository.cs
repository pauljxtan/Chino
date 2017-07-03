using Chino.Database;
using System;
using System.Collections.Generic;

namespace Chino.Model
{
    public static class ChinoRepository
    {
        public static bool ImageExists(string imageName)
        {
            return ChinoDbHelper.ImageExists(imageName);
        }

        public static bool TagExists(string tagName)
        {
            return ChinoDbHelper.TagExists(tagName);
        }

        public static bool ImageTagRelationExists(string imageName, string tagName)
        {
            return ChinoDbHelper.ImageTagRelationExists(imageName, tagName);
        }

        public static bool AddImage(string imageName)
        {
            return ChinoDbHelper.InsertImage(imageName);
        }

        public static bool AddTag(string tagName)
        {
            return ChinoDbHelper.InsertTag(tagName);
        }

        public static bool AddImageTagRelation(string imageName, string tagName)
        {
            // Add image if not exising
            if (!ImageExists(imageName))
            {
                AddImage(imageName);
            }
            // Add tag if not existing
            if (!TagExists(tagName))
            {
                AddTag(tagName);
            }
            // Associate tag with image
            var success = true;
            if (!ImageTagRelationExists(imageName, tagName))
            {
                success = ChinoDbHelper.InsertImageTagRelation(imageName, tagName);
            }
            return success;
        }

        public static bool RemoveImageTagRelation(string imageName, string tagName)
        {
            if (!(ImageExists(imageName) && TagExists(tagName))) return true;
            return ChinoDbHelper.DeleteImageTagRelation(imageName, tagName);
        }

        public static void RemoveTag(string imageName, string tagName)
        {
            if (TagExists(tagName))
            {
                ChinoDbHelper.DeleteTag(tagName);
            }
            if (ImageTagRelationExists(imageName, tagName))
            {
                ChinoDbHelper.DeleteImageTagRelation(imageName, tagName);
            }
        }

        public static List<Tag> GetAllTags()
        {
            return ChinoDbHelper.GetAllTags();
        }

        public static List<Tag> GetTagsByImage(string imageName)
        {
            var tags = ChinoDbHelper.GetTagsByImage(imageName);
            if (tags.Count == 0)
            {
                tags.Add(new Tag("TAGME"));
            }
            return tags;
        }

        public static List<string> GetTagNamesByImage(string imageName)
        {
            return ChinoDbHelper.GetTagNamesByImage(imageName);
        }

        public static List<Image> GetImagesByTag(string tagName)
        {
            return ChinoDbHelper.GetImagesByTag(tagName);
        }

        //public static List<Image> ImageRepo;
        //public static List<Tag> TagRepo;
        //public static List<Tuple<Image, Tag>> ImageTagRepo;

        //static ChinoRepository()
        //{
        //    ReloadImageRepo();
        //    ReloadTagRepo();
        //    ReloadImageTagRelationRepo();
        //}

        //public static void ReloadImageRepo()
        //{
        //    ImageRepo = ChinoDbHelper.GetAllImages();
        //}

        //public static void ReloadTagRepo()
        //{
        //    TagRepo = ChinoDbHelper.GetAllTags();
        //}

        //public static void ReloadImageTagRelationRepo()
        //{
        //    ImageTagRepo = ChinoDbHelper.GetAllImageTagRelations();
        //}

        //public static void ReloadAllRepos()
        //{
        //    ChinoRepository.ReloadImageRepo();
        //    ChinoRepository.ReloadTagRepo();
        //    ChinoRepository.ReloadImageTagRelationRepo();
        //}
    }
}
