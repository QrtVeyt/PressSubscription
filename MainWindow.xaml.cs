using Microsoft.Win32;
using PressSubscription.Services;
using PressSubscription.Views;
using System;
using System.Windows;

namespace PressSubscription;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ApplyUserPermissions();
        SetUserInfo();
    }

    private void SetUserInfo()
    {
        var user = AuthService.GetCurrentUser();
        if (user != null && UserInfoText != null)
        {
            var roleIcon = user.Role == "Admin" ? "👑" : "👤";
            UserInfoText.Text = $"{roleIcon} {user.FullName} ({user.Role})";
        }
    }

    private void ApplyUserPermissions()
    {
        var isAdmin = AuthService.IsAdmin();
        
        if (AdminPanelButton != null)
        {
            AdminPanelButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }
        
        if (ImportButton != null)
        {
            ImportButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void AdminPanel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var adminPanel = new AdminPanelWindow();
            adminPanel.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Subscribers_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            new SubscribersWindow().ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    private void Publications_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            new PublicationsWindow().ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    private void Subscriptions_Click(object sender, RoutedEventArgs e)
    {
        new SubscriptionsWindow().ShowDialog();
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Excel files (*.xlsx)|*.xlsx";

            if (dialog.ShowDialog() == true)
            {
                ExcelImportService.Import(dialog.FileName);
                MessageBox.Show("Импорт успешен!");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    
    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        AuthService.Logout();
        
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        
        Close();
    }
}