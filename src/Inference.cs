using System.Drawing;
using System.Drawing.Imaging;
using Processing;
using System.Diagnostics;
using CCL;

#pragma warning disable CA1416


namespace Inference
{
    public enum ShapeType
    {
        Rectangle,
        Square,
        Circle,
        Ellipse,
        Triangle,
        Unknown
    }
    public class Moment
    {
        public float area;
        public PointF centroid;   
        public Moment(float area , PointF centroid)
        {
            this.area = area;
            this.centroid = centroid;
        }
    }
    public class Shape
    {
        public ShapeType type;
        public Rectangle Box;
        public Shape(ShapeType type, Rectangle Box)
        {
            this.type = type;
            this.Box = Box;
        }
    }
    class Classifier
    {
        public static void PrintResults(Bitmap img , int count = 1)
        {

            string dir = @"results";
            if (!Directory.Exists(dir))
               Directory.CreateDirectory(dir);

            
            Bitmap InputImage = img.Clone(new Rectangle(0, 0, img.Width, img.Height) , img.PixelFormat);

            if (InputImage.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                InputImage = ImageProcessor.ConvertToBinaryRGB(InputImage , 200);
            }
            else
            {
                InputImage = ImageProcessor.ConvertToBinaryGray(InputImage , 200);
                img = ImageProcessor.ConvertTo24Bpp(img);
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            List<List<Point>> ShapesPoints = ccl.GetShapes(InputImage); 
            List<Shape> ClassifedShapes = GetClassifiedShapes(ShapesPoints);

            stopWatch.Stop();


             foreach (Shape shape in ClassifedShapes)
            {
                float x = ((shape.Box.X + shape.Box.Width) + shape.Box.X ) / 2 - 15;
                float y = ((shape.Box.Y + shape.Box.Height) + shape.Box.Y ) / 2 ;

                ImageProcessor.InsertShapeType(ref img, shape.type.ToString() , new PointF(x, y) , 8);
            }   
            img.Save("results/FinalResult"+ count.ToString() +".bmp" , ImageFormat.Bmp);
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Image : {count}");
            Console.WriteLine($"{ClassifedShapes.Count} Shapes Found");
            Console.WriteLine($"Execution Time: {stopWatch.ElapsedMilliseconds} ms");
        }
        public static unsafe List<Shape> GetClassifiedShapes(List<List<Point>> Shapes)
        {

            List<Shape> ShapeList = new List<Shape>();

            foreach (List<Point> shapePoints in Shapes)
            {
                int minX = shapePoints.Min(p => p.X);
                int minY = shapePoints.Min(p => p.Y);
                int maxX = shapePoints.Max(p => p.X);
                int maxY = shapePoints.Max(p => p.Y);
                
                int width = maxX - minX + 1;
                int height = maxY - minY + 1;

                Bitmap shapeImage = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

                BitmapData shapeData = shapeImage.LockBits(new Rectangle(0, 0, width, height),
                 ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                int stride = shapeData.Stride;

                byte* PixlePtr = (byte*)shapeData.Scan0;


                foreach (Point point in shapePoints)
                {
                    PixlePtr[(point.Y - minY) * stride + (point.X - minX)] = 0xff;
                }
                shapeImage.UnlockBits(shapeData);
                ShapeList.Add(new Shape(Infer(shapeImage), new Rectangle(minX ,minY, width, height)));
            }
            return ShapeList;
        }
        private static ShapeType Infer(Bitmap ShapeImg , double RatioThreshold = 0.22)
        {

            double Width = ShapeImg.Width, Height = ShapeImg.Height;

            Moment features = CalculateMoments(ShapeImg);
            double areaRatio = features.area / (Width * Height);

            if (!Isfilled(areaRatio))
            {
                ImageProcessor.FloodFill(ShapeImg, 0x00, 0xff);
                features = CalculateMoments(ShapeImg);
                areaRatio = features.area / (Width * Height);
            }

            if(areaRatio < RatioThreshold)
                return ShapeType.Unknown;

            double Xs = Math.Abs(features.centroid.X - Width / 2);
            double Ys = Math.Abs(features.centroid.Y - Height / 2);
            
            

            if(Xs > 1 || Ys > 1)
            {
                return ShapeType.Triangle;
            }

            double[] AreasDifference = new double[3]
            {
                Math.Abs(Width * Height - features.area),
                Math.Abs(Math.PI * Width  / 2 * Height / 2  - features.area),
                Math.Abs(0.5 * Width * Height - features.area)
            }; 

            double value = AreasDifference.Min();
            int index = Array.IndexOf(AreasDifference, value);

            switch (index)
            {
                case 0:
                    if (Width != Height)
                        return ShapeType.Rectangle;
                    return ShapeType.Square;
                case 1:
                    if(Width == Height)
                        return ShapeType.Circle;
                    return ShapeType.Ellipse;
                case 2:
                    return ShapeType.Rectangle;
                default: return ShapeType.Unknown;
            }
        }
        private static unsafe Moment CalculateMoments(Bitmap src)
        {
            int width = src.Width, height = src.Height;

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            
            byte* Srcptr = (byte*)srcData.Scan0;
            int SrcStride = srcData.Stride;
            

            float area = 0;
            float Xs = 0;
            float Ys = 0;

            for (int y = 0; y < height; y++)
            {
                byte* SrcRow = (byte*)(Srcptr + y * SrcStride);
                for (int x = 0; x < width; x++)
                {
                    if (SrcRow[x] == 0xff)
                    {                       
                        area += 1;
                        Xs += x;
                        Ys += y;
                    }
                }
            }
            src.UnlockBits(srcData);
            return new Moment(area , new PointF(Xs / area , Ys / area));
        }  
        private static bool Isfilled(double AreaRatio)
        {
            if (AreaRatio > 0.55)
                return true;
            return false;
        }
    }
}
