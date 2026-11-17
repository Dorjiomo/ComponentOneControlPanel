namespace ComponentOneControlPanel.Services;

public interface IPackageInstaller
{
    Task<InstallResult> InstallAsync(
        ProductCardInfo product,
        IProgress<(double percent, string status)> progress,
        CancellationToken cancellationToken);
}
