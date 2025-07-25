using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.DependencyInjection;
using Serilog;
//When you can't even say my name
namespace Alua.Services.ViewModels;
/// <summary>
/// Main VM.
/// </summary>
public partial class SettingsVM  : ObservableObject
{
    #region Build info
    /// <summary>
    /// Shown in settings
    /// </summary>
    [JsonIgnore]
    public string BuildNumber = "0.3.2";

    /// <summary>
    /// Shown under build number, enables debug mode.
    /// Generally a reference to a song.
    /// </summary>
    [JsonIgnore]
    public string BuildString = "Sunday Sauce";
    #endregion

    #region Alua Data
    /// <summary>
    /// All games we have data for.
    /// </summary>
    [ObservableProperty, JsonInclude, JsonPropertyName("ScannedGames")]
    private Dictionary<string, Game> _games;
    
    /// <summary>
    /// Steam ID of user we are getting data for.
    /// </summary>
    [ObservableProperty, JsonInclude, JsonPropertyName("SteamUsername")]
    private string? _steamID;
    
    /// <summary>
    /// Steam ID of user we are getting data for.
    /// </summary>
    [ObservableProperty, JsonInclude, JsonPropertyName("RAUsername")]
    private string? _retroAchievementsUsername;

    /// <summary>
    /// Controls if we show the first run dialog or game list
    /// </summary>
    [ObservableProperty, JsonInclude, JsonPropertyName("Init")]
    private bool _initialised;
    #endregion

    #region FilterData
    [ObservableProperty, JsonPropertyName("FilterHideComplete")]
    private bool _hideComplete;

    [ObservableProperty, JsonPropertyName("FilterHideNoAchievements")]
    private bool _hideNoAchievements;

    [ObservableProperty, JsonPropertyName("FilterHideUnstarted")]
    private bool _hideUnstarted;

    [ObservableProperty, JsonPropertyName("FilterReverse")]
    private bool _reverse;

    [ObservableProperty, JsonPropertyName("FilterOrderBy")]
    private OrderBy _orderBy = OrderBy.Name;

    [ObservableProperty, JsonPropertyName("FilterSingleColumnLayout")]
    private bool _singleColumnLayout;
    #endregion

    public SettingsVM()
    {
        _games = new();
    }

    /// <summary>
    /// Adds or updates a game in the collection and notifies listeners.
    /// </summary>
    /// <param name="game">Game to add or update.</param>
    public void AddOrUpdateGame(Game game)
    {
        Games[game.Identifier] = game;
        OnPropertyChanged(nameof(Games));
    }


    /// <summary>
    /// Saves settings to disk
    /// </summary>
    public async Task Save()
    {
        try
        {
            //Get folder
            StorageFile settings = await ApplicationData.Current.LocalFolder.CreateFileAsync("Settings.json",
                CreationCollisionOption.ReplaceExisting);

            //Write to disk.
            Log.Information($"Saving settings to {settings.Path}");
            string json = JsonSerializer.Serialize(this);
            await File.WriteAllTextAsync(settings.Path, json);
            Log.Information("Saved settings.");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to save settings");
        }
    }

    ///<summary>
    /// Reads settings from disk
    /// </summary>
    public static SettingsVM Load()
    {
        try
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Settings.json");
            //Read from disk.
            Log.Information("Loading settings from path");
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                if (string.IsNullOrEmpty(content))
                {
                    Log.Warning("Settings file is empty, returning default settings.");
                    return new SettingsVM();
                }
                
                return JsonSerializer.Deserialize<SettingsVM>(content)!;
            }
            
            //File doesn't exist.
            Log.Information("Settings file not found.");
            return new SettingsVM();
        }
        catch (Exception ex) //Loading error.
        {
            Log.Error(ex, "Could not load settings");
            return new SettingsVM();
        }
    }
}
