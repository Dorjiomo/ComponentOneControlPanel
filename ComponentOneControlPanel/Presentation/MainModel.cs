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

    public IReadOnlyList<string> Categories =>
    _products
        .Select(x => x.Category)
        .Distinct()
        .OrderBy(x => x)
        .Prepend("All")
        .ToList();

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

        product.IsInstalling = true;
        product.InstallProgress = 0;

        try
        {
            var progress = new Progress<(double percent, string status)>(
                x =>
                {
                    product.InstallProgress = x.percent;

                    ProgressPercent = x.percent;

                    Status = x.status;
                });

            await _installer.InstallAsync(
                product,
                progress,
                cancellationToken);

            product.IsInstalled = true;
        }
        catch (Exception ex)
        {
            Status = $"Install failed: {ex.Message}";

            _logger.LogError(
                ex,
                "Install failed for {Product}",
                product.Name);
        }
        finally
        {
            product.IsInstalling = false;

            product.InstallProgress = 0;

            ProgressPercent = 0;

            Status = string.Empty;

            SnackbarMessage = $"{product.Name} installed";
        }
    }
    public async Task Uninstall(ProductCardInfo product)
    {
        product.IsUninstalling = true;

        for (var i = 0; i <= 100; i += 10)
        {
            product.InstallProgress = i;

            await Task.Delay(150);
        }

        product.IsInstalled = false;

        product.IsUninstalling = false;

        product.InstallProgress = 0;

        SnackbarMessage = $"{product.Name} uninstalled";
    }

    public void ClearSnackbar()
    {
        SnackbarMessage = string.Empty;
    }
}
