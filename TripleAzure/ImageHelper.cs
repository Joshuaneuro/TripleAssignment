﻿using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace TripleAzure
{
    public class ImageHelper
    {
        public static Stream AddTextToImage(Stream imageStream, params (string text, (float x, float y) position, int fontSize, string colorHex)[] texts)
        {
            var memoryStream = new MemoryStream();

            var image = Image.Load(imageStream);

            image.Clone(img =>
            {
               

                foreach (var (text, (x, y), fontSize, colorHex) in texts)
                {
                    var font = SystemFonts.CreateFont("Verdana", fontSize);
                    var color = Rgba32.ParseHex(colorHex);

                    img.DrawText(text, font, color, new PointF(x, y));
                }
            })
                .SaveAsPng(memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}