using ATMC.Common;
using ATMC.Common.WorkCell;
using LotusAPI;
using LotusAPI.Controls.Editors;
using LotusAPI.MV;
using LotusAPI.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// DEFINE APPLICATION CONFIGURATION HERE
namespace ATMC.App.KMC.H5.HoodInstall {
    //================================================
    // GENERAL SETTINGS
    //================================================
    public class MySetting : SettingObject {
        //GENERALS
        public Property_<string> Language { get; } = new Property_<string>("ko", "GENERAL", WriteProtectionType.AskForPermission);
        public Property_<string> ResultDir { get; } = new Property_<string>("D:/Results", "GENERAL", typeof(DirectoryLocationEditor), WriteProtectionType.AskForPermission);
        public Property_<int> HistoryDayCount { get; } = new Property_<int>(90, "GENERAL", WriteProtectionType.AskForPermission);
        public Property_<int> DecimationStep { get; } = new Property_<int>(3, "DISPLAY");
        public Property_<int> CadModelOpacity { get; } = new Property_<int>(50, "DISPLAY");
        public Property_<bool> ShowHwErrAlarm { get; } = new Property_<bool>(false, "GENERAL");
        public Property_<WorkCell.PlcSystemEnum> PlcSystem { get; } = new Property_<WorkCell.PlcSystemEnum>(WorkCell.PlcSystemEnum.OnePLC, "PLC", WriteProtectionType.AskForPermission);

        public Property_<float> NullToolTolerance { get; } = new Property_<float>(10.0f, "ROBOT", WriteProtectionType.AskForPermission);
        public Property_<int> UFVarIdx { get; } = new Property_<int>(11, "ROBOT", WriteProtectionType.AskForPermission);
        public Property_<int> DataSendTryCount { get; } = new Property_<int>(3, "ROBOT", WriteProtectionType.AskForPermission);

        //PROFILERS
        public MySetting() : base() {
            this.PropertyChangedEvent += MySetting_PropertyChangedEvent;
        }

        private void MySetting_PropertyChangedEvent(object sender, Property e) {
            //TODO: Handle property changed event
        }
    }


    //================================================
    // Define INPUT / OUTPUT /MEMORY BLOCKS
    //================================================
    public enum OutputPin { PICK_OK, PICK_NG, INSTALL_OK, INSTALL_NG, PCRUN, HW_ERR, AUTO }
    public enum InputPin { RESET, START, PICK, PICK_deg, INSTALL, END }
    public enum PlcBlock { CAR_TYPE, BODY_NO, SEQ_NO, HW_ERRCODE, PCRUN }

    //================================================
    // APP CONFIGS
    //================================================
    internal static partial class Global {
        //================================================
        // HARDWARE ERROR BITS
        //================================================
        //+-----+-----+----+----+----+----+----+----+----+----+----+
        //|bit  | 0   | 1  | 2  | 3  | 4  | 5  | 6  | 7  | 8  | 9  |
        //+-----+-----+----+----+----+----+----+----+----+----+----+
        //|name | R1  | R2 | S1 | S2 | S3 | S4 | S5 | S6 |    |    |
        //+-----+-----+----+----+----+----+----+----+----+----+----+
        public static string[] HW_BITS => new string[] { "R1", "R2", "S1", "S2", "S3", "S4", "S5", "S6" };

        //================================================
        // KEY NAMES
        //================================================
        public const string R1 = "R1";
        public const string R2 = "R2";
        public const string S1 = "S1";
        public const string S2 = "S2";
        public const string S3 = "S3";
        public const string S4 = "S4";
        public const string S5 = "S5";
        public const string S6 = "S6";

        public const string WC_to_R1 = "WC->R1";
        public const string WC_to_R2 = "WC->R2";

        //registration keys
        public const string KEY_INSTALL = "install";

        public const string KEY_PICK = "pick";
        public const string KEY_PICK_deg = "pick_deg";


        public const string KEY_INSTALL1 = "install1";
        public const string KEY_INSTALL2 = "install2";

        public const string KEY_PICK1 = "pick1";
        public const string KEY_PICK2 = "pick2";

        public const string KEY_PICK1_deg = "pick1_deg";
        public const string KEY_PICK2_deg = "pick2_deg";

        // TO DO: 디그리드 모드일때 따로 포즈값 줘야 되는지
        public const string KEY_PICK_SCAN_POSE = "pick_scan_pose";
        public const string KEY_INSTALL1_POSE = "install1_pose";
        public const string KEY_INSTALL2_POSE = "install2_pose";

        //SCAN ACTIONS
        // define scan actions
        // clouds will be saved as p0_wc_raw.asd, p1_wc_raw.asd...
        // robot poses will be saved in robot_poses.json with <key, value> = <p0, pose>, <p1, pose>...
        //internal static string[] ScanActionKeys = new string[] { KEY_BODY12 };

        internal static List<ScanAction> ScanActionList; 
        public static Dictionary<string, ScanAction> ScanActionDict;

        //INSTALL POSES
        public static List<RobotPoseDef> InstallPoses = new List<RobotPoseDef>{
            new RobotPoseDef($"{KEY_INSTALL1_POSE}:R1"),
            new RobotPoseDef($"{KEY_INSTALL2_POSE}:R1"),
        };


        //SCAN ACTION GROUP
        //if combine = true, combined cloud will be saved as model_raw_wc.asd
        public static List<ScanActionGroup> ScanActionGroups; 

        public static void InitActions() {
            ScanDef Scan_S1R1 = new ScanDef($"S1+R1->WC"); //bolting robot 1
            ScanDef Scan_S2R1 = new ScanDef($"S2+R1->WC");
            ScanDef Scan_S3R2 = new ScanDef($"S3+R2->WC");
            ScanDef Scan_S4R2 = new ScanDef($"S4+R2->WC");
            ScanDef Scan_S5R2 = new ScanDef($"S5+R2->WC");
            ScanDef Scan_S6R2 = new ScanDef($"S6+R2->WC");

            ScanActionList = new List<ScanAction> {
                new ScanAction(KEY_INSTALL1, Scan_S1R1),
                new ScanAction(KEY_INSTALL2, Scan_S2R1),
                new ScanAction(KEY_PICK1, Scan_S3R2, new RobotPoseDef($"{KEY_PICK_SCAN_POSE}:R2")),
                new ScanAction(KEY_PICK2, Scan_S4R2, new RobotPoseDef($"{KEY_PICK_SCAN_POSE}:R2")),
                new ScanAction(KEY_PICK1_deg, Scan_S5R2, new RobotPoseDef($"{KEY_PICK_SCAN_POSE}:R2")),
                new ScanAction(KEY_PICK2_deg, Scan_S6R2, new RobotPoseDef($"{KEY_PICK_SCAN_POSE}:R2")),
            };

            ScanActionDict = ScanActionList.ToActionDict();

            ScanActionGroups = new List<ScanActionGroup> { 
                new ScanActionGroup(name:KEY_INSTALL,actions:ScanActionDict.GetActionList(KEY_INSTALL1,KEY_INSTALL2),combine:true),
                new ScanActionGroup(name:KEY_PICK,actions:ScanActionDict.GetActionList(KEY_PICK1, KEY_PICK2),combine:true),
                new ScanActionGroup(name:KEY_PICK_deg,actions:ScanActionDict.GetActionList(KEY_PICK1_deg, KEY_PICK2_deg),combine:true),
            };
        }
    }


    //================================================
    // MAIN WORK FLOW
    //================================================
    public partial class FormMain {
        async Task DO_PICK_SCAN(int deg)
        {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            string key;
            string signal = "PICK"; //just OK,NG
            string action_key1;
            string action_key2;

            if (deg == 0)
            {
                action_key1 = Global.KEY_PICK1;
                action_key2 = Global.KEY_PICK2;
                key = Global.KEY_PICK;
            }
            else
            {
                action_key1 = Global.KEY_PICK1_deg;
                action_key2 = Global.KEY_PICK2_deg;
                key = Global.KEY_PICK_deg;
            }

            await dashboard.SetResultTag(0, key, "..."); //processing
            FlyTo(key); //fly to view name = install
            try
            {
                if (IsAuto) { await Clear_OKNG(signal); }
                var r = _cycle.GetResult();
                var m = _cycle.GetModel();

                Pointcloud scene = null;

                // Scan
                if (manual_load)
                {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        scene = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else
                {
                    //Scan
                    Logger.Log($"Processing ({key})...");
                    var scanAction1 = Global.ScanActionDict[action_key1];
                    var scanAction2 = Global.ScanActionDict[action_key2];

                    var t1 = Task.Run(async delegate { return await Scan(scanAction1); });
                    var t2 = Task.Run(async delegate { return await Scan(scanAction2); });
                    await Task.WhenAll(t1, t2);
                    var pc1 = t1.Result ?? throw new Exception($"Failed to scan ({scanAction1.Name})");
                    var pc2 = t2.Result ?? throw new Exception($"Failed to scan ({scanAction2.Name})");
                    r.Clouds[action_key1] = pc1;
                    r.Clouds[action_key2] = pc2;

                    scene = Pointcloud.Combine(pc1, pc2);
                }
                //display
                AsyncInvoke(delegate
                {
                    var sv = sv_Main.GetViewer();
                    sv[$"BODY/{key}"].UpdateObject(scene.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //Finish scan => compute result 
                //CALCUCLATE REGISTRATION
                var reg = m.RegDict[key];

                //align but dont check limit because we are in WC
                var res = reg.Align(scene: scene, initial_matrix: null, check_limit: false)
                    ?? throw new Exception($"[{key}] Registration result is NULL!");

                r.RegResults[key] = res;

                var H_wc = res.Matrix;
                var H_r2 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R2, H_wc);
                var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                //check limit with centered shift
                res.LimitMatrix = H_cen;
                res.CheckLimit();

                //display 3D
                AsyncInvoke(delegate
                {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"PICK/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"PICK/CAD"].Matrix = H_wc;
                    sv.Render();
                });
                //update result table
                await UpdateResultView();

                //check OK
                if (!res.OK) throw new Exception("Result is NG!");

                //send to robot
                //if (!await SendRobotShift(Global.R1, H_r1, Global.Setting.UFVarIdx))
                //    throw new Exception($"Failed to send robot shift to {Global.R1}");
                //if (!await SendRobotShift(Global.R2, H_r2, Global.Setting.UFVarIdx))
                //    throw new Exception($"Failed to send robot shift to {Global.R2}");

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
            }
            catch (Exception ex)
            {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Debug(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }

        async Task DO_INSTALL_SCAN()
        {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            string key;
            var signal = "INSTALL"; //just OK,NG // Check always if we use different signals

            string action_key1;
            string action_key2;

            action_key1 = Global.KEY_INSTALL1;
            action_key2 = Global.KEY_INSTALL2;
            key = Global.KEY_INSTALL;

            await dashboard.SetResultTag(0, key, "..."); //processing

            FlyTo(key); //fly to view name = install
            try
            {
                if (IsAuto) { await Clear_OKNG(signal); }
                var r = _cycle.GetResult();
                var m = _cycle.GetModel();

                Pointcloud scene = null;

                // Scan
                if (manual_load)
                {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        scene = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else
                {
                    //Scan
                    Logger.Log($"Processing ({key})...");
                    var scanAction1 = Global.ScanActionDict[action_key1];
                    var scanAction2 = Global.ScanActionDict[action_key2];

                    var t1 = Task.Run(async delegate { return await Scan(scanAction1); });
                    var t2 = Task.Run(async delegate { return await Scan(scanAction2); });
                    await Task.WhenAll(t1, t2);
                    var pc1 = t1.Result ?? throw new Exception($"Failed to scan ({scanAction1.Name})");
                    var pc2 = t2.Result ?? throw new Exception($"Failed to scan ({scanAction2.Name})");
                    r.Clouds[action_key1] = pc1;
                    r.Clouds[action_key2] = pc2;

                    scene = Pointcloud.Combine(pc1, pc2);
                }
                //display
                AsyncInvoke(delegate
                {
                    var sv = sv_Main.GetViewer();
                    sv[$"INSTALL/{key}"].UpdateObject(scene.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //Finish scan => compute result 
                //CALCUCLATE REGISTRATION
                var reg = m.RegDict[key];

                //align but dont check limit because we are in WC
                var res = reg.Align(scene: scene, initial_matrix: null, check_limit: false)
                    ?? throw new Exception($"[{key}] Registration result is NULL!");

                r.RegResults[key] = res;

                // 2.H_cen에 Offset을 적용한 경우
                var offset = reg.ShiftOffset;
                if (offset != null)
                    Logger.Log($"[{key}] ShiftOffset = {(offset == null ? "null (no offset set)" : $"DX:{offset.DX:F3} DY:{offset.DY:F3} DZ:{offset.DZ:F3} DRx:{offset.DRx:F3} DRy:{offset.DRy:F3} DRz:{offset.DRz:F3}")}");
                var H_wc = res.Matrix;
                var H_wc_raw = res.Matrix; // offset 적용 전 저장
                Logger.Log($"[{key}] Model.Center = ({reg.Model.Center.X:F3}, {reg.Model.Center.Y:F3}, {reg.Model.Center.Z:F3})");
                var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);
                H_cen = offset != null ? offset.ApplyShift(H_cen) : H_cen;
                H_wc = Global.WorkCell.Graph.ChangeBasis(H_cen, -reg.Model.Center); // H_wc 다시 계산 (역변환: T(center)*H_cen*T(-center))
                res.Matrix = H_wc;
                var H_r1 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R1, H_wc);
                var H_r2 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R2, H_wc);

                //var H_wc = res.Matrix;
                //var H_r1 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R1, H_wc);
                //var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                //check limit with centered shift
                res.LimitMatrix = H_cen;
                res.CheckLimit();

                //display 3D
                AsyncInvoke(delegate
                {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"INSTALL/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"INSTALL/CAD"].Matrix = H_wc;
                    sv.Render();
                });
                //update result table
                await UpdateResultView();

                //check OK
                if (!res.OK) throw new Exception("Result is NG!");

                //if (IsAuto) 
                if (!await SendInstallUF())
                    throw new Exception($"[Install_R2] Failed to send shift data");

                //send to robot
                if (!await SendRobotShift(Global.R1, H_r1, Global.Setting.UFVarIdx))
                    throw new Exception($"[Install_R1] Failed to send shift data");

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
            }
            catch (Exception ex)
            {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Debug(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }

        async Task <bool> SendInstallUF()
        {
            try
            {
                var m = _cycle.GetModel();
                var r = _cycle.GetResult();
                if (!r.RegResults.TryGetValue(Global.KEY_INSTALL, out var install_res)) { throw new Exception("INSTALL result is not defined!"); }
                if (install_res == null) throw new Exception("INSTALL result is NULL!");
                
                RegistrationResult pick_res = null;
                if (!r.RegResults.TryGetValue(Global.KEY_PICK, out pick_res))
                {
                    if (!r.RegResults.TryGetValue(Global.KEY_PICK_deg, out pick_res))
                    {
                        throw new Exception($"PICK result is not defined!");
                    }
                }
                if (!m.RobotPoses.TryGetValue(Global.KEY_PICK_SCAN_POSE, out var pick_scan_pose)) { throw new Exception("pick_scan_pose is not defined in model!"); }
                if (pick_scan_pose == null) throw new Exception("pick_scan_pose is NULL!");
                if (!m.RobotPoses.TryGetValue(Global.KEY_INSTALL1_POSE, out var install1_pose)) { throw new Exception("install1_pose is not defined in model!"); }
                if (install1_pose == null) throw new Exception("install1_pose is NULL!");
                if (!m.RobotPoses.TryGetValue(Global.KEY_INSTALL2_POSE, out var install2_pose)) { throw new Exception("install2_pose is not defined in model!"); }
                if (install2_pose == null) throw new Exception("install1_pose is NULL!");

                var H_pick_scan_pose = pick_scan_pose.Matrix;
                var H_install1_pose = install1_pose.Matrix;
                var H_install2_pose = install2_pose.Matrix;

                var H_pick_wc = pick_res.Matrix;
                var H_install_r2 = Global.WorkCell.Graph.ChangeBasis("WC->R2", install_res.Matrix);
                var H_install_r2_tool = Global.WorkCell.Graph.FrameToTool("WC", H_pick_wc.Inv(), "R2", H_pick_scan_pose); //frame shift to tool shift
                var H1_r2 = H_install_r2 * Global.WorkCell.Graph.ToolToFrame("R2", H_install1_pose, H_install_r2_tool, "R2");
                var H2_r2 = H_install_r2 * Global.WorkCell.Graph.ToolToFrame("R2", H_install1_pose, H_install_r2_tool, "R2");

                //send door shift data to R2
                if (await SendRobotShift("R2", H1_r2, Global.Setting.UFVarIdx))
                    return true;
            }
            catch (Exception ex)
            {
                LotusAPI.Logger.Error(ex.Message); LotusAPI.Logger.Debug(ex.StackTrace);
            }
            return false;
        }

        //28. MAIN IO SIGNAL PROCESSING FUNCTION
        void ProcessInputSignal(string signal_name) {
            try {
                //TODO: Other manual signal, eg: M_<MODEL>...
                if (signal_name.StartsWith("M_"))
                {
                    SetCurrentModel(signal_name.Substring(2));
                    return;
                }
                InputPin key = (InputPin)Enum.Parse(typeof(InputPin), signal_name);

                switch(key) {
                    case InputPin.RESET: _tq.Enqueue(DO_RESET); break;
                    case InputPin.START: _tq.Enqueue(DO_START); break;
                    case InputPin.PICK: _tq1.Enqueue(async () => await DO_PICK_SCAN(0)); break;
                    case InputPin.PICK_deg: _tq1.Enqueue(async () => await DO_PICK_SCAN(1)); break;
                    case InputPin.INSTALL: _tq2.Enqueue(async () => await DO_INSTALL_SCAN()); break;
                    case InputPin.END: _tq.Enqueue(DO_END); break;
                        //TODO: SCAN SIGNAL
                }

            } 
            catch(Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
            }
        }
        //29. CYCLE TESTING
        MessageBuilder GetCycleTestState() {
            MessageBuilder mb = new MessageBuilder();
            mb.Set("groups", new Json[] {
                new Json(){
                    ["title"]= "PICK",
                    ["actions"] = new Enum[] {
                        InputPin.PICK,  
                    }
                }
            });
            return mb;
        }
    }
}


