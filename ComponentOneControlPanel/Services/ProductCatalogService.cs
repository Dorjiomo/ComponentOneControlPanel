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

        if (catalog is null)
        {
            return [];
        }

        var products = new List<ProductCardInfo>();

        foreach (var root in catalog.Root)
        {
            foreach (var product in root.Products)
            {
                foreach (var edition in product.Editions)
                {
                    var latestVersion = edition.Versions
                        .FirstOrDefault();

                    if (latestVersion is null)
                    {
                        continue;
                    }

                    products.Add(new ProductCardInfo(
                        id: edition.Id,
                        productId: edition.SKU,
                        name: edition.Name,
                        badge: edition.SKU,
                        badgeColor: GetBadgeColor(edition.SKU),
                        version: latestVersion.Version,
                        framework: "net8.0",
                        category: edition.Category,
                        installEnabled: edition.Name.Contains(
                            "MVC",
                            StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        return products;
    }

    private static string GetBadgeColor(string sku)
    {
        return sku switch
        {
            "ANMC" => "#C2185B",
            "B" => "#7B1FA2",
            "W" => "#00897B",
            "WF" => "#5D6470",
            _ => "#1976D2"
        };
    }
}
