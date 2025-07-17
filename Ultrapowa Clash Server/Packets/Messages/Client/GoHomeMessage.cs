using System;
using System.IO;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using UCS.Packets.Commands.Client;
using System.Text;
using static UCS.Logic.ClientAvatar;
using System.Collections.Generic;
using UCS.Files.Logic;

namespace UCS.Packets.Messages.Client
{
    // Packet 14101
    internal class GoHomeMessage : Message
    {
        public GoHomeMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal override void Decode()
        {
            this.State = this.Reader.ReadInt32();
        }

        public int State;

        internal override async void Process()
        {
            try
            {
                /*var info = default(ClientAvatar.AttackInfo);
                if (!level.Avatar.AttackingInfo.TryGetValue(level.Avatar.GetId(), out info))
                {
                    Logger.Write("Unable to obtain attack info.");
                }
                else
                {
                    Level defender = info.Defender;
                    Level attacker = info.Attacker;

                    int lost = info.Lost;
                    int reward = info.Reward;

                    List<DataSlot> usedtroop = info.UsedTroop;

                    int attackerscore = attacker.Avatar.GetScore();
                    int defenderscore = defender.Avatar.GetScore();

                    if (defender.Avatar.GetScore() > 0)
                        defender.Avatar.SetScore(defenderscore -= lost);

                    Logger.Write("Used troop type: " + usedtroop.Count);
                    foreach(DataSlot a in usedtroop)
                    {
                        Logger.Write("Troop Name: " + a.Data.GetName());
                        Logger.Write("Troop Used Value: " + a.Value);
                    }
                    attacker.Avatar.SetScore(attackerscore += reward);
                    attacker.Avatar.AttackingInfo.Clear(); //Since we use userid for now,We need to clear to prevent overlapping
                    Resources(attacker);

                    Resources.DatabaseManager.Save(attacker);
                    Resources.DatabaseManager.Save(defender);

                }
                player.State = UserState.Home;
            }*/

                ClientAvatar player = this.Device.Player.Avatar;
                if (this.State == 0)
                {
                    if (this.Device.AttackInfo == "multiplayer")
                    {
                        Resources(Device.Player);
                    }
                }
                if (State == 1)
                {
                    this.Device.PlayerState = Logic.Enums.State.WAR_EMODE;
                    this.Device.Player.Tick();
                    new OwnHomeDataMessage(this.Device, this.Device.Player).Send();
                }
                else if (this.Device.PlayerState == Logic.Enums.State.LOGGED)
                {
                    ResourcesManager.DisconnectClient(Device);
                }
                else
                {
                    this.Device.AttackInfo = null;
                    this.Device.PlayerState = Logic.Enums.State.LOGGED;
                    this.Device.Player.Tick();
                    Alliance alliance = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
                    new OwnHomeDataMessage(Device, this.Device.Player).Send();
                    if (alliance != null)
                    {
                        new AllianceStreamMessage(Device, alliance).Send();
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        public void Resources(Level level)
        {
            ClientAvatar avatar = level.Avatar;
            int currentGold = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("Gold"));
            int currentElixir = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("Elixir"));
            int currentDarkElixir = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("DarkElixir"));
            ResourceData goldLocation = CSVManager.DataTables.GetResourceByName("Gold");
            ResourceData elixirLocation = CSVManager.DataTables.GetResourceByName("Elixir");
            ResourceData darkelixirLocation = CSVManager.DataTables.GetResourceByName("DarkElixir");

            Random random = new Random();
    
            int goldvalue = 0;
            int elexirvalue = 0;
            int darkelixirvalue = random.Next(500, 4200);
            int chance = random.Next(100); // 0-99

            if (chance < 50) // 50%
            {
                darkelixirvalue = random.Next(500, 1000);
                goldvalue = random.Next(10000, 420000);
                elexirvalue = random.Next(10000, 420000);
                avatar.SetScore(avatar.GetScore() + random.Next(10, 30));
            }
            else if (chance < 80) // 30% (50-79)
            {
                darkelixirvalue = random.Next(500, 4200);
                goldvalue = random.Next(10000, 750000);
                elexirvalue = random.Next(10000, 750000);
                avatar.SetScore(avatar.GetScore() + random.Next(10, 20));
            }
            else if (chance < 95) // 15% (80-94)
            {
                darkelixirvalue = random.Next(1000, 4200);
                goldvalue = random.Next(10000, 800000);
                elexirvalue = random.Next(10000, 800000);
                avatar.SetScore(avatar.GetScore() + random.Next(2, 30));
            }
            else // 5% (95-99)
            {
                darkelixirvalue = random.Next(1500, 4200);
                goldvalue = random.Next(10000, 1000000);
                elexirvalue = random.Next(10000, 1000000);
                avatar.SetScore(avatar.GetScore() + random.Next(2, 10));
            }

            if (currentGold+goldvalue <= avatar.GetResourceCap(goldLocation))
            {
                avatar.SetResourceCount(goldLocation, currentGold + goldvalue);
            }
            else
            {
                avatar.SetResourceCount(goldLocation, avatar.GetResourceCap(goldLocation));
            }

            if (currentElixir+elexirvalue <= avatar.GetResourceCap(elixirLocation))
            {
                avatar.SetResourceCount(elixirLocation, currentElixir + elexirvalue);
            }
            else
            {
                avatar.SetResourceCount(elixirLocation, avatar.GetResourceCap(elixirLocation));
            }

            if (avatar.m_vTownHallLevel >= 7)
            {
                if (currentDarkElixir+darkelixirvalue <= avatar.GetResourceCap(darkelixirLocation))
                {
                    avatar.SetResourceCount(darkelixirLocation, currentDarkElixir + darkelixirvalue);
                }
                else
                {
                    avatar.SetResourceCount(darkelixirLocation, avatar.GetResourceCap(darkelixirLocation));
                }
            }
        } 
    }
}
