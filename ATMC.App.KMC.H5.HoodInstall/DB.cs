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
                    DbEngine.CreateFloatColumn(name:"body13_FIT"),
                    DbEngine.CreateFloatColumn(name:"body13_OVL"),
                    DbEngine.CreateFloatColumn(name:"body13_DX"),
                    DbEngine.CreateFloatColumn(name:"body13_DY"),
                    DbEngine.CreateFloatColumn(name:"body13_DZ"),
                    DbEngine.CreateFloatColumn(name:"body13_DRX"),
                    DbEngine.CreateFloatColumn(name:"body13_DRY"),
                    DbEngine.CreateFloatColumn(name:"body13_DRZ"), 
                    //offset
                    DbEngine.CreateFloatColumn(name:"body13_OX"),
                    DbEngine.CreateFloatColumn(name:"body13_OY"),
                    DbEngine.CreateFloatColumn(name:"body13_OZ"),
                    DbEngine.CreateFloatColumn(name:"body13_ORX"),
                    DbEngine.CreateFloatColumn(name:"body13_ORY"),
                    DbEngine.CreateFloatColumn(name:"body13_ORZ"),
                    //shift data (UF)
                    DbEngine.CreateFloatColumn(name:"body24_FIT"),
                    DbEngine.CreateFloatColumn(name:"body24_OVL"),
                    DbEngine.CreateFloatColumn(name:"body24_DX"),
                    DbEngine.CreateFloatColumn(name:"body24_DY"),
                    DbEngine.CreateFloatColumn(name:"body24_DZ"),
                    DbEngine.CreateFloatColumn(name:"body24_DRX"),
                    DbEngine.CreateFloatColumn(name:"body24_DRY"),
                    DbEngine.CreateFloatColumn(name:"body24_DRZ"), 
                    //offset
                    DbEngine.CreateFloatColumn(name:"body24_OX"),
                    DbEngine.CreateFloatColumn(name:"body24_OY"),
                    DbEngine.CreateFloatColumn(name:"body24_OZ"),
                    DbEngine.CreateFloatColumn(name:"body24_ORX"),
                    DbEngine.CreateFloatColumn(name:"body24_ORY"),
                    DbEngine.CreateFloatColumn(name:"body24_ORZ"),
                    //result dir
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
