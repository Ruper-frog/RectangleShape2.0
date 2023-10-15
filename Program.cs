using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace RectangleShape2._0
{
    internal class Program
    {
        //PC
        static string ImagePath = @"C:\Users\USER\OneDrive\שולחן העבודה\Name list images\3.jpg";
        static string OutputImagePath = @"C:\Users\USER\OneDrive\שולחן העבודה\Name list images\New folder\" + GetFileName(ImagePath) + ".jpg";
        static string PerspectiveTransformationPath = @"C:\Users\USER\OneDrive\שולחן העבודה\Name list images\New folder\New folder\" + GetFileName(ImagePath) + ".jpg";
        static string PerspectiveTransformationWithGridPath = @"C:\Users\USER\OneDrive\שולחן העבודה\Name list images\New folder\New folder\New folder\" + GetFileName(ImagePath) + ".jpg";

        //Leptop
        //static string ImagePath = @"C:\Users\ruper\OneDrive\שולחן העבודה\Name list images\2.jpg";
        //static string OutputImagePath = @"C:\Users\ruper\OneDrive\שולחן העבודה\Name list images\New folder\output.jpg";

        static bool[,] PhoneNetwork;
        static int NumPerPocketColumn;
        static int NumPerPocketRow;

        static (int X, int Y)[] Corners = new (int X, int Y)[4];
        static (int X, int Y)[] CroppedCorners = new (int X, int Y)[4];

        static (double Width, double Height) PocketTable = (65.9892, 93.4974);
        static (double Width, double Height) SinglePocket = (10.0076, 12.0142);
        static double ImageToRealRatio;

        static List<bool> Line = new List<bool>();
        static int LineSuccessRates = 70;

        static int ColorDistLimit = 21;
        static Color CornerColor = Color.FromArgb(60, 50, 51);
        static Color NumberColor = Color.FromArgb(233, 230, 237);
        static Color BackgroundPocketColor = Color.FromArgb(236, 186, 27);
        static void Main(string[] args)
        {
            //TODO: align the frame: Done
            //TODO: make sure that the if the corner of the object is slightly above the slope try to shorter the line or lower the line from a certain point
            //      you maybe can look at all the other lines that been search up and see where the main problem is

            Mat img = CvInvoke.Imread(ImagePath, ImreadModes.Color);

            if (img == null || img.IsEmpty)
            {
                Console.WriteLine("File could not be read, check if it exists.");
                return;
            }

            // Check for EXIF orientation tag and rotate the image if needed
            //int rotationalCase = RotateImageIfNecessaryForMat(ref img, ImagePath);

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
            //CvInvoke.Imwrite(OutputImagePath, img);

            ////Console.WriteLine("Processed image saved to: " + OutputImagePath);

            //// Open the image using the default image viewer
            //Process.Start(OutputImagePath);


            Corners = new (int X, int Y)[4];

            for (int i = 0; i < cornersPoints.Length; i++)
            {
                string currentString = cornersPoints[i].ToString();

                Corners[i] = ((int)Math.Floor(Double.Parse(currentString.Substring(currentString.IndexOf('=') + 1, currentString.IndexOf(',') - currentString.IndexOf('=') - 1))), (int)Math.Floor(Double.Parse(currentString.Substring(currentString.IndexOf('=', currentString.IndexOf('=') + 1) + 1, currentString.IndexOf('}') - currentString.IndexOf('=', currentString.IndexOf('=') + 1) - 1))));
            }

            SelectionSortBy_Y(Corners);

            (int X, int Y)[] Top = { Corners[0], Corners[1] };
            SelectionSortBy_X(Top);

            (int X, int Y)[] Bottom = { Corners[2], Corners[3] };
            SelectionSortBy_X(Bottom);

            Corners = Top.Concat(Bottom).ToArray();

            AlignRectangleCorners();

            /*for (int i = 0; i < Corners.Length; i++)
            //    Console.WriteLine($"{Corners[i].X} {Corners[i].Y}");
            //Console.WriteLine();*/

            int x = Corners[0].X; // Y-coordinate of the top-left corner
            int y = Corners[0].Y; // Y-coordinate of the top-left corner
            int width = Corners[1].X - Corners[0].X; // Width of the cropped region
            int height = Corners[2].Y - Corners[0].Y; // Height of the cropped region

            Image image = Image.FromFile(ImagePath);

            RotateImageIfNecessaryForMatForImage(ref image, ImagePath);

            Bitmap picture = new Bitmap(image, image.Width, image.Height);
            image.Dispose();

            Bitmap croppedImage = CropImage(picture, x, y, width, height);

            Corners[0] = (0, 0);
            Corners[1] = (croppedImage.Width - 1, 0);
            Corners[2] = (0, croppedImage.Height - 1);
            Corners[3] = (croppedImage.Width - 1, croppedImage.Height - 1);

            /*foreach ((int X, int Y) i in Corners)
            //    Console.WriteLine(i);*/

            FindObjectsCorners(ref croppedImage);


            for (int i = 0; i < Corners[0].X; i++)// Draw a line from the edge of the image to the edge of the object
                croppedImage.SetPixel(i, Corners[0].Y, Color.Red);

            for (int i = 0; i < croppedImage.Width - Corners[1].X; i++)// Draw a line from the edge of the image to the edge of the object
                croppedImage.SetPixel(Corners[1].X + i, Corners[1].Y, Color.Red);

            for (int i = 0; i < Corners[2].X; i++)// Draw a line from the edge of the image to the edge of the object
                croppedImage.SetPixel(i, Corners[2].Y, Color.Red);

            for (int i = 0; i < croppedImage.Width - Corners[3].X; i++)// Draw a line from the edge of the image to the edge of the object
                croppedImage.SetPixel(Corners[3].X + i, Corners[3].Y, Color.Red);


            // Save the cropped image
            croppedImage.Save(OutputImagePath, ImageFormat.Jpeg);

            Mat transformedImage = PerformPerspectiveTransformation(OutputImagePath, croppedImage, Corners);

            CvInvoke.Imwrite(PerspectiveTransformationPath, transformedImage);

            // Dispose of the image objects to free up resources
            img.Dispose();
            croppedImage.Dispose();
            transformedImage.Dispose();

            Bitmap imageWithGrids = new Bitmap(PerspectiveTransformationPath);

            ImageToRealRatio = imageWithGrids.Width / PocketTable.Width;
            (int Width, int Height) SinglePocketImageSize = ((int)(ImageToRealRatio * SinglePocket.Width), (int)(ImageToRealRatio * SinglePocket.Height));

            NumPerPocketColumn = imageWithGrids.Width / SinglePocketImageSize.Width;
            NumPerPocketRow = CountNumber(ref imageWithGrids, SinglePocketImageSize);

            PhoneNetwork = CountPhones(ref imageWithGrids, SinglePocketImageSize);

            for (int i = 0; i < PhoneNetwork.GetLength(0); i++)
            {
                for (int j = 0; j < PhoneNetwork.GetLength(1); j++)
                    Console.Write(PhoneNetwork[i, j] ? "[1]" : "[0]");
                Console.WriteLine();
            }


            /*int jumps = imageWithGrids.Width / 12;
            //for (int i = jumps; i < imageWithGrids.Width; i += jumps * 2)
            //{
            //    for (int j = 0; j < imageWithGrids.Height; j++)
            //    {
            //        imageWithGrids.SetPixel(i, j, Color.Red);
            //    }
            //}*/

            imageWithGrids.Save(PerspectiveTransformationWithGridPath, ImageFormat.Jpeg);
            imageWithGrids.Dispose();

            Console.WriteLine("Image cropped and saved successfully.");

            //Process.Start(OutputImagePath);
            //Process.Start(PerspectiveTransformationPath);
            Process.Start(PerspectiveTransformationWithGridPath);
        }
        static bool[,] CountPhones(ref Bitmap image, (int Width, int Height) singlePocketImageSize)
        {
            PhoneNetwork = new bool[NumPerPocketRow, NumPerPocketColumn];
            List<int> Ys = new List<int>();
            List<bool> Colors = new List<bool>();

            int count = 0, jumps = image.Width / 12;
            bool sequence = false;

            int y = 0, x, column = -1, row;
            for (x = jumps; column != 5; x += jumps * 2)
            {
                column++;

                for (y = 0; y < image.Height; y++)
                {
                    if (ColorDist(image.GetPixel(x, y), BackgroundPocketColor, 50))
                        sequence = true;

                    else
                    {
                        if (Ys.Count() > 2)
                        {
                            count++;
                            y += singlePocketImageSize.Height;
                            row = Ys.Last() / (image.Height / NumPerPocketRow);
                            PhoneNetwork[row, column] = sequence;

                            /*foreach (int Y in Ys)
                            //{
                                //for (int T = 0; T < imageWithGrids.Width; T++)// Draw a column from the edge of the imageWithGrids to the edge of the object
                                //image.SetPixel(x, Y, Color.Red);
                            //}*/

                            Ys.Clear();
                        }
                        sequence = false;
                    }

                    if (sequence)
                        Ys.Add(y);
                }
            }
            Console.WriteLine("Number of pockets: " + count);
            return PhoneNetwork;
        }
        static int CountNumber(ref Bitmap image, (int Width, int Height) SinglePocketImageSize)
        {
            List<int> Ys = new List<int>();

            int count = 0, jumps = SinglePocketImageSize.Height;
            bool foundInCell = true, sequence = false;

            int y, x, NumOfFoundCells;
            for (y = image.Height - 1; y >= 80; y -= 5)// with the new algorithm might be able to make y = image.Height
            {
                NumOfFoundCells = 1;
                foundInCell = true;

                for (int i = 0; foundInCell && NumOfFoundCells != 6; i++)
                {
                    foundInCell = false;

                    for (x = image.Width / 12 * NumOfFoundCells; x < image.Width / 12 * (NumOfFoundCells + 1) && x < image.Width - image.Width / 12; x++)
                    {
                        if (ColorDist(image.GetPixel(x, y), NumberColor, 25))
                        {
                            NumOfFoundCells++;
                            foundInCell = true;
                            break;
                        }
                    }
                }
                if (foundInCell && NumOfFoundCells == 6)
                    sequence = true;
                else
                {
                    if (Ys.Count() != 0)
                    {
                        count++;
                        //jumps = Ys.First() - Ys.Last();
                        y -= jumps;

                        /*foreach (int Y in Ys)
                        //{
                        //    for (int i = 0; i < image.Width; i++)// Draw a line from the edge of the image to the edge of the object
                        //        image.SetPixel(i, Y, Color.Red);
                        //}*/

                        Ys.Clear();
                    }
                    sequence = false;
                }

                if (sequence)
                    Ys.Add(y);
            }
            return count;
        }
        static void FindObjectsCorners(ref Bitmap image)
        {
            int whichCornerTouchesFirst = -1, secondCornerNumber;

            while (whichCornerTouchesFirst == -1)
            {
                for (int i = 0; i < Corners.Length; i++)
                {
                    if (ColorDist(image.GetPixel(Corners[i].X, Corners[i].Y), CornerColor))
                    {
                        whichCornerTouchesFirst = i;
                        break;
                    }
                }
                if (whichCornerTouchesFirst == -1)
                {
                    Corners[0] = (Corners[0].X + 1, Corners[0].Y + 1);
                    Corners[1] = (Corners[1].X - 1, Corners[1].Y + 1);
                    Corners[2] = (Corners[2].X + 1, Corners[2].Y - 1);
                    Corners[3] = (Corners[3].X - 1, Corners[3].Y - 1);
                }
            }
            Array.Copy(Corners, CroppedCorners, Corners.Length);

            //Console.WriteLine("\n" + Corners[whichCornerTouchesFirst] + "\n");

            bool foundCorner = false;
            (int X, int Y) secondPoint, truePoint;
            secondCornerNumber = whichCornerTouchesFirst - 2 >= 0 ? whichCornerTouchesFirst - 2 : whichCornerTouchesFirst + 2;

            for (int i = 0; !foundCorner; i++)
            {// problem cuz of the location of the first dot
                secondPoint = Corners[secondCornerNumber];

                truePoint = VerticalLine(Corners[whichCornerTouchesFirst], (secondPoint.X > image.Width / 2 ? secondPoint.X -= i : secondPoint.X += i, secondPoint.Y), image, whichCornerTouchesFirst);

                if (CountTrueValues(Line) >= LineSuccessRates * Line.Count / 100)
                {
                    foundCorner = true;
                    Corners[secondCornerNumber] = truePoint;
                }
                Line.Clear();
            }
            foundCorner = false;
            whichCornerTouchesFirst = secondCornerNumber;

            if (whichCornerTouchesFirst == 0)
                secondCornerNumber = 1;

            else if (whichCornerTouchesFirst == 3)
                secondCornerNumber = 2;

            else
                secondCornerNumber = Corners[whichCornerTouchesFirst - 1].Y == CroppedCorners[whichCornerTouchesFirst].Y ? whichCornerTouchesFirst - 1 : whichCornerTouchesFirst + 1;

            for (int i = 0; !foundCorner; i++)
            {
                secondPoint = Corners[secondCornerNumber];

                truePoint = HorizontalLine(Corners[whichCornerTouchesFirst], (secondPoint.X, secondPoint.Y > image.Height / 2 ? secondPoint.Y -= i : secondPoint.Y += i), image, whichCornerTouchesFirst);

                if (CountTrueValues(Line) >= LineSuccessRates * Line.Count / 100)
                {
                    foundCorner = true;
                    Corners[secondCornerNumber] = truePoint;
                }
                Line.Clear();
            }
            foundCorner = false;
            whichCornerTouchesFirst = whichCornerTouchesFirst - 2 >= 0 ? whichCornerTouchesFirst - 2 : whichCornerTouchesFirst + 2;

            if (whichCornerTouchesFirst == 0)
                secondCornerNumber = 1;

            else if (whichCornerTouchesFirst == 3)
                secondCornerNumber = 2;

            else
                secondCornerNumber = Corners[whichCornerTouchesFirst - 1].Y == CroppedCorners[whichCornerTouchesFirst].Y ? whichCornerTouchesFirst - 1 : whichCornerTouchesFirst + 1;

            for (int i = 0; !foundCorner; i++)
            {
                secondPoint = Corners[secondCornerNumber];

                truePoint = HorizontalLine(Corners[whichCornerTouchesFirst], (secondPoint.X, secondPoint.Y > image.Height / 2 ? secondPoint.Y -= i : secondPoint.Y += i), image, whichCornerTouchesFirst);

                if (CountTrueValues(Line) >= LineSuccessRates * Line.Count / 100)
                {
                    foundCorner = true;
                    Corners[secondCornerNumber] = truePoint;
                }
                Line.Clear();
            }

            /*foreach ((int X, int Y) i in Corners)
            //    Console.WriteLine(i);*/
        }
        public static (int X, int Y) HorizontalLine((int X, int Y) point1, (int X, int Y) point2, Bitmap image, int whichCornerTouchesFirst)
        {
            // Determine the start point and end point based on X-coordinates
            (int X, int Y) startPoint, endPoint;

            if (point1.X < point2.X)
            {
                startPoint = point1;
                endPoint = point2;
            }
            else
            {
                startPoint = point2;
                endPoint = point1;
            }
            bool startPointIsFirstCorner = Corners[whichCornerTouchesFirst] == startPoint;

            // Calculate the slope (incline) between the two points
            float slope = (float)(endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X);

            // Generate the mathematical equation of the line (y = mx + b)
            float intercept = startPoint.Y - slope * startPoint.X;

            // Iterate through points along the X-axis
            int y, x;
            for (x = startPoint.X; x <= endPoint.X; x++)
            {
                // Calculate the corresponding Y-coordinate using the equation
                y = (int)(slope * x + intercept);

                Line.Add(ColorDist(image.GetPixel(x, y), CornerColor));

                // Set the pixel color to draw the line
                //image.SetPixel(x, y, Color.Red);
            }
            if (startPointIsFirstCorner)
                x -= Line.Count() - FindLastTrueIndex(Line);
            else
            {
                x = startPoint.X;
                x += FindFirstTrueIndex(Line);
            }
            y = (int)(slope * x + intercept);

            return (x, y);
        }
        public static (int X, int Y) VerticalLine((int X, int Y) point1, (int X, int Y) point2, Bitmap image, int whichCornerTouchesFirst)
        {
            // Determine the start point and end point based on Y-coordinates
            (int X, int Y) startPoint, endPoint;

            if (point1.Y < point2.Y)
            {
                startPoint = point1;
                endPoint = point2;
            }
            else
            {
                startPoint = point2;
                endPoint = point1;
            }
            bool startPointIsFirstCorner = Corners[whichCornerTouchesFirst] == startPoint;

            // Calculate the slope (incline) between the two points
            float slope = (float)(endPoint.X - startPoint.X) / (endPoint.Y - startPoint.Y);

            // Generate the mathematical equation of the line (x = my + b)
            float intercept = startPoint.X - slope * startPoint.Y;

            // Iterate through points along the Y-axis
            int y, x;
            for (y = startPoint.Y; y <= endPoint.Y; y++)
            {
                // Calculate the corresponding X-coordinate using the equation
                x = (int)(slope * y + intercept);

                Line.Add(ColorDist(image.GetPixel(x, y), CornerColor));

                // Set the pixel color to draw the line
                //image.SetPixel(x, y, Color.Red);
            }
            if (startPointIsFirstCorner)
                y -= Line.Count() - FindLastTrueIndex(Line);
            else
            {
                y = startPoint.Y;
                y += FindFirstTrueIndex(Line);
            }
            x = (int)(slope * y + intercept);

            return (x, y);
        }
        public static int FindLastTrueIndex(List<bool> list)
        {
            for (int i = list.Count() - 1; i >= 0; i--)
            {
                if (list[i])
                    return i;
            }

            // If no 'true' value is found, you can return a default value or throw an exception.
            // For example, return -1 to indicate that there are no 'true' values in the array.
            return -1;
        }
        public static int FindFirstTrueIndex(List<bool> list)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                if (list[i])
                    return i;
            }

            // If no 'true' value is found, you can return a default value or throw an exception.
            // For example, return -1 to indicate that there are no 'true' values in the array.
            return -1;
        }
        static int CountTrueValues(List<bool> boolList)
        {
            // Use the Count method with a lambda expression to count true values
            int count = boolList.Count(b => b == true); // You can also use b == true or simply b

            return count;
        }

        static Mat PerformPerspectiveTransformation(string imageFilePath, Bitmap image, (int X, int Y)[] points)
        {
            using (Mat img = CvInvoke.Imread(imageFilePath))
            {
                if (img != null && points.Length == 4)
                {
                    PointF[] pts1 = new PointF[4];

                    for (int i = 0; i < 4; i++)
                    {
                        pts1[i] = new PointF(points[i].X, points[i].Y);
                    }

                    PointF[] pts2 = new PointF[]
                    {
                    new PointF(0, 0),
                    new PointF(image.Width - 1, 0),
                    new PointF(0, image.Height - 1),
                    new PointF(image.Width - 1, image.Height - 1)
                    };

                    using (Mat M = CvInvoke.GetPerspectiveTransform(pts1, pts2))
                    {
                        Mat dst = new Mat();
                        CvInvoke.WarpPerspective(img, dst, M, new Size(image.Width, image.Height));

                        return dst;
                    }
                }
                else
                {
                    return null;
                }
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
        public static void AlignRectangleCorners()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            // Find the minimum and maximum X and Y coordinates among the corners
            foreach ((int X, int Y) corner in Corners)
            {
                minX = Math.Min(minX, corner.X);
                minY = Math.Min(minY, corner.Y);
                maxX = Math.Max(maxX, corner.X);
                maxY = Math.Max(maxY, corner.Y);
            }

            // Create a new array of aligned corners
            (int X, int Y)[] alignedCorners = new (int X, int Y)[4];
            Corners[0] = (minX, minY);
            Corners[1] = (maxX, minY);
            Corners[2] = (minX, maxY);
            Corners[3] = (maxX, maxY);
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
            return CalculateCIE76ColorDifference(c1.R, c1.G, c1.B, c2.R, c2.G, c2.B) <= ColorDistLimit;
        }
        static bool ColorDist(Color c1, Color c2, int colorDistLimit)
        {
            return CalculateCIE76ColorDifference(c1.R, c1.G, c1.B, c2.R, c2.G, c2.B) <= colorDistLimit;
        }
        public static double CalculateCIE76ColorDifference(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            // Convert the RGB values to the Lab color space
            double[] lab1 = RGBToLab(r1, g1, b1);
            double[] lab2 = RGBToLab(r2, g2, b2);

            // Calculate the CIE76 color difference
            double deltaL = lab1[0] - lab2[0];
            double deltaa = lab1[1] - lab2[1];
            double deltab = lab1[2] - lab2[2];

            return Math.Sqrt(deltaL * deltaL + deltaa * deltaa + deltab * deltab);
        }
        private static double[] RGBToLab(int R, int G, int B)
        {
            // Convert RGB to XYZ color space
            double rLinear = R / 255.0;
            double gLinear = G / 255.0;
            double bLinear = B / 255.0;

            rLinear = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / 1.055, 2.4) : rLinear / 12.92;
            gLinear = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / 1.055, 2.4) : gLinear / 12.92;
            bLinear = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / 1.055, 2.4) : bLinear / 12.92;

            double x = rLinear * 0.4124564 + gLinear * 0.3575761 + bLinear * 0.1804375;
            double y = rLinear * 0.2126729 + gLinear * 0.7151522 + bLinear * 0.0721750;
            double z = rLinear * 0.0193339 + gLinear * 0.1191920 + bLinear * 0.9503041;

            // Convert XYZ to Lab
            x /= 0.950456;
            y /= 1.0;
            z /= 1.088754;

            x = (x > 0.008856) ? Math.Pow(x, 1.0 / 3.0) : (903.3 * x + 16.0) / 116.0;
            y = (y > 0.008856) ? Math.Pow(y, 1.0 / 3.0) : (903.3 * y + 16.0) / 116.0;
            z = (z > 0.008856) ? Math.Pow(z, 1.0 / 3.0) : (903.3 * z + 16.0) / 116.0;

            double L = Math.Max(0, 116.0 * y - 16.0);
            double a = (x - y) * 500.0;
            double b = (y - z) * 200.0;

            return new double[] { L, a, b };
        }
        static string GetFileName(string File)
        {
            return GetFileNameWithExtension(File.Substring(0, File.IndexOf('.')));
        }
        static string GetFileNameWithExtension(string File)
        {
            int BackSlashIndex = File.LastIndexOf("\\");

            return File.Substring(BackSlashIndex + 1, File.Length - BackSlashIndex - 1);
        }
    }
}
