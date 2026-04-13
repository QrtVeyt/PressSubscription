using System.Windows;
using PressSubscription.Data;
using Microsoft.EntityFrameworkCore;

namespace PressSubscription.Views;

public partial class SubscribersWindow : Window
{
    private readonly AppDbContext _db = new();

    public SubscribersWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        SubscribersGrid.ItemsSource = _db.Subscribers.ToList();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        
    }
}