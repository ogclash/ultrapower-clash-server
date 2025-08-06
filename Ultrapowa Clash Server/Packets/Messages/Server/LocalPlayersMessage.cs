using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Helpers.List;
using UCS.Logic;

namespace UCS.Packets.Messages.Server
{
    // Packet 24404
    internal class LocalPlayersMessage : Message
    {
        public LocalPlayersMessage(Device client) : base(client)
        {
            this.Identifier = 24404;
        }

        internal override async void Encode()
        {
            List<byte> data = new List<byte>();
            var i = 0;
            
            List<Level> localPlayers = new List<Level>();
            localPlayers.Add(this.Device.Player);

            string playerRegion = this.Device.Player.Avatar.Region;  // Get current player's region

            foreach (Level player in ResourcesManager.m_vInMemoryLevels.Values)
            {
                if (player == null || player.Avatar == null)
                    continue;

                ClientAvatar pl = player.Avatar;

                if (pl.Region == playerRegion && player != this.Device.Player  && player.Avatar.AvatarName != "NoNameYet")
                {
                    localPlayers.Add(player);
                }
            }

            foreach (Level player in localPlayers.OrderByDescending(t => t.Avatar.GetScore()))
            {
                try
                {
                    ClientAvatar pl = player.Avatar;
                    if (i >= 100)
                        break;
                    data.AddLong(pl.UserId);
                    data.AddString(pl.AvatarName);
                    data.AddInt(i + 1);
                    data.AddInt(pl.GetScore());
                    // TODO
                    data.AddInt(i + 1);
                    data.AddInt(pl.m_vAvatarLevel);
                    data.AddInt(0);
                    data.AddInt(1);
                    data.AddInt(0);
                    data.AddInt(1);
                    data.AddInt(pl.m_vLeagueId);
                    data.AddString(pl.Region.ToUpper());
                    data.AddLong(pl.UserId);
                    data.AddInt(1);
                    data.AddInt(1);
                    if (pl.AllianceId > 0)
                    {
                        data.Add(1); // 1 = Have an alliance | 0 = No alliance
                        data.AddLong(pl.AllianceId);
                        Alliance _Alliance = ObjectManager.GetAlliance(pl.AllianceId);
                        data.AddString(_Alliance.m_vAllianceName);
                        data.AddInt(_Alliance.m_vAllianceBadgeData);
                    }
                    else
                        data.Add(0);
                    //data.AddInt(52);
                    i++;
                }
                catch (Exception) { }
            }

            this.Data.AddInt(i);
            this.Data.AddRange(data.ToArray());
        }
    }
}