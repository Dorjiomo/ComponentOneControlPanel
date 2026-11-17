using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ComponentOneControlPanel.Models;

public sealed partial class ProductCardInfo : INotifyPropertyChanged
{
    public ProductCardInfo(
        string id,
        string productId,
        string name,
        string badge,
        string badgeColor,
        string version,
        string framework,
        string category,
        bool installEnabled,
        string downloadProductId,
        string downloadVersion,
        string downloadFramework,
        string downloadLanguage)
    {
        Id = id;
        ProductId = productId;
        Name = name;
        Badge = badge;
        BadgeColor = badgeColor;
        Version = version;
        Framework = framework;
        Category = category;
        InstallEnabled = installEnabled;
        DownloadProductId = downloadProductId;
        DownloadVersion = downloadVersion;
        DownloadFramework = downloadFramework;
        DownloadLanguage = downloadLanguage;
    }

    public string Id { get; }

    // UI/catalog identifier only. It is not necessarily the PackageDownloaderCore identifier.
    public string ProductId { get; }

    public string Name { get; }

    public string Badge { get; }

    public string BadgeColor { get; }

    // UI display version.
    public string Version { get; }

    // UI display framework.
    public string Framework { get; }

    public string Category { get; }

    public bool InstallEnabled { get; }

    // Exact PackageDownloaderCore request values.
    public string DownloadProductId { get; }

    public string DownloadVersion { get; }

    public string DownloadFramework { get; }

    public string DownloadLanguage { get; }

    private bool _isInstalled;

    public bool IsInstalled
    {
        get => _isInstalled;
        set
        {
            _isInstalled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActionButtonText));
            OnPropertyChanged(nameof(ModifyEnabled));
            OnPropertyChanged(nameof(UninstallEnabled));
        }
    }

    private bool _isInstalling;

    public bool IsInstalling
    {
        get => _isInstalling;
        set
        {
            _isInstalling = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowProgress));
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    private bool _isUninstalling;

    public bool IsUninstalling
    {
        get => _isUninstalling;
        set
        {
            _isUninstalling = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowProgress));
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    private double _installProgress;

    public double InstallProgress
    {
        get => _installProgress;
        set
        {
            _installProgress = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressPercentageText));
        }
    }

    public string ActionButtonText =>
        IsInstalled ? "Update" : "Install";

    public bool ModifyEnabled =>
        IsInstalled;

    public bool UninstallEnabled =>
        IsInstalled;

    public string ProgressText =>
        IsUninstalling ? "Uninstalling..." : "Installing...";

    public string ProgressPercentageText =>
        $"{InstallProgress:F0}%";

    public bool ShowProgress =>
        IsInstalling || IsUninstalling;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
