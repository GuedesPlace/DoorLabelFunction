using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GuedesPlace.DoorLabel.Models;
using Microsoft.Extensions.Azure;

namespace GuedesPlace.DoorLabel.Services;
public class GeneratePictureService(ILogger<GeneratePictureService> logger)
{
    private readonly ILogger<GeneratePictureService> _logger = logger;

    public PictureCreationResult CreatePicture(RoomLabel label)
    {
        List<int> grayScale = [];
        float rightShift = 10F;

        PointF firstLocation = new PointF(10f, 10f);

        //Bitmap bitmap = (Bitmap)Image.FromFile(imageFilePath);//load the image file
        Bitmap bitmap = new Bitmap(540, 960);


        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            Font arialFont24 = new Font("Arial", 24, FontStyle.Bold);
            Font arialFont20 = new Font("Arial", 20);
            Font arialFont20Italic = new Font("Arial", 20, FontStyle.Italic);


            graphics.Clear(Color.White);
            graphics.FillRectangle(Brushes.Black, 0, 0, 540, 54);

            graphics.DrawString(label.Name, arialFont24, Brushes.White, firstLocation);

            var startPoint = 75F;
            foreach (var element in label.Elements)
            {
                graphics.DrawString(element.Name, arialFont24, Brushes.Black, rightShift, startPoint);
                startPoint += 37;
                graphics.DrawString(element.Title, arialFont20, Brushes.Black, rightShift, startPoint);
                startPoint += 31;

                if (element.OutOfOfficeUntil.HasValue)
                {
                    var until = $"Abwesend bis: {element.OutOfOfficeUntil.Value:dd.MM.yyyy}";
                    graphics.DrawString(until, arialFont20Italic, Brushes.DarkGray, rightShift, startPoint);
                    startPoint += 31;
                }
                startPoint += 20;
            }


            arialFont20Italic.Dispose();
            arialFont20.Dispose();
            arialFont24.Dispose();
        }
            for (int x = 0; x < 540; x++)
            {
                for (var y = 959; y >= 0; y--)
                {
                    if (y % 2 == 0) {
                        var c = bitmap.GetPixel(x,y);
                        int grayScaleValue = (int)((c.R * 0.2126) + (c.G * 0.7152) + (c.B * 0.0722));
                        grayScale.Add(grayScaleValue);

                    }
                }
            }

        using (var stream = new MemoryStream())
        {

            bitmap.Save(stream, ImageFormat.Png);//save the image file
            return new PictureCreationResult() { Data = new BinaryData(stream.ToArray()), GreyScale=grayScale };
        }
    }
}