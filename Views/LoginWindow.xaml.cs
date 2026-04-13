using System;
using System.Windows;
using PressSubscription.Services;

namespace PressSubscription.Views;

public partial class LoginWindow : Window
{
    private bool _isLoginMode = true;

    public LoginWindow()
    {
        InitializeComponent();
        
        // Проверяем сохранённую сессию ПОСЛЕ инициализации компонентов
        Loaded += LoginWindow_Loaded;
    }

    private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Проверяем, есть ли сохранённая сессия
            if (AuthService.IsAuthenticated())
            {
                var user = AuthService.GetCurrentUser();
                if (user != null && user.IsActive)
                {
                    // Автоматически входим в главное окно
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    // Сессия недействительна, выходим
                    AuthService.Logout();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при проверке сессии: {ex.Message}");
            AuthService.Logout();
        }
    }

    private void SwitchToLoginMode(object sender, RoutedEventArgs e)
    {
        _isLoginMode = true;
        TitleText.Text = "Вход в систему";
        ActionButton.Content = "Войти";
        
        LoginPanel.Visibility = Visibility.Visible;
        RegisterPanel.Visibility = Visibility.Collapsed;
        
        LoginModeBtn.Background = System.Windows.Media.Brushes.LightGray;
        RegisterModeBtn.Background = System.Windows.Media.Brushes.LightGray;
    }

    private void SwitchToRegisterMode(object sender, RoutedEventArgs e)
    {
        _isLoginMode = false;
        TitleText.Text = "Регистрация";
        ActionButton.Content = "Зарегистрироваться";
        
        LoginPanel.Visibility = Visibility.Collapsed;
        RegisterPanel.Visibility = Visibility.Visible;
        
        RegUsernameBox.Text = "";
        RegPasswordBox.Password = "";
        RegConfirmPasswordBox.Password = "";
        RegFullNameBox.Text = "";
        RegEmailBox.Text = "";
        
        LoginModeBtn.Background = System.Windows.Media.Brushes.LightGray;
        RegisterModeBtn.Background = System.Windows.Media.Brushes.LightGray;
    }

    private void Action_Click(object sender, RoutedEventArgs e)
    {
        if (_isLoginMode)
        {
            Login();
        }
        else
        {
            Register();
        }
    }

    private void Login()
    {
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;
        
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Введите логин и пароль");
            return;
        }
        
        if (AuthService.Login(username, password))
        {
            OpenMainWindow();
        }
        else
        {
            ShowError("Неверный логин или пароль");
        }
    }

    private void Register()
    {
        var username = RegUsernameBox.Text.Trim();
        var password = RegPasswordBox.Password;
        var confirmPassword = RegConfirmPasswordBox.Password;
        var fullName = RegFullNameBox.Text.Trim();
        var email = RegEmailBox.Text.Trim();
        
        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Введите логин");
            return;
        }
        
        if (username.Length < 3)
        {
            ShowError("Логин должен содержать минимум 3 символа");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Введите пароль");
            return;
        }
        
        if (password.Length < 3)
        {
            ShowError("Пароль должен содержать минимум 3 символа");
            return;
        }
        
        if (password != confirmPassword)
        {
            ShowError("Пароли не совпадают");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(fullName))
        {
            ShowError("Введите ФИО");
            return;
        }
        
        if (AuthService.CreateUser(username, password, "User", fullName, email))
        {
            MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            SwitchToLoginMode(this, new RoutedEventArgs());
            UsernameBox.Text = username;
            PasswordBox.Password = "";
        }
        else
        {
            ShowError("Пользователь с таким логином уже существует");
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
        
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(3);
        timer.Tick += (s, args) =>
        {
            ErrorText.Visibility = Visibility.Collapsed;
            timer.Stop();
        };
        timer.Start();
    }

    private void OpenMainWindow()
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}