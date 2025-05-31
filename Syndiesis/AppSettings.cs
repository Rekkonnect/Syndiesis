using Serilog;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;
using System.IO;
using System.Text.Json;

namespace Syndiesis;

public sealed class AppSettings
{
    public const string DefaultPath = "appsettings.json";

    public static AppSettings Instance = new();

    #region Settings
    public AnalysisNodeCreationOptions NodeLineOptions = new();
    public IndentationOptions IndentationOptions = new();
    public UpdateOptions UpdateOptions = new();

    public StylePreferences NodeColorPreferences = new();
    public ColorizationPreferences ColorizationPreferences = new();

    public bool ShowWhitespaceGlyphs = true;
    public bool WordWrap = true;

    public bool EnableColorization = true;
    public bool EnableSemanticColorization = true;
    public bool AutomaticallyDetectLanguage = true;

    public bool DiagnosticsEnabled = true;

    public TimeSpan UserInputDelay = TimeSpan.FromMilliseconds(600);
    public TimeSpan HoverInfoDelay = TimeSpan.FromMilliseconds(400);

    public int RecursiveExpansionDepth = 4;

    public AnalysisNodeKind DefaultAnalysisTab = AnalysisNodeKind.Syntax;
    public AnalysisViewKind DefaultAnalysisView = AnalysisViewKind.Tree;
    #endregion

    #region Persistence
    public static async Task<bool> TryLoad(string path = DefaultPath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path);
            var returned = await Dispatcher.UIThread.InvokeAsync(() =>
            {
                return JsonSerializer.Deserialize<AppSettings>(
                    json, AppSettingsSerialization.DefaultOptions);
            });
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

    public static async Task<bool> TrySave(string path = DefaultPath)
    {
        try
        {
            var json = JsonSerializer.Serialize(
                Instance, AppSettingsSerialization.DefaultOptions);
            await File.WriteAllTextAsync(path, json);
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
