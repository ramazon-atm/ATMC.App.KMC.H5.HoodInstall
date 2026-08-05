using Abeo.Controls.Common;
using ATMC.Common;
using LotusAPI;
using LotusAPI.Controls.Editors;
using LotusAPI.Math;
using LotusAPI.MV;
using LotusAPI.Robotics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Design;
using System.IO;
using System.Security.Policy;

namespace ATMC.App.KMC.H5.HoodInstall {
    //Define car model configurations
    [JsonSerializable]
    [TypeConverter(typeof(AbeoTypeConverter))]
    public class ModelConfig : ModelConfigBase {
        [Category("REGISTRATION")]
        public FastRegistrationConfig PickReg { get; set; } = new FastRegistrationConfig();
        //[Category("REGISTRATION")]
        //public FastRegistrationConfig Pick_degReg { get; set; } = new FastRegistrationConfig();
        [Category("REGISTRATION")]
        public FastRegistrationConfig InstallReg { get; set; } = new FastRegistrationConfig();
        
        //TODO: add more registration configs if needed 

        //Define registration keys
        internal Dictionary<string, FastRegistrationConfig> RegDict => new Dictionary<string, FastRegistrationConfig> {
            { Global.KEY_PICK, PickReg},
            //{ Global.KEY_PICK_deg, Pick_degReg},
            { Global.KEY_INSTALL, InstallReg},
        };

        //Robot poses
        internal Dictionary<string, RobotPoseEx> RobotPoses = new Dictionary<string, RobotPoseEx>();
        [Category("GENERAL")]
        [ReadOnly(true)]
        public bool Valid => RobotPoses != null && RobotPoses.Count > 0
            && PickReg != null && PickReg.Valid
            && InstallReg != null && InstallReg.Valid;
            //&& Pick_degReg != null && Pick_degReg.Valid;

        //model name = model_wc.ply
        //cad name = cad_wc.ply
        public override void Init() {
            try {
                //make sure registration is not null
                PickReg = PickReg ?? new FastRegistrationConfig();
                //Pick_degReg = Pick_degReg ?? new FastRegistrationConfig();
                InstallReg = InstallReg ?? new FastRegistrationConfig();
                //Init registrations
                //model name = <key>_model_wc.ply
                //cad name = <key>_cad_wc.stl
                foreach (var kv in RegDict) {
                    kv.Value.Init(ModelDir, kv.Key, "wc");
                }
                //Read robot poses from file
                var robot_pose_file = $"{ModelDir}/robot_poses.json";
                if (File.Exists(robot_pose_file)) {
                    RobotPoses = JsonUtils.Read<Dictionary<string, RobotPoseEx>>(Json.ReadFromFile(robot_pose_file));
                }
            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //Create a new result
        public Result CreateResult() { return new Result(this); }
    }
}
