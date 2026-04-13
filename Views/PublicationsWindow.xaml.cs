using System.Linq;
using System.Windows;
using PressSubscription.Data;

namespace PressSubscription.Views;

public partial class PublicationsWindow : Window
{
    private readonly AppDbContext _db = new();

    public PublicationsWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        PublicationsList.ItemsSource = _db.Publications.ToList();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {

    }
}