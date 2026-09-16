using LinxUniverse.Algo.Common;
using LinxUniverse.Utils;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using TrayScanStandard.Models;

namespace TrayScanStandard.Utils
{
    public static class Error2DataImageSaver
    {
        public const string DirectoryName = "Error2Data";

        public static bool SaveIfHasMissingCodes(
            string imagePath,
            IEnumerable<BarCodeRegionInfo> regions,
            IEnumerable<CodeInfo> codes)
        {
            var regionList = regions
                .Where(s => s.Width > 0 && s.Height > 0)
                .OrderBy(s => s.ChannelIdx)
                .ToArray();
            if (regionList.Length == 0 || string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                return false;
            }

            var successIndexes = codes
                .Where(s => !string.IsNullOrWhiteSpace(s.Code))
                .Select(s => s.Index)
                .ToHashSet();
            var failedIndexes = regionList
                .Where(s => !successIndexes.Contains(s.ChannelIdx))
                .Select(s => s.ChannelIdx)
                .ToHashSet();
            if (failedIndexes.Count == 0)
            {
                return false;
            }

            Directory.CreateDirectory(GetOutputDirectory());
            SaveAnnotatedImage(imagePath, regionList, failedIndexes);
            return true;
        }

        private static void SaveAnnotatedImage(
            string imagePath,
            BarCodeRegionInfo[] regions,
            System.Collections.Generic.HashSet<int> failedIndexes)
        {
            using var source = Image.FromFile(imagePath);
            using var bitmap = new Bitmap(source);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var lineWidth = Math.Max(6, Math.Min(bitmap.Width, bitmap.Height) / 180);
            using var okPen = new Pen(Color.LimeGreen, lineWidth);
            using var ngPen = new Pen(Color.Red, lineWidth);
            using var okBrush = new SolidBrush(Color.LimeGreen);
            using var ngBrush = new SolidBrush(Color.Red);
            using var labelBackBrush = new SolidBrush(Color.FromArgb(170, Color.Black));
            using var font = new Font("Arial", Math.Max(18, lineWidth * 5), FontStyle.Bold, GraphicsUnit.Pixel);

            foreach (var region in regions)
            {
                var isFailed = failedIndexes.Contains(region.ChannelIdx);
                var rectangle = ClampRectangle(region, bitmap.Width, bitmap.Height);
                graphics.DrawRectangle(isFailed ? ngPen : okPen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                DrawLabel(
                    graphics,
                    isFailed ? $"NG {region.ChannelIdx}" : $"OK {region.ChannelIdx}",
                    font,
                    isFailed ? ngBrush : okBrush,
                    labelBackBrush,
                    rectangle,
                    bitmap.Height);
            }

            bitmap.Save(BuildOutputPath(imagePath, failedIndexes), ImageFormat.Png);
        }

        private static RectangleF ClampRectangle(BarCodeRegionInfo region, int imageWidth, int imageHeight)
        {
            var maxX = Math.Max(0, imageWidth - 1);
            var maxY = Math.Max(0, imageHeight - 1);
            var left = Math.Clamp(region.Left, 0, maxX);
            var top = Math.Clamp(region.Top, 0, maxY);
            var right = Math.Clamp(region.Left + region.Width, left + 1, imageWidth);
            var bottom = Math.Clamp(region.Top + region.Height, top + 1, imageHeight);

            return new RectangleF(left, top, right - left, bottom - top);
        }

        private static void DrawLabel(
            Graphics graphics,
            string label,
            Font font,
            Brush textBrush,
            Brush backBrush,
            RectangleF rectangle,
            int imageHeight)
        {
            var labelSize = graphics.MeasureString(label, font);
            var labelX = rectangle.X;
            var labelY = rectangle.Y - labelSize.Height - 2;
            if (labelY < 0)
            {
                labelY = Math.Min(rectangle.Y + 2, imageHeight - labelSize.Height);
            }

            var labelRect = new RectangleF(labelX, Math.Max(0, labelY), labelSize.Width + 8, labelSize.Height + 4);
            graphics.FillRectangle(backBrush, labelRect);
            graphics.DrawString(label, font, textBrush, labelRect.X + 4, labelRect.Y + 2);
        }

        private static string BuildOutputPath(string imagePath, IEnumerable<int> failedIndexes)
        {
            var imageName = Path.GetFileNameWithoutExtension(imagePath);
            var failedText = string.Join("_", failedIndexes.OrderBy(s => s).Take(16));
            var suffix = failedIndexes.Count() > 16 ? "_More" : string.Empty;
            var fileName = $"{imageName}-NG-{failedText}{suffix}-{DateTime.Now:HHmmssfff}.png";
            return Path.Combine(GetOutputDirectory(), fileName);
        }

        private static string GetOutputDirectory() =>
            Path.Combine(FilenameHelper.AppPath, DirectoryName);
    }
}
