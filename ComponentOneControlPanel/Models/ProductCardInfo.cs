namespace ComponentOneControlPanel.Models;

public sealed partial record ProductCardInfo(
    string Id,
    string ProductId,
    string Name,
    string Badge,
    string BadgeColor,
    string Version,
    string Framework,
    string Category,
    bool InstallEnabled)
{
    public bool IsInstalled { get; set; }

    public bool IsInstalling { get; set; }

    public double InstallProgress { get; set; }

    public string InstallStatus { get; set; } = string.Empty;

    public string ActionButtonText =>
        IsInstalled ? "Update" : "Install";

    public bool ModifyEnabled =>
        IsInstalled;
}
