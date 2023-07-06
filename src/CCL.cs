using System.Drawing;
using System.Drawing.Imaging;

#pragma warning disable CA1416



namespace CCL
{

    class ccl
    {
        public unsafe static List<List<Point>> GetShapes(Bitmap image)
        {   
            List<List<Point>> Shapes = new List<List<Point>>();

            int Height = image.Height, Width = image.Width;

            BitmapData ImageData = image.LockBits(new Rectangle(0, 0, Width, Height),
             ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);


            byte* Scan0 = (byte*)ImageData.Scan0;

            bool[,] visited = new bool[Width, Height];

            int stride = ImageData.Stride;


            for (int y = 0; y < Height; y++)
            {
                byte* pixelPtr = (byte*)(Scan0 + y * stride);

                for (int x = 0; x < Width; x++)
                {

                    if (!visited[x, y] && pixelPtr[x] == 0x00)
                    {
                        visited[x, y] = true;


                        List<Point> Shape = new List<Point>();
                        Shape.Add(new Point(x, y));

                        Queue<Point> queue = new Queue<Point>();
                        queue.Enqueue(new Point(x, y));

                        while (queue.Count > 0)
                        {
                            Point CurrentPixle = queue.Dequeue();
                            for (int i = -1; i <= 1; i++)
                            {
                                int Xs = CurrentPixle.X + i;

                                if (Xs < 0 || Xs >= Width)
                                    continue;

                                for (int j = -1; j <= 1; j++)
                                {
                                    int Ys = CurrentPixle.Y + j;
                                    
                                    if ((i == 0 && j == 0) || Ys < 0 || Ys >= Height)
                                    {
                                        continue;
                                    }

                                    int Row = Ys * stride;
                                    
                                    byte* adjacentPixelPtr = (byte*)(Scan0 + Row + Xs);

                                    if (!visited[Xs, Ys] && *adjacentPixelPtr == 0x00)
                                    {
                                        visited[Xs, Ys] = true;
                                        queue.Enqueue(new Point(Xs, Ys));
                                        Shape.Add(new Point(Xs, Ys));
                                    }
                                }
                            }
                        }
                        Shapes.Add(Shape);
                    }
                }
            }

            image.UnlockBits(ImageData);
            return Shapes;
        }

    }
}