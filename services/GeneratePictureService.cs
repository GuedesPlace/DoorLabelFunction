using Microsoft.Extensions.Logging;
using GuedesPlace.DoorLabel.Models;
using SkiaSharp;

namespace GuedesPlace.DoorLabel.Services;
public class GeneratePictureService(ILogger<GeneratePictureService> logger)
{
    private readonly ILogger<GeneratePictureService> _logger = logger;

    public PictureCreationResult CreatePicture(RoomLabel label)
    {
        DynamicsDisplayConfiguration configuration = label.Configuration;
        List<int> grayScale = [];

        var skBitmap = new SKBitmap(540, 960);
        using (var canvas = new SKCanvas(skBitmap))
        {
            canvas.Clear(SKColors.White);
            using SKPaint paintHeader = CreatePaint(configuration.gp_font_family_header, configuration.gp_font_size_header, configuration.gp_font_weight_header, configuration.gp_font_style_header);
            using SKPaint paintName = CreatePaint(configuration.gp_font_family_name, configuration.gp_font_size_name, configuration.gp_font_weight_name, configuration.gp_font_style_name);
            using SKPaint paintSubtitle = CreatePaint(configuration.gp_font_family_title, configuration.gp_font_size_title, configuration.gp_font_weight_title, configuration.gp_font_style_title);
            using SKPaint paintOutOfOffice = CreatePaint(configuration.gp_font_family_leave, configuration.gp_font_size_leave, configuration.gp_font_weight_leave, configuration.gp_font_style_leave);

            //using var paint = new SKPaint();
            //paint.Color = SKColors.Black;
            //var square = new SKRect(0, 0, 540, 54);
            //canvas.DrawRect(square, paint);
            if (configuration.gp_has_picture && label.Picture != null)
            {
                ProcessPictureToCanvas(canvas, label.Picture, configuration);
            }
            var xPostion = CalculateXPosition(configuration.gp_alignment_header, configuration.gp_margin_left, configuration.gp_margin_right, label.Name, paintHeader);
            canvas.DrawText(label.Name, xPostion, (float)configuration.gp_start_header_block, paintHeader);
            var startPoint = (float)configuration.gp_start_employee_block;
            var lineLength = 540 - configuration.gp_margin_left - configuration.gp_margin_right;
            var orderedElements = OrderElements(label.Elements, label.SpecialSortOrder);
            foreach (var element in orderedElements)
            {
                startPoint = WriteText(canvas,element.Name,lineLength,paintName,configuration.gp_alignment_name,startPoint,configuration);
                startPoint = WriteText(canvas,element.Title,lineLength,paintSubtitle,configuration.gp_alignment_title,startPoint,configuration);

                if (element.OutOfOfficeUntil.HasValue)
                {
                    var until = $"Abwesend bis: {element.OutOfOfficeUntil.Value:dd.MM.yyyy}";
                    startPoint = WriteText(canvas,until,lineLength,paintOutOfOffice,configuration.gp_alignment_leave,startPoint,configuration);                  
                }
                startPoint += 10;
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

        using var stream = new MemoryStream();

        skBitmap.Encode(stream, SKEncodedImageFormat.Png, quality: 100);//save the image file
        return new PictureCreationResult() { Data = new BinaryData(stream.ToArray()), GreyScale = grayScale };
    }

    private static List<RoomLabelElement> OrderElements(List<RoomLabelElement> elements, string specialSortOrder)
    {
        if(string.IsNullOrWhiteSpace(specialSortOrder)) {
            return elements;
        }
        var sortOrderEmails = specialSortOrder.Split("\n").Select(x=>x.Trim()).ToList();
        var elementsOrdered = sortOrderEmails.Select(x=>elements.Find(roomLabelElement=>roomLabelElement.EMail.Equals(x, StringComparison.InvariantCultureIgnoreCase))).Where(element=>element is not null).ToList();
        var elementsNotFound = elements.Where(element=>sortOrderEmails.Any(email=>email.Equals(element.EMail, StringComparison.InvariantCultureIgnoreCase))).ToList();
        return [.. elementsOrdered.Concat(elementsNotFound).Where(x=>x is not null)];
    }

    private void ProcessPictureToCanvas(SKCanvas canvas, byte[] data, DynamicsDisplayConfiguration configuration)
    {
        SKImage skImage = SKImage.FromEncodedData(data);
        canvas.DrawImage(skImage, (float)configuration.gp_picture_position_x, (float)configuration.gp_picture_position_y);
    }

    private SKPaint CreatePaint(string fontFamilyName, int size, int weight, int style)
    {
        return new()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextSize = size,
            Typeface = SKTypeface.FromFamilyName(
                    familyName: fontFamilyName,
                    weight: ConvertToWeight(weight),
                    width: SKFontStyleWidth.Normal,
                    slant: ConvertToSlant(style)),
        };
    }
    private SKFontStyleWeight ConvertToWeight(int weight)
    {
        switch (weight)
        {
            case 122810001:
                return SKFontStyleWeight.SemiBold;
            case 122810002:
                return SKFontStyleWeight.Bold;
            case 122810003:
                return SKFontStyleWeight.Black;
            case 122810004:
                return SKFontStyleWeight.ExtraBold;
            case 122810005:
                return SKFontStyleWeight.ExtraBlack;
            case 122810006:
                return SKFontStyleWeight.Medium;
            case 122810007:
                return SKFontStyleWeight.Thin;
            case 122810008:
                return SKFontStyleWeight.Light;
            case 122810009:
                return SKFontStyleWeight.ExtraLight;
            case 122810010:
                return SKFontStyleWeight.Invisible;
        }
        return SKFontStyleWeight.Normal;
    }
    private SKFontStyleSlant ConvertToSlant(int slant)
    {
        switch (slant)
        {
            case 122810001:
                return SKFontStyleSlant.Italic;
            case 122810002:
                return SKFontStyleSlant.Oblique;
        }
        return SKFontStyleSlant.Upright;
    }
    private float CalculateXPosition(int alignment, int margin_left, int margin_right, string text, SKPaint paint)
    {
        if (alignment == 122810000)
        {
            return (float)margin_left;
        }
        SKRect rect = new();
        paint.MeasureText(text, ref rect);
        var width = rect.Width;
        if (alignment == 122810001)
        {
            return (float)(270 - (width / 2));
        }
        return 540 - margin_right - width;
    }
    private List<string> GetWrappedLines(string longLine, float lineLengthLimit, SKPaint defPaint)
    {
        var wrappedLines = new List<string>();
        var lineLength = 0f;
        var line = "";
        foreach (var word in longLine.Split(' '))
        {
            var wordWithSpace = word + " ";
            var wordWithSpaceLength = defPaint.MeasureText(wordWithSpace);
            if (lineLength + wordWithSpaceLength > lineLengthLimit)
            {
                wrappedLines.Add(line);
                line = "" + wordWithSpace;
                lineLength = wordWithSpaceLength;
            }
            else
            {
                line += wordWithSpace;
                lineLength += wordWithSpaceLength;
            }
        }
        wrappedLines.Add(line);
        return wrappedLines;
    }
    private float WriteText(SKCanvas canvas, string text, float lineLength, SKPaint skPaint, int alignment, float lastStart, DynamicsDisplayConfiguration configuration )
    {
        float startPoint = lastStart;
        var wrappedLines = GetWrappedLines(text, lineLength, skPaint);
        foreach (var wrappedLine in wrappedLines)
        {
            startPoint += skPaint.FontSpacing;
            var xPositionName = CalculateXPosition(alignment, configuration.gp_margin_left, configuration.gp_margin_right, wrappedLine, skPaint);
            canvas.DrawText(wrappedLine, xPositionName, startPoint, skPaint);
        }
        return startPoint;
    }
}