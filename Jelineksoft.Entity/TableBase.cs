using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace Jelineksoft.Entity
{
    public abstract class TableBase
    {
        protected TableBase(string name)
            : this(name, Settings.DefaultEngine)
        {
        }

        protected TableBase(string name, string engine)
        {
            TableName = name;
            TableEngine = engine;
        }

        public abstract RowBase GetNewRow();
        public abstract IList GetCollectionRows();

        public string TableName { get; set; }
        public string TableEngine { get; set; }
        private ProviderBase provider;

        public ProviderBase Provider
        {
            get
            {
                if (provider == null)
                {
                    provider = Settings.DefaultProvider.GetProvider();
                }

                return provider;
            }
            set { provider = value; }
        }

        public List<Column> CustomColumnsToSelect = new List<Column>();
        public List<TableBase> CustomTablesToSelect = new List<TableBase>();
        public int JoinIndex = 0;
        private int JoinIndexX = 0;

        public IList LoadData()
        {
            return LoadData(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clearRows">If true, befor load data is Rows collection cleared.
        /// If false, loaded data will be add to last item in rows collection.</param>
        /// <returns></returns>
        public IList LoadData(bool clearRows)
        {
            IList rows = GetCollectionRows();
            if (clearRows)
                rows.Clear();

            var columns = GetTableColumns(true);
            foreach (var c in columns)
            {
                Provider.AddColumnToSelect(c.DbName, this.GetTableNameShortcut());
            }

            foreach (var cc in CustomColumnsToSelect)
            {
                Provider.AddColumnToSelect(cc.DbName, cc.DbTable.GetTableNameShortcut());
            }

            foreach (var custTable in CustomTablesToSelect)
            {
                var cols = custTable.GetTableColumns(true);
                foreach (var c in cols)
                {
                    Provider.AddColumnToSelect(c.DbName, c.DbTable.GetTableNameShortcut());
                }
            }

            using (var reader = Provider.CreateSqlSelect())
            {
                while (reader.Read())
                {
                    int i = 0;
                    FillDataToNewRow(reader, columns, this, ref i);
                    foreach (var custTabl in CustomTablesToSelect)
                    {
                        FillDataToNewRow(reader, custTabl.GetTableColumns(true), custTabl, ref i);
                    }
                }
            }

            Provider.Disconnect();

            return rows;
        }

        public void FillDataToNewRow(IDataReader reader, List<Column> columns, TableBase table,ref int i)
        {
            var r = table.GetNewRow();
            r.DbHelpers.IsNew = false;
            r.DbHelpers.Provider = Provider;
            // r.DbHelpers.Table = this;

            foreach (var col in columns)
            {
                var pi = r.GetType().GetProperty(col.PropertyName);
                try
                {
                    if (reader.IsDBNull(i))
                    {
                        if (!Settings.SetDBNullToDefaultClassValue)
                        {
                            if (col.AllowNull)
                            {
                                pi.SetValue(r, null);
                            }
                            else
                            {
                               //Settings.Log.LogSQL($"Column {col.PropertyName} is not nullable type but data has null value.",null);
                            }                           
                        }
                    }
                    else
                    {
                        var val = reader.GetValue(i);
                        //TODO: FLOAT
                        //try
                        //{ pi.SetValue(r, Convert.ChangeType(val, pi.PropertyType)); }
                        //catch (Exception ex)
                        //{
                        //    Settings.Log.LogSQL(ex.ToString(), null);
                        //}


                        pi.SetValue(r, provider.ReaderReturnNetType(val));
                    }
                }
                catch (Exception e)
                {
                    Settings.Log.LogSQL(e.ToString(), null);
                }

                i++;
            }

            if (CustomColumnsToSelect.Count > 0)
            {
                r.DbHelpers.CustomColumnValues = new Dictionary<string, object>();
            }

            foreach (var custColumn in CustomColumnsToSelect)
            {
                try
                {
                    r.DbHelpers.CustomColumnValues.Add(custColumn.GetFullName(),
                        provider.ReaderReturnNetType(reader.GetValue(i)));
                }
                catch (Exception e)
                {
                    Settings.Log.LogSQL(e.ToString(), null);
                }

                i++;
            }
        }

        public IList LoadDataById(object id)
        {
            Provider.ResetProvider();
            SetAllColumnsToSelect();
            Provider.AddFrom(this.TableName, this.GetTableNameShortcut());
            var idCol = GetIdColumn();
            Provider.AddWhereParameter(idCol.DbName, this.GetTableNameShortcut(), id, PropertyCompareOperatorEnum.IsEqual,
                WhereMergeOperatorEnum.And);
            return LoadData(true);
        }

        public IList LoadDataAll()
        {
            Provider.ResetProvider();
            SetAllColumnsToSelect();
            Provider.AddFrom(this.TableName, this.GetTableNameShortcut());
            return LoadData(true);
        }

        public void SetAllColumnsToNoSelect()
        {
            var cols = GetTableColumns(true);
            {
                foreach (var col in cols)
                {
                    col.IsInSelect = false;
                }
            }
        }

        public void SetAllColumnsToSelect()
        {
            var cols = GetTableColumns(false);
            {
                foreach (var col in cols)
                {
                    col.IsInSelect = true;
                }
            }
        }

        public List<Column> GetTableColumns(bool onlyInSelect)
        {
            var cols = new List<Column>();
            foreach (var prop in this.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(Column))
                {
                    var c = (Column) prop.GetValue(this);
                    if (onlyInSelect)
                    {
                        if (c.IsInSelect)
                            cols.Add(c);
                    }
                    else
                    {
                        cols.Add(c);
                    }
                }
            }

            return cols;
        }

        public Column GetIdColumn()
        {
            foreach (var column in GetTableColumns(false))
            {
                if (column.Primary)
                    return column;
            }

            return null;
        }

        /// <summary>
        /// Add ORDER BY. If exists, replace.
        /// </summary>
        /// <param name="orderBy"></param>
        public void AddOrderBy(string orderBy)
        {
            Provider.AddOrderBy(orderBy);
        }

        /// <summary>
        /// Add ORDER BY ASC.
        /// </summary>
        /// <param name="column"></param>
        public void AddOrderBy(Column column)
        {
            AddOrderBy(column, OrderByEnum.Asc);
        }

        public void AddOrderBy(Column column, OrderByEnum type)
        {
            switch (type)
            {
                case OrderByEnum.Asc:
                    Provider.AddOrderBy(column.DbName, column.DbTable.GetTableNameShortcut(), true);
                    break;
                default:
                    Provider.AddOrderBy(column.DbName, column.DbTable.GetTableNameShortcut(), false);
                    break;
            }
        }

        public void AddFrom(TableBase table)
        {
            if (table != this)
            {
                this.JoinIndexX++;
                table.JoinIndex = this.JoinIndexX;
                foreach (var c in table.GetTableColumns(false))
                {
                    //TODO: Mozna tu bude chyba
                    //c.DbTable = table;
                }
            }
            Provider.AddFrom(table.TableName, table.GetTableNameShortcut());
        }

        public void AddWhere(Column column, object value)
        {
            AddWhere(column, value, PropertyCompareOperatorEnum.IsEqual);
        }

        public void AddWhere(Column column, object value, PropertyCompareOperatorEnum compare)
        {
            AddWhere(column, value, compare, WhereMergeOperatorEnum.And);
        }

        public void AddWhere(Column column, object value, PropertyCompareOperatorEnum compare,
            WhereMergeOperatorEnum merge)
        {
            Provider.AddWhereParameter(column.DbName, column.DbTable.GetTableNameShortcut(), value, compare, merge);
        }

        public void AddJoin(JoinTypeEnum type, TableBase table, Column columnLeft, Column columnRight, bool loadJoinTableData)
        {
            AddJoin(type, table, columnLeft, columnRight);
            AddTableToLoadData(table);
        }

        public void AddJoin(JoinTypeEnum type, TableBase table, Column columnLeft, Column columnRight)
        {
            this.JoinIndexX++;
            table.JoinIndex = this.JoinIndexX;
            Provider.AddJoin(type, table.TableName, table.GetTableNameShortcut(), columnLeft.DbTable.GetTableNameShortcut(), 
                columnLeft.DbName, columnRight.DbTable.GetTableNameShortcut(), columnRight.DbName);
        }

        public void AddWhereParenthesisStart()
        {
            Provider.AddWhereParenthesisStart();
        }

        public void AddWhereParenthesisEnd()
        {
            Provider.AddWhereParenthesisEnd();
        }

        public void AddTableToLoadData(TableBase table)
        {
            this.CustomTablesToSelect.Add(table);
        }

        public string GetTableNameShortcut()
        {
            return "t" + JoinIndex.ToString();
        }

        public void AddDistinct()
        {
            Provider.AddDistinct();
        }
    }
}