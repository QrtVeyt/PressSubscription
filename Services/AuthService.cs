using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PressSubscription.Data;
using PressSubscription.Models;

namespace PressSubscription.Services;

public static class AuthService
{
    private static User? _currentUser;
    private const string CurrentUserCacheKey = "SavedUser";
    private static bool _sessionLoaded = false;

    public static bool Login(string username, string password)
    {
        using var db = new AppDbContext();
        var passwordHash = HashPassword(password);
        
        var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
        
        if (user != null && user.IsActive)
        {
            _currentUser = user;
            user.LastLoginAt = DateTime.Now;
            db.SaveChanges();
            
            CacheService.Set(CurrentUserCacheKey, user, TimeSpan.FromDays(7));
            return true;
        }
        
        return false;
    }

    public static void Logout()
    {
        _currentUser = null;
        CacheService.Remove(CurrentUserCacheKey);
    }

    public static User? GetCurrentUser()
    {
        if (_currentUser != null)
        {
            return _currentUser;
        }
        
        // Загружаем сохранённую сессию только один раз
        if (!_sessionLoaded)
        {
            _sessionLoaded = true;
            _currentUser = CacheService.LoadSavedSession();
            if (_currentUser != null)
            {
                // Обновляем кеш
                CacheService.Set(CurrentUserCacheKey, _currentUser, TimeSpan.FromDays(7));
            }
        }
        
        return _currentUser;
    }

    public static bool IsAuthenticated()
    {
        var user = GetCurrentUser();
        return user != null && user.IsActive;
    }

    public static bool IsAdmin()
    {
        var user = GetCurrentUser();
        return user != null && user.Role == "Admin";
    }

    public static bool IsUser()
    {
        var user = GetCurrentUser();
        return user != null && user.Role == "User";
    }

    public static bool CreateUser(string username, string password, string role, string fullName, string? email = null)
    {
        using var db = new AppDbContext();
        
        if (db.Users.Any(u => u.Username == username))
        {
            return false;
        }
        
        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            Role = role,
            FullName = fullName,
            Email = email,
            CreatedAt = DateTime.Now,
            IsActive = true
        };
        
        db.Users.Add(user);
        db.SaveChanges();
        
        return true;
    }

    public static bool UserExists(string username)
    {
        using var db = new AppDbContext();
        return db.Users.Any(u => u.Username == username);
    }

    public static bool ChangePassword(string username, string oldPassword, string newPassword)
    {
        using var db = new AppDbContext();
        var oldHash = HashPassword(oldPassword);
        
        var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == oldHash);
        
        if (user == null)
        {
            return false;
        }
        
        user.PasswordHash = HashPassword(newPassword);
        db.SaveChanges();
        
        if (_currentUser != null && _currentUser.Username == username)
        {
            Logout();
        }
        
        return true;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}