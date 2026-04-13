using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PressSubscription.Data;
using PressSubscription.Models;
using PressSubscription.Services;

namespace PressSubscription.Views;

public partial class PublicationsWindow : Window
{
    private readonly AppDbContext _db = new();
    private ObservableCollection<Publication> _publications = new();

    public PublicationsWindow()
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
        var list = _db.Publications.ToList();
        _publications = new ObservableCollection<Publication>(list);
        PublicationsList.ItemsSource = _publications;
        ApplyFilterAndSort();
    }

    private void ApplyFilterAndSort()
    {
        var query = _db.Publications.AsEnumerable();
        
        var searchText = SearchBox.Text?.Trim().ToLower();
        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(p => 
                p.Title.ToLower().Contains(searchText) || 
                p.Publisher.ToLower().Contains(searchText));
        }
        
        var selectedSort = (SortCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
        query = selectedSort switch
        {
            "Названию (А-Я)" => query.OrderBy(p => p.Title),
            "Названию (Я-А)" => query.OrderByDescending(p => p.Title),
            "Цене (возрастание)" => query.OrderBy(p => p.PricePerMonth),
            "Цене (убывание)" => query.OrderByDescending(p => p.PricePerMonth),
            _ => query.OrderBy(p => p.Title)
        };
        
        _publications.Clear();
        foreach (var item in query)
        {
            _publications.Add(item);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilterAndSort();
    }

    private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilterAndSort();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        SortCombo.SelectedIndex = -1;
        LoadData();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var window = new EntityEditWindow();
        window.SetEntity((Publication?)null);
        
        if (window.ShowDialog() == true && window.Entity is Publication publication)
        {
            if (string.IsNullOrWhiteSpace(publication.ImagePath))
            {
                publication.ImagePath = Path.Combine("Images", "placeholder.png");
            }
            
            _db.Publications.Add(publication);
            _db.SaveChanges();
            LoadData();
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (PublicationsList.SelectedItem is not Publication pub)
        {
            MessageBox.Show("Выберите издание для редактирования", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var window = new EntityEditWindow();
        window.SetEntity(pub);
        
        if (window.ShowDialog() == true && window.Entity is Publication updated)
        {
            pub.Title = updated.Title;
            pub.Publisher = updated.Publisher;
            pub.PricePerMonth = updated.PricePerMonth;
            pub.ImagePath = updated.ImagePath;
            
            _db.SaveChanges();
            LoadData();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (PublicationsList.SelectedItem is not Publication pub) return;

        var result = MessageBox.Show($"Удалить издание \"{pub.Title}\"?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            _db.Publications.Remove(pub);
            _db.SaveChanges();
            LoadData();
        }
    }
}