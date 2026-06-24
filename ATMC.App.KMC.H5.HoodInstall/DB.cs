using Abeo.Controls;
using ATMC.Common;
using LotusAPI;
using LotusAPI.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using COL = LotusAPI.Data.DbEngine.ColumnDefinition;

namespace ATMC.App.KMC.H5.HoodInstall {
    public static class DB {
        public const string TableName = "RESULT";
        internal static DbEngine Engine = null;
        public static void Init() {
            try {
                //Create db engine
                string constr = $"Data Source={Global.DbFile};";
                Engine = DbEngine.CreateSqliteEngine(constr);

                //TODO: add a list of column definitions
                var columns = new List<COL> {
                    //cycle info
                    DbEngine.CreateIntColumn(name:"ID",primary:true,autoIncrement:true), //auto
                    DbEngine.CreateDateTimeColumn(name:"DATE",defaultValue:"CURRENT_TIMESTAMP"), //auto
                    DbEngine.CreateCharColumn(name:"MODEL",length:32),
                    DbEngine.CreateCharColumn(name:"BODY_NO",length:32),
                    DbEngine.CreateCharColumn(name:"SEQ_NO",length:8),
                    DbEngine.CreateCharColumn(name:"RESULT",length:2), //OK|NG|PS...
                    //shift data (UF)
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK + "_DRZ"),

                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_PICK_deg + "_DRZ"),
                    //shift data (UF)
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_DRZ"), 
                    //offset
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_OX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_OY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_OZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_ORX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_ORY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_INSTALL + "_ORZ"),
                    DbEngine.CreateTextColumn(name:"RESULT_DIR", length:256),//max 256 bytes
                };

                //Create table, the engine will automatically adjust table structure
                Engine.EnsureTable(TableName, columns);

            } 
            catch(Exception ex) { 
                Logger.Error(ex.Message);
                Logger.Trace(ex.StackTrace); 
            }

        }

    }
}
