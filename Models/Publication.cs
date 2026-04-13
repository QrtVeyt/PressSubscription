using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PressSubscription.Models;

public class Publication
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Publisher { get; set; } = "";
    public decimal PricePerMonth { get; set; }

    public string ImagePath { get; set; } = "";

    public BitmapImage Image
    {
        get
        {
            var path = File.Exists(ImagePath)
                ? ImagePath
                : GetPlaceholderPath();

            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }
    }

    private static string GetPlaceholderPath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "Images", "placeholder.png");
    }
}