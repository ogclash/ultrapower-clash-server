
using System;
using System.Linq;
using UCS.Core;
using UCS.Core.Checker;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.Messages.Client
{
    // Packet 14715
    internal class SendGlobalChatLineMessage : Message
    {
        public SendGlobalChatLineMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        public string Message { get; set; }

        internal override void Decode()
        {
            this.Message = this.Reader.ReadString();
        }

        internal override async void Process()
        {
            if (Message.Length > 0 && Message.Length < 420)
            {
                if (Message[0] == '/')
                {
                    object obj = GameOpCommandFactory.Parse(Message);
                    if (obj != null)
                    {
                        string player = "";
                        if (this.Device.Player != null)
                            player += " (" + this.Device.Player.Avatar.UserId + ", " +
                                      this.Device.Player.Avatar.AvatarName + ")";
                        ((GameOpCommand) obj).Execute(this.Device.Player);
                    }
                }
                else
                {
                    long senderId = this.Device.Player.Avatar.UserId;
                    string senderName = this.Device.Player.Avatar.AvatarName;

                    bool badword = DirectoryChecker.badwords.Any(s => Message.Contains(s));

                    try
                    {
                        if (badword)
                        {
                            foreach (Level pl in ResourcesManager.m_vOnlinePlayers)
                            {
                                if (pl.Avatar.Region == this.Device.Player.Avatar.Region)
                                {
                                    string NewMessage = "";

                                    for (int i = 0; i < Message.Length; i++){NewMessage += "*";}

                                    GlobalChatLineMessage p = new GlobalChatLineMessage(pl.Client)
                                    {
                                        PlayerName = senderName,
                                        Message = NewMessage,
                                        HomeId = senderId,
                                        CurrentHomeId = senderId,
                                        LeagueId = this.Device.Player.Avatar.m_vLeagueId
                                    };

                                    p.SetAlliance(ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId));
                                    p.Send();
                                }
                            }
                        }
                        else
                        {
                            Logger.Say($"Global Chat Message: '{Message}' from: {senderName}:{senderId}");
                            foreach (Level onlinePlayer in ResourcesManager.m_vOnlinePlayers)
                            {
                                if (onlinePlayer.Avatar.Region == this.Device.Player.Avatar.Region)
                                {
                                    GlobalChatLineMessage p = new GlobalChatLineMessage(onlinePlayer.Client)
                                    {
                                        PlayerName = senderName,
                                        Message = this.Message,
                                        HomeId = senderId,
                                        CurrentHomeId = senderId,
                                        LeagueId = this.Device.Player.Avatar.m_vLeagueId
                                    };
                                    p.SetAlliance(ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId));
                                    p.Send();
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}