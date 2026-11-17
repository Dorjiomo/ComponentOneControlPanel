namespace ComponentOneControlPanel.Models;

public sealed partial record ProductCatalogRoot(
    List<CatalogRootItem> Root);

public sealed partial record CatalogRootItem(
    string? HelpLink,
    List<CatalogProduct> Products);

public sealed partial record CatalogProduct(
    List<CatalogEdition> Editions);

public sealed partial record CatalogEdition(
    string Name,
    string Id,
    string SKU,
    string Category,
    string Description,
    string HelpLink,
    List<CatalogVersion> Versions);

public sealed partial record CatalogVersion(
    string Version,
    List<CatalogDirectory> Directories);

public sealed partial record CatalogDirectory(
    string Base,
    string Path,
    List<CatalogExtract> Extracts);

public sealed partial record CatalogExtract(
    string Url,
    string ExtractVal,
    bool InstallSamples = false);
