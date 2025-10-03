using System;
using System.Collections.Generic;
using System.Linq;
using UCS.Core;
using UCS.Helpers.Binary;
using UCS.Packets.Commands.Client;

namespace UCS.Packets.Messages.Client
{
    // Packet 14102
    internal class ExecuteCommandsMessage : Message
    {
        public ExecuteCommandsMessage(Device device, Reader reader) : base(device, reader)
        {
        }

        internal int CTick;
        internal int STick;
        internal int Checksum;
        internal int Count;

        internal byte[] Commands;
        internal List<Command> LCommands;

        internal override void Decode()
        {
            this.CTick = this.Reader.ReadInt32();
            this.Checksum = this.Reader.ReadInt32();
            this.Count = this.Reader.ReadInt32();
            this.STick =  this.STick = (int) Math.Floor(DateTime.UtcNow.Subtract(this.Device.Player.Avatar.LastTickSaved).TotalSeconds * 20);
            this.LCommands = new List<Command>((int) this.Count);
            this.Commands = this.Reader.ReadBytes((int) (this.Reader.BaseStream.Length - this.Reader.BaseStream.Position));
        }

        internal override void Process()
        {

            this.Device.Player.Tick();

            if (this.Count > -1 && this.Count <= 450)
            {
                using (Reader Reader = new Reader(this.Commands))
                {
                    for (int _Index = 0; _Index < this.Count; _Index++)
                    {
                        int CommandID = Reader.ReadInt32();
                        if (CommandFactory.Commands.ContainsKey(CommandID))
                        {
                            Command Command = Activator.CreateInstance(CommandFactory.Commands[CommandID], Reader, this.Device,CommandID) as Command;

                            if (Command != null)
                            {
                                Logger.Say($"{Command.GetType().Name} (" + CommandID + $") is handled by {this.Device.Player.Avatar.AvatarName} [{this.Device.Player.Avatar.UserId}]");
                                Command.Decode();
                                Command.Process();

                                this.LCommands.Add(Command);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            if (this.LCommands.Any())
                            {
                                if (CommandID == 0 && LCommands.First() is RotateDefenseCommand)
                                {
                                    RotateDefenseCommand shadowcommand = new RotateDefenseCommand(Reader, this.Device,554);
                                    shadowcommand.BuildingID = ((RotateDefenseCommand)LCommands.First()).BuildingID;
                                    shadowcommand.layoutId = ((RotateDefenseCommand)LCommands.First()).layoutId;
                                    shadowcommand.Process();
                                }
                                else
                                    Logger.Say("\nCommand " + CommandID + " has not been handled.\nPrevious command was " + this.LCommands.Last().Identifier + ". [" + (_Index + 1) + " / " + this.Count + "]\n");
                            }
                            else
                            {
                                Logger.Say("\nCommand " + CommandID + " has not been handled.\nNo previous command was handled\n");
                            }
                            Console.ResetColor();
                            Command command = Activator.CreateInstance(CommandFactory.Commands[404], Reader, this.Device,CommandID) as Command;
                            if (command != null)
                            {
                                command.Decode();
                                command.Process();

                                this.LCommands.Add(command);
                            }
                        }
                    }
                }
            }
            else
            {
                //new OutOfSyncMessage(this.Device).Send();
            }
        }
    }
}