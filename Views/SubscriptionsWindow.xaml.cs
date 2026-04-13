using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PressSubscription.Data;
using PressSubscription.Models;
using PressSubscription.Services;

namespace PressSubscription.Views;

public partial class SubscriptionsWindow : Window
{
    private readonly AppDbContext _db = new();
    private ObservableCollection<Subscription> _subscriptions = new();

    public SubscriptionsWindow()
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
        var list = _db.Subscriptions
            .Include(x => x.Subscriber)
            .Include(x => x.Publication)
            .ToList();

        foreach (var s in list)
        {
            SubscriptionCalculator.Calculate(s);
        }

        _subscriptions = new ObservableCollection<Subscription>(list);
        SubscriptionsGrid.ItemsSource = _subscriptions;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var searchText = SearchBox.Text?.Trim().ToLower();
        
        if (string.IsNullOrEmpty(searchText))
        {
            SubscriptionsGrid.ItemsSource = _subscriptions;
            return;
        }
        
        var filtered = _subscriptions.Where(s =>
            (s.Subscriber?.FullName?.ToLower().Contains(searchText) ?? false) ||
            (s.Publication?.Title?.ToLower().Contains(searchText) ?? false) ||
            s.Id.ToString().Contains(searchText)).ToList();
        
        SubscriptionsGrid.ItemsSource = filtered;
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
        window.SetEntity((Subscription?)null);
        
        if (window.ShowDialog() == true && window.Entity is Subscription subscription)
        {
            var subscriber = _db.Subscribers.FirstOrDefault(x => x.Id == subscription.SubscriberId);
            var publication = _db.Publications.FirstOrDefault(x => x.Id == subscription.PublicationId);
            
            if (subscriber == null)
            {
                MessageBox.Show($"Подписчик с ID {subscription.SubscriberId} не найден", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (publication == null)
            {
                MessageBox.Show($"Издание с ID {subscription.PublicationId} не найдено", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            SubscriptionCalculator.Calculate(subscription);
            
            _db.Subscriptions.Add(subscription);
            _db.SaveChanges();
            LoadData();
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (SubscriptionsGrid.SelectedItem is not Subscription sub)
        {
            MessageBox.Show("Выберите подписку для редактирования", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var originalSubscriberId = sub.SubscriberId;
        var originalPublicationId = sub.PublicationId;
        
        var window = new EntityEditWindow();
        window.SetEntity(sub);
        
        if (window.ShowDialog() == true && window.Entity is Subscription updated)
        {
            if (originalSubscriberId != updated.SubscriberId)
            {
                var subscriber = _db.Subscribers.FirstOrDefault(x => x.Id == updated.SubscriberId);
                if (subscriber == null)
                {
                    MessageBox.Show($"Подписчик с ID {updated.SubscriberId} не найден", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                sub.SubscriberId = updated.SubscriberId;
            }
            
            if (originalPublicationId != updated.PublicationId)
            {
                var publication = _db.Publications.FirstOrDefault(x => x.Id == updated.PublicationId);
                if (publication == null)
                {
                    MessageBox.Show($"Издание с ID {updated.PublicationId} не найдено", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                sub.PublicationId = updated.PublicationId;
            }
            
            sub.Months = updated.Months;
            sub.StartDate = updated.StartDate;
            
            SubscriptionCalculator.Calculate(sub);
            
            _db.SaveChanges();
            LoadData();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SubscriptionsGrid.SelectedItem is not Subscription sub) return;

        var result = MessageBox.Show($"Удалить подписку #{sub.Id}?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _db.Subscriptions.Remove(sub);
            _db.SaveChanges();
            LoadData();
        }
    }
}