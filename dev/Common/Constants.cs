﻿namespace ArtWork.Common;
public static class Constants
{
    public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductNameAndVersion);
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");

    public static readonly string AppName = ProcessInfoHelper.ProductNameAndVersion;

    public static readonly string ArtWorkBaseUrl = "https://kraken66.blob.core.windows.net/images4000xn/";
    public static readonly string JsonBaseUrl = "https://kraken66.blob.core.windows.net/tileinfo/";

    private static readonly string ArtWorkFolderName = "ArtWork";
    public static readonly string ArtWorkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), AppName, ArtWorkFolderName);
    public static readonly string NudesPath = "Assets/Files/nudes.txt";
}
