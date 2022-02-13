using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // ==============================================================
        //  Instructions
        // ==============================================================
        //
        // To use, simply run the script with a gps waypoint as an argument.
        // The script will then attempt to jump to the destination.
        //
        // You may also put "abort" (without quotes) into the argument
        // box to abort any active jump.
        // 
        // ==============================================================
        //  Settings
        // ==============================================================
        //
        // Replace the value in quotes with the name of your jump drive block.
        private const string jumpDriveName = "Jump Drive";
        //
        // ==============================================================


        private IMyJumpDrive jumpDrive;
        public Program()
        {
            jumpDrive = (IMyJumpDrive)GridTerminalSystem.GetBlockWithName(jumpDriveName);
            if(jumpDrive == null)
            {
                List<IMyJumpDrive> drives = new List<IMyJumpDrive>();
                GridTerminalSystem.GetBlocksOfType<IMyJumpDrive>(drives, b => drives.Count == 0);
                jumpDrive = drives.FirstOrDefault();
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (jumpDrive == null)
            {
                Echo("No jump drive!");
                return;
            }

            if (argument.Length == 5 && argument.ToLowerInvariant() == "abort")
                AbortJump();
            else
                Jump(argument);

        }

        private void Jump(string argument)
        {
            if (jumpDrive.Status != MyJumpDriveStatus.Ready)
            {
                Echo("Jump drive is not ready!");
                return;
            }

            Vector3D pos;
            if (TryParseGPS(argument.Trim(), out pos))
            {
                Echo("Jumping to " + argument);
                jumpDrive.SetValue<Vector3D?>("ScriptJumpTarget", pos);
                jumpDrive.ApplyAction("ScriptJump");
            }
            else
            {
                Echo("Unable to parse " + argument + " into a gps coordinate.");
            }
        }

        private void AbortJump()
        {
            Echo("Aborted jump.");
            jumpDrive.ApplyAction("AbortJump");
        }

        private bool TryParseGPS(string argument, out Vector3D pos)
        {
            pos = new Vector3D();

            //GPS:name:x:y:z:color:
            string[] args = argument.Split(':');
            if (args.Length < 5 || args[0] != "GPS")
                return false;

            double x, y, z;
            if (!double.TryParse(args[2], out x) || !double.TryParse(args[3], out y) || !double.TryParse(args[4], out z))
                return false;

            pos = new Vector3D(x, y, z);
            return true;
        }
    }
}
