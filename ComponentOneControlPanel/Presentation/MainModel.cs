namespace ComponentOneControlPanel.Presentation;

public partial record MainModel
{
    private readonly ProductCatalogService _catalogService;
    private readonly IPackageInstaller _installer;
    private readonly ILogger<MainModel> _logger;

    private readonly List<ProductCardInfo> _products = [];

    public MainModel(
        ProductCatalogService catalogService,
        IPackageInstaller installer,
        ILogger<MainModel> logger)
    {
        _catalogService = catalogService;
        _installer = installer;
        _logger = logger;

        SelectedCategory = "All";
        SelectedTab = "Available";

        Status = string.Empty;
    }

    public string SelectedCategory { get; private set; }

    public string SelectedTab { get; private set; }

    public string Status { get; private set; }

    public double ProgressPercent { get; private set; }

    public bool IsBusy { get; private set; }

    public string SnackbarMessage { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        if (_products.Count > 0)
        {
            return;
        }

        var products = await _catalogService.GetProductsAsync();

        _products.AddRange(products);
    }

    public IReadOnlyList<ProductCardInfo> GetVisibleProducts()
    {
        IEnumerable<ProductCardInfo> query = _products;

        query = SelectedTab switch
        {
            "Installed" => query.Where(x => x.IsInstalled),
            _ => query
        };

        query = SelectedCategory switch
        {
            "Desktop" => query.Where(x => x.Category == "Desktop"),
            "Web" => query.Where(x => x.Category == "Web"),
            "Mobile" => query.Where(x => x.Category == "Mobile"),
            "Service Components" => query.Where(x => x.Category == "Service Components"),
            _ => query
        };

        return query.ToList();
    }

    public async Task SelectCategory(string category)
    {
        SelectedCategory = category;

        await Task.CompletedTask;
    }

    public async Task SelectTab(string tab)
    {
        SelectedTab = tab;

        await Task.CompletedTask;
    }

    public async Task Install(
        ProductCardInfo product,
        CancellationToken cancellationToken)
    {
        if (product.IsInstalling)
        {
            return;
        }

        IsBusy = true;

        product.IsInstalling = true;
        product.InstallProgress = 0;
        product.InstallStatus = "Installing...";

        Status = $"Installing {product.Name}...";

        try
        {
            var progress = new Progress<(double percent, string status)>(
                x =>
                {
                    product.InstallProgress = x.percent;
                    product.InstallStatus = x.status;

                    ProgressPercent = x.percent;
                    Status = x.status;
                });

            await _installer.InstallAsync(
                product,
                progress,
                cancellationToken);

            product.IsInstalled = true;
            product.IsInstalling = false;
            product.InstallProgress = 100;
            product.InstallStatus = "Installed";

            Status = $"{product.Name} installed";

            SnackbarMessage = $"{product.Name} installed";

            _logger.LogInformation(
                "{Product} installed",
                product.Name);
        }
        catch (Exception ex)
        {
            product.IsInstalling = false;
            product.InstallStatus = "Failed";

            Status = $"Install failed: {ex.Message}";

            _logger.LogError(
                ex,
                "Install failed for {Product}",
                product.Name);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ClearSnackbar()
    {
        SnackbarMessage = string.Empty;
    }
}
