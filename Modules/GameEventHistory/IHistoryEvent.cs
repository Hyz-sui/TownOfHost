using System.Text;

namespace TownOfHost.Modules.GameEventHistory;

public interface IHistoryEvent
{
    public void AppendDiscordString(StringBuilder builder);
}
