using System;
using System.Drawing;
using System.Drawing.Imaging;

#pragma warning disable CA1416

namespace TestGenerator
{
    class Generator
    {
        public static Bitmap GenerateTest(int count, int NumShapes)

        {
            NumShapes = Math.Min(200, NumShapes);
            
            Bitmap bitmap = new Bitmap(1920, 1080, PixelFormat.Format24bppRgb);

            const int padding = 10;

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                Random random = new Random();

                List<Rectangle> shapes = new List<Rectangle>();

                for (int i = 0; i < NumShapes; i++)
                {
                    int shapeType = random.Next(5);
                    int x, y, width, height;
                    Rectangle rect;
                    do
                    {
                        x = random.Next(bitmap.Width);
                        y = random.Next(bitmap.Height);
                        width = random.Next(50, 100);
                        height = random.Next(50, 100);
                        rect = new Rectangle(x - padding, y - padding, width + padding, height + padding);
                    }
                    while (shapes.Any(shape => shape.IntersectsWith(rect)) ||
                     x + width > bitmap.Width || y + height > bitmap.Height);

                    shapes.Add(rect);

                    Color color = Color.FromArgb(0x00, 0x00, 0x00);

                    int PenWidth = random.Next(1, 5);

                    Pen myPen = new Pen(color , PenWidth);


                    int size = Math.Min(width, height);
                    switch (shapeType)
                    {
                        case 0: //shapeType = triangle
                            int Tri = random.Next(4);
                            Point[] points = new Point[3];
                            switch (Tri)
                            {
                                case 0:
                                    points = new Point[]
                                           {
                    new Point(x, y),
                    new Point(x, y + height),
                    new Point(x + width,y + height)
                                           };
                                    graphics.DrawPolygon(myPen, points);
                                    break;

                                case 1:
                                    points = new Point[]
                                           {
                    new Point(x, y),
                    new Point(x + width, y),
                    new Point(x + width / 2, y + height)
                                           };
                                    graphics.DrawPolygon(myPen, points);
                                    break;

                                case 2:
                                    points = new Point[]
                                           {
                    new Point(x, y + height),
                    new Point(x + width, y + height),
                    new Point(x + width / 2, y)
                                           };
                                    graphics.DrawPolygon(myPen, points);
                                    break;
                                case 3:
                                    points = new Point[]
                                           {
                    new Point(x, y),
                    new Point(x, y + height),
                    new Point(x + width, y + height / 2)
                                           };
                                    graphics.DrawPolygon(myPen, points);
                                    break;
                            }
                            break;

                        case 1: //shapeType = Rectangle
                            graphics.DrawRectangle(myPen, x, y, width, height);
                            break;

                        case 2: //shapeType = square
                            graphics.DrawRectangle(myPen, x, y, size, size);
                            break;

                        case 3:  //shapeType = Ellipse
                            graphics.DrawEllipse(myPen, x, y, width, height);
                            break;

                        case 4: //shapeType = circle
                            graphics.DrawEllipse(myPen, x, y, size, size);
                            break;
                    }
                }
            }
            return bitmap;
        }
    }
}
