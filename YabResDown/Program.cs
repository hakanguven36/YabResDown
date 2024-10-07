using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace YabResDown
{
    class Program
    {
        private const int OrientationKey = 0x0112;
        private const int NotSpecified = 0;
        private const int NormalOrientation = 1;
        private const int MirrorHorizontal = 2;
        private const int UpsideDown = 3;
        private const int MirrorVertical = 4;
        private const int MirrorHorizontalAndRotateRight = 5;
        private const int RotateLeft = 6;
        private const int MirorHorizontalAndRotateLeft = 7;
        private const int RotateRight = 8;

        public static string webRoot = "http://etiketlendirtici.online/dosyalar/";
        public static string target_dir = @"D:\DTS";
        public static Size size = new Size(180, 180);
        public static int counter = 0;

        static void Main(string[] args)
        {

            string jsonfile = Path.Combine(target_dir, "y2label.json");
            string json_content = File.OpenText(jsonfile).ReadToEnd();
            Project project = JsonConvert.DeserializeObject<Project>(json_content);

            List<DirectoryInfo> annoDirList = new List<DirectoryInfo>();
            project.annotations.ForEach(u =>
            {
                DirectoryInfo di = new DirectoryInfo(Path.Combine(target_dir, "Completed", u.name));
                if (!di.Exists) di.Create();
                annoDirList.Add(di);
            });

            int totalPhotoCount = project.photos.Count;

            Console.WriteLine("totalPhotoCount: " + totalPhotoCount.ToString());

            foreach (Photo photo in project.photos)
            {
                try
                {
                    if (photo.labels.Count == 0)
                        continue;

                    using (WebClient client = new WebClient())
                    {
                        string fullUrl = webRoot + photo.path;

                        using (Stream stream = client.OpenRead(new Uri(fullUrl)))
                        {
                            Image image = Image.FromStream(stream);
                            int rotation = CheckRotation(image);
                            if (rotation == 6)
                                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            else if (rotation == 8)
                                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            else if (rotation == 3)
                                image.RotateFlip(RotateFlipType.Rotate180FlipNone);

                            int w = image.Width;
                            int h = image.Height;

                            int labelCounter = 1;
                            foreach (Label label in photo.labels)
                            {
                                string dirPath = annoDirList[label.annoID].FullName;
                                Console.WriteLine("labelCounter: " + labelCounter.ToString());
                                labelCounter++;
                                int x = label.points[0].x;
                                int y = label.points[0].y;
                                int kenar = label.points[2].x - x;

                                using (Image smallimage = CropImage(image, new Rectangle(x, y, kenar, kenar)))
                                {
                                    using (Image resizedImage = ResizeImage(smallimage, size))
                                    {
                                        string uniqName = GetUniqueFileName("") + ".jpg";
                                        string savePath = Path.Combine(dirPath, uniqName);
                                        resizedImage.Save(savePath);
                                    }
                                }
                            }
                        }
                    }
                    counter++;
                    Console.WriteLine($"Processed photos:{counter}  (%{(counter * 100) / totalPhotoCount})");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Hata: " + e.Message);
                }
            }

            Console.WriteLine("TAMAMLANDI");
            Console.Read();
        }

        private static int CheckRotation(Image img)
        {
            if (img.PropertyIdList.Contains(OrientationKey))
            {
                return (int)img.GetPropertyItem(OrientationKey).Value[0];
            }
            return 0;
        }


        private static Image CropImage(Image img, Rectangle cropArea)
        {
            return new Bitmap(img).Clone(cropArea, img.PixelFormat);
        }

        private static Image ResizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            // Calculate width and height with new desired size
            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            nPercent = Math.Min(nPercentW, nPercentH);
            // New Width and Height
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);


            using (Graphics g = Graphics.FromImage((Image)b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            }
            return (Image)b;

        }

        private static string GetUniqueFileName(string fileName)
        {
            return Guid.NewGuid().ToString() + Path.GetExtension(fileName);
        }
    }
}
