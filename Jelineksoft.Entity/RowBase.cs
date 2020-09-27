using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Jelineksoft.Entity
{
    public abstract class RowBase
    {
        public RowBase()
        {
            
        }
        public RowBase(TableBase table)
        {
            DbHelpers.Table = table;
        }

        public RowBase(ProviderBase provider)
        {
            DbHelpers.Provider = provider;
        }

        public void Save()
        {
            var provider = DbHelpers.Provider;
            provider.ResetProvider();

            var idCol = DbHelpers.Table.GetIdColumn();
            var idVal = GetType().GetProperty(idCol.PropertyName).GetValue(this);
            if (DbHelpers.IsNew)
            { //Insert
                foreach (var col in DbHelpers.Table.GetTableColumns(true))
                {
                    if (col.Primary && col.AutoIncrement)
                        continue;
                    
                    var val = this.GetType().GetProperty(col.PropertyName).GetValue(this);
                    provider.AddColumnToInsert(col.DbName, val);
                }

                if (idCol.Primary && idCol.AutoIncrement)
                {
                    var retId =provider.CreateSqlInsert(DbHelpers.Table.TableName, true);
                    GetType().GetProperty(idCol.PropertyName).SetValue(this, Convert.ChangeType(retId, GetType().GetProperty(idCol.PropertyName).PropertyType)); 
                }
                else
                {
                    provider.CreateSqlInsert(DbHelpers.Table.TableName, false);
                }
            }
            else
            { //update
                foreach (var col in DbHelpers.Table.GetTableColumns(true))
                {
                    if (col.Primary)
                        continue;

                    var val = this.GetType().GetProperty(col.PropertyName).GetValue(this);
                    provider.AddColumnToUpdate(col.DbName, val);
                }

                provider.AddWhereParameter(idCol.DbName, DbHelpers.Table.TableName, idVal, PropertyCompareOperatorEnum.IsEqual, WhereMergeOperatorEnum.And);
                provider.CreateSqlUpDate(DbHelpers.Table.TableName);
                
            }
        }
        
        public void Delete()
        {
            var provider = DbHelpers.Provider;
            var idCol = DbHelpers.Table.GetIdColumn();
            var idVal = GetType().GetProperty(idCol.PropertyName).GetValue(this);
            if (DbHelpers.IsNew)
                return;
            provider.CreateDelete(idCol.DbName, idVal, idCol.DbTable.TableName);

        }

        [JsonIgnore]
        public HelpersClass DbHelpers { get; set; } = new HelpersClass();
        public class HelpersClass
        {
            private ProviderBase provider;
            public ProviderBase Provider
            {
                get
                {
                    if (provider == null)
                    {
                        if (this.Table.Provider == null)
                        {
                            provider = Settings.DefaultProvider.GetProvider();
                        }
                        else
                        {
                            provider = Table.Provider.GetProvider();
                        }
                    }
                    return provider;}
                set { provider = value; }
            }
            public TableBase Table { get; set; }
            public Boolean IsNew { get; set; } = true;
            
            public Dictionary<string, object> CustomColumnValues { get; set; }

            public object GetCustomValue(Column col)
            {
                if (CustomColumnValues != null)
                {
                    if (CustomColumnValues.ContainsKey(col.GetFullName()))
                    {
                        return CustomColumnValues[col.GetFullName()];
                    }
                }
                return null;
            }
        }
        
    }
}