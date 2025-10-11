using System;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class AccountInformationGameOpCommand : GameOpCommand
    {
        private readonly string[] m_vArgs;
        private string Message;
        public AccountInformationGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }
        public override async void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                try
                {
                    if (m_vArgs.Length >= 2 && (level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount") || level.Avatar.AccountPrivileges >= 1))
                    {
                        long id = Convert.ToInt64(m_vArgs[1]);
                        Level l = await ResourcesManager.GetPlayer(id);
                        if (l != null)
                        {
                            ClientAvatar acc = l.Avatar;
                            Message = "Player Info : \n\n" + "ID = " + id + "\nName = " + acc.AvatarName +
                                      "\nCreation Date : " + acc.m_vAccountCreationDate + "\nRegion : " +
                                      acc.Region;
                            Message += "\nCurrent Gems: " + acc.m_vCurrentGems;
                            if (acc.AllianceId != 0)
                            {
                                Alliance a = ObjectManager.GetAlliance(acc.AllianceId);
                                Message += "\nClan Name : " + a.m_vAllianceName;
                                switch (await acc.GetAllianceRole())
                                {
                                    case 1:
                                        Message += "\nClan Role : Member";
                                        break;

                                    case 2:
                                        Message += "\nClan Role : Leader";
                                        break;

                                    case 3:
                                        Message += "\nClan Role : Elder";
                                        break;

                                    case 4:
                                        Message += "\nClan Role : Co-Leader";
                                        break;

                                    default:
                                        Message += "\nClan Role : Unknown";
                                        break;
                                }
                            }

                            Message += "\nLevel : " + acc.m_vAvatarLevel + "\nTrophies : " +
                                      acc.GetScore() +
                                      "\nTown Hall Level : " + (acc.m_vTownHallLevel + 1) +
                                      "\nAlliance Castle Level : " + (acc.GetAllianceCastleLevel() + 1);

                            if (acc.AccountPrivileges > 0)
                                Message += "\n\nServer-Admin";
                            GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                            _MSG.PlayerName = "Server";
                            _MSG.LeagueId = 22;
                            _MSG.Message = Message;
                            _MSG.Send();
                        }
                    }
                    else if (m_vArgs.Length >= 1)
                    {
                        Level l = level;
                        if (l != null)
                        {
                            ClientAvatar acc = l.Avatar;
                            Message = "Player Info : \n\n" 
                                      + "ID = " + l.Avatar.UserId 
                                      + "\nName = " + acc.AvatarName 
                                      + "\nCreation Date : " + acc.m_vAccountCreationDate 
                                      + "\nRegion : " + acc.Region;
                            Message += "\nCurrent Gems: " + acc.m_vCurrentGems;
                            if (acc.AllianceId != 0)
                            {
                                Alliance a = ObjectManager.GetAlliance(acc.AllianceId);
                                Message = Message + "\nClan Name : " + a.m_vAllianceName;
                                switch (await acc.GetAllianceRole())
                                {
                                    case 1:
                                        Message += "\nClan Role : Member";
                                        break;

                                    case 2:
                                        Message += "\nClan Role : Leader";
                                        break;

                                    case 3:
                                        Message += "\nClan Role : Elder";
                                        break;

                                    case 4:
                                        Message += "\nClan Role : Co-Leader";
                                        break;

                                    default:
                                        Message += "\nClan Role : Unknown";
                                        break;
                                }
                            }

                            Message = Message + "\nLevel : " + acc.m_vAvatarLevel 
                                      + "\nTrophies : " + acc.GetScore() 
                                      + "\nTown Hall Level : " + (acc.m_vTownHallLevel + 1) 
                                      + "\nAlliance Castle Level : " + (acc.GetAllianceCastleLevel() + 1);

                            GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                            _MSG.PlayerName = "Server";
                            _MSG.LeagueId = 22;
                            _MSG.Message = Message;
                            _MSG.Send();
                        }
                    }
                }
                catch (Exception)
                {
                    GlobalChatLineMessage c = new GlobalChatLineMessage(level.Client)
                    {
                        Message = "Command Failed, Wrong Format Or User Doesn't Exist (/accinfo id).",
                        HomeId = level.Avatar.UserId,
                        CurrentHomeId = level.Avatar.UserId,
                        LeagueId = 22,
                        PlayerName = "Server"
                    };
                    Processor.Send(c);
                }
            }
        }
    }
}


