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

namespace ATMC.App.PosAdj {
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
                    DbEngine.CreateFloatColumn(name:"bat_FIT"),
                    DbEngine.CreateFloatColumn(name:"bat_OVL"),
                    DbEngine.CreateFloatColumn(name:"bat_DX"),
                    DbEngine.CreateFloatColumn(name:"bat_DY"),
                    DbEngine.CreateFloatColumn(name:"bat_DZ"),
                    DbEngine.CreateFloatColumn(name:"bat_DRX"),
                    DbEngine.CreateFloatColumn(name:"bat_DRY"),
                    DbEngine.CreateFloatColumn(name:"bat_DRZ"), 
                    //offset
                    DbEngine.CreateFloatColumn(name:"bat_OX"),
                    DbEngine.CreateFloatColumn(name:"bat_OY"),
                    DbEngine.CreateFloatColumn(name:"bat_OZ"),
                    DbEngine.CreateFloatColumn(name:"bat_ORX"),
                    DbEngine.CreateFloatColumn(name:"bat_ORY"),
                    DbEngine.CreateFloatColumn(name:"bat_ORZ"),
                    //shift data (UF)
                    DbEngine.CreateFloatColumn(name:"body_FIT"),
                    DbEngine.CreateFloatColumn(name:"body_OVL"),
                    DbEngine.CreateFloatColumn(name:"body_DX"),
                    DbEngine.CreateFloatColumn(name:"body_DY"),
                    DbEngine.CreateFloatColumn(name:"body_DZ"),
                    DbEngine.CreateFloatColumn(name:"body_DRX"),
                    DbEngine.CreateFloatColumn(name:"body_DRY"),
                    DbEngine.CreateFloatColumn(name:"body_DRZ"), 
                    //offset
                    DbEngine.CreateFloatColumn(name:"body_OX"),
                    DbEngine.CreateFloatColumn(name:"body_OY"),
                    DbEngine.CreateFloatColumn(name:"body_OZ"),
                    DbEngine.CreateFloatColumn(name:"body_ORX"),
                    DbEngine.CreateFloatColumn(name:"body_ORY"),
                    DbEngine.CreateFloatColumn(name:"body_ORZ"),
                    //offset
                    DbEngine.CreateFloatColumn(name:"lhbody_OX"),
                    DbEngine.CreateFloatColumn(name:"lhbody_OY"),
                    DbEngine.CreateFloatColumn(name:"lhbody_OZ"),
                    DbEngine.CreateFloatColumn(name:"lhbody_ORX"),
                    DbEngine.CreateFloatColumn(name:"lhbody_ORY"),
                    DbEngine.CreateFloatColumn(name:"lhbody_ORZ"),
                    //offset
                    DbEngine.CreateFloatColumn(name:"rhbody_OX"),
                    DbEngine.CreateFloatColumn(name:"rhbody_OY"),
                    DbEngine.CreateFloatColumn(name:"rhbody_OZ"),
                    DbEngine.CreateFloatColumn(name:"rhbody_ORX"),
                    DbEngine.CreateFloatColumn(name:"rhbody_ORY"),
                    DbEngine.CreateFloatColumn(name:"rhbody_ORZ"),
                    //result dir
                    DbEngine.CreateTextColumn(name:"RESULT_DIR", length:256),//max 256 bytes
                };

                //Create table, the engine will automatically adjust table structure
                Engine.EnsureTable(TableName, columns);

            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

    }
}
