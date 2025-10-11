using System;
using System.IO;
using System.Linq;
using System.Management;
using UCS.Core;
using UCS.Core.Network;
using UCS.Helpers;
using UCS.Logic;
using UCS.Packets.Messages.Server;

namespace UCS.Packets.GameOpCommands
{
    internal class ServerStatusGameOpCommand : GameOpCommand
    {
        readonly string[] m_vArgs;

        public ServerStatusGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(1);
        }
        private double RAMUsage;
        private DriveInfo DiskSpace;
        private string DriveLetter;
        private double DiskspaceUsed;
        private double TotalFreeSpace;
        private double TotalDiskSize;

        public override void Execute(Level level)
        {
            if (level.Avatar.AccountPrivileges >= GetRequiredAccountPrivileges() || level.Avatar.UserId == Utils.ParseConfigInt("AdminAccount"))
            {
                if (m_vArgs.Length >= 1)
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
                    var cpuTimes = searcher.Get()
                        .Cast<ManagementObject>()
                        .Select(mo => new
                        {
                            Name = mo["Name"],
                            Usage = mo["PercentProcessorTime"]
                        }
                        )
                        .ToList();
                    var query = cpuTimes.Where(x => x.Name.ToString() == "_Total").Select(x => x.Usage);
                    var CPUParcentage = query.SingleOrDefault();
                    RAMUsage = PerformanceInfo.GetTotalMemoryInMiB() - PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
                    DriveLetter = Path.GetPathRoot(Directory.GetCurrentDirectory());
                    DiskSpace = new DriveInfo(DriveLetter.Substring(0, DriveLetter.Length - 2));
                    TotalFreeSpace = DiskSpace.TotalFreeSpace / 1073741824;
                    TotalDiskSize = DiskSpace.TotalSize / 1073741824;
                    DiskspaceUsed = TotalDiskSize - TotalFreeSpace;
                    ClientAvatar avatar = level.Avatar;
                    GlobalChatLineMessage _MSG = new GlobalChatLineMessage(level.Client);
                    _MSG.PlayerName = "Server";
                    _MSG.LeagueId = 22;
                    _MSG.Message = @"Online Players: " + ResourcesManager.m_vOnlinePlayers.Count +
                                   "\nIn Memory Players: " + ResourcesManager.m_vInMemoryLevels.Count +
                                   "\nConnected Players: " + ResourcesManager.GetConnectedClients().Count +
                                   "\nTotal System CPU Usage: " + CPUParcentage + "%" +
                                   "\nServer RAM: " + Performances.GetUsedMemory() + "% / " + Performances.GetTotalMemory() + "MB" +
                                   "\nTotal Server Ram Usage: " + RAMUsage + "MB / " + Performances.GetTotalMemory() + "MB" +
                                   "\nServer Disk Space Used: " + Math.Round(DiskspaceUsed, 2) + "GB / " + Math.Round(TotalDiskSize, 2) + "GB";
                    _MSG.Send();
                    /*
                    var mail = new AllianceMailStreamEntry();
                    mail.ID = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    mail.SetSender(avatar);
                    mail.IsNew = 2;
                    mail.AllianceId = 0;
                    mail.AllianceBadgeData = 1526735450;
                    mail.AllianceName = "UCS Server Information";

                    //var p = new AvatarStreamEntryMessage(level.Client);
                    //p.SetAvatarStreamEntry(mail);
                    //Processor.Send(p);
                    */
                }
            }
            else
            {
                SendCommandFailedMessage(level.Client);
            }
        }
    }
}
