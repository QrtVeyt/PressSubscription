using System.Windows;
using PressSubscription.Data;

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
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "DB ERROR");
        }

        var main = new MainWindow();
        main.Show();
    }
}