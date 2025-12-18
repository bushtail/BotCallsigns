using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using Path = System.IO.Path;

namespace BotCallsigns;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1), UsedImplicitly]
public class BotCallsigns(ISptLogger<BotCallsigns> logger, DatabaseService databaseService, ModHelper modHelper) : IOnLoad
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private static UserDefinedCallsigns? _userDefinedCallsigns;
    private static string? _botCallsignsModPath;
    
    public Task OnLoad()
    {
        logger.Info("[Bot Callsigns] Loading names from config...");
        
        _botCallsignsModPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        _userDefinedCallsigns = GetOrCreateUserDefinedCallsigns();
        
        logger.Success($"[Bot Callsigns] Loaded {_userDefinedCallsigns.CallsignsBEAR.Length} BEAR callsigns and {_userDefinedCallsigns.CallsignsUSEC.Length} USEC callsigns from config.");
        var botTypes = databaseService.GetTables().Bots.Types;
        
        logger.Info("[Bot Callsigns] Pushing USEC names to bot type...");
        if (!botTypes.TryGetValue("usec", out var usecType) || usecType is null)
        {
            logger.Error("[Bot Callsigns] USEC bot type not found.");
            throw new NullReferenceException(nameof(usecType));
        }
        PushNamesToBotType(usecType, CallsignsUSEC.Names, _userDefinedCallsigns.CallsignsUSEC);
        logger.Success($"[Bot Callsigns] Pushed {CallsignsUSEC.Names.Length} names and {_userDefinedCallsigns.CallsignsUSEC.Length} user-defined names to USEC bot type.");
        
        logger.Info("[Bot Callsigns] Pushing BEAR names to bot type...");
        if (!botTypes.TryGetValue("bear", out var bearType) || bearType is null)
        {
            logger.Error("[Bot Callsigns] BEAR bot type not found.");
            throw new NullReferenceException(nameof(bearType));
        }
        PushNamesToBotType(bearType, CallsignsBEAR.Names, _userDefinedCallsigns.CallsignsBEAR);
        logger.Success($"[Bot Callsigns] Pushed {CallsignsBEAR.Names.Length} names and {_userDefinedCallsigns.CallsignsBEAR.Length} user-defined names to BEAR bot type.");
        
        logger.Success("[Bot Callsigns] Signaling Twitch Players that this mod is ready, if it is installed...");
        
        
        return Task.CompletedTask;
    }

    private void PushNamesToBotType(BotType botType, string[] constantNames, string[]? userDefinedNames)
    {
        var firstNames = botType.FirstNames;
        foreach (var name in constantNames)
        {
            if (firstNames.Contains(name)) continue;
            firstNames.Add(name);
        }

        if (userDefinedNames is null) return;
        
        foreach (var name in userDefinedNames)
        {
            if (firstNames.Contains(name)) continue;
            firstNames.Add(name);
        }
    }

    private void SignalTwitchPlayers()
    {
        try
        {
            var modsDirectory = Directory.GetParent(_botCallsignsModPath!)?.FullName;
            if (modsDirectory is null)
            {
                logger.Warning(
                    "[Bot Callsigns] Unable to find this mod's parent directory. Twitch Players most likely not installed.");
                return;
            }

            var pathToTwitchPlayersMod = Path.Combine(_botCallsignsModPath!, "TwitchPlayers");

            if (!Directory.Exists(pathToTwitchPlayersMod))
            {
                logger.Warning(
                    "[Bot Callsigns] Unable to find Twitch Players mod directory. Twitch Players mod not installed.");
                return;
            }

            var temp = Path.Combine(pathToTwitchPlayersMod, "Temp/");

            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }

            var flagPath = Path.Combine(temp, "mod.ready");

            if (File.Exists(flagPath))
            {
                logger.Success("[Bot Callsigns] Twitch Players mod flag already exists.");
                return;
            }

            File.Create(flagPath).Dispose();

            logger.Success("[Bot Callsigns] Twitch Players mod signalled that Boss Callsigns is ready for use.");
        }
        catch (Exception ex)
        {
            logger.Warning($"[Bot Callsigns] Unable to send mod ready flag for Twitch Players mod. Skipping due to: {ex}");
        }
    }

    private UserDefinedCallsigns GetOrCreateUserDefinedCallsigns()
    {
        const string fileName = "userDefinedCallsigns.json";
        if (File.Exists($"{_botCallsignsModPath}/{fileName}"))
            return modHelper.GetJsonDataFromFile<UserDefinedCallsigns>(_botCallsignsModPath!, fileName);
        var json = JsonSerializer.Serialize(new UserDefinedCallsigns());
        File.WriteAllText($"{_botCallsignsModPath}/{fileName}", json);
        return modHelper.GetJsonDataFromFile<UserDefinedCallsigns>(_botCallsignsModPath!, fileName);
    }
}