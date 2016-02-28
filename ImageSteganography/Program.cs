using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImageSteganography
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Arguments.FromArgs(args);

            if (arguments == null)
            {
                Console.WriteLine("Please run program with following arguments: hide key imageFilePath messageFilePath outputImageFilePath");
                Console.WriteLine("                                           : discover key imageFilePath outputMessageFilePath");
                return;
            }

            if (!File.Exists(arguments.ImageFilePath))
            {
                Console.WriteLine("Image file does not exist");
                return;
            }

            switch (arguments.Command)
            {
                case Arguments.CommandHide:
                    HideMessage(arguments as HideArguments);
                    break;
                case Arguments.CommandDiscover:
                    DiscoverMessage(arguments as DiscoverArguments);
                    break;
            }
        }

        private static void HideMessage(HideArguments arguments)
        {
            if (!File.Exists(arguments.MessageFilePath))
            {
                Console.WriteLine("Message file does not exist");
                return;
            }

            if (string.IsNullOrEmpty(arguments.OutputImageFilePath))
            {
                Console.WriteLine("Please specify output image file path");
                return;
            }

            using (var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(arguments.Key)))
            using (var imageStream = Image.FromFile(arguments.ImageFilePath))
            using (var messageStream = new FileStream(arguments.MessageFilePath, FileMode.Open, FileAccess.Read))
            {
                var bitmapSteganography = new BitmapSteganography(keyStream);
                var bitmapWithMessage = bitmapSteganography.HideMessage((Bitmap)imageStream, messageStream);

                bitmapWithMessage .Save(arguments.OutputImageFilePath);
            }
        }

        private static void DiscoverMessage(DiscoverArguments arguments)
        {
            if (string.IsNullOrEmpty(arguments.OutputMessageFilePath))
            {
                Console.WriteLine("Please specify output message file path");
                return;
            }

            using (var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(arguments.Key)))
            using (var imageStream = Image.FromFile(arguments.ImageFilePath))
            using (var outputMessageStream = new FileStream(arguments.OutputMessageFilePath, FileMode.Create, FileAccess.Write))
            {
                var bitmapSteganography = new BitmapSteganography(keyStream);
                var outputStream = bitmapSteganography.DiscoverMessage((Bitmap)imageStream);

                outputStream.CopyTo(outputMessageStream);
            }
        }
    }
}