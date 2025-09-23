using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Security.Cryptography;
using UCS.Old.Helpers;
using UCS.Core;
using UCS.Network;
using UCS.Logic;
using UCS.Packets;

namespace UCS.PacketProcessing
{
    //Packet 10101
    class LoginMessage : Message
    {
        private long m_vAccountId;
        private string m_vPassToken;
        private int m_vClientMajorVersion;
        private int m_vClientBuild;
        private int m_vClientContentVersion;
        private string m_vResourceSha;
        private string m_vUDID;
        private string m_vOpenUDID;
        private string m_vMacAddress;
        private string m_vDevice;
        private string m_vPreferredDeviceLanguage;
        //unchecked
        private string m_vPhoneId;
        private string m_vGameVersion;
        private string m_vSignature2;
        private string m_vSignature3;
        private string m_vSignature4;
        private uint m_vClientSeed; 

        public LoginMessage(Device client, BinaryReader br) : base (client, br)
        {
        }

        public override void Decode()
        {
            using (var br = new BinaryReader(new MemoryStream(GetData())))
            {
                m_vAccountId = br.ReadInt64WithEndian();
                m_vPassToken = br.ReadScString();
                m_vClientMajorVersion = br.ReadInt32WithEndian();
                m_vClientContentVersion = br.ReadInt32WithEndian();
                m_vClientBuild = br.ReadInt32WithEndian();
                m_vResourceSha = br.ReadScString();
                m_vUDID = br.ReadScString();
                m_vOpenUDID = br.ReadScString();
                m_vMacAddress = br.ReadScString();
                m_vDevice = br.ReadScString();
                br.ReadInt32WithEndian();//00 1E 84 81, readDataReference for m_vPreferredLanguage
                m_vPreferredDeviceLanguage = br.ReadScString();
                //unchecked
                m_vPhoneId = br.ReadScString();
                m_vGameVersion = br.ReadScString();
                br.ReadByte();//01
                br.ReadInt32WithEndian();//00 00 00 00
                m_vSignature2 = br.ReadScString();
                m_vSignature3 = br.ReadScString();
                br.ReadByte();//01
                m_vSignature4 = br.ReadScString();
                m_vClientSeed = br.ReadUInt32WithEndian();
                if(GetMessageVersion() >=7 )//7.156
                {
                    br.ReadByte();
                    br.ReadUInt32WithEndian();
                    br.ReadUInt32WithEndian();
                }
            }
        }

        public override async void Process(Level level)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["maintenanceMode"]))
            {
                var p = new LoginFailedMessage(this.Client);
                p.SetErrorCode(10);
                PacketManager.ProcessOutgoingPacket(p);
                return;
            }

            string[] versionData = ConfigurationManager.AppSettings["clientVersion"].Split('.');
            if(versionData.Length >= 2)
            {
                if(m_vClientMajorVersion != Convert.ToInt32(versionData[0]) || m_vClientBuild != Convert.ToInt32(versionData[1]))
                {
                    var p = new LoginFailedMessage(this.Client);
                    p.SetErrorCode(8);
                    p.SetUpdateURL("market://details?id=com.supercell.clashofclans");
                    PacketManager.ProcessOutgoingPacket(p);
                    return;
                }
            }
            else
            {
                Logger.Write("Connection failed. UCS config key clientVersion is not properly set.");
            }

            level = await ResourcesManager.GetPlayer(m_vAccountId);

            if(Convert.ToBoolean(ConfigurationManager.AppSettings["useCustomPatch"]))
            {
                if (m_vResourceSha != ObjectManager.FingerPrint.sha)
                {
                    var p = new LoginFailedMessage(this.Client);
                    p.SetErrorCode(7);
                    p.SetResourceFingerprintData(ObjectManager.FingerPrint.SaveToJson());
                    p.SetContentURL(ConfigurationManager.AppSettings["patchingServer"]);
                    p.SetUpdateURL("market://details?id=com.supercell.clashofclans");
                    PacketManager.ProcessOutgoingPacket(p);
                    return;
                }
            }

            this.Client.ClientSeed = m_vClientSeed;
            PacketManager.ProcessOutgoingPacket(new SessionKeyMessage(this.Client));
            //Console.WriteLine("Debug: Retrieve Player Data for player " + auth.PlayerId.ToString());
            //New player
            if (level == null)
            {
                level = ObjectManager.CreateAvatar(0, null);
                byte[] tokenSeed = new byte[20];
                new Random().NextBytes(tokenSeed);
                SHA1 sha = new SHA1CryptoServiceProvider();
                m_vPassToken = BitConverter.ToString(sha.ComputeHash(tokenSeed)).Replace("-","");
            }
            if (level.Avatar.AccountPrivileges > 0)
                level.Avatar.m_vLeagueId = (21);
            if (level.Avatar.AccountPrivileges > 4)
                level.Avatar.m_vLeagueId = (22);
            ResourcesManager.LogPlayerIn(level, this.Client);
            level.Tick();

            var loginOk = new LoginOkMessage(this.Client);
            var avatar = level.Avatar;
            loginOk.SetAccountId(avatar.UserId);
            loginOk.SetPassToken(m_vPassToken);
            loginOk.SetServerMajorVersion(m_vClientMajorVersion);
            loginOk.SetServerBuild(m_vClientBuild);
            loginOk.SetContentVersion(m_vClientContentVersion);
            loginOk.SetServerEnvironment("prod");
            loginOk.SetDaysSinceStartedPlaying(10);
            loginOk.SetServerTime(Math.Round((level.Avatar.LastTickSaved.Subtract(new DateTime(1970, 1, 1))).TotalSeconds * 1000).ToString());
            loginOk.SetAccountCreatedDate("1414003838000");
            loginOk.SetStartupCooldownSeconds(0);
            loginOk.SetCountryCode("FR");
            PacketManager.ProcessOutgoingPacket(loginOk);

            Alliance alliance = ObjectManager.GetAlliance(level.Avatar.AllianceId);
            if (alliance == null)
                level.Avatar.AllianceId = (0);
            PacketManager.ProcessOutgoingPacket(new OwnHomeDataMessage(this.Client, level));
            if (alliance != null)
                PacketManager.ProcessOutgoingPacket(new AllianceStreamMessage(this.Client, alliance));
        }
    }
}
