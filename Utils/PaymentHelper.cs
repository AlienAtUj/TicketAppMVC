using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;

namespace TicketAppMVC.Utils
{
    public static class PaymentHelper
    {
        private static readonly Random random = new Random();

        public static string GenerateRandomTicketCode()
        {
            // Generate 10 random digits
            return random.Next(1000000000, 2147483647).ToString("D10");
        }

        public static string GenerateQRCode(string ticketCode, string savePath)
        {
            try
            {
                var qrWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 250,
                        Width = 250,
                        Margin = 1
                    }
                };

                var qrCodeImage = qrWriter.Write(ticketCode);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save the image
                qrCodeImage.Save(savePath, ImageFormat.Png);

                return $"/Content/Tickets/{ticketCode}.png";
            }
            catch (Exception ex)
            {
                // Log error or handle appropriately
                Console.WriteLine($"QR Code generation failed: {ex.Message}");
                return "/Content/Tickets/default_qr.png"; // Fallback image
            }
        }
    }
}