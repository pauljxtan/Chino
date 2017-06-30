using System.Collections.Generic;

namespace Chino.Model
{
    public class Image
    {
        public string Name { get; set; }
        public List<Tag> Tags { get; set; }

        public Image(string name)
        {
            Name = name;
            Tags = new List<Tag>();
        }

        public Image(string name, List<Tag> tags)
        {
            Name = name;
            Tags = tags;
        }
    }
}