using Microsoft.Extensions.Logging;
using GuedesPlace.DoorLabel.Models;
using SkiaSharp;

namespace GuedesPlace.DoorLabel.Services;
public class GeneratePictureService(ILogger<GeneratePictureService> logger)
{
    private readonly ILogger<GeneratePictureService> _logger = logger;

    public PictureCreationResult CreatePicture(RoomLabel label)
    {
        List<int> grayScale = [];
        float rightShift = 10F;

        var skBitmap = new SKBitmap(540, 960);
        using (var canvas = new SKCanvas(skBitmap))
        {
            canvas.Clear(SKColors.White);
            using SKPaint paintHeader = new()
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 30,
                Typeface = SKTypeface.FromFamilyName(
                    familyName: "Arial",
                    weight: SKFontStyleWeight.Bold,
                    width: SKFontStyleWidth.SemiExpanded, 
                    slant:SKFontStyleSlant.Upright),
            };
            using SKPaint paintName = new()
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = 30,
                Typeface = SKTypeface.FromFamilyName(
                    familyName: "Arial",
                    weight: SKFontStyleWeight.Bold,
                    width: SKFontStyleWidth.Normal, 
                    slant:SKFontStyleSlant.Upright),
            };
            using SKPaint paintSubtitle = new()
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = 24,
                Typeface = SKTypeface.FromFamilyName(
                    familyName: "Arial",
                    weight: SKFontStyleWeight.Normal,
                    width: SKFontStyleWidth.Normal, 
                    slant:SKFontStyleSlant.Upright),
            };
            using SKPaint paintOutOfOffice = new()
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextSize = 24,
                Typeface = SKTypeface.FromFamilyName(
                    familyName: "Arial",
                    weight: SKFontStyleWeight.Normal,
                    width: SKFontStyleWidth.Normal, 
                    slant:SKFontStyleSlant.Italic),
            };
            using var paint = new SKPaint();
            paint.Color = SKColors.Black;
            var square = new SKRect(0, 0, 540, 54);
            canvas.DrawRect(square, paint);
            canvas.DrawText(label.Name, 10F, 40F, paintHeader);
            var startPoint = 95F;
            foreach(var element in label.Elements) 
            {
                canvas.DrawText(element.Name, rightShift, startPoint, paintName);
                startPoint += 37;
                canvas.DrawText(element.Title, rightShift, startPoint, paintSubtitle);
                startPoint += 31;

                if (element.OutOfOfficeUntil.HasValue)
                {
                    var until = $"Abwesend bis: {element.OutOfOfficeUntil.Value:dd.MM.yyyy}";
                    canvas.DrawText(until, rightShift, startPoint, paintOutOfOffice);
                    startPoint += 31;
                }
                startPoint += 20;
            }
        };


        for (int x = 0; x < 540; x++)
        {
            for (var y = 959; y >= 0; y--)
            {
                if (y % 2 == 0)
                {
                    var c = skBitmap.GetPixel(x, y);
                    int grayScaleValue = (int)((c.Red * 0.2126) + (c.Green * 0.7152) + (c.Blue * 0.0722));
                    grayScale.Add(grayScaleValue);

                }
            }
        }

        using (var stream = new MemoryStream())
        {

            skBitmap.Encode(stream, SKEncodedImageFormat.Png, quality: 100);//save the image file
            return new PictureCreationResult() { Data = new BinaryData(stream.ToArray()), GreyScale = grayScale };
        }
    }
}