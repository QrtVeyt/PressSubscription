using ClosedXML.Excel;
using PressSubscription.Data;
using PressSubscription.Models;
using System.Globalization;

namespace PressSubscription.Services;

public static class ExcelImportService
{
    public static void Import(string filePath)
    {
        using var db = new AppDbContext();
        using var wb = new XLWorkbook(filePath);

        ImportSubscribers(db, wb);
        ImportPublications(db, wb);
        ImportSubscriptions(db, wb);

        db.SaveChanges();
    }

    // ---------------- SUBSCRIBERS ----------------
    private static void ImportSubscribers(AppDbContext db, XLWorkbook wb)
    {
        var ws = wb.Worksheet("Subscribers");
        var rows = ws.RangeUsed()?.RowsUsed().Skip(1);

        if (rows == null) return;

        foreach (var row in rows)
        {
            try
            {
                var fullName = row.Cell(2).GetString();
                if (string.IsNullOrWhiteSpace(fullName))
                    continue;

                var subscriber = new Subscriber
                {
                    FullName = fullName,
                    Address = row.Cell(3).GetString(),
                    Phone = row.Cell(4).GetString(),
                    Email = row.Cell(5).GetString()
                };

                db.Subscribers.Add(subscriber);
            }
            catch
            {
                continue;
            }
        }
    }

    // ---------------- PUBLICATIONS ----------------
    private static void ImportPublications(AppDbContext db, XLWorkbook wb)
    {
        var ws = wb.Worksheet("Publications");
        var rows = ws.RangeUsed()?.RowsUsed().Skip(1);

        if (rows == null) return;

        foreach (var row in rows)
        {
            try
            {
                var title = row.Cell(2).GetString();
                if (string.IsNullOrWhiteSpace(title))
                    continue;

                var pub = new Publication
                {
                    Title = title,
                    Publisher = row.Cell(3).GetString(),
                    PricePerMonth = ParseDecimal(row.Cell(4).GetString()),
                    ImagePath = row.Cell(5).GetString()
                };

                db.Publications.Add(pub);
            }
            catch
            {
                continue;
            }
        }
    }

    // ---------------- SUBSCRIPTIONS ----------------
    private static void ImportSubscriptions(AppDbContext db, XLWorkbook wb)
    {
        var ws = wb.Worksheet("Subscriptions");
        var rows = ws.RangeUsed()?.RowsUsed().Skip(1);

        if (rows == null) return;

        foreach (var row in rows)
        {
            try
            {
                var subscriberId = ParseInt(row.Cell(2).GetString());
                var publicationId = ParseInt(row.Cell(3).GetString());

                if (subscriberId == 0 || publicationId == 0)
                    continue;

                var sub = new Subscription
                {
                    SubscriberId = subscriberId,
                    PublicationId = publicationId,
                    StartDate = ParseDate(row.Cell(4).GetString()),
                    Months = ParseInt(row.Cell(5).GetString())
                };

                db.Subscriptions.Add(sub);
            }
            catch
            {
                continue;
            }
        }
    }

    // ---------------- HELPERS ----------------
    private static decimal ParseDecimal(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        var cleaned = new string(input.Where(c =>
            char.IsDigit(c) || c == ',' || c == '.').ToArray());

        cleaned = cleaned.Replace(',', '.');

        decimal.TryParse(cleaned,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var result);

        return result;
    }

    private static int ParseInt(string input)
    {
        int.TryParse(input, out var result);
        return result;
    }

    private static DateTime ParseDate(string input)
    {
        DateTime.TryParse(input, out var result);
        return result == default ? DateTime.Now : result;
    }
}