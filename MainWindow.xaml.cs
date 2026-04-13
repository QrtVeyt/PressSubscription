using Microsoft.Win32;
using PressSubscription.Services;
using PressSubscription.Views;
using System.Windows;

namespace PressSubscription;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
            OpenFileDialog dialog = new();

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
}