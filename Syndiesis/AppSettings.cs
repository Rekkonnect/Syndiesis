using Serilog;
using Syndiesis.Core.DisplayAnalysis;
using System;
using System.IO;
using System.Text.Json;

namespace Syndiesis;

public sealed class AppSettings
{
    public const string DefaultPath = "appsettings.json";

    public static AppSettings Instance = new();

    public NodeLineCreationOptions NodeLineOptions = new();
    public IndentationOptions IndentationOptions = new();

    public TimeSpan UserInputDelay = TimeSpan.FromMilliseconds(600);

    public int RecursiveExpansionDepth = 4;

    public bool EnableExpandingAllNodes = false;

    #region Persistence
    public static bool TryLoad(string path = DefaultPath)
    {
        try
        {
            var json = File.ReadAllText(path);
            var returned = JsonSerializer.Deserialize(
                json, AppSettingsSerializationContext.CustomDefault.AppSettings);
            if (returned is null)
                return false;

            Instance = returned;
            Log.Information($"Settings loaded from '{path}'");
            return true;
        }
        catch (Exception ex)
        {
            App.Current.ExceptionListener.HandleException(ex, $"Failed to load settings from '{path}'");
            return false;
        }
    }

    public static bool TrySave(string path = DefaultPath)
    {
        try
        {
            var json = JsonSerializer.Serialize(
                Instance, AppSettingsSerializationContext.CustomDefault.AppSettings);
            File.WriteAllText(path, json);
            Log.Information($"Settings saved to '{path}'");
            return true;
        }
        catch (Exception ex)
        {
            App.Current.ExceptionListener.HandleException(ex, $"Failed to save settings to '{path}'");
            return false;
        }
    }
    #endregion
}
