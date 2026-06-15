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
        public FastRegistrationConfig Body12 { get; set; } = new FastRegistrationConfig();

        //TODO: add more registration configs if needed 

        //Define registration keys
        internal Dictionary<string, FastRegistrationConfig> RegDict => new Dictionary<string, FastRegistrationConfig> {
            { Global.KEY_BODY12, Body12}
        };


        //model name = model_wc.ply
        //cad name = cad_wc.ply
        public override void Init() {
            try {
                //make sure registration is not null
                Body12 = Body12 ?? new FastRegistrationConfig();
                //Init registrations
                //model name = <key>_model_wc.ply
                //cad name = <key>_cad_wc.stl
                foreach (var kv in RegDict) {
                    kv.Value.Init(ModelDir, kv.Key, "wc");
                }

            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //Create a new result
        public Result CreateResult() { return new Result(this); }
    }
}
