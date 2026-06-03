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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// DEFINE APPLICATION CONFIGURATION HERE
namespace ATMC.App.PosAdj {
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
        public Property_<bool> UseLHDegradeMode { get; } = new Property_<bool>(false, "GENERAL");
        public Property_<bool> UseRHDegradeMode { get; } = new Property_<bool>(false, "GENERAL");

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
    public enum OutputPin { OK, NG, BODY_OK, BODY_NG, BAT_OK, BAT_NG, BAT_NEXT, BODY_NEXT, PCRUN, HW_ERR }
    public enum InputPin { RESET, RESET_Body, START, START_Body, BAT, LHBAT,RHBAT, BODY, LHBODY1, LHBODY2, RHBODY1, RHBODY2, END }
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
        //|name | R1  | R2 | R3 | S1 | S2 | S3 | S4 |    |    |    |
        //+-----+-----+----+----+----+----+----+----+----+----+----+
        public static string[] HW_BITS => new string[] { "R1", "R2", "R3", "S1", "S2", "S3", "S4" };

        //================================================
        // KEY NAMES
        //================================================
        public const string R1 = "R1";
        public const string R2 = "R2";
        public const string R3 = "R3";
        public const string S1 = "S1";
        public const string S2 = "S2";
        public const string S3 = "S3";
        public const string S4 = "S4";

        public const string WC_to_R1 = "WC->R1";
        public const string WC_to_R2 = "WC->R2";
        public const string WC_to_R3 = "WC->R3";

        //registration keys
        public const string KEY_BAT =  "bat";
        public const string KEY_BODY = "body";
        public const string KEY_LHBODY = "lhbody";
        public const string KEY_RHBODY = "rhbody";

        public const string KEY_BAT1 = KEY_BAT + "1";
        public const string KEY_BAT2 = KEY_BAT + "2";
        public const string KEY_BODY1 = KEY_BODY + "1";
        public const string KEY_BODY2 = KEY_BODY + "2";

        public const string KEY_LH_BODY1 = KEY_BODY + "lh_1";
        public const string KEY_LH_BODY2 = KEY_BODY + "lh_2";
        public const string KEY_RH_BODY1 = KEY_BODY + "rh_1";
        public const string KEY_RH_BODY2 = KEY_BODY + "rh_2";

        //SCAN ACTIONS
        // define scan actions
        // clouds will be saved as p0_wc_raw.asd, p1_wc_raw.asd...
        // robot poses will be saved in robot_poses.json with <key, value> = <p0, pose>, <p1, pose>...
        internal static string[] ScanActionKeys = new string[] { KEY_BAT, KEY_BODY, KEY_LHBODY, KEY_RHBODY };

        internal static List<ScanAction> ScanActionList; 
        public static Dictionary<string, ScanAction> ScanActionDict; 

        //INSTALL POSES
        public static List<RobotPoseDef> InstallPoses = null; //no install pose

        //SCAN ACTION GROUP
        //if combine = true, combined cloud will be saved as model_raw_wc.asd
        public static List<ScanActionGroup> ScanActionGroups; 

        public static void InitActions() {
            ScanDef Scan_S1R1 = new ScanDef($"S1+R1->WC"); //bolting robot 1
            ScanDef Scan_S2R2 = new ScanDef($"S2+R2->WC"); //bolting robot 2
            ScanDef Scan_S3R3 = new ScanDef($"S3+R3->WC"); 
            ScanDef Scan_S4R3 = new ScanDef($"S4+R3->WC");

            ScanActionList = new List<ScanAction> {
                new ScanAction(KEY_BODY1, Scan_S1R1),
                new ScanAction(KEY_BODY2, Scan_S2R2),
                new ScanAction(KEY_BAT1, Scan_S3R3),
                new ScanAction(KEY_BAT2, Scan_S4R3),
                new ScanAction(KEY_LH_BODY1, Scan_S1R1),
                new ScanAction(KEY_LH_BODY2, Scan_S1R1),
                new ScanAction(KEY_RH_BODY1, Scan_S2R2),
                new ScanAction(KEY_RH_BODY2, Scan_S2R2),
            };

            ScanActionDict = ScanActionList.ToActionDict();

            ScanActionGroups = new List<ScanActionGroup> { 
                new ScanActionGroup(name:KEY_BAT,actions:ScanActionDict.GetActionList(KEY_BAT1),combine:false),
                new ScanActionGroup(name:KEY_BODY,actions:ScanActionDict.GetActionList(KEY_BODY1,KEY_BODY2),combine:true),
                new ScanActionGroup(name:KEY_LHBODY,actions:ScanActionDict.GetActionList(KEY_LH_BODY1,KEY_LH_BODY2),combine:true),
                new ScanActionGroup(name:KEY_RHBODY,actions:ScanActionDict.GetActionList(KEY_RH_BODY1,KEY_RH_BODY2),combine:true)
            };
        }
    }


    //================================================
    // MAIN WORK FLOW
    //================================================
    public partial class FormMain {
        async Task Do_BAT_SCAN() {
   
            var key = Global.KEY_BAT;
            var signal = "BAT"; //just OK,NG
            await dashboard.SetResultTag(0, key, "..."); //processing
            string action_key1 = Global.KEY_BAT1;
            string action_key2 = Global.KEY_BAT2;
            FlyTo(key); //fly to view name = bat
            try {
                if (IsAuto) { await Clear_OKNG(signal); }
                var r = _cycle.GetResult();
                var m = _cycle.GetModel();

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

                var scene = Pointcloud.Combine(pc1,pc2);
                //display
                AsyncInvoke(delegate {
                    var sv = sv_Main.GetViewer();
                    sv[$"BAT/{key}"].UpdateObject(scene.Decimate(Global.Setting.DecimationStep), Color.Gray);
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
                var H_r3 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R3, H_wc);
                var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                //check limit with centered shift
                res.LimitMatrix = H_cen;
                res.CheckLimit();

                //display 3D
                AsyncInvoke(delegate {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"BAT/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"BAT/CAD"].Matrix = H_wc;
                    sv.Render();
                });
                //update result table
                await UpdateResultView();

                //check OK
                if (!res.OK) throw new Exception("Result is NG!");

                //send to robot
                if (!await SendRobotShift(Global.R3, H_r3, Global.Setting.UFVarIdx))
                    throw new Exception($"Failed to send robot shift to {Global.R3}");

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
            }
            catch (Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Debug(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }

        async Task Do_BODY_SCAN() {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            var key = Global.KEY_BODY;
            var signal = "BODY"; //just OK,NG
            await dashboard.SetResultTag(0, key, "..."); //processing
            string action_key1 = Global.KEY_BODY1;
            string action_key2 = Global.KEY_BODY2;
            FlyTo(key); //fly to view name = bat
            try {
                if (IsAuto) { await Clear_OKNG(signal); }
                var r = _bodyCycle.GetResult();
                var m = _bodyCycle.GetModel();

                Pointcloud scene = null;

                // Scan
                if (manual_load) {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        scene = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                } 
                else { 
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
                AsyncInvoke(delegate {
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
                var H_r1 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R1, H_wc);
                var H_r2 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R2, H_wc);
                var H_r3 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R3, H_wc);
                var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                //check limit with centered shift
                res.LimitMatrix = H_cen;
                res.CheckLimit();

                //display 3D
                AsyncInvoke(delegate {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"BODY/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"BODY/CAD"].Matrix = H_wc;
                    sv.Render();
                });
                //update result table
                await UpdateResultView();

                //check OK
                if (!res.OK) throw new Exception("Result is NG!");

                //send to robot
                if (!await SendRobotShift(Global.R1, H_r1, Global.Setting.UFVarIdx))
                    throw new Exception($"Failed to send robot shift to {Global.R1}");
                if (!await SendRobotShift(Global.R2, H_r2, Global.Setting.UFVarIdx))
                    throw new Exception($"Failed to send robot shift to {Global.R2}");
                if (!await SendRobotShift(Global.R3, H_r3, Global.Setting.UFVarIdx+10))
                    throw new Exception($"Failed to send robot shift to {Global.R3}");

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
            }
            catch (Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Debug(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }

        async Task Do_INV_BAT_SCAN(int idx) {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            
            
            var key = Global.KEY_BAT;
            var signal = "BAT"; //just OK,NG
            await dashboard.SetResultTag(0, key, "..."); //processing
            string action_key = Global.KEY_BAT + idx.ToString();
            FlyTo(key); //fly to scan view
            try {
                if (IsAuto && idx == 1) { await Clear_OKNG(signal); }

                var r = _cycle.GetResult();
                var m = _cycle.GetModel();
                Pointcloud pc = null;

                // Scan
                if (manual_load) {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        pc = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else {
                    //Scan
                    Logger.Log($"Processing ({action_key})...");
                    var scanAction = Global.ScanActionDict[action_key];
                    pc = await Scan(scanAction);
                    r.Clouds[key] = pc;
                }

                //pulse next pin
                //if (IsAuto && !UseRobotIO && idx == 1) { await _plc?.Client?.PulseDOBit(OutputPin.BAT_NEXT, 500); }

                //display
                AsyncInvoke(delegate {
                    var sv = sv_Main.GetViewer();
                    sv[$"BAT/{key}"].UpdateObject(pc.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });


                //CALCUCLATE REGISTRATION
                var reg = m.RegDict[key];
                //combine scene
                var scene = pc;

                //align but dont check limit because we are in WC
                var res = reg.Align(scene: scene, initial_matrix: null, check_limit: false)
                    ?? throw new Exception($"[{key}] Registration result is NULL!");

                r.RegResults[key] = res;

                var H_wc = res.Matrix;
                var H_r3 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R3, H_wc);
                var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                //check limit with centered shift
                res.LimitMatrix = H_cen;
                res.CheckLimit();

                //display 3D
                AsyncInvoke(delegate {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"BAT/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"BAT/CAD"].Matrix = H_wc;
                    sv.Render();
                });
                //update result table
                await UpdateResultView();

                //check OK
                if (!res.OK) throw new Exception("Result is NG!");

                //send to robot
                if (!await SendRobotShift(Global.R3, H_r3, Global.Setting.UFVarIdx))
                    throw new Exception($"Failed to send robot shift to {Global.R3}");

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
            }
            catch (Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Debug(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }

        async Task Do_LH_BODY_SCAN(int idx) {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            var key = Global.KEY_LHBODY;
            var signal = "BODY"; //just OK,NG
            await dashboard.SetResultTag(0, key, "..."); //processing
            string action_key = Global.KEY_LH_BODY1;
            if(idx==2) action_key = Global.KEY_LH_BODY2;
            FlyTo(action_key); //fly to scan view
            try {
                var r = _bodyCycle.GetResult();
                var m = _bodyCycle.GetModel();
                Pointcloud pc = null;
                if (manual_load && idx == 2)
                {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        pc = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else {
                    //Scan
                    Logger.Log($"Processing ({action_key})...");
                    var scanAction = Global.ScanActionDict[action_key];
                    pc = await Scan(scanAction);
                    if (idx == 1)
                    {
                        r.Clouds[Global.KEY_LH_BODY1] = pc;
                    }
                    else
                    {
                        r.Clouds[Global.KEY_LH_BODY2] = pc;
                    }
                }

                //pulse next pin
                if (IsAuto && !UseRobotIO && idx == 1) { await _plc?.Client?.SetDOBit(OutputPin.BODY_NEXT,true); }

                //display
                AsyncInvoke(delegate {
                    var sv = sv_Main.GetViewer();
                    sv[$"BODY/{action_key}"].UpdateObject(pc.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //Finish scan => compute result 
                if (idx == 2) {
                    //CALCUCLATE REGISTRATION
                    var reg = m.RegDict[key];
                    Pointcloud scene = null;
                    if (manual_load && idx == 2) {
                        scene = pc;
                    }
                    else {
                        //combine scene
                        scene = Pointcloud.Combine(r.Clouds[Global.KEY_RH_BODY1], r.Clouds[Global.KEY_RH_BODY2]);
                    }
                    //align but dont check limit because we are in WC
                    var res = reg.Align(scene: scene, initial_matrix: null, check_limit: false)
                        ?? throw new Exception($"[{key}] Registration result is NULL!");

                    r.RegResults[key] = res;

                    var H_wc = res.Matrix;
                    var H_r1 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R1, H_wc);
                    var H_r3 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R3, H_wc);
                    var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                    //check limit with centered shift
                    res.LimitMatrix = H_cen;
                    res.CheckLimit();

                    //display 3D
                    AsyncInvoke(delegate {
                        //cloud view
                        var sv = sv_Main.GetViewer();
                        sv[$"BODY/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                        sv[$"BODY/CAD"].Matrix = H_wc;
                        sv.Render();
                    });
                    //update result table
                    await UpdateResultView();

                    //check OK
                    if (!res.OK) throw new Exception("Result is NG!");

                    //send to robot
                    if (!await SendRobotShift(Global.R1, H_r1, Global.Setting.UFVarIdx))
                        throw new Exception($"Failed to send robot shift to {Global.R1}");
                    if (!await SendRobotShift(Global.R3, H_r3, Global.Setting.UFVarIdx + 10))
                        throw new Exception($"Failed to send robot shift to {Global.R3}");
                    //set PLC OK/NG
                    if (IsAuto) await Set_OK(signal);
                }
            }
            catch (Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Debug(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }

        async Task Do_RH_BODY_SCAN(int idx) {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            var key = Global.KEY_RHBODY;
            var signal = "BODY"; //just OK,NG
            await dashboard.SetResultTag(0, key, "..."); //processing
            string action_key = Global.KEY_RH_BODY1;
            if (idx == 2) action_key = Global.KEY_RH_BODY2;
            FlyTo(action_key); //fly to scan view
            try {
                var r = _bodyCycle.GetResult();
                var m = _bodyCycle.GetModel();
                Pointcloud pc = null;
                if (manual_load && idx == 2)
                {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        pc = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else {
                    //Scan
                    Logger.Log($"Processing ({action_key})...");
                    var scanAction = Global.ScanActionDict[action_key];
                    pc = await Scan(scanAction);
                    if (idx == 1)
                    {
                        r.Clouds[Global.KEY_RH_BODY1] = pc;
                    }
                    else {
                        r.Clouds[Global.KEY_RH_BODY2] = pc;
                    }
                }
                //pulse next pin
                if (IsAuto && !UseRobotIO && idx == 1) { await _plc?.Client?.SetDOBit(OutputPin.BODY_NEXT, true); }

                //display
                AsyncInvoke(delegate {
                    var sv = sv_Main.GetViewer();
                    sv[$"BODY/{action_key}"].UpdateObject(pc.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //Finish scan => compute result 
                if (idx == 2) {
                    //CALCUCLATE REGISTRATION
                    var reg = m.RegDict[key];
                    Pointcloud scene = null;
                    if (manual_load && idx == 2) {
                        scene = pc;
                    }
                    else {
                        //combine scene
                        scene = Pointcloud.Combine(r.Clouds[Global.KEY_RH_BODY1], r.Clouds[Global.KEY_RH_BODY2]);
                    }
                    //align but dont check limit because we are in WC
                    var res = reg.Align(scene: scene, initial_matrix: null, check_limit: false)
                        ?? throw new Exception($"[{key}] Registration result is NULL!");

                    r.RegResults[key] = res;

                    var H_wc = res.Matrix;
                    var H_r2 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R2, H_wc);
                    var H_r3 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R3, H_wc);
                    var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                    //check limit with centered shift
                    res.LimitMatrix = H_cen;
                    res.CheckLimit();

                    //display 3D
                    AsyncInvoke(delegate {
                        //cloud view
                        var sv = sv_Main.GetViewer();
                        sv[$"BODY/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                        sv[$"BODY/CAD"].Matrix = H_wc;
                        sv.Render();
                    });
                    //update result table
                    await UpdateResultView();

                    //check OK
                    if (!res.OK) throw new Exception("Result is NG!");

                    //send to robot
                    if (!await SendRobotShift(Global.R2, H_r2, Global.Setting.UFVarIdx))
                        throw new Exception($"Failed to send robot shift to {Global.R2}");
                    if (!await SendRobotShift(Global.R3, H_r3, Global.Setting.UFVarIdx + 10))
                        throw new Exception($"Failed to send robot shift to {Global.R3}");
                    //set PLC OK/NG
                    if (IsAuto) await Set_OK(signal);
                }
            }
            catch (Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Debug(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }

        //28. MAIN IO SIGNAL PROCESSING FUNCTION
        void ProcessInputSignal(string signal_name) {
            try {
                //TODO: Other manual signal, eg: M_<MODEL>...
                if (signal_name.StartsWith("M_"))
                {
                    SetCurrentModel(signal_name.Substring(2));
                    SetCurrentBodyModel(signal_name.Substring(2));
                    return;
                }
                InputPin key = (InputPin)Enum.Parse(typeof(InputPin), signal_name);

                switch(key) {
                    case InputPin.RESET: _tq.Enqueue(Do_RESET); break;
                    case InputPin.START: _tq.Enqueue(Do_START); break;
                    case InputPin.RESET_Body: _tq.Enqueue(Do_BodyRESET); break;
                    case InputPin.START_Body: _tq.Enqueue(Do_BodySTART); break;
                    case InputPin.BAT: _tq.Enqueue(async () => await Do_INV_BAT_SCAN(1)); break;
                    case InputPin.BODY: _tq.Enqueue(Do_BODY_SCAN); break;
                    case InputPin.LHBODY1: _tq.Enqueue(async () => await Do_LH_BODY_SCAN(1)); break;
                    case InputPin.LHBODY2: _tq.Enqueue(async () => await Do_LH_BODY_SCAN(2)); break;
                    case InputPin.RHBODY1: _tq.Enqueue(async () => await Do_RH_BODY_SCAN(1)); break;
                    case InputPin.RHBODY2: _tq.Enqueue(async () => await Do_RH_BODY_SCAN(2)); break;
                    case InputPin.END: _tq.Enqueue(Do_END); break;
                        //TODO: SCAN SIGNAL
                }

            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); }
        }
        //29. CYCLE TESTING
        MessageBuilder GetCycleTestState() {
            MessageBuilder mb = new MessageBuilder();
            mb.Set("groups", new Json[] {
                new Json(){
                    ["title"]= "BAT",
                    ["actions"] = new Enum[] {
                        InputPin.BAT,  
                    }
                },
                new Json(){
                    ["title"]= "BODY",
                    ["actions"] = new Enum[] {
                        InputPin.BODY,
                    }
                }
            });
            return mb;
        }
    }
}


