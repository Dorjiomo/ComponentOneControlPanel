using PackageDownloaderCore.Feeds;
using PackageDownloaderCore.Models;
using PackageDownloaderCore.Services;

namespace ComponentOneControlPanel.Services;

public sealed class PackageDownloaderAdapter : IPackageInstaller
{
    private readonly ILogger<PackageDownloaderAdapter> _logger;

    public PackageDownloaderAdapter(
        ILogger<PackageDownloaderAdapter> logger)
    {
        _logger = logger;
    }

    public async Task<InstallResult> InstallAsync(
        ProductCardInfo product,
        IProgress<(double percent, string status)> progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(progress);

        if (string.IsNullOrWhiteSpace(product.DownloadProductId))
        {
            throw new InvalidOperationException(
                $"Product '{product.Name}' has no PackageDownloaderCore product ID.");
        }

        if (string.IsNullOrWhiteSpace(product.DownloadVersion))
        {
            throw new InvalidOperationException(
                $"Product '{product.Name}' has no PackageDownloaderCore release version.");
        }

        if (string.IsNullOrWhiteSpace(product.DownloadLanguage))
        {
            throw new InvalidOperationException(
                $"Product '{product.Name}' has no PackageDownloaderCore language.");
        }

        var tempRootDirectory = Path.Combine(
            Path.GetTempPath(),
            "C1Nugets");

        var tempDirectory = Path.Combine(
            tempRootDirectory,
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempRootDirectory);

        var frameworks = new List<string>();

        if (!string.IsNullOrWhiteSpace(product.DownloadFramework))
        {
            frameworks.Add(product.DownloadFramework);
        }

        var request = new PackageDownloadRequest
        {
            Language = product.DownloadLanguage,
            ProductId = product.DownloadProductId,
            Version = product.DownloadVersion,
            Frameworks = frameworks,
            OutputDirectory = tempDirectory
        };

        var packageProgress = new Progress<PackageDownloadProgress>(
            packageDownloadProgress =>
            {
                progress.Report((
                    packageDownloadProgress.Percentage,
                    packageDownloadProgress.Message));
            });

        _logger.LogInformation(
            "Starting PackageDownloaderCore download. ProductId={ProductId}, Version={Version}, Frameworks={Frameworks}, Output={OutputDirectory}",
            request.ProductId,
            request.Version,
            string.Join(", ", request.Frameworks ?? []),
            request.OutputDirectory);

        // Do not use PackageFeedSelector from the old WPF Control Panel.
        // It is internal, WPF-dependent, and not safe for macOS/Uno.
        // The POC intentionally uses the public NuGet.org feed.
        var packageFeed = new PackageFeedFactory().CreateNuGetOrg();

        var factory = new PackageDownloadServiceFactory();

        using var service = factory.Create(packageFeed);

        var result = await service.DownloadAsync(
            request,
            packageProgress,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Package download completed. Output={OutputDirectory}; PackageDirectories={PackageDirectories}",
            result.OutputDirectory,
            string.Join("; ", result.PackageDirectories));

        return new InstallResult(
            result.OutputDirectory,
            result.PackageDirectories);
    }
}
