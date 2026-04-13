using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using PressSubscription.Data;
using PressSubscription.Models;
using PressSubscription.Services;

namespace PressSubscription.Views;

// Конвертер для отображения роли
public class RoleToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string role)
        {
            return role == "Admin" ? "👑 Администратор" : "👤 Пользователь";
        }
        return "❓ Неизвестно";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Конвертер для отображения статуса активности
public class ActiveToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "✅ Активен" : "🔒 Заблокирован";
        }
        return "❓ Неизвестно";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class AdminPanelWindow : Window
{
    private readonly AppDbContext _db = new();
    private ObservableCollection<User> _users = new();
    private User? _currentUser;

    public AdminPanelWindow()
    {
        InitializeComponent();
        _currentUser = AuthService.GetCurrentUser();
        LoadData();
        UpdateAdminCount();
    }

    private void LoadData()
    {
        var list = _db.Users.OrderBy(u => u.Id).ToList();
        _users.Clear();
        foreach (var user in list)
        {
            _users.Add(user);
        }
        UsersGrid.ItemsSource = _users;
    }

    private void UpdateAdminCount()
    {
        var adminCount = _db.Users.Count(u => u.Role == "Admin");
        AdminCountText.Text = $"Всего администраторов: {adminCount}";
    }

    private User? GetSelectedUser()
    {
        if (UsersGrid.SelectedItem is not User user)
        {
            MessageBox.Show("Выберите пользователя", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        return user;
    }

    private bool CheckNotSelf(User user)
    {
        if (_currentUser != null && user.Id == _currentUser.Id)
        {
            MessageBox.Show("Нельзя изменять права самого себя!", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private void MakeAdmin_Click(object sender, RoutedEventArgs e)
    {
        var user = GetSelectedUser();
        if (user == null || !CheckNotSelf(user)) return;

        if (user.Role == "Admin")
        {
            MessageBox.Show("Пользователь уже является администратором", "Информация", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"Назначить пользователя \"{user.FullName}\" администратором?", 
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            user.Role = "Admin";
            _db.SaveChanges();
            LoadData();
            UpdateAdminCount();
            MessageBox.Show("Права администратора выданы!", "Успех", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void RemoveAdmin_Click(object sender, RoutedEventArgs e)
    {
        var user = GetSelectedUser();
        if (user == null || !CheckNotSelf(user)) return;

        if (user.Role != "Admin")
        {
            MessageBox.Show("Пользователь не является администратором", "Информация", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Проверяем, не последний ли это админ
        var adminCount = _db.Users.Count(u => u.Role == "Admin");
        if (adminCount <= 1)
        {
            MessageBox.Show("Нельзя снять права администратора с последнего администратора!", 
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Снять права администратора с пользователя \"{user.FullName}\"?\n\nОн станет обычным пользователем.", 
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            user.Role = "User";
            _db.SaveChanges();
            LoadData();
            UpdateAdminCount();
            MessageBox.Show("Права администратора сняты!", "Успех", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BlockUser_Click(object sender, RoutedEventArgs e)
    {
        var user = GetSelectedUser();
        if (user == null || !CheckNotSelf(user)) return;

        if (!user.IsActive)
        {
            MessageBox.Show("Пользователь уже заблокирован", "Информация", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"Заблокировать пользователя \"{user.FullName}\"?\n\nЗаблокированный пользователь не сможет войти в систему.", 
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            user.IsActive = false;
            _db.SaveChanges();
            LoadData();
            MessageBox.Show("Пользователь заблокирован!", "Успех", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void UnblockUser_Click(object sender, RoutedEventArgs e)
    {
        var user = GetSelectedUser();
        if (user == null) return;

        if (user.IsActive)
        {
            MessageBox.Show("Пользователь уже активен", "Информация", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"Разблокировать пользователя \"{user.FullName}\"?", 
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            user.IsActive = true;
            _db.SaveChanges();
            LoadData();
            MessageBox.Show("Пользователь разблокирован!", "Успех", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
        UpdateAdminCount();
        MessageBox.Show("Список пользователей обновлён", "Обновление", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}