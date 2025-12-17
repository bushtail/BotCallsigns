using JetBrains.Annotations;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace BotCallsigns;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1), UsedImplicitly]
public class BotCallsigns : IOnLoad
{
    public Task OnLoad()
    {
        
        return Task.CompletedTask;
    }
}