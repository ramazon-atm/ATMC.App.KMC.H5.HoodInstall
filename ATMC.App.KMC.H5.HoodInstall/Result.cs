using ATMC.Common;
using LotusAPI;
using LotusAPI.Math;
using LotusAPI.MV;
using LotusAPI.Robotics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATMC.App.KMC.H5.HoodInstall {
    [JsonSerializable]
    public class Result : ResultBase {
        public Result(ModelConfig model) : base(model, Global.Setting.ResultDir) { }
        //public override void SaveDB()
        //{
        //}
        public override void SaveDB()
        {
            if (RegResults.Count == 0) { return; }
            try {
                OK = RegResults.Count > 0 && RegResults.Values.All(x => x != null && x.OK);
                
                //ID and DATE is automatic
                //TODO: add sequence info
                var values = new Dictionary<string, object> {
                    { "DATE",Date},
                    { "MODEL",Model.Name??"" },
                    { "BODY_NO",BodyNo??""},
                    { "SEQ_NO",SeqNo??""},
                    { "RESULT",OK?"OK":"NG"},
                    { "RESULT_DIR",GetResultDir()},
                };

                string key = "";
                //TODO: add shift data (WC shift)
                //convert to UF shift data if needed
                {
                    key = "body12";
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.Matrix ?? res.LimitMatrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                        values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }
                }

                {
                    var offset = (Model as ModelConfig).Body12.ShiftOffset;
                    if (offset != null) {
                        values["body12_OX"] = (float)offset.DX;
                        values["body12_OY"] = (float)offset.DY;
                        values["body12_OZ"] = (float)offset.DZ;
                        values["body12_ORX"] = (float)offset.DRx;
                        values["body12_ORY"] = (float)offset.DRy;
                        values["body12_ORZ"] = (float)offset.DRz;
                    }
                }

                // insert to table
                DB.Engine.NewQuery()
                    .InsertInto(DB.TableName)
                    .Values(values)
                    .Execute();

            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //Save result
        public override void Save(string prefix = "") {
            if(RegResults.Count == 0) return;
            base.Save();
        }
    }
}
