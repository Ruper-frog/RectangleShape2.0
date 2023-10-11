using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace RectangleShape2._0
{
    internal class Program
    {
        static string imagePath = @"C:\Users\ruper\OneDrive\שולחן העבודה\Name list images\2.jpg";
        static string outputImagePath = @"C:\Users\ruper\OneDrive\שולחן העבודה\Name list images\New folder\output.jpg";

        static (int X, int Y)[] Corners;

        static int ColorDistLimit = 5000;
        static Color CornerColor = Color.FromArgb(60, 50, 51);
        static void Main(string[] args)
        {
            Mat img = CvInvoke.Imread(imagePath, ImreadModes.Color);

            if (img == null || img.IsEmpty)
            {
                Console.WriteLine("File could not be read, check if it exists.");
                return;
            }

            // Check for EXIF orientation tag and rotate the image if needed
            //int rotationalCase = RotateImageIfNecessaryForMat(ref img, imagePath);

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

            // Print the coordinates of the four cornersPoints and their locations
            PointF[] cornersPoints = largestRotatedRect.GetVertices();

            //// Save the resulting image to the specified output path
            //CvInvoke.Imwrite(outputImagePath, img);

            ////Console.WriteLine("Processed image saved to: " + outputImagePath);

            //// Open the image using the default image viewer
            //Process.Start(outputImagePath);


            Corners = new (int X, int Y)[4];

            for (int i = 0; i < cornersPoints.Length; i++)
            {
                string currentString = cornersPoints[i].ToString();

                Corners[i] = ((int)Math.Round(Double.Parse(currentString.Substring(currentString.IndexOf('=') + 1, currentString.IndexOf(',') - currentString.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(currentString.Substring(currentString.IndexOf('=', currentString.IndexOf('=') + 1) + 1, currentString.IndexOf('}') - currentString.IndexOf('=', currentString.IndexOf('=') + 1) - 1))));
            }

            SelectionSortBy_Y(Corners);

            (int X, int Y)[] Top = { Corners[0], Corners[1] };
            SelectionSortBy_X(Top);

            (int X, int Y)[] Bottom = { Corners[2], Corners[3] };
            SelectionSortBy_X(Bottom);

            (int X, int Y) corner, corner1;

            /*switch (rotationalCase)
            {
                case 1:
                    {
                        corner = Top[1];
                        Top[1] = Top[0];
                        corner1 = Bottom[1];
                        Bottom[1] = corner;
                        corner = Bottom[0];
                        Bottom[0] = corner1;
                        Top[0] = corner;
                    }
                    break;
                case 2:
                    {
                        corner = Top[0];
                        Top[0] = Top[1];
                        corner1 = Bottom[0];
                        Bottom[0] = corner;
                        corner = Bottom[1];
                        Bottom[1] = corner1;
                        Top[1] = corner;
                    }
                    break;
            }*/

            Corners = Top.Concat(Bottom).ToArray();

            for (int i = 0; i < Top.Length; i++)
                Console.WriteLine($"{Top[i].X} {Top[i].Y}");

            for (int i = 0; i < Bottom.Length; i++)
                Console.WriteLine($"{Bottom[i].X} {Bottom[i].Y}");

            /*string strTopLeft = cornersPoints[0].ToString();
            string strTopRight = cornersPoints[1].ToString();
            string strBottomLeft = cornersPoints[2].ToString();
            string strBottomRight = cornersPoints[3].ToString();

            string CurrnetStr = strTopLeft;
            //(int Y, int Y) FirstCorner = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));

            //CurrnetStr = strTopRight;
            //(int Y, int Y) SecondCorner = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));

            //CurrnetStr = strBottomLeft;
            //(int Y, int Y) ThirdCorner = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));

            //CurrnetStr = strBottomRight;
            //(int Y, int Y) FourthCorner = ((int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=') + 1, CurrnetStr.IndexOf(',') - CurrnetStr.IndexOf('=') - 1))), (int)Math.Round(Double.Parse(CurrnetStr.Substring(CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) + 1, CurrnetStr.IndexOf('}') - CurrnetStr.IndexOf('=', CurrnetStr.IndexOf('=') + 1) - 1))));


            int[,] coordinatesArray = StoreCoordinatesInArray(FirstCorner, SecondCorner, ThirdCorner, FourthCorner);*/


            (int X, int Y) TopLeft = (Top[0].X, Top[0].Y);
            (int X, int Y) TopRight = (Top[1].X, Top[1].Y);
            (int X, int Y) BottomLeft = (Bottom[0].X, Bottom[0].Y);
            (int X, int Y) BottomRight = (Bottom[1].X, Bottom[1].Y);


            int x = TopLeft.X; // Y-coordinate of the top-left corner
            int y = TopLeft.Y; // Y-coordinate of the top-left corner
            int width = TopRight.X - TopLeft.X; // Width of the cropped region
            int height = BottomLeft.Y - TopLeft.Y; // Height of the cropped region

            Image image = Image.FromFile(imagePath);

            RotateImageIfNecessaryForMatForImage(ref image, imagePath);

            Bitmap picture = new Bitmap(image, image.Width, image.Height);

            Bitmap croppedImage = CropImage(picture, x, y, width, height);

            Corners[0] = (0, 0);
            Corners[1] = (croppedImage.Width - 1, 0);
            Corners[2] = (0, croppedImage.Height - 1);
            Corners[3] = (croppedImage.Width - 1, croppedImage.Height - 1);

            FindObjectsCorners(ref croppedImage);

            // Save the cropped image
            croppedImage.Save(outputImagePath, ImageFormat.Jpeg);

            // Dispose of the image objects to free up resources
            croppedImage.Dispose();
            img.Dispose();

            Console.WriteLine("Image cropped and saved successfully.");

            Process.Start(outputImagePath);
        }
        static void FindObjectsCorners(ref Bitmap image)
        {
            int whichCornerTouches = -1;

            while (whichCornerTouches == -1)
            {
                for (int i = 0; i < Corners.Length; i++)
                {
                    if (ColorDist(image.GetPixel(Corners[i].X, Corners[i].Y), CornerColor))
                    {
                        whichCornerTouches = i;
                        break;
                    }
                }
                if (whichCornerTouches == -1)
                {
                    Corners[0] = (Corners[0].X + 1, Corners[0].Y + 1);
                    Corners[1] = (Corners[1].X - 1, Corners[1].Y + 1);
                    Corners[2] = (Corners[2].X + 1, Corners[2].Y - 1);
                    Corners[3] = (Corners[3].X - 1, Corners[3].Y - 1);
                }
            }
            /*for (int i = 0; i < image.Width - Corners[whichCornerTouches].X; i++)
                image.SetPixel(Corners[whichCornerTouches].X + i, Corners[whichCornerTouches].Y, Color.Red);*/

            Console.WriteLine(Corners[whichCornerTouches]);

            
        }
        public static void LinearLine(Point startPoint, Point endPoint, Color lineColor)
        {
            // Calculate the slope (incline) between two points
            float slope = (float)(endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X);

            // Generate the mathematical equation of the line (y = mx + b)
            float intercept = startPoint.Y - slope * startPoint.X;

            // Iterate through points along the X-axis
            for (int x = startPoint.X; x <= endPoint.X; x++)
            {
                // Calculate the corresponding Y-coordinate using the equation
                int y = (int)(slope * x + intercept);

                // Set the pixel color to draw the line
                //Picture.SetPixel(x, y, lineColor);
            }
        }
        static int RotateImageIfNecessaryForMat(ref Mat img, string imagePath)
        {
            int rotationalCase = -1;
            using (Image image = Image.FromFile(imagePath))
            {
                // Check for the orientation tag in the EXIF data
                foreach (PropertyItem propertyItem in image.PropertyItems)
                {
                    if (propertyItem.Id == 0x112)
                    {
                        // Get the orientation value
                        int orientation = BitConverter.ToUInt16(propertyItem.Value, 0);

                        // Rotate the image based on the orientation tag
                        switch (orientation)
                        {
                            case 3:
                                {
                                    CvInvoke.Rotate(img, img, RotateFlags.Rotate180);
                                    rotationalCase = 0;
                                }
                                break;
                            case 6:
                                {
                                    CvInvoke.Rotate(img, img, RotateFlags.Rotate90CounterClockwise);
                                    rotationalCase = 1;
                                }
                                break;
                            case 8:
                                {
                                    CvInvoke.Rotate(img, img, RotateFlags.Rotate90Clockwise);
                                    rotationalCase = 2;
                                }
                                break;
                                // Add more cases as needed for other orientations
                        }
                    }
                }
            }
            return rotationalCase;
        }
        static void RotateImageIfNecessaryForMatForImage(ref Image photo, string imagePath)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                // Check for the orientation tag in the EXIF data
                foreach (PropertyItem propertyItem in image.PropertyItems)
                {
                    if (propertyItem.Id == 0x112)
                    {
                        // Get the orientation value
                        int orientation = BitConverter.ToUInt16(propertyItem.Value, 0);

                        // Rotate the image based on the orientation tag
                        switch (orientation)
                        {
                            case 3:
                                photo.RotateFlip(RotateFlipType.Rotate180FlipNone);// Rotate180
                                break;
                            case 6:
                                photo.RotateFlip(RotateFlipType.Rotate90FlipNone);// Rotate90CounterClockwise
                                break;
                            case 8:
                                photo.RotateFlip(RotateFlipType.Rotate270FlipNone);// Rotat90Clockwise
                                break;
                                // Add more cases as needed for other orientations
                        }
                    }
                }
            }
        }
        public static void SelectionSortBy_Y((int X, int Y)[] arr)
        {
            int n = arr.Length;

            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;

                // Find the index of the minimum element in the remaining unsorted portion
                for (int j = i + 1; j < n; j++)
                {
                    if (arr[j].Y < arr[minIndex].Y)
                    {
                        minIndex = j;
                    }
                }

                // Swap the found minimum element with the element at index Y
                (int X, int Y) temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;
            }
        }
        public static void SelectionSortBy_X((int X, int Y)[] arr)
        {
            int n = arr.Length;

            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;

                // Find the index of the minimum element in the remaining unsorted portion
                for (int j = i + 1; j < n; j++)
                {
                    if (arr[j].X < arr[minIndex].X)
                    {
                        minIndex = j;
                    }
                }

                // Swap the found minimum element with the element at index Y
                (int X, int Y) temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;
            }
        }
        // Function to crop an image based on an array of PointF coordinates
        static Bitmap CropImage(Bitmap image, int x, int y, int width, int height)
        {
            // Load the source image
            Bitmap sourceImage = new Bitmap(image);

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
        static bool ColorDist(Color c1, Color c2)
        {
            double rDiff = c1.R - c2.R;
            double gDiff = c1.G - c2.G;
            double bDiff = c1.B - c2.B;

            return Math.Pow(rDiff, 2) + Math.Pow(gDiff, 2) + Math.Pow(bDiff, 2) <= ColorDistLimit;
        }
    }
}
