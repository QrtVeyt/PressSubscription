using System.Windows;
using Microsoft.EntityFrameworkCore;
using PressSubscription.Data;

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
        SubscriptionsGrid.ItemsSource = _db.Subscriptions
            .Include(x => x.Subscriber)
            .Include(x => x.Publication)
            .ToList();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {

    }
}