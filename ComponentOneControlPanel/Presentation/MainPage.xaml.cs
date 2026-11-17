using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ComponentOneControlPanel.Presentation;

public sealed partial class MainPage : Page
{
    private MainViewModel? ViewModel =>
        DataContext as MainViewModel;

    public MainPage()
    {
        this.InitializeComponent();

        DataContextChanged += MainPage_DataContextChanged;
    }

    private async void MainPage_DataContextChanged(
        FrameworkElement sender,
        DataContextChangedEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        await ViewModel.Model.InitializeAsync();

        await RefreshUiAsync();
    }

    private async Task RefreshUiAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var products =
            ViewModel.Model.GetVisibleProducts();

        ProductsRepeater.ItemsSource = products;

        var isEmpty = products.Count == 0;

        EmptyTextBlock.Visibility =
            isEmpty
                ? Visibility.Visible
                : Visibility.Collapsed;

        if (isEmpty)
        {
            EmptyTextBlock.Text =
                ViewModel.Model.SelectedTab == "Installed"
                    ? "No installed products."
                    : "No available products match this filter.";
        }

        StatusTextBlock.Text =
            ViewModel.Model.Status;

        InstallProgressBar.Value =
            ViewModel.Model.ProgressPercent;

        UpdateFilterButtonStates();

        UpdateTabStates();

        await Task.CompletedTask;
    }

    private async void Install_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (sender is not Button button)
        {
            return;
        }

        if (button.DataContext is not ProductCardInfo product)
        {
            return;
        }

        await ViewModel.Model.Install(
            product,
            CancellationToken.None);

        await RefreshUiAsync();

        await ShowSnackbarAsync(
            ViewModel.Model.SnackbarMessage);

        ViewModel.Model.ClearSnackbar();
    }

    private async Task ShowSnackbarAsync(
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        SnackbarTextBlock.Text = message;

        SnackbarBorder.Visibility =
            Visibility.Visible;

        await Task.Delay(3000);

        SnackbarBorder.Visibility =
            Visibility.Collapsed;
    }

    private async void InstalledTab_Click(
        object sender,
        RoutedEventArgs e)
    {
        await ChangeTab("Installed");
    }

    private async void AvailableTab_Click(
        object sender,
        RoutedEventArgs e)
    {
        await ChangeTab("Available");
    }

    private async Task ChangeTab(string tab)
    {
        if (ViewModel is null)
        {
            return;
        }

        await ViewModel.Model.SelectTab(tab);

        await RefreshUiAsync();
    }

    private async void AllFilter_Click(
        object sender,
        RoutedEventArgs e)
    {
        await ChangeCategory("All");
    }

    private async void DesktopFilter_Click(
        object sender,
        RoutedEventArgs e)
    {
        await ChangeCategory("Desktop");
    }

    private async void WebFilter_Click(
        object sender,
        RoutedEventArgs e)
    {
        await ChangeCategory("Web");
    }

    private async void MobileFilter_Click(
        object sender,
        RoutedEventArgs e)
    {
        await ChangeCategory("Mobile");
    }

    private async void ServiceFilter_Click(
        object sender,
        RoutedEventArgs e)
    {
        await ChangeCategory("Service Components");
    }

    private async Task ChangeCategory(
        string category)
    {
        if (ViewModel is null)
        {
            return;
        }

        await ViewModel.Model.SelectCategory(
            category);

        await RefreshUiAsync();
    }

    private void UpdateTabStates()
    {
        if (ViewModel is null)
        {
            return;
        }

        var selected =
            ViewModel.Model.SelectedTab;

        SetTopTabStyle(
            InstalledTabButton,
            selected == "Installed");

        SetTopTabStyle(
            AvailableTabButton,
            selected == "Available");
    }

    private void UpdateFilterButtonStates()
    {
        if (ViewModel is null)
        {
            return;
        }

        var selected =
            ViewModel.Model.SelectedCategory;

        SetFilterButtonStyle(
            AllButton,
            selected == "All");

        SetFilterButtonStyle(
            DesktopButton,
            selected == "Desktop");

        SetFilterButtonStyle(
            WebButton,
            selected == "Web");

        SetFilterButtonStyle(
            MobileButton,
            selected == "Mobile");

        SetFilterButtonStyle(
            ServiceButton,
            selected == "Service Components");
    }

    private static void SetTopTabStyle(
        Button button,
        bool selected)
    {
        button.Foreground =
            new SolidColorBrush(
                selected
                    ? Colors.Black
                    : Color.FromArgb(
                        255,
                        110,
                        110,
                        110));

        button.FontWeight =
            selected
                ? Microsoft.UI.Text.FontWeights.SemiBold
                : Microsoft.UI.Text.FontWeights.Normal;
    }

    private static void SetFilterButtonStyle(
        Button button,
        bool selected)
    {
        button.Background =
            new SolidColorBrush(
                selected
                    ? Color.FromArgb(
                        255,
                        0,
                        120,
                        212)
                    : Color.FromArgb(
                        255,
                        247,
                        247,
                        247));

        button.BorderBrush =
            new SolidColorBrush(
                selected
                    ? Color.FromArgb(
                        255,
                        0,
                        120,
                        212)
                    : Color.FromArgb(
                        255,
                        208,
                        208,
                        208));

        button.Foreground =
            new SolidColorBrush(
                selected
                    ? Colors.White
                    : Color.FromArgb(
                        255,
                        68,
                        68,
                        68));
    }
}
