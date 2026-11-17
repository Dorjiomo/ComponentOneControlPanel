using System.Text.Json;
using Windows.Storage;

namespace ComponentOneControlPanel.Services;

public sealed class ProductCatalogService
{
    private readonly ILogger<ProductCatalogService> _logger;

    public ProductCatalogService(
        ILogger<ProductCatalogService> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductCardInfo>> GetProductsAsync()
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(
            new Uri("ms-appx:///Assets/Data/products.json"));

        var json = await FileIO.ReadTextAsync(file);

        var catalog = JsonSerializer.Deserialize<ProductCatalogRoot>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (catalog?.Root is null)
        {
            _logger.LogWarning(
                "The bundled product catalog was empty or could not be deserialized.");

            return [];
        }

        var products = new List<ProductCardInfo>();

        foreach (var root in catalog.Root)
        {
            foreach (var catalogProduct in root.Products ?? [])
            {
                foreach (var edition in catalogProduct.Editions ?? [])
                {
                    var latestVersion = edition.Versions?.FirstOrDefault();

                    if (latestVersion is null)
                    {
                        _logger.LogWarning(
                            "Skipping product {ProductName} because it has no version entry.",
                            edition.Name);

                        continue;
                    }

                    var isDownloadConfigured =
                        !string.IsNullOrWhiteSpace(edition.DownloadProductId) &&
                        !string.IsNullOrWhiteSpace(edition.DownloadVersion) &&
                        !string.IsNullOrWhiteSpace(edition.DownloadLanguage);

                    products.Add(new ProductCardInfo(
                        id: edition.Id,
                        productId: edition.Id,
                        name: edition.Name,
                        badge: edition.SKU,
                        badgeColor: GetBadgeColor(edition.SKU),
                        version: latestVersion.Version,
                        framework: GetDisplayFramework(edition),
                        category: edition.Category,
                        installEnabled: isDownloadConfigured,
                        downloadProductId: edition.DownloadProductId,
                        downloadVersion: edition.DownloadVersion,
                        downloadFramework: edition.DownloadFramework,
                        downloadLanguage: edition.DownloadLanguage));
                }
            }
        }

        _logger.LogInformation(
            "Loaded {ProductCount} products from bundled products.json.",
            products.Count);

        return products;
    }

    private static string GetDisplayFramework(CatalogEdition edition)
    {
        return string.Equals(
            edition.DownloadFramework,
            "60",
            StringComparison.OrdinalIgnoreCase)
            ? "net8.0"
            : string.IsNullOrWhiteSpace(edition.DownloadFramework)
                ? string.Empty
                : edition.DownloadFramework;
    }

    private static string GetBadgeColor(string sku)
    {
        return sku switch
        {
            "MVCCore" => "#C2185B",
            "ANMC" => "#C2185B",
            "B" => "#7B1FA2",
            "W" => "#00897B",
            "WF" => "#5D6470",
            _ => "#1976D2"
        };
    }
}
