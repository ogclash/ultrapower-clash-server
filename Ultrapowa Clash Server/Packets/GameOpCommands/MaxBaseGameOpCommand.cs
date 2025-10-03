using System;
using System.IO;
using UCS.Core;
using UCS.Core.Network;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    class MaxBaseGameOpCommand : GameOpCommand
    {
        public MaxBaseGameOpCommand(string[] Args)
        {
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (GetRequiredAccountPrivileges())
            {
                string Home;

                try
                {
                    using (StreamReader sr = new StreamReader(@"Gamefiles/level/NPC/highlevel1.json"))
                    {
                        Home = sr.ReadToEnd();
                    }

                    if (string.IsNullOrWhiteSpace(Home))
                    {
                        Logger.Write("Error: highlevel1.json is empty or invalid.");
                        SendCommandFailedMessage(level.Client);
                        return;
                    }

                    try
                    {
                        ResourcesManager.SetGameObject(level, Home);
                        Processor.Send(new OutOfSyncMessage(level.Client));
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"Error while setting game object: {ex.Message}");
                        SendCommandFailedMessage(level.Client);
                    }
                }
                catch (FileNotFoundException)
                {
                    Logger.Write("Error: Base30.json file not found.");
                    SendCommandFailedMessage(level.Client);
                }
                catch (Exception ex)
                {
                    Logger.Write($"Unexpected error while reading Base30.json: {ex.Message}");
                    SendCommandFailedMessage(level.Client);
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}
