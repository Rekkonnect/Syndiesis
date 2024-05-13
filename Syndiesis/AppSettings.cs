using Syndiesis.Controls.SyntaxVisualization.Creation;
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
            return true;
        }
        catch
        {
            // TODO log
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
            return true;
        }
        catch
        {
            // TODO log
            return false;
        }
    }
    #endregion
}
