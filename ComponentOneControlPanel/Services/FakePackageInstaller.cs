namespace ComponentOneControlPanel.Services;

public sealed class FakePackageInstaller : IPackageInstaller
{
    private readonly ILogger<FakePackageInstaller> _logger;

    public FakePackageInstaller(ILogger<FakePackageInstaller> logger)
    {
        _logger = logger;
    }

    public async Task<InstallResult> InstallAsync(
        ProductCardInfo product,
        IProgress<(double percent, string status)> progress,
        CancellationToken cancellationToken)
    {
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "C1ControlPanelPOC",
            product.Id,
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempDirectory);

        for (var i = 0; i <= 100; i += 10)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress.Report((i, $"Downloading {product.Name}... {i}%"));

            await Task.Delay(250, cancellationToken);
        }

        var packageDirectory = Path.Combine(tempDirectory, "Packages");

        Directory.CreateDirectory(packageDirectory);

        var fakePackage = Path.Combine(
            packageDirectory,
            $"{product.ProductId}.{product.Version}.nupkg");

        await File.WriteAllTextAsync(
            fakePackage,
            "Fake package content",
            cancellationToken);

        _logger.LogInformation(
            "Fake install completed for {Product} in {Directory}",
            product.Name,
            tempDirectory);

        return new InstallResult(
            tempDirectory,
            new[] { packageDirectory });
    }
}
