using System.Drawing;
using System.Drawing.Imaging;
#pragma warning disable CA1416


namespace Processing
{
    class ImageProcessor
    {
        public static unsafe Bitmap ConvertTo24Bpp(Bitmap indexedBitmap)
        {
            int Width = indexedBitmap.Width, Height = indexedBitmap.Height;

            Bitmap newBitmap = new Bitmap(indexedBitmap.Width, indexedBitmap.Height, PixelFormat.Format24bppRgb);

            BitmapData indexedData = indexedBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, indexedBitmap.PixelFormat);
            BitmapData newData = newBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, newBitmap.PixelFormat);

            for (int y = 0; y < Height; y++)
            {
                byte* indexedRow = (byte*)indexedData.Scan0 + (y * indexedData.Stride);
                byte* NewRow = (byte*)newData.Scan0 + (y * newData.Stride);

                for (int x = 0; x < Width; x++)
                {

                    byte colorIndex = indexedRow[x];

                    ColorPalette palette = indexedBitmap.Palette;
                    Color color = palette.Entries[colorIndex];

                    NewRow[x * 3] = color.B;
                    NewRow[x * 3 + 1] = color.G;
                    NewRow[x * 3 + 2] = color.R;
                }
            }
            indexedBitmap.UnlockBits(indexedData);
            newBitmap.UnlockBits(newData);

            return newBitmap;
        }
        public static unsafe Bitmap ConvertToBinaryRGB(Bitmap ColoredImage, byte threshold)
        {

            if (ColoredImage.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new ArgumentException("the image should be (24bpp) format");
            }
            int Width = ColoredImage.Width, Height = ColoredImage.Height;

            Bitmap BinImage = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
            BitmapData BinData = BinImage.LockBits(new Rectangle(0, 0, Width, Height),
             ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            BitmapData ColoredData = ColoredImage.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            byte* Rgb_Ptr = (byte*)(void*)ColoredData.Scan0;
            byte* Gray_Ptr = (byte*)(void*)BinData.Scan0;

            int Rbg_nOffset = ColoredData.Stride - Width * 3;
            int Gray_nOffset = BinData.Stride - Width;

            byte red, green, blue;

            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    blue = Rgb_Ptr[0];
                    green = Rgb_Ptr[1];
                    red = Rgb_Ptr[2];

                    *Gray_Ptr = (byte)(.299 * red
                        + .587 * green
                        + .114 * blue);

                    byte binaryValue = (byte)(*Gray_Ptr < threshold ? 0x00 : 0xff);
                    *Gray_Ptr++ = binaryValue;

                    Rgb_Ptr += 3;
                }
                Rgb_Ptr += Rbg_nOffset;
                Gray_Ptr += Gray_nOffset;
            }

            ColoredImage.UnlockBits(ColoredData);
            BinImage.UnlockBits(BinData);

            return BinImage;

        }
        public static unsafe Bitmap ConvertToBinaryGray(Bitmap grayImage, byte threshold)
        {

            int Width = grayImage.Width, Height = grayImage.Height;

            Bitmap binaryImage = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);

            BitmapData grayData = grayImage.LockBits(new Rectangle(0, 0, Width, Height),
             ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            BitmapData binaryData = binaryImage.LockBits(new Rectangle(0, 0, Width, Height),
             ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            int grayStride = grayData.Stride;
            int binaryStride = binaryData.Stride;

            byte* grayPtr = (byte*)grayData.Scan0;
            byte* binaryPtr = (byte*)binaryData.Scan0;

            for (int y = 0; y < Height; y++)
            {
                byte* grayRow = (byte*)(grayPtr + y * grayStride);
                byte* binaryRow = (byte*)(binaryPtr + y * binaryStride);

                for (int x = 0; x < Width; x++)
                {
                    byte grayValue = grayRow[x];
                    byte binaryValue = (byte)(grayValue < threshold ? 0x00 : 0xff);
                    binaryRow[x] = binaryValue;
                }
            }

            grayImage.UnlockBits(grayData);
            binaryImage.UnlockBits(binaryData);

            return binaryImage;
        }
        public static unsafe Bitmap FloodFill(Bitmap shape, byte targetColor, byte replacementColor)
        {

            int Width = shape.Width, Height = shape.Height;


            BitmapData shapeData = shape.LockBits(new Rectangle(0, 0, Width, Height),
                 ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int Stride = shapeData.Stride;

            byte* ptr = (byte*)shapeData.Scan0;

            Point StartPoint = GetFillPoint(shapeData, targetColor, replacementColor);

            targetColor = ptr[StartPoint.Y * Stride + StartPoint.X];

            if (targetColor == replacementColor)
            {
                shape.UnlockBits(shapeData);
                return shape;
            }


            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(StartPoint);

            while (pixels.Count > 0)
            {
                Point NPoint = pixels.Pop();
                if (NPoint.X < Width && NPoint.X > 0 &&
                        NPoint.Y < Height && NPoint.Y > 0)
                {
                    if (ptr[NPoint.Y * Stride + NPoint.X] == targetColor)
                    {
                        ptr[NPoint.Y * Stride + NPoint.X] = replacementColor;
                        pixels.Push(new Point(NPoint.X - 1, NPoint.Y));
                        pixels.Push(new Point(NPoint.X + 1, NPoint.Y));
                        pixels.Push(new Point(NPoint.X, NPoint.Y - 1));
                        pixels.Push(new Point(NPoint.X, NPoint.Y + 1));
                    }
                }
            }
            shape.UnlockBits(shapeData);
            return shape;
        }
        private static unsafe Point GetFillPoint(BitmapData Pixles, byte targetColor, byte replacementColor)
        {
            int Width = Pixles.Width, Height = Pixles.Height;

            int Stride = Pixles.Stride;

            byte* ptr = (byte*)Pixles.Scan0;

            Point point = new Point();
            int startX = 0;
            int endX = 0;


            int Ys = (Height / 2);

            //X

            byte * Row = (byte*)ptr + Ys * Stride;
            for (int x = 0; x < Width; x++)
            {
                if (Row[x] == replacementColor)
                {
                    startX = x;
                    break;
                }
            }
            for (int x = Width - 1; x >= 0; --x)
            {
                if (Row[x] == replacementColor)
                {
                    endX = x;
                    break;
                }
            }
            point.X = (endX + startX) / 2;
            point.Y = Ys;
            return point;

        }
        public static void InsertShapeType(ref Bitmap InputImage, string type, PointF pos, int TextSize = 12)
        {
            using (Graphics graphics = Graphics.FromImage(InputImage))
            {

                Font font = new Font("Arial", TextSize, FontStyle.Regular);

                SizeF textSize = graphics.MeasureString(type.ToString(), font);
                SizeF rectSize = new SizeF(textSize.Width, textSize.Height);
                
                graphics.DrawString(type, font, Brushes.Red, pos);


                Brush brush = new SolidBrush(Color.Blue);

            }
        }
    }
}