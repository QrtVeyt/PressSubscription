using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PressSubscription.Data;
using PressSubscription.Models;
using PressSubscription.Services;

namespace PressSubscription.Views;

public partial class SubscriptionsWindow : Window
{
    private readonly AppDbContext _db = new();

    public SubscriptionsWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        var list = _db.Subscriptions
            .Include(x => x.Subscriber)
            .Include(x => x.Publication)
            .ToList();

        foreach (var s in list)
        {
            SubscriptionCalculator.Calculate(s);
        }

        SubscriptionsGrid.ItemsSource = list;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var subIdText = Microsoft.VisualBasic.Interaction.InputBox("ID подписчика:");
        if (!int.TryParse(subIdText, out var subId)) return;

        var pubIdText = Microsoft.VisualBasic.Interaction.InputBox("ID издания:");
        if (!int.TryParse(pubIdText, out var pubId)) return;

        var monthsText = Microsoft.VisualBasic.Interaction.InputBox("Месяцев:");
        if (!int.TryParse(monthsText, out var months)) return;

        var dateText = Microsoft.VisualBasic.Interaction.InputBox("Дата начала (yyyy-MM-dd):");
        if (!DateTime.TryParse(dateText, out var startDate)) return;

        var subscriber = _db.Subscribers.FirstOrDefault(x => x.Id == subId);
        var publication = _db.Publications.FirstOrDefault(x => x.Id == pubId);

        if (subscriber == null || publication == null)
        {
            MessageBox.Show("Неверный ID подписчика или издания");
            return;
        }

        var sub = new Subscription
        {
            SubscriberId = subId,
            PublicationId = pubId,
            Months = months,
            StartDate = startDate
        };

        _db.Subscriptions.Add(sub);
        _db.SaveChanges();

        LoadData();
    }
}