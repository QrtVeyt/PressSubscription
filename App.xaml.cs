using System;
using System.Linq;
using System.Windows;
using PressSubscription.Data;
using PressSubscription.Services;
using PressSubscription.Views;

namespace PressSubscription;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                var result = MessageBox.Show(
                    "База данных пуста. Создать учётную запись администратора?\n\n" +
                    "Логин: admin\n" +
                    "Пароль: admin\n\n" +
                    "Вы сможете изменить пароль позже.",
                    "Первый запуск",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    AuthService.CreateUser("admin", "admin", "Admin", "Администратор", "admin@example.com");
                    MessageBox.Show("Администратор создан!\n\nЛогин: admin\nПароль: admin", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Вы можете создать администратора позже через меню.", "Информация", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка инициализации БД: {ex.Message}", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}