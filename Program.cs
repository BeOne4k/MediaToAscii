using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using OpenCvSharp;

class MediaToAscii
{
    private static readonly string AsciiChars = " .,;:+*?%S#@";

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MediaToAscii <path_to_file> [width] [--save]");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"Error: file not found - {filePath}");
            return;
        }

        int outputWidth = 100;
        bool saveToFile = false;

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--save") saveToFile = true;
            else if (int.TryParse(args[i], out int w) && w > 0) outputWidth = w;
        }

        string ext = Path.GetExtension(filePath).ToLower();

        if (ext == ".mp4" || ext == ".avi" || ext == ".mov" || ext == ".mkv")
        {
            ProcessVideo(filePath, outputWidth);
        }
        else
        {
            ProcessImage(filePath, outputWidth, saveToFile);
        }
    }

    static void ProcessImage(string imagePath, int outputWidth, bool saveToFile)
    {
        try
        {
            using Bitmap original = new Bitmap(imagePath);
            string asciiArt = ConvertBitmapToAscii(original, outputWidth);
            Console.WriteLine(asciiArt);

            if (saveToFile)
            {
                string outputPath = Path.ChangeExtension(imagePath, ".txt");
                File.WriteAllText(outputPath, asciiArt, Encoding.UTF8);
                Console.Error.WriteLine($"Saved to: {outputPath}");
            }
        }
        catch (Exception ex) { Console.Error.WriteLine($"Error processing image: {ex.Message}"); }
    }

    static void ProcessVideo(string videoPath, int outputWidth)
    {
        using var capture = new VideoCapture(videoPath);
        if (!capture.IsOpened())
        {
            Console.Error.WriteLine("Error: Could not open video file.");
            return;
        }

        double fps = capture.Fps;
        int delay = fps > 0 ? (int)(1000 / fps) : 33;

        using var frame = new Mat();
        var sb = new StringBuilder();

        Console.CursorVisible = false;
        Console.Clear();

        while (capture.Read(frame) && !frame.Empty())
        {
            int outputHeight = (int)(frame.Height * outputWidth / (double)frame.Width * 0.45);
            outputHeight = Math.Max(1, outputHeight);
            if (outputWidth > Console.WindowWidth)
            {
                outputWidth = Console.WindowWidth - 1;
                outputHeight = (int)(frame.Height * outputWidth / (double)frame.Width * 0.45);
            }
            if (outputHeight > Console.WindowHeight)
            {
                outputHeight = Console.WindowHeight - 1;
                outputWidth = (int)(outputHeight * (double)frame.Width / (frame.Height * 0.45));
            }
            outputWidth = Math.Max(1, outputWidth);
            outputHeight = Math.Max(1, outputHeight);

            using var resizedFrame = new Mat();
            Cv2.Resize(frame, resizedFrame, new OpenCvSharp.Size(outputWidth, outputHeight));

            sb.Clear();

            int rows = resizedFrame.Rows;
            int cols = resizedFrame.Cols;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vec3b pixel = resizedFrame.At<Vec3b>(y, x);
                    byte b = pixel.Item0;
                    byte g = pixel.Item1;
                    byte r = pixel.Item2;

                    double brightness = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;

                    int index = Math.Clamp((int)(brightness * (AsciiChars.Length - 1)), 0, AsciiChars.Length - 1);
                    sb.Append(AsciiChars[index]);
                }
                if (y < rows - 1) sb.AppendLine();
            }
            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());

            Thread.Sleep(Math.Max(1, delay - 5));

            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                break;
        }

        Console.CursorVisible = true;
    }

    static string ConvertBitmapToAscii(Bitmap original, int outputWidth)
    {
        int outputHeight = (int)(original.Height * outputWidth / (double)original.Width * 0.45);
        outputHeight = Math.Max(1, outputHeight);

        using Bitmap resized = new Bitmap(original, new System.Drawing.Size(outputWidth, outputHeight));
        var sb = new StringBuilder();

        for (int y = 0; y < resized.Height; y++)
        {
            for (int x = 0; x < resized.Width; x++)
            {
                Color pixel = resized.GetPixel(x, y);
                double alpha = pixel.A / 255.0;
                double brightness = (0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B) / 255.0;
                double effective = brightness * alpha;
                int index = Math.Clamp((int)(effective * (AsciiChars.Length - 1)), 0, AsciiChars.Length - 1);
                sb.Append(AsciiChars[index]);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}