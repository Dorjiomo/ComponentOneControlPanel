using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions.Reactive.Bindings;
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
        var model = ViewModel?.Model;

        if (model is null)
        {
            return;
        }

        var products =
            model.GetVisibleProducts();

        ProductsRepeater.ItemsSource = products;

        CategoryRepeater.ItemsSource =
            model.Categories;

        var isEmpty = products.Count == 0;

        EmptyTextBlock.Visibility =
            isEmpty
                ? Visibility.Visible
                : Visibility.Collapsed;

        if (isEmpty)
        {
            EmptyTextBlock.Text =
                model.SelectedTab == "Installed"
                    ? "No installed products."
                    : "No available products match this filter.";
        }

        StatusTextBlock.Text =
            model.Status;

        InstallProgressBar.Value =
            model.ProgressPercent;

        UpdateTabStates();

        await Task.CompletedTask;
    }

    private async void CategoryButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var model = ViewModel?.Model;

        if (model is null)
        {
            return;
        }

        if (sender is not Button button)
        {
            return;
        }

        if (button.DataContext is not string category)
        {
            return;
        }

        await model.SelectCategory(category);

        await RefreshUiAsync();
    }

    private void CategoryButton_Loaded(
    object sender,
    RoutedEventArgs e)
    {
        ApplyCategoryButtonStyle(sender as Button);
    }

    private void ApplyCategoryButtonStyle(Button? button)
    {
        if (button is null)
        {
            return;
        }

        var model = ViewModel?.Model;

        if (model is null)
        {
            return;
        }

        var category =
            button.DataContext?.ToString();

        var isSelected =
            category == model.SelectedCategory;

        button.Background =
            new SolidColorBrush(
                isSelected
                    ? Color.FromArgb(255, 0, 120, 212)
                    : Colors.White);

        button.Foreground =
            new SolidColorBrush(
                isSelected
                    ? Colors.White
                    : Color.FromArgb(255, 64, 64, 64));

        button.BorderBrush =
            new SolidColorBrush(
                isSelected
                    ? Color.FromArgb(255, 0, 120, 212)
                    : Color.FromArgb(255, 208, 208, 208));
    }

    private void CategoryButton_PointerEntered(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var model = ViewModel?.Model;

        if (model is null)
        {
            return;
        }

        var category =
            button.DataContext?.ToString();

        if (category == model.SelectedCategory)
        {
            return;
        }

        button.Background =
            new SolidColorBrush(
                Color.FromArgb(255, 245, 245, 245));
    }

    private void CategoryButton_PointerExited(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ApplyCategoryButtonStyle(sender as Button);
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
    private async void Uninstall_Click(
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

        await ViewModel.Model.Uninstall(product);

        await RefreshUiAsync();

        await ShowSnackbarAsync(
            ViewModel.Model.SnackbarMessage);

        ViewModel.Model.ClearSnackbar();
    }
}
