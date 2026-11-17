namespace ComponentOneControlPanel.Services;

public sealed record InstallResult(
    string TempDirectory,
    IReadOnlyList<string> PackageDirectories);
