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

namespace ATMC.App.PosAdj {
    [JsonSerializable]
    public class Result : ResultBase {
        public Result(ModelConfig model) : base(model, Global.Setting.ResultDir) { }
        public override void SaveDB()
        {
        }
        public void SaveDB1(Result res_bat)
        {
            if (RegResults.Count == 0) { return; }
            try {
                OK = RegResults.Count > 0 && RegResults.Values.All(x => x != null && x.OK)
                    &&
                    res_bat.RegResults.Count > 0 && res_bat.RegResults.Values.All(x => x != null && x.OK);
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
                    key = "bat";
                    res_bat.RegResults.TryGetValue(key, out var res);
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
                    key = "body";
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

                //TODO: add offset
                {
                    var offset = (Model as ModelConfig).LhBodyReg.ShiftOffset;
                    if(offset != null) {
                        values["lhbody_OX"] = (float)offset.DX;
                        values["lhbody_OY"] = (float)offset.DY;
                        values["lhbody_OZ"] = (float)offset.DZ;
                        values["lhbody_ORX"] = (float)offset.DRx;
                        values["lhbody_ORY"] = (float)offset.DRy;
                        values["lhbody_ORZ"] = (float)offset.DRz;
                    }
                }

                {
                    var offset = (Model as ModelConfig).RhBodyReg.ShiftOffset;
                    if (offset != null) {
                        values["rhbody_OX"] = (float)offset.DX;
                        values["rhbody_OY"] = (float)offset.DY;
                        values["rhbody_OZ"] = (float)offset.DZ;
                        values["rhbody_ORX"] = (float)offset.DRx;
                        values["rhbody_ORY"] = (float)offset.DRy;
                        values["rhbody_ORZ"] = (float)offset.DRz;
                    }
                }

                {
                    var offset = (Model as ModelConfig).BodyReg.ShiftOffset;
                    if (offset != null) {
                        values["body_OX"] = (float)offset.DX;
                        values["body_OY"] = (float)offset.DY;
                        values["body_OZ"] = (float)offset.DZ;
                        values["body_ORX"] = (float)offset.DRx;
                        values["body_ORY"] = (float)offset.DRy;
                        values["body_ORZ"] = (float)offset.DRz;
                    }
                }

                {
                    var offset = (Model as ModelConfig).BatReg.ShiftOffset;
                    if (offset != null) {
                        values["bat_OX"] = (float)offset.DX;
                        values["bat_OY"] = (float)offset.DY;
                        values["bat_OZ"] = (float)offset.DZ;
                        values["bat_ORX"] = (float)offset.DRx;
                        values["bat_ORY"] = (float)offset.DRy;
                        values["bat_ORZ"] = (float)offset.DRz;
                    }
                }
                // insert to table
                if (key == "body") {
                    DB.Engine.NewQuery()
                    .InsertInto(DB.TableName)
                    .Values(values)
                    .Execute();
                }

            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //Save result
        public override void Save(string prefix = "") {
            if(RegResults.Count == 0) return;
            base.Save();
        }
    }
}
