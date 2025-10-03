using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
using UCS.Packets.Messages.Server.Support;
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
        
        bool IsBinaryToken(string token)
        {
            foreach (char c in token)
            {
                if (c < 0x20 || c > 0x7E) // outside printable ASCII
                    return true;
            }
            return false;
        }

        internal override void Decode()
        {
            this.UserID = this.Reader.ReadInt64();
            var bytes = Reader.ReadBytes();
            if (bytes == null)
                this.UserToken = null;
            else
                this.UserToken = Encoding.UTF8.GetString(bytes);
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
            this.Android = this.Reader.ReadBoolean();
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
            if (UserToken != null && IsBinaryToken(this.UserToken))
                this.UserToken = BitConverter.ToUInt64(bytes, 0).ToString();
            Logger.Write($"UserID: {this.UserID}");
            Logger.Write($"UserToken: {this.UserToken}");
            Logger.Write($"MajorVersion: {this.MajorVersion}");
            Logger.Write($"MinorVersion: {this.MinorVersion}");
            Logger.Write($"Region: {this.Region}");
            if (this.Android)
            {
                this.Device.AndroidID = this.AndroidDeviceID;
                Logger.Write($"Device: Android");
                Logger.Write($"AndroidDeviceID: {this.AndroidDeviceID}");
            }
            else
            {
                this.Device.OpenUDID = this.OpenUDID;
                Logger.Write($"Device: IOS");
                Logger.Write($"OpenUDID: {this.OpenUDID}");
                this.Device.AdvertiseID = this.AdvertisingGUID;
                Logger.Write($"AdvertisingGUID: {this.AdvertisingGUID}");
            }
            this.Device.Model = this.DeviceModel;
            Logger.Write($"DeviceModel: {this.DeviceModel}");
            this.Device.OSVersion = this.OSVersion;
            Logger.Write($"OSVersion: {this.OSVersion}");
            Logger.Write($"Seed: {this.Seed}");
            Logger.Write($"MasterHash: {this.MasterHash}\n");
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

        private async void LogUser(bool empty_login = false)
        {
            if (!this.Android && this.level.Avatar.TutorialStepsCount < 10)
            {
                Version deviceOs = new Version(this.OSVersion);
                Version requiredVersion = new Version("18.0");
                if (deviceOs.CompareTo(requiredVersion) >= 0)
                {
                    string fileName = this.OpenUDID + ".txt";
                    string filePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        fileName
                    );

                    int counter = 0;

                    // If file already exists, read its value
                    if (File.Exists(filePath))
                    {
                        string content = File.ReadAllText(filePath);
                        int.TryParse(content, out counter);
                    }

                    // Increment counter
                    counter++;

                    // Write new value back to file
                    File.WriteAllText(filePath, counter.ToString());
                    if (counter > 5)
                    {
                        Level dummy = await ResourcesManager.GetPlayer(171);
                        dummy.Avatar.UserId = level.Avatar.UserId;
                        dummy.Avatar.UserToken = level.Avatar.UserToken;
                        dummy.Avatar.AvatarName = "NoNameYet";
                        dummy.Avatar.TutorialStepsCount = 10;
                        dummy.Avatar.HighID = 0;
                        dummy.Avatar.LowID = (int)level.Avatar.UserId;
                        dummy.Avatar.CurrentHomeId = level.Avatar.UserId;
                        level.Avatar = dummy.Avatar;
                        level.GameObjectManager = dummy.GameObjectManager;
                    }
                }
            }
            
            level.Avatar.old_account = (int)UserID;

            if (!empty_login)
            {
                level.Avatar.LowID = (int)this.UserID;
                level.Avatar.CurrentHomeId = this.UserID;
                level.Avatar.UserId = this.UserID;
                level.Avatar.UserToken = this.UserToken;
                ObjectManager.getDatabaseManager().CreateAccount(level);
                
            }
            ResourcesManager.LogPlayerIn(level, Device);
            level.Avatar.m_vAndroid = this.Android;
            level.Avatar.Region = this.Region.Split('-')[0].ToUpper();

            var message = new LoginOkMessage(this.Device)
            {
                ServerMajorVersion = 8,
                ServerBuild = 709,
                ContentVersion = 16
            };
            this.Device.Player.Avatar.mayorversion = this.MajorVersion;
            this.Device.Player.Avatar.minorversion = this.MinorVersion;
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
            new AvatarStreamMessage(this.Device, true).Send();
            if (this.MinorVersion < 709)
            {
                new OwnHomeDataForOldClients(this.Device, level).Send();
                level.Avatar.SendCLanMessagesToOldClient(this.Device);
            }
            else
                new OwnHomeDataMessage(this.Device, level).Send();
            new BookmarkMessage(this.Device).Send();

            foreach (AvatarStreamEntry amessage in Device.Player.Avatar.messages)
            {
                var type = amessage.GetStreamEntryType();
                if (!amessage.wasOnline)
                {
                    amessage.IsNew = 2;
                    amessage.wasOnline = true;
                }
                if (type == 3)
                {
                    AllianceDeclineStreamEntry ai = (AllianceDeclineStreamEntry) amessage;
                    AvatarStreamEntryMessage p = new AvatarStreamEntryMessage(level.Client);
                    p.SetAvatarStreamEntry(ai, false);
                    p.Send();
                }
                else if (type == 4)
                {
                    if (this.Device.Player.Avatar.AllianceId > 0)
                        continue;
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

            if (level.Avatar.reports.Count > 0 && level.Avatar.reports.Last().timer != null)
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
                if (string.IsNullOrEmpty(UserToken))
                {
                    NewUser(true);
                    return;
                }
                if (Android == false)
                {
                    var lines = File.ReadAllLines("auth");
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length < 3) continue;

                        long id = long.Parse(parts[0]);
                        string token = parts[1];
                        if (token == "disabled")
                        {
                            string deviceid = parts[2];
                            int dummyId = int.Parse(parts[3]);
                            if (UserID == id && OpenUDID == deviceid)
                            {
                                var dummy = await ResourcesManager.GetPlayer(dummyId);
                                UserID = dummy.Avatar.UserId;
                                UserToken = dummy.Avatar.UserToken;
                                break;
                            }
                        }
                        else
                        {
                            int dummyId = int.Parse(parts[2]);
                            if (UserID == id && UserToken == token)
                            {
                                var dummy = await ResourcesManager.GetPlayer(dummyId);
                                UserID = dummy.Avatar.UserId;
                                UserToken = dummy.Avatar.UserToken;
                                break;
                            }
                        }
                    }
                }
                if (UserID == 0)
                    UserID = Seed;
                level = await ResourcesManager.GetPlayer(UserID);
                if (level != null)
                {
                    if (level.Avatar.UserToken != UserToken)
                    {
                        if (level.Avatar.AvatarName == "NoNameYet")
                            level.Avatar.UserToken = UserToken;
                        else
                        {
                            new LoginFailedMessage(Device)
                            {
                                ErrorCode = 12,
                                Reason = "We have some Problems with your Account. Please contact Server Support."
                            }.Send();
                            return;
                        }
                    }
                    if (level.Avatar.account_switch > 1)
                    {
                        Logger.Write("Switching to UserID: " + level.Avatar.account_switch);
                        level = await ResourcesManager.GetPlayer(level.Avatar.account_switch);
                    } else if (level.Avatar.account_switch == 1)
                    {
                        Logger.Write("Creating new Account because of command");
                        level.Avatar.account_switch = 0;
                        NewUser(true);
                        return;
                    }
                    
                    if (level.Avatar.AccountBanned)
                    {
                        new LoginFailedMessage(Device) { ErrorCode = 11 }.Send();
                        return;
                    }

                    if (level.Client != null)
                    {
                        if (level.Client.Connected)
                        {
                            if (level.Avatar.old_account != level.Avatar.UserId)
                            {
                                Level oldplayer = await ResourcesManager.GetPlayer(level.Avatar.old_account);
                                if (oldplayer.Avatar.UserId == this.adminaccount)
                                {
                                    new LoginFailedMessage(Device)
                                    {
                                        ErrorCode = 1,
                                        Reason = "An admin is logged in currently. Please wait a moment"
                                    }.Send();
                                    return;
                                }
                            }

                            ResourcesManager.DisconnectClient(level.Client);
                            new LoginFailedMessage(Device)
                            {
                                ErrorCode = 1,
                                Reason = "Seem like and old session was active, it was terminated now."
                            }.Send();
                            return;
                        }
                    }
                    LogUser(true);
                }
                else
                {
                    Logger.Write("Creating new Account because of UserID not existent in DB");
                    NewUser(false, UserID);
                }
            }
            catch (Exception e)
            {
                Logger.Write("Creating new Account because of Error: " + e.Message);
                NewUser();
            }
        }

        private void NewUser(bool empty_login = false, long user_id = 0)
        {
            Level oldlevel = level;
            level = ObjectManager.CreateAvatar(user_id, null);
            if (oldlevel != null)
                oldlevel.Avatar.account_switch = (int)level.Avatar.UserId;
            if (empty_login)
                level.Avatar.UserToken = GenerateUserToken(level.Avatar.UserId);
            else
            {
                level.Avatar.UserToken = UserToken;
                level.Avatar.UserId = UserID;
            }
            
            level.Avatar.InitializeAccountCreationDate();
            level.Avatar.Region = this.Region;
            level.Avatar.m_vAndroid = this.Android;

            Resources.DatabaseManager.Save(level);
            LogUser(empty_login);
        }
        public string GenerateUserToken(long userId)
        {
            string deterministicPart;
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(userId.ToString()));
                deterministicPart = BitConverter.ToString(hash)
                    .Replace("-", "")
                    .ToLower()
                    .Substring(0, 20);
            }
            
            string randomPart;
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[16]; // 8 bytes → enough for 10 Base64 chars
                rng.GetBytes(bytes);
                randomPart = Convert.ToBase64String(bytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "")
                    .Substring(0, 20);
            }

            // Combine
            string token = deterministicPart + randomPart;
            return token; // 20 chars
        }
    }
}
