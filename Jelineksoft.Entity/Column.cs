namespace Jelineksoft.Entity
{
    public class Column
    {
        public Column(TableBase table, string name, string dataType)
        {
            DbTable = table;
            DbName = name;
            PropertyName = name;
            DataType = dataType;
        }

        public Column(TableBase table,string name, string dataType, bool _allowNull)
        {
            DbTable = table;
            DbName = name;
            PropertyName = name;
            DataType = dataType;
            AllowNull = _allowNull;
        }

        public Column(TableBase table,string name, string dataType, bool primary, bool _autoIncrement)
        {
            DbTable = table;
            DbName = name;
            PropertyName = name;
            DataType = dataType;
            Primary = primary;
            AutoIncrement = _autoIncrement;
        }
        public Column(TableBase table,string dbName, string propertyName, string dataType, bool allowNull, string _default, string comment )
        {
            DbTable = table;
            DbName = dbName;
            PropertyName = propertyName;
            DataType = dataType;
            AllowNull = allowNull;
            Default = _default;
            Comment = comment;
        }
        

        public string DbName { get; private set; }
        public string PropertyName { get; private set; }
        public string DataType { get; private set; }
        public bool AllowNull { get; set; } = true;
        public bool Unique { get; set; } = false;
        public bool Primary { get; set; } = false;
        public bool AutoIncrement { get; set; } = false;
        public string Default { get; set; } = "";
        public string Comment { get; set; } = "";
        public bool Compressed { get; set; } = false;
        public bool IsInSelect { get; set; } = true;
        public TableBase DbTable { get; set; }

        public string GetFullName()
        {
            return DbTable.GetTableNameShortcut() + "." + DbName ;
        }

    }
}