using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Packets.Messages.Server;
using Newtonsoft.Json.Linq;
using UCS.Core.LogicMath;
using UCS.Files.Logic;
using UCS.Packets.Messages.Server.Support;

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
            try {
                ClientAvatar player = this.Device.Player.Avatar;
                if (this.State == 0)
                {
                    if (this.Device.AttackInfo == "multiplayer")
                    {
                        ResourcesMultiplayer(Device.Player);
                        new AvatarStreamMessage(this.Device).Send();
                    } else if (this.Device.AttackInfo == "npc" && this.Device.NpcAttacked)
                    {
                        ResourcesNpc(Device.Player);
                    }
                    this.Device.Player.Avatar.battle = new BattleResult();
                }
                else if (State == 1)
                {
                    this.Device.PlayerState = Logic.Enums.State.WAR_EMODE;
                }
            } catch (Exception) { }
            
            this.Device.AttackInfo = null;
            this.Device.PlayerState = Logic.Enums.State.LOGGED;
            this.Device.Player.Tick();
            Alliance alliance = ObjectManager.GetAlliance(this.Device.Player.Avatar.AllianceId);
            if (this.Device.Player.Avatar.minorversion >= 709)
            {
                new OwnHomeDataMessage(Device, this.Device.Player).Send();
                if (alliance != null)
                {
                    new AllianceStreamMessage(Device, alliance).Send();
                }
            }
            else
            {
                new OwnHomeDataForOldClients(this.Device, this.Device.Player).Send();
                if (alliance != null)
                {
                    new AllianceStreamMessage(Device, alliance).Send();
                }
                if (this.Device.Player.Avatar.AllianceId > 0)
                {
                    this.Device.Player.Avatar.SendCLanMessagesToOldClient(this.Device);
                }
            }
        }

        public void ResourcesNpc(Level level)
        {
            ClientAvatar avatar = level.Avatar;
            int currentGold = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("Gold"));
            int currentElixir = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("Elixir"));
            ResourceData goldLocation = CSVManager.DataTables.GetResourceByName("Gold");
            ResourceData elixirLocation = CSVManager.DataTables.GetResourceByName("Elixir");

            int goldvalue;
            int elixirvalue;
            level.Avatar.NpcLevels[this.Device.AttackedNpc].Stars = 3;
            if (avatar.GetResourceCap(goldLocation) >= 800000)
                goldvalue = 800000;
            else
                goldvalue = avatar.GetResourceCap(goldLocation);
            
            if (avatar.GetResourceCap(elixirLocation) >= 800000)
                elixirvalue = 800000;
            else
                elixirvalue = avatar.GetResourceCap(elixirLocation);
            
            if (currentGold+goldvalue <= avatar.GetResourceCap(goldLocation))
            {
                avatar.SetResourceCount(goldLocation, currentGold + goldvalue);
            }
            else
            {
                avatar.SetResourceCount(goldLocation, avatar.GetResourceCap(goldLocation));
            }

            if (currentElixir+elixirvalue <= avatar.GetResourceCap(elixirLocation))
            {
                avatar.SetResourceCount(elixirLocation, currentElixir + elixirvalue);
            }
            else
            {
                avatar.SetResourceCount(elixirLocation, avatar.GetResourceCap(elixirLocation));
            }
            this.Device.NpcAttacked = false;
        }

        public void ResourcesMultiplayer(Level level)
        {
            ClientAvatar avatar = level.Avatar;
            Random random = new Random();
            int goldvalue = 0;
            int elexirvalue = 0;
            int darkelixirvalue = random.Next(500, 4200);
            int chance = random.Next(100); // 0-99
            int score;
            
            bool win = false;
            foreach (JArray unit in this.Device.Player.Avatar.battle.units.ToList())
            {
                int currentCount = (int)unit[1];
                if (currentCount > 1)
                {
                    win = true;
                    break;
                }
            }

            if (this.Device.Player.Avatar.battle.units.Count > 1)
                win = true;

            JObject result = new JObject();
            if (win)
            {
                int currentGold = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("Gold"));
                int currentElixir = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("Elixir"));
                int currentDarkElixir = avatar.GetResourceCount(CSVManager.DataTables.GetResourceByName("DarkElixir"));
                ResourceData goldLocation = CSVManager.DataTables.GetResourceByName("Gold");
                ResourceData elixirLocation = CSVManager.DataTables.GetResourceByName("Elixir");
                ResourceData darkelixirLocation = CSVManager.DataTables.GetResourceByName("DarkElixir");
                
                if (chance < 50) // 50%
                {
                    darkelixirvalue = random.Next(500, 1000);
                    goldvalue = random.Next(10000, 420000);
                    elexirvalue = random.Next(10000, 420000);
                }
                else if (chance < 80) // 30% (50-79)
                {
                    darkelixirvalue = random.Next(500, 4200);
                    goldvalue = random.Next(10000, 750000);
                    elexirvalue = random.Next(10000, 750000);
                }
                else if (chance < 95) // 15% (80-94)
                {
                    darkelixirvalue = random.Next(1000, 4200);
                    goldvalue = random.Next(10000, 800000);
                    elexirvalue = random.Next(10000, 800000);
                }
                else // 5% (95-99)
                {
                    darkelixirvalue = random.Next(1500, 4200);
                    goldvalue = random.Next(10000, 1000000);
                    elexirvalue = random.Next(10000, 1000000);
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
                var oldscore = avatar.GetScore();
                var newAttackerScore = LogicELOMath.CalculateNewRating(true, avatar.GetScore(), Device.AttackVictim.Avatar.GetScore(), 20 * 3);
                score = newAttackerScore - oldscore;
                result =  new JObject
                {
                    ["new"] = 2,
                    ["score"] = score,
                    ["attacker"] = avatar.UserId,
                    ["defender"] = Device.AttackVictim.Avatar.UserId,
                    ["result"] = new JObject
                    {
                        ["loot"] = new JArray
                        {
                            new JArray(3000002, goldvalue),
                            new JArray(3000001, elexirvalue)
                        },
                        ["availableLoot"] = new JArray(),
                        ["units"] = Device.Player.Avatar.battle.units,
                        ["spells"] = Device.Player.Avatar.battle.spells,
                        ["levels"] = Device.Player.Avatar.battle.levels,
                        ["stats"] = new JObject
                        {
                            ["townhallDestroyed"] = true,
                            ["battleEnded"] = true,
                            ["allianceUsed"] = false,
                            ["destructionPercentage"] = 100,
                            ["battleTime"] = 180,
                            ["originalAttackerScore"] = avatar.GetScore(),
                            ["attackerScore"] = score,
                            ["originalDefenderScore"] = Device.AttackVictim.Avatar.GetScore(),
                            ["defenderScore"] = -score,
                            ["attackerStars"] = 3,
                            ["homeID"] = new JArray(0, Device.AttackVictim.Avatar.UserId),
                            ["deployedHousingSpace"] = 120,
                            ["armyDeploymentPercentage"] = 75
                        }
                    }
                };
                avatar.attacks_won++;
                avatar.SetScore(newAttackerScore);
                Device.AttackVictim.Avatar.SetScore(Device.AttackVictim.Avatar.GetScore() - score);
                Device.AttackVictim.Avatar.AllianceUnits.Clear();
                Device.AttackVictim.Avatar.SetAllianceCastleUsedCapacity(0);
                try
                {
                    foreach (GameObject go in
                             new List<GameObject>(Device.AttackVictim.GameObjectManager.GetAllGameObjects()[0]))
                    {
                        try
                        {
                            Building b = (Building)go;
                            if (b.GetData().GetGlobalID() == 1000027 || b.GetData().GetGlobalID() == 1000021 || b.GetData().GetGlobalID() == 1000031)
                            {
                                if (go?.GetComponent(1, true) != null)
                                    ((CombatComponent) go.GetComponent(1, true)).useAmmo();
                            }
                        } catch (Exception) {}
                    }
                } catch (Exception) {}
                
                try
                {
                    foreach (GameObject go2 in
                             new List<GameObject>(Device.AttackVictim.GameObjectManager.GetAllGameObjects()[4]))
                    {
                        try
                        {
                            Trap t = (Trap)go2;
                            ((TriggerComponent)t.GetComponent(8)).TriggerTrap();
                        } catch (Exception) {}
                    }
                } catch (Exception) {}
                

            }
            else
            {
                var defender_oldscore = Device.AttackVictim.Avatar.GetScore();
                var newdefenderscore = LogicELOMath.CalculateNewRating(true, Device.AttackVictim.Avatar.GetScore(),  avatar.GetScore(), 20 * 2);
                score = newdefenderscore - defender_oldscore;
                result =  new JObject
                {
                    ["new"] = 2,
                    ["score"] = score,
                    ["attacker"] = avatar.UserId,
                    ["defender"] = Device.AttackVictim.Avatar.UserId,
                    ["result"] = new JObject
                    {
                        ["loot"] = new JArray
                        {
                            new JArray(3000002, goldvalue),
                            new JArray(3000001, elexirvalue)
                        },
                        ["availableLoot"] = new JArray(),
                        ["units"] = Device.Player.Avatar.battle.units,
                        ["spells"] = Device.Player.Avatar.battle.spells,
                        ["levels"] = Device.Player.Avatar.battle.levels,
                        ["stats"] = new JObject
                        {
                            ["townhallDestroyed"] = false,
                            ["battleEnded"] = true,
                            ["allianceUsed"] = false,
                            ["destructionPercentage"] = 0,
                            ["battleTime"] = 10,
                            ["originalAttackerScore"] = avatar.GetScore(),
                            ["attackerScore"] = -score,
                            ["originalDefenderScore"] = Device.AttackVictim.Avatar.GetScore(),
                            ["defenderScore"] = score,
                            ["attackerStars"] = 0,
                            ["homeID"] = new JArray(0, Device.AttackVictim.Avatar.UserId),
                            ["deployedHousingSpace"] = 120,
                            ["armyDeploymentPercentage"] = 75
                        }
                    }
                };
                Device.AttackVictim.Avatar.defenses_won++;
                Device.AttackVictim.Avatar.SetScore(newdefenderscore);
                avatar.SetScore(avatar.GetScore() - score);
            }
            Device.AttackVictim.Avatar.battles.Add(result);
            avatar.battles.Add(result);
            if (ResourcesManager.IsPlayerOnline(Device.AttackVictim))
                new GoHomeMessage(Device.AttackVictim.Client, Reader).Send();
        }
    }
}
