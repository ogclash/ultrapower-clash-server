using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCS.Core;
using UCS.Helpers.List;
using UCS.Logic;

namespace UCS.Packets.Messages.Server
{
    // Packet 24403
    internal class GlobalPlayersMessage : Message
    {
        public GlobalPlayersMessage(Device client) : base(client)
        {
            this.Identifier = 24403;
        }

        internal override void Encode()
        {
            List<byte> packet1 = new List<byte>();
            int i = 0;

            foreach (Level player in ResourcesManager.m_vInMemoryLevels.Values.ToList().OrderByDescending(t => t.Avatar.GetScore()).Take(200))
            {
                try
                {
                    if (player.Avatar.m_vAvatarLevel >= 1 && player.Avatar.AvatarName != "NoNameYet")
                    {
                        ClientAvatar pl = player.Avatar;
                        if (i >= 200)
                            break;
                        packet1.AddLong(pl.UserId);
                        packet1.AddString(pl.AvatarName);
                        packet1.AddInt(i + 1);
                        packet1.AddInt(pl.GetScore());
                        packet1.AddInt(i + 1);
                        packet1.AddInt(pl.m_vAvatarLevel);
                        //Attacks
                        packet1.AddInt(0);
                        packet1.AddInt(i);
                        //Defense
                        packet1.AddInt(0);
                        packet1.AddInt(1);
                        packet1.AddInt(pl.m_vLeagueId);
                        packet1.AddString(pl.Region.ToUpper());
                        packet1.AddLong(pl.UserId);
                        packet1.AddInt(1);
                        packet1.AddInt(1);
                        if (pl.AllianceId > 0)
                        {
                            Alliance _Alliance = ObjectManager.GetAlliance(pl.AllianceId);
                            if (_Alliance.m_vAllianceMembers.ContainsKey(pl.UserId))
                            {
                                packet1.Add(1); // 1 = Have an alliance | 0 = No alliance
                                packet1.AddLong(pl.AllianceId);
                                packet1.AddString(_Alliance.m_vAllianceName);
                                packet1.AddInt(_Alliance.m_vAllianceBadgeData);
                            }
                            else
                            {
                                packet1.Add(0);
                                pl.AllianceId = 0;
                            }
                        }
                        else
                            packet1.Add(0);
                        i++;
                    }
                }
                catch (Exception) { }
            }

            this.Data.AddInt(i);
            this.Data.AddRange(packet1);
            if (File.Exists(ObjectManager.filePathPrevius))
            {
                this.Data.AddInt(Convert.ToInt32(File.ReadAllText(ObjectManager.filePathPreviusNumber)));
                this.Data.AddRange(File.ReadAllBytes(ObjectManager.filePathPrevius));
            }
            else
                this.Data.AddInt(0);
            
            DateTime now = DateTime.Now;
            DateTime nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
            TimeSpan timeUntilNextMonth = nextMonth - now;
            this.Data.AddInt((int)timeUntilNextMonth.TotalSeconds);
            this.Data.AddInt(now.Year);
            this.Data.AddInt(now.Month);
            this.Data.AddInt(now.Year);
            this.Data.AddInt(now.Month - 1);
        }
    }
}