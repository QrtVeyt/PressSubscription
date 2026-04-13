using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PressSubscription.Data;
using PressSubscription.Models;
using PressSubscription.Services;

namespace PressSubscription.Views;

public partial class SubscribersWindow : Window
{
    private readonly AppDbContext _db = new();
    private ObservableCollection<Subscriber> _subscribers = new();

    public SubscribersWindow()
    {
        InitializeComponent();
        LoadData();
        ApplyUserPermissions();
    }

    private void ApplyUserPermissions()
    {
        var isAdmin = AuthService.IsAdmin();
        
        if (!isAdmin)
        {
            AddButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
        }
    }

    private void LoadData()
    {
        var list = _db.Subscribers.ToList();
        _subscribers = new ObservableCollection<Subscriber>(list);
        SubscribersGrid.ItemsSource = _subscribers;
    }

    private void ApplyFilter()
    {
        var searchText = SearchBox.Text?.Trim().ToLower();
        var filtered = string.IsNullOrEmpty(searchText)
            ? _subscribers.ToList()
            : _subscribers.Where(s =>
                s.FullName.ToLower().Contains(searchText) ||
                s.Email.ToLower().Contains(searchText) ||
                (s.Phone?.Contains(searchText) ?? false) ||
                (s.Address?.Contains(searchText) ?? false)).ToList();

        SubscribersGrid.ItemsSource = filtered;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        ApplyFilter();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var window = new EntityEditWindow();
        window.SetEntity((Subscriber?)null);
        
        if (window.ShowDialog() == true && window.Entity is Subscriber subscriber)
        {
            _db.Subscribers.Add(subscriber);
            _db.SaveChanges();
            LoadData();
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (SubscribersGrid.SelectedItem is not Subscriber sub)
        {
            MessageBox.Show("Выберите подписчика", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var window = new EntityEditWindow();
        window.SetEntity(sub);
        
        if (window.ShowDialog() == true && window.Entity is Subscriber updated)
        {
            sub.FullName = updated.FullName;
            sub.Address = updated.Address;
            sub.Phone = updated.Phone;
            sub.Email = updated.Email;
            
            _db.SaveChanges();
            LoadData();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SubscribersGrid.SelectedItem is not Subscriber sub) return;

        var result = MessageBox.Show($"Удалить подписчика \"{sub.FullName}\"?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _db.Subscribers.Remove(sub);
            _db.SaveChanges();
            LoadData();
        }
    }
}