using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace RectangleShape2._0
{
    internal class Program
    {
        static string imagePath = @"C:\Users\USER\OneDrive\שולחן העבודה\Name list images\3.jpg";
        static string outputImagePath = @"C:\Users\USER\OneDrive\שולחן העבודה\Name list images\New folder\output.jpg";
        static void Main(string[] args)
        {

            Mat img = CvInvoke.Imread(imagePath, ImreadModes.Color);

            if (img == null || img.IsEmpty)
            {
                Console.WriteLine("File could not be read, check if it exists.");
                return;
            }

            // Convert the image to grayscale
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(img, grayImage, ColorConversion.Bgr2Gray);

            // Apply Canny edge detection
            Mat edges = new Mat();
            CvInvoke.Canny(grayImage, edges, 100, 200);

            // Find contours in the edge-detected image
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(edges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            // Initialize variables to keep track of the largest rectangle
            double largestRectArea = 0;
            RotatedRect largestRotatedRect = new RotatedRect();

            // Iterate through the contours to find the largest rectangle
            for (int i = 0; i < contours.Size; i++)
            {
                RotatedRect minAreaRect = CvInvoke.MinAreaRect(contours[i]);
                Rectangle rect = minAreaRect.MinAreaRect();

                double rectArea = rect.Width * rect.Height;

                if (rectArea > largestRectArea)
                {
                    largestRectArea = rectArea;
                    largestRotatedRect = minAreaRect;
                }
            }

            // Draw the largest rectangle on the original image
            CvInvoke.Rectangle(img, largestRotatedRect.MinAreaRect(), new MCvScalar(0, 0, 255), 2);

            // Print the coordinates of the four corners and their locations
            PointF[] corners = largestRotatedRect.GetVertices();
            Console.WriteLine("Coordinates of the four corners:");
            Console.WriteLine("Top Left: " + corners[0].ToString());
            Console.WriteLine("Top Right: " + corners[1].ToString());
            Console.WriteLine("Bottom Right: " + corners[2].ToString());
            Console.WriteLine("Bottom Left: " + corners[3].ToString());

            //// Save the resulting image to the specified output path
            //CvInvoke.Imwrite(outputImagePath, img);

            ////Console.WriteLine("Processed image saved to: " + outputImagePath);

            //// Open the image using the default image viewer
            //Process.Start(outputImagePath);


            string strTopLeft = corners[2].ToString();
            string strTopRight = corners[3].ToString();
            string strBottomLeft = corners[1].ToString();
            string strBottomRight = corners[0].ToString();

            string CurrnetStr = strTopLeft;
            (int X, int Y) TopLeft = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));

            CurrnetStr = strTopRight;
            (int X, int Y) TopRight = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));

            CurrnetStr = strBottomLeft;
            (int X, int Y) BottomLeft = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));

            CurrnetStr = strBottomRight;
            (int X, int Y) BottomRight = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));


            int x = TopLeft.X; // X-coordinate of the top-left corner
            int y = TopLeft.Y; // Y-coordinate of the top-left corner
            int width = TopRight.X - TopLeft.X; // Width of the cropped region
            int height = BottomLeft.Y - TopLeft.Y; // Height of the cropped region

            Bitmap croppedImage = CropImage(imagePath, x, y, width, height);

            // Save the cropped image
            croppedImage.Save(outputImagePath, ImageFormat.Jpeg);

            // Dispose of the image objects to free up resources
            croppedImage.Dispose();

            Console.WriteLine("Image cropped and saved successfully.");

            Process.Start(outputImagePath);
        }
        // Function to crop an image based on an array of PointF coordinates
        static Bitmap CropImage(string imagePath, int x, int y, int width, int height)
        {
            // Load the source image
            Bitmap sourceImage = new Bitmap(imagePath);

            // Create a rectangle specifying the cropping region
            Rectangle cropRegion = new Rectangle(x, y, width, height);

            // Create a new bitmap with the specified dimensions
            Bitmap croppedImage = new Bitmap(cropRegion.Width, cropRegion.Height);

            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                // Draw the specified region of the source image onto the new bitmap
                g.DrawImage(sourceImage, new Rectangle(0, 0, cropRegion.Width, cropRegion.Height), cropRegion, GraphicsUnit.Pixel);
            }

            // Dispose of the source image to free up resources
            sourceImage.Dispose();

            return croppedImage;
        }
    }
}
