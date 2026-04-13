using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace PressSubscription.Services;

public static class CacheService
{
    private static readonly ConcurrentDictionary<string, CacheItem> _cache = new();
    private static readonly Timer _cleanupTimer;
    private static readonly string SessionFilePath;
    private const string SessionKey = "SavedUser";

    static CacheService()
    {
        var appDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        Directory.CreateDirectory(appDataFolder);
        SessionFilePath = Path.Combine(appDataFolder, "session.json");
        
        _cleanupTimer = new Timer(CleanupExpired, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public static void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var cacheItem = new CacheItem
        {
            Value = value,
            ExpiresAt = DateTime.Now.Add(expiration ?? TimeSpan.FromMinutes(30))
        };
        
        _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        
        if (key == SessionKey && value is Models.User user)
        {
            SaveSessionToDisk(user);
        }
    }

    public static T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var cacheItem))
        {
            if (cacheItem.ExpiresAt > DateTime.Now)
            {
                return (T?)cacheItem.Value;
            }
            
            _cache.TryRemove(key, out _);
        }
        
        return default;
    }

    public static void Remove(string key)
    {
        _cache.TryRemove(key, out _);
        
        if (key == SessionKey)
        {
            DeleteSessionFromDisk();
        }
    }

    public static void Clear()
    {
        _cache.Clear();
        DeleteSessionFromDisk();
    }

    public static bool Exists(string key)
    {
        if (_cache.TryGetValue(key, out var cacheItem))
        {
            if (cacheItem.ExpiresAt > DateTime.Now)
            {
                return true;
            }
            
            _cache.TryRemove(key, out _);
        }
        
        return false;
    }

    public static Models.User? LoadSavedSession()
    {
        try
        {
            if (File.Exists(SessionFilePath))
            {
                var json = File.ReadAllText(SessionFilePath);
                var sessionData = JsonSerializer.Deserialize<SavedSession>(json);
                
                if (sessionData != null && sessionData.SavedAt > DateTime.Now.AddDays(-7))
                {
                    using var db = new Data.AppDbContext();
                    var user = db.Users.FirstOrDefault(u => u.Username == sessionData.Username && u.IsActive);
                    
                    if (user != null)
                    {
                        return user;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сессии: {ex.Message}");
        }
        
        DeleteSessionFromDisk();
        return null;
    }

    private static void SaveSessionToDisk(Models.User user)
    {
        try
        {
            var sessionData = new SavedSession
            {
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Email = user.Email,
                SavedAt = DateTime.Now
            };
            
            var json = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SessionFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения сессии: {ex.Message}");
        }
    }

    private static void DeleteSessionFromDisk()
    {
        try
        {
            if (File.Exists(SessionFilePath))
            {
                File.Delete(SessionFilePath);
            }
        }
        catch { }
    }

    private static void CleanupExpired(object? state)
    {
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var cacheItem) && cacheItem.ExpiresAt <= DateTime.Now)
            {
                _cache.TryRemove(key, out _);
            }
        }
    }

    private class CacheItem
    {
        public object? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
    
    private class SavedSession
    {
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public string? Email { get; set; }
        public DateTime SavedAt { get; set; }
    }
}