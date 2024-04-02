using System;
using System.Collections.Generic;
using System.Text;
using TownOfHost.Attributes;

namespace TownOfHost.Modules.GameEventHistory;

public sealed class EventHistory : IHistoryEvent
{
    public static EventHistory CurrentInstance { get; private set; }

    [GameModuleInitializer]
    public static void NewGame()
    {
        CurrentInstance = new();
    }

    private readonly List<Event> events = [];

    public void AddEvent(Event @event)
    {
        events.Add(@event);
    }
    public void AppendDiscordString(StringBuilder builder)
    {
        foreach (var @event in events)
        {
            builder.Append(@event.Bullet);
            builder.Append(' ');
            builder.Append("<t:");
            var unixTime = (long)@event.UtcTime.Subtract(Epoch).TotalSeconds;
            builder.Append(unixTime);
            builder.Append(":T> ");
            @event.AppendDiscordString(builder);
            builder.AppendLine();
        }
    }
    public string ToDiscordString()
    {
        var builder = new StringBuilder();
        AppendDiscordString(builder);
        return builder.ToString();
    }

    private readonly static DateTime Epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
}
