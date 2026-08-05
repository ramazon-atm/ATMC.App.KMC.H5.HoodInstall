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
                    key = Global.KEY_PICK;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        //var matrix = res.Matrix ?? res.LimitMatrix;
                        var matrix = res.LimitMatrix ?? res.Matrix;
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

                //{
                //    key = Global.KEY_PICK_deg;
                //    RegResults.TryGetValue(key, out var res);
                //    if (res != null)
                //    {
                //        //var matrix = res.Matrix ?? res.LimitMatrix;
                //        var matrix = res.LimitMatrix ?? res.Matrix;
                //        var pose = new RobotPose(matrix);
                //        values[key + "_FIT"] = (float)res.Fitness;
                //        values[key + "_OVL"] = (float)res.Overlap;
                //        values[key + "_DX"] = (float)pose.X;
                //        values[key + "_DY"] = (float)pose.Y;
                //        values[key + "_DZ"] = (float)pose.Z;
                //        values[key + "_DRX"] = (float)pose.Rx;
                //        values[key + "_DRY"] = (float)pose.Ry;
                //        values[key + "_DRZ"] = (float)pose.Rz;
                //    }
                //}

                {
                    key = Global.KEY_INSTALL;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        //var matrix = res.Matrix ?? res.LimitMatrix;
                        var matrix = res.LimitMatrix ?? res.Matrix;
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

                    var offset = (Model as ModelConfig).InstallReg.ShiftOffset;
                    if (offset != null)
                    {
                        values[key + "_OX"] = (float)offset.DX;
                        values[key + "_OY"] = (float)offset.DY;
                        values[key + "_OZ"] = (float)offset.DZ;
                        values[key + "_ORX"] = (float)offset.DRx;
                        values[key + "_ORY"] = (float)offset.DRy;
                        values[key + "_ORZ"] = (float)offset.DRz;
                    }
                }

                DB.Engine.NewQuery()
                    .InsertInto(DB.TableName)
                    .Values(values)
                    .Execute();

            }
            catch (Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //Save result
        public override void Save(string prefix = "")
        {
            if (RegResults.Count == 0) return;
            base.Save();
        }
    }
}
