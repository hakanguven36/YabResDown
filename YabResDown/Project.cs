using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabResDown
{
    public class Project
    {
        public string name { get; set; }

        public List<Annotation> annotations { get; set; }

        public List<Photo> photos { get; set; }

    }

    public class Annotation
    {
        public string name { get; set; }
    }

    public class Photo
    {
        public string path { get; set; }

        public List<Label> labels { get; set; }
    }

    public class Label
    {
        public int annoID { get; set; }

        public List<Point> points { get; set; }
    }

    public class Point
    {
        public int x { get; set; }

        public int y { get; set; }

    }


}
