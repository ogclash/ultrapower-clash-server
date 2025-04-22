using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

public static class BroadcastManager
{
    private static readonly List<string> Messages = new List<string>
    {
        "Welcome to the server!",
        "Don't forget to join our Discord!",
        "Check out the latest updates on our website!",
        "Be respectful to other players.",
        "Need help? Ask an admin!",
        "Enjoy your time on the server!",
        "Follow us on social media!",
        "Remember to save your progress!",
        "Invite your friends to play!",
        "Stay tuned for upcoming events!",
        "Have feedback? Let us know!",
        "Thank you for playing!"
    };

    private static System.Threading.Timer _broadcastTimer;
    public static bool IsBroadcastEnabled { get; private set; } = false;
    public static TimeSpan BroadcastInterval { get; private set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Starts the broadcast system.
    /// </summary>
    internal static void StartBroadcast(Level level)
    {
        if (!HasPrivileges(level, 4, "StartBroadcast")) return;

        if (IsBroadcastEnabled)
        {
            SendGlobalChatMessage(level, "Broadcast system is already enabled.");
            return;
        }

        IsBroadcastEnabled = true;
        _broadcastTimer = new System.Threading.Timer(SendRandomMessage, null, TimeSpan.Zero, BroadcastInterval);
        Logger.Write("Broadcast system started.");
        SendGlobalChatMessage(level, "Broadcast system started.");
    }

    /// <summary>
    /// Stops the broadcast system.
    /// </summary>
    internal static void StopBroadcast(Level level)
    {
        if (!HasPrivileges(level, 4, "StopBroadcast")) return;

        if (!IsBroadcastEnabled)
        {
            SendGlobalChatMessage(level, "Broadcast system is already disabled.");
            return;
        }

        IsBroadcastEnabled = false;
        _broadcastTimer?.Dispose();
        Logger.Write("Broadcast system stopped.");
        SendGlobalChatMessage(level, "Broadcast system stopped.");
    }

    /// <summary>
    /// Sets the broadcast interval.
    /// </summary>
    internal static void SetBroadcastInterval(Level level, int minutes)
    {
        if (!HasPrivileges(level, 3, "SetBroadcastInterval")) return;

        if (minutes <= 0)
        {
            SendGlobalChatMessage(level, "Invalid interval. Please provide a value greater than 0.");
            return;
        }

        BroadcastInterval = TimeSpan.FromMinutes(minutes);

        if (IsBroadcastEnabled)
        {
            StopBroadcast(level);
            StartBroadcast(level);
        }

        Logger.Write($"Broadcast interval set to {minutes} minutes.");
        SendGlobalChatMessage(level, $"Broadcast interval set to {minutes} minutes.");
    }

    /// <summary>
    /// Adds a new message to the broadcast list.
    /// </summary>
    internal static void AddMessage(Level level, string message)
    {
        if (!HasPrivileges(level, 3, "AddMessage")) return;

        if (string.IsNullOrWhiteSpace(message))
        {
            SendGlobalChatMessage(level, "Message cannot be empty.");
            return;
        }

        Messages.Add(message);
        Logger.Write($"Message added: {message}");
        SendGlobalChatMessage(level, $"Message added: {message}");
    }

    /// <summary>
    /// Removes a message from the broadcast list.
    /// </summary>
    internal static void RemoveMessage(Level level, string message)
    {
        if (!HasPrivileges(level, 3, "RemoveMessage")) return;

        if (Messages.Remove(message))
        {
            Logger.Write($"Message removed: {message}");
            SendGlobalChatMessage(level, $"Message removed: {message}");
        }
        else
        {
            Logger.Write($"Message not found: {message}");
            SendGlobalChatMessage(level, $"Message not found: {message}");
        }
    }

    /// <summary>
    /// Lists all broadcast messages.
    /// </summary>
    internal static void ListMessages(Level level)
    {
        if (!HasPrivileges(level, 2, "ListMessages")) return;

        if (!Messages.Any())
        {
            SendGlobalChatMessage(level, "No messages in the broadcast list.");
            return;
        }

        string messageList = "Broadcast Messages:\n" + string.Join("\n", Messages.Select((msg, index) => $"{index + 1}. {msg}"));
        SendGlobalChatMessage(level, messageList);
    }

    /// <summary>
    /// Sends a random message to all online players.
    /// </summary>
    private static void SendRandomMessage(object state)
    {
        if (!IsBroadcastEnabled || !ResourcesManager.m_vOnlinePlayers.Any()) return;

        var random = new Random();
        var message = Messages[random.Next(Messages.Count)];

        foreach (var onlinePlayer in ResourcesManager.m_vOnlinePlayers)
        {
            var p = new GlobalChatLineMessage(onlinePlayer.Client)
            {
                Message = message,
                HomeId = 0,
                CurrentHomeId = 0,
                LeagueId = 22,
                PlayerName = "Ultrapower Clash Server AI"
            };
            p.Send();
        }

        Logger.Write($"Broadcast message sent: {message}");
    }

    /// <summary>
    /// Sends a message to the global chat for a specific level.
    /// </summary>
    private static void SendGlobalChatMessage(Level level, string message)
    {
        var chatMessage = new GlobalChatLineMessage(level.Client)
        {
            Message = message,
            HomeId = level.Avatar.UserId,
            CurrentHomeId = level.Avatar.UserId,
            LeagueId = level.Avatar.m_vLeagueId,
            PlayerName = level.Avatar.AvatarName
        };
        chatMessage.Send();
    }

    /// <summary>
    /// Checks if the user has sufficient privileges.
    /// </summary>
    private static bool HasPrivileges(Level level, byte requiredPrivileges, string action)
    {
        if (level.Avatar.AccountPrivileges < requiredPrivileges)
        {
            SendGlobalChatMessage(level, $"Insufficient privileges to perform '{action}'. Required: {requiredPrivileges}, Your Level: {level.Avatar.AccountPrivileges}");
            return false;
        }
        return true;
    }
}
