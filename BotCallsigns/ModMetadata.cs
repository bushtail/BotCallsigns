using JetBrains.Annotations;
using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace BotCallsigns;

[UsedImplicitly]
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.harmonyzt.botcallsigns";
    public override string Name { get; init; } = "Bot Callsigns";
    public override string Author { get; init; } = "harmony";
    public override List<string>? Contributors { get; init; } = ["yuyui.moe", "Helldiver", "bushtail"];
    // Fetch assembly's version and apply it as our mod version.
    public override Version Version { get; init; } = new(typeof(ModMetadata).Assembly.GetName().Version?.ToString(3));
    public override Range SptVersion { get; init; } = new("~4.0.0");
    public override string? Url { get; init; } = "https://github.com/harmonyzt/BotCallsigns/tree/master";
    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; }
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Range>? ModDependencies { get; init; }
}