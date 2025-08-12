using System;
using System.Configuration;
using System.Linq;
using UCS.Core;
using UCS.Core.Crypto;
using UCS.Core.Network;
using UCS.Core.Settings;
using UCS.Helpers;
using UCS.Helpers.Binary;
using UCS.Logic;
using UCS.Logic.AvatarStreamEntry;
using UCS.Logic.Enums;
using UCS.Packets.Messages.Server;
using UCS.Utilities.Blake2B;
using UCS.Utilities.Sodium;

namespace UCS.Packets.Messages.Client
{
    // Packet 10101
    internal  class LoginMessage : Message
    {
        public LoginMessage(Device device, Reader reader) : base(device, reader)
        {
            this.Device.PlayerState = State.LOGIN;
        }

        public string AdvertisingGUID;
        public string AndroidDeviceID;
        public string ClientVersion;
        public string DeviceModel;
        public string FacebookDistributionID;
        public string Region;
        public string MacAddress;
        public string MasterHash;
        public string UDID;
        public string OpenUDID;
        public string OSVersion;
        public string UserToken;
        public string VendorGUID;
        public int ContentVersion;
        public int LocaleKey;
        public int MajorVersion;
        public int MinorVersion;
        public uint Seed;
        public bool IsAdvertisingTrackingEnabled;
        public bool Android;
        public long UserID;
        public Level level;
        long adminaccount =  0;


        /// <summary>
        /// Decrypts this message.
        /// </summary>
        internal override void Decrypt()
        {
            byte[] Buffer = this.Reader.ReadBytes(this.Length);
            this.Device.Keys.PublicKey = Buffer.Take(32).ToArray();

            Blake2BHasher Blake = new Blake2BHasher();

            Blake.Update(this.Device.Keys.PublicKey);
            Blake.Update(Key.PublicKey);

            this.Device.Keys.RNonce = Blake.Finish();

            Buffer = Sodium.Decrypt(Buffer.Skip(32).ToArray(), this.Device.Keys.RNonce, Key.PrivateKey, this.Device.Keys.PublicKey);
            this.Device.Keys.SNonce = Buffer.Skip(24).Take(24).ToArray();
            this.Reader = new Reader(Buffer.Skip(48).ToArray());

            this.Length = (ushort)Buffer.Length;

        }

        internal override void Decode()
        {
            this.UserID = this.Reader.ReadInt64();
            this.UserToken = Reader.ReadString();
            this.MajorVersion = Reader.ReadInt32();
            this.ContentVersion = Reader.ReadInt32();
            this.MinorVersion = Reader.ReadInt32();
            this.MasterHash = Reader.ReadString();
            this.UDID = this.Reader.ReadString();
            this.OpenUDID = this.Reader.ReadString();
            this.MacAddress = this.Reader.ReadString();
            this.DeviceModel = this.Reader.ReadString();
            this.LocaleKey = this.Reader.ReadInt32();
            this.Region = this.Reader.ReadString();
            this.AdvertisingGUID = this.Reader.ReadString();
            this.OSVersion = this.Reader.ReadString();
            this.Android = true;
            this.Reader.ReadString();
            this.AndroidDeviceID = this.Reader.ReadString();
            this.FacebookDistributionID = this.Reader.ReadString();
            this.IsAdvertisingTrackingEnabled = false;
            this.VendorGUID = this.Reader.ReadString();
            this.Seed = this.Reader.ReadUInt32();
            this.Reader.ReadByte();
            this.Reader.ReadString();
            this.Reader.ReadString();
            this.ClientVersion = this.Reader.ReadString();
        }

        internal override void Process()
        {
            try
            {
                if (this.Device.PlayerState == State.LOGIN)
                {
                    this.adminaccount=  0;
                    try {
                        this.adminaccount = Utils.ParseConfigInt("AdminAccount");
                    }catch (Exception){}
                    if (this.UserID == this.adminaccount)
                    {
                        CheckClient();
                        return;
                    }
                    if (ResourcesManager.m_vOnlinePlayers.Count >= Constants.MaxOnlinePlayers)
                    {
                        new LoginFailedMessage(Device)
                        {
                            ErrorCode = 12,
                            Reason = "Sorry the Server is currently full! \n\nPlease try again in a few Minutes.\n"
                        }.Send();
                        return;
                    }

                    if (ParserThread.GetMaintenanceMode())
                    {
                        new LoginFailedMessage(Device)
                        {
                            ErrorCode = 10,
                            RemainingTime = ParserThread.GetMaintenanceTime(),
                            Version = 8
                        }.Send();
                        return;
                    }

                    int time = Convert.ToInt32(ConfigurationManager.AppSettings["maintenanceTimeleft"]);
                    if (time != 0)
                    {
                        new LoginFailedMessage(Device)
                        {
                            ErrorCode = 10,
                            RemainingTime = time,
                            Version = 8
                        }.Send();
                        return;
                    }

                    if (ConfigurationManager.AppSettings["CustomMaintenance"] != string.Empty)
                    {
                        new LoginFailedMessage(Device)
                        {
                            ErrorCode = 10,
                            Reason = Utils.ParseConfigString("CustomMaintenance")
                        }.Send();
                        return;
                    }
                    

                    if (Convert.ToBoolean(ConfigurationManager.AppSettings["useCustomPatch"]) &&
                        MasterHash != ObjectManager.FingerPrint.sha)
                    {
                        new LoginFailedMessage(Device)
                        {
                            ErrorCode = 7,
                            ResourceFingerprintData = ObjectManager.FingerPrint.SaveToJson(),
                            ContentUrl = Utils.ParseConfigString("patchingServer"),
                            UpdateUrl = Utils.ParseConfigString("UpdateUrl")
                        }.Send();
                        return;
                    }
                    CheckClient();
                }
            }
            catch { }
        }

        private async void LogUser()
        {
            ResourcesManager.LogPlayerIn(level, Device);
            level.Avatar.Region = this.Region;

            var message = new LoginOkMessage(this.Device)
            {
                ServerMajorVersion = 8,
                ServerBuild = 709,
                ContentVersion = 16
            };
            
            message.Send();

            if (level.Avatar.AllianceId > 0)
            {

                Alliance alliance = ObjectManager.GetAlliance(level.Avatar.AllianceId);
                if (alliance != null)
                {
                    new AllianceFullEntryMessage(this.Device, alliance).Send();
                    new AllianceStreamMessage(this.Device, alliance).Send();
                    new AllianceWarHistoryMessage(this.Device, alliance).Send();
                }
                else
                {
                    this.level.Avatar.AllianceId = 0;
                }
            }
            new AvatarStreamMessage(this.Device).Send();
            new OwnHomeDataMessage(this.Device, level).Send();
            new BookmarkMessage(this.Device).Send();

            foreach (AvatarStreamEntry amessage in Device.Player.Avatar.messages)
            {
                var type = amessage.GetStreamEntryType();
                if (type == 3)
                {
                    AllianceDeclineStreamEntry ai = (AllianceDeclineStreamEntry) amessage;
                    AvatarStreamEntryMessage p = new AvatarStreamEntryMessage(level.Client);
                    p.SetAvatarStreamEntry(ai, false);
                    p.Send();
                }
                else if (type == 4)
                {
                    AllianceInviteStreamEntry ai = (AllianceInviteStreamEntry) amessage;
                    AvatarStreamEntryMessage p = new AvatarStreamEntryMessage(level.Client);
                    p.SetAvatarStreamEntry(ai, false);
                    p.Send();
                }
                else if (type == 6)
                {
                    AllianceMailStreamEntry ai = (AllianceMailStreamEntry) amessage;
                    AvatarStreamEntryMessage p = new AvatarStreamEntryMessage(level.Client);
                    p.SetAvatarStreamEntry(ai, false);
                    p.Send();
                }
            }

            if (level.Avatar.reports.Last().timer != null)
            {
                AvatarChatBanMessage _AvatarChatBanMessage = new AvatarChatBanMessage(level.Client);
                _AvatarChatBanMessage.SetBanPeriod(level.Avatar.reports.Last().timer.GetRemainingSeconds(DateTime.Now)); // 30 Minutes
                _AvatarChatBanMessage.Send();
            }
            if (this.adminaccount != 0 && Utils.ParseConfigString("AdminMessage") != "")
            {
                String amessage = Utils.ParseConfigString("AdminMessage").Replace("/n:", "\n");
                AllianceMailStreamEntry server_update = new AllianceMailStreamEntry();
                Level admin = await ResourcesManager.GetPlayer(this.adminaccount);
                var admin_alliance = ObjectManager.GetAlliance(admin.Avatar.AllianceId);
                server_update.SetSender(admin.Avatar);
                server_update.AllianceId = admin_alliance.m_vAllianceId;
                server_update.AllianceBadgeData = admin_alliance.m_vAllianceBadgeData;
                server_update.AllianceName = admin_alliance.m_vAllianceName;
                server_update.Message = amessage;
                AvatarStreamEntryMessage sys_message = new AvatarStreamEntryMessage(level.Client);
                sys_message.SetAvatarStreamEntry(server_update, false);
                sys_message.Send();
            }
        }

        private async void CheckClient()
        {
            try
            {
                if (UserID == 0 || string.IsNullOrEmpty(UserToken))
                {
                     NewUser();
                     return;
                }
                level = await ResourcesManager.GetPlayer(UserID);
                if (level.Avatar.account_switch != 0)
                {
                    level = await ResourcesManager.GetPlayer(level.Avatar.account_switch);
                }
                if (level != null)
                {
                    if (level.Avatar.AccountBanned)
                    {
                        new LoginFailedMessage(Device) {ErrorCode = 11}.Send();
                        return;
                    }
                    if (level.Client != null)
                    {
                        if (level.Client.Connected)
                        {
                            new LoginFailedMessage(Device) {ErrorCode = 12}.Send();
                            if (!ResourcesManager.IsPlayerOnline(level))
                                ResourcesManager.LogPlayerIn(level, level.Client);
                            return;
                        }
                    }
                    level.Avatar.old_account = (int)UserID;
                    LogUser();
                }
                else
                {
                    new LoginFailedMessage(Device)
                    {
                        ErrorCode = 12,
                        Reason = "We have some Problems with your Account. Please contact Server Support."
                    }.Send();
                }
            } catch (Exception) { }
        }

        private void NewUser()
        {
            level = ObjectManager.CreateAvatar(0, null);
            if (string.IsNullOrEmpty(UserToken))
            {
                for (int i = 0; i < 20; i++)
                {
                    char letter = (char)Resources.Random.Next('A', 'Z');
                    this.level.Avatar.UserToken +=  letter;
                }
            }
            
            level.Avatar.InitializeAccountCreationDate();
            level.Avatar.Region = this.Region;
            level.Avatar.m_vAndroid = this.Android;

            Resources.DatabaseManager.Save(level);
            LogUser();
        }
    }
}
