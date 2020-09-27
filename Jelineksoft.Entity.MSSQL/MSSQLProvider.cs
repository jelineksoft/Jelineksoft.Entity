using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Jelineksoft.Entity.Providers
{


    public class MSSQLProvider : Jelineksoft.Entity.ProviderBase
    {

        public MSSQLProvider()
        {
            this.ConnectionString = Jelineksoft.Entity.Settings.ConnectionString;
            ResetProvider();
        }
        public MSSQLProvider(String connectionString)
        {
            this.ConnectionString = connectionString;
            ResetProvider();
        }
        
        public MSSQLProvider(int timeout)
        {
            this.CommandTimeout = timeout;
            this.ConnectionString = Jelineksoft.Entity.Settings.ConnectionString;
            ResetProvider();
        }

        public MSSQLProvider(String connectionString, int timeout)
        {
            this.ConnectionString = connectionString;
            this.CommandTimeout = timeout;
            ResetProvider();
        }

        private System.Data.SqlClient.SqlConnection _connection = null;
        private System.Data.SqlClient.SqlCommand _command = null;

        private const String _sql_paramPrefix = "@_";
        private String _where_addParenthesisStart = String.Empty;
        private Int32 _sql_paramIndex = 0;

        private StringBuilder _sb_select = null;
        private StringBuilder _sb_join = null;
        private StringBuilder _sb_where = null;
        private StringBuilder _sb_orderBy = null;
        private StringBuilder _sb_groupBy = null;
        private StringBuilder _sb_from = null;
        private StringBuilder _sb_update = null;
        private StringBuilder _sb_insertCol = null;
        private StringBuilder _sb_insertVal = null;
        private string _sb_distinct = null;
        private string _limit = "";


        public override void ResetProvider()
        {
            this._sb_select = new StringBuilder();
            this._sb_orderBy = new StringBuilder();
            this._sb_join = new StringBuilder();
            this._sb_where = new StringBuilder();
            this._sb_groupBy = new StringBuilder();
            this._sb_from = new StringBuilder();
            this._sb_update = new StringBuilder();
            this._sb_insertCol = new StringBuilder();
            this._sb_insertVal = new StringBuilder();
            this._sb_distinct = "";
            this._sql_paramIndex = 0;
            this._where_addParenthesisStart = String.Empty;
            this._limit = "";
            CreateCommand();
        }

        
        public override IDbConnection Connect()
        {
            if (this._connection == null)
            {
                this._connection = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
            }
            if (this._connection.State != System.Data.ConnectionState.Open)
            {
                this._connection.ConnectionString = this.ConnectionString;
                this._connection.Open();
            }

            _command.Connection = _connection;
            return _connection;
        }
        
        public override void Disconnect()
        {
            if (this._connection.State == System.Data.ConnectionState.Open)
            {
                this._connection.Close();
            }
            if (this._connection != null)
            {
                this._connection.Dispose();
                this._connection = null;
            }
            if (this._command != null)
            {
                this._command.Dispose();
                this._command = null;
            }
        }
        private void CreateCommand()
        {
            if (this._command == null)
            {
                this._command = new System.Data.SqlClient.SqlCommand();
            }
            this._command.CommandTimeout = this.CommandTimeout;
        }

        public override void ParametersRemoveAll()
        {
            this._command.Parameters.Clear();
            this._sql_paramIndex = 0;
        }

        public override ProviderBase GetProvider()
        {
            return new MSSQLProvider(this.ConnectionString, this.CommandTimeout);
        }

        public override void AddWhereParameter(String _columnName, String _tableName, Object _value, PropertyCompareOperatorEnum _propertyCompare, WhereMergeOperatorEnum _whereMergeOperator)
        {
            if (_sb_where.Length == 0)
            {
                _sb_where.Append(" WHERE ");
            }
            else
            {
                _sb_where.Append(GetWhereMergeOperatorString(_whereMergeOperator));
            }
            if (_value != null)
            {
                if (_propertyCompare == PropertyCompareOperatorEnum.Like)
                {
                    if (_value is string)
                    {
                        string a = (string)(_value);
                        a = a + "%";
                        _value = a;
                    }
                }
                this._command.Parameters.AddWithValue(_sql_paramPrefix + _sql_paramIndex.ToString(), _value);
                this._sb_where.Append( _where_addParenthesisStart + "[" + _tableName + "].[" + _columnName + "]"  + GetCompareOperator(_propertyCompare, _value) + _sql_paramPrefix + _sql_paramIndex.ToString());
                this._sql_paramIndex += 1;
            }
            else
            {
                if (_propertyCompare == PropertyCompareOperatorEnum.IsEqual)
                {
                    this._sb_where.Append(GetWhereMergeOperatorString(_whereMergeOperator) + _where_addParenthesisStart + "[" +  _tableName + "].[" + _columnName + "] IS NULL");
                }
                if (_propertyCompare == PropertyCompareOperatorEnum.IsNotEqual)
                {
                    this._sb_where.Append(GetWhereMergeOperatorString(_whereMergeOperator) + _where_addParenthesisStart + "[" +  _tableName +"].[" + _columnName + "] IS NOT NULL");
                }
            }
            this._where_addParenthesisStart = String.Empty;
        }
        public override void AddWhereParameterIN(String _columnName, String _tableName, Array _values, WhereMergeOperatorEnum _whereMergeOperator, Boolean _notIn)
        {
            if (_sb_where.Length == 0)
            {
                _sb_where.Append(" WHERE ");
            }
            else
            {
                _sb_where.Append(GetWhereMergeOperatorString(_whereMergeOperator));
            }

            this._sb_where.Append(  _where_addParenthesisStart + "["+_tableName + "].[" + _columnName+ "]" + (_notIn ? " NOT" : "").ToString() + " IN (");

            String _sep = "";
            foreach (var i in _values)
            {
                this._command.Parameters.AddWithValue(_sql_paramPrefix + _sql_paramIndex.ToString(), i);
                _sb_where.Append(_sep + _sql_paramPrefix + _sql_paramIndex.ToString());
                this._sql_paramIndex += 1;
                _sep = ", ";
            }
            _sb_where.Append(")");
            this._where_addParenthesisStart = String.Empty;
        }

        public override void AddWhereParenthesisStart()
        {
            this._where_addParenthesisStart += " (";
        }

        public override void AddWhereParenthesisEnd()
        {
            this._sb_where.Append(")");
        }

        public override void AddWhereString(string _whereString)
        {
            _sb_where.Append(" " + _whereString);
        }

        public override void AddOrderBy(string _columnName, string _tableName, bool _asc)
        {
            if (this._sb_orderBy.Length > 0)
            {
                this._sb_orderBy.Append(", ");
            }
            else
            {
                _sb_orderBy.Append(" ORDER BY ");
            }

            this._sb_orderBy.Append("[" + _tableName + "].[" + _columnName + "]");

            if (_asc == false)
            {
                this._sb_orderBy.Append(" DESC");
            }
        }

        public override void AddOrderBy(string _string)
        {
            if (this._sb_orderBy.Length > 0)
            {
                this._sb_orderBy.Clear();
            }

            this._sb_orderBy.Append(_string);
        }

        private string GetWhereMergeOperatorString(WhereMergeOperatorEnum _operator)
        {
                switch (_operator)
                {
                    case WhereMergeOperatorEnum.And:
                        return " AND ";
                    case WhereMergeOperatorEnum.Or:
                        return " OR ";
                    default:
                        return " AND ";
                }
        }

        private String GetCompareOperator(PropertyCompareOperatorEnum _oper, Object _value)
        {

            if (_value is String)
            {
                switch (_oper)
                {
                    case PropertyCompareOperatorEnum.IsEqual:
                        return "=";
                    case PropertyCompareOperatorEnum.IsNotEqual:
                        return "!=";
                    case PropertyCompareOperatorEnum.IsGreater:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.IsGreaterOrEqual:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.IsLess:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.IsLessOrEqual:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.Like:
                        return " LIKE ";
                }
            }


            if ((_value is Int16) || (_value is Int32) || (_value is Int64) || (_value is Decimal) || (_value is float) || (_value is Single) || (_value is uint) || (_value is ulong) || (_value is ushort))
            {
                switch (_oper)
                {
                    case PropertyCompareOperatorEnum.IsEqual:
                        return "=";
                    case PropertyCompareOperatorEnum.IsNotEqual:
                        return "<>";
                    case PropertyCompareOperatorEnum.IsGreater:
                        return ">";
                    case PropertyCompareOperatorEnum.IsGreaterOrEqual:
                        return ">=";
                    case PropertyCompareOperatorEnum.IsLess:
                        return "<";
                    case PropertyCompareOperatorEnum.IsLessOrEqual:
                        return "<=";
                    case PropertyCompareOperatorEnum.Like:
                        throw new Exception("Operator is not supported.");
                }
            }

            if (_value is DateTime)
            {
                switch (_oper)
                {
                    case PropertyCompareOperatorEnum.IsEqual:
                        return "=";
                    case PropertyCompareOperatorEnum.IsNotEqual:
                        return "<>";
                    case PropertyCompareOperatorEnum.IsGreater:
                        return ">";
                    case PropertyCompareOperatorEnum.IsGreaterOrEqual:
                        return ">=";
                    case PropertyCompareOperatorEnum.IsLess:
                        return "<";
                    case PropertyCompareOperatorEnum.IsLessOrEqual:
                        return "<=";
                    case PropertyCompareOperatorEnum.Like:
                        throw new Exception("Operator is not supported.");
                }
            }

            if (_value is Boolean)
            {
                switch (_oper)
                {
                    case PropertyCompareOperatorEnum.IsEqual:
                        return "=";
                    case PropertyCompareOperatorEnum.IsNotEqual:
                        return "!=";
                    case PropertyCompareOperatorEnum.IsGreater:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.IsGreaterOrEqual:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.IsLess:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.IsLessOrEqual:
                        throw new Exception("Operator is not supported.");
                    case PropertyCompareOperatorEnum.Like:
                        throw new Exception("Operator is not supported.");
                }
            }

            throw new Exception("Column type is not supported");

        }


        public override System.Data.IDataReader CreateSqlSelect()
        {
            if (_sb_from.Length <1) throw new ArgumentException("Please use AddFrom method too.");
            var sql = new StringBuilder();
            sql.Append("SELECT " + _sb_distinct + _limit);
            sql.Append(_sb_select);
            sql.Append(_sb_from);
            sql.Append(_sb_join);
            sql.Append(_sb_where);
            sql.Append(_sb_groupBy);
            sql.Append(_sb_orderBy);
            


            this._command.CommandText = sql.ToString();
            Settings.Log.LogSQL(_command.CommandText, _command.Parameters);

            Connect();
            return this._command.ExecuteReader();
        }

        public override void AddColumnToUpdate(string _name, object value)
        {
            var paramName = "@p" + _sql_paramIndex.ToString();
            if (_sb_update.Length == 0)
            {
                _sb_update.Append(" SET ");
            }
            else
            {
                _sb_update.Append(", ");
            }

            _sb_update.Append("[" + _name + "]="+ paramName);
            
            if (value != null)
            {
                if (value is DateTime)
                {
                    _command.Parameters.AddWithValue(paramName, ((DateTime)value).ToLocalTime());
                    /*
                    var par = new MySqlParameter(paramName, MySqlDbType.DateTime);
                    par.Value = new MySqlDateTime((DateTime) value);
                    _command.Parameters.Add(par);
                */
                }
                else
                {
                    _command.Parameters.AddWithValue(paramName, value);
                }
            }
            else
            {
                _command.Parameters.AddWithValue(paramName, DBNull.Value);
            }
            
            _sql_paramIndex++;
        }
        public override void AddColumnToInsert(string _name, object value)
        {
            var paramName = "@p" + _sql_paramIndex.ToString();
            if (_sb_insertCol.Length == 0)
            {
            }
            else
            {
                _sb_insertCol.Append(", ");
                _sb_insertVal.Append(", ");
            }

            _sb_insertCol.Append("[" +  _name + "]");
            _sb_insertVal.Append(paramName);
            
            if (value != null)
            {
                if (value is DateTime)
                {
                    //_command.Parameters.AddWithValue(paramName, value);
                    
                    var par = new SqlParameter(paramName, SqlDbType.DateTime2);
                    par.Value =value;
                    _command.Parameters.Add(par);
                
                }
                else
                {
                    _command.Parameters.AddWithValue(paramName, value);
                }

            }
            else
            {
                _command.Parameters.AddWithValue(paramName, DBNull.Value);
            }
            
            _sql_paramIndex++;
        }

        public override void CreateSqlUpDate(string _tableName)
        {
            var sql = new StringBuilder();

            sql.Append("UPDATE ");
            sql.Append(_tableName);
            sql.Append(_sb_update);
            sql.Append(_sb_where);

            this._command.CommandText = sql.ToString();
            Settings.Log.LogSQL(_command.CommandText, _command.Parameters);
            Connect();
            this._command.ExecuteNonQuery();
            Disconnect();
        }

        public override Object CreateSqlInsert(string _tableName, bool autoincr)
        {
            System.Text.StringBuilder _sb = new System.Text.StringBuilder();

            _sb.Append("INSERT INTO ");
            _sb.Append("[" +  _tableName + "]");
            _sb.Append(" (");
            _sb.Append(_sb_insertCol);
            _sb.Append(") VALUES(");
            _sb.Append(_sb_insertVal);
            _sb.Append(") ");

            if (autoincr)
            {
                _sb.AppendLine("SELECT SCOPE_IDENTITY()");
            }

            this._command.CommandText = _sb.ToString();
            Settings.Log.LogSQL(_command.CommandText, _command.Parameters);
            Connect();
            var ret =_command.ExecuteScalar();
            Disconnect();
            return ret;

        }

        public override void CreateDelete(string _idColumnName, object _idValue, string _tableName)
        {
            System.Text.StringBuilder _sb = new System.Text.StringBuilder();
            this._command.Parameters.Clear();

            _sb.Append("DELETE FROM ");
            _sb.Append("[" + _tableName + "[");
            _sb.Append(" WHERE ");
            _sb.Append(_idColumnName);
            _sb.Append(" = @ID");

            _command.Parameters.AddWithValue("@ID", _idValue);


            this._command.CommandText = _sb.ToString();
            Settings.Log.LogSQL(_command.CommandText, _command.Parameters);

            this._command.ExecuteNonQuery();
        }

        public override void CreateDelete(String _tableName)
        {
            System.Text.StringBuilder _sb = new System.Text.StringBuilder();

            _sb.Append("DELETE FROM ");
            _sb.Append("[" + _tableName + "]");
            if (_sb_where.ToString().Length > 0)
            {
                _sb.Append(" WHERE ");
                _sb.Append(_sb_where.ToString());
            }
            //Console.WriteLine(_sb.ToString());
            this._command.CommandText = _sb.ToString();
            Settings.Log.LogSQL(_command.CommandText, _command.Parameters);

            this._command.ExecuteNonQuery();
        }


        /*
        public override System.Data.IDataReader CreateSqlSelectWithJOIN()
        {
            this.CreateCommand();
            if (_sbSelect == null)
            {
                _sbSelect = new System.Text.StringBuilder();
            }

            if (_sbSelect.Length < 1)
            {
                _sbSelect.Append("SELECT ");
            }

            _sbSelect.Append(this._sb_selectJoin.ToString());
            if (this._sb_where.Length > 0)
            {
                _sbSelect.Append(" WHERE ");
                _sbSelect.Append(this._sb_where);
            }

            //Prida GroupBy
            if (this._sb_groupBy != null)
            {
                _sbSelect.Append(_sb_groupBy.ToString());
            }

            if (this._sb_orderBy.Length > 0)
            {
                _sbSelect.Append(" ORDER BY ");
                _sbSelect.Append(this._sb_orderBy);
            }

            if (_limit != "")
            {
                _sbSelect.Append(_limit);
            }

            //Console.WriteLine(_sbSelect.ToString());
            this._command.CommandText = _sbSelect.ToString();
            Settings.Log.LogSQL(_command.CommandText, _command.Parameters);
            //return null;
            return this._command.ExecuteReader();
        }
        */

        public override void AddColumnToSelect(String _columnName, String _tableName)
        {
            AddColumnToSelect("[" + _tableName + "].["+_columnName + "]");
        }
        public override void AddColumnToSelect(string _name)
        {
            if (this._sb_select.Length == 0)
            {
                this._sb_select.Append( _name );
            }
            else
            {
                this._sb_select.Append(", " + _name);
            }
        }
        public override void AddFrom(String _tableName, string _tableNameShortcut)
        {
            this._sb_from.Append(" FROM ");
            this._sb_from.Append("[" + _tableName + "] " + _tableNameShortcut);
        }
        public override void AddJoin(JoinTypeEnum _joinType, String _joinTableName, string _joinTableNameShortcut, String _tableNameLeft, String _columnNameLeft, String _tableNameRight, String _columnNameRight)
        {
            switch (_joinType)
            {
                case JoinTypeEnum.InnerJoin:
                    _sb_join.Append(" INNER JOIN ");
                    break;
                case JoinTypeEnum.LeftJoin:
                    _sb_join.Append(" LEFT JOIN ");
                    break;
                case JoinTypeEnum.RightJoin:
                    _sb_join.Append(" RIGHT JOIN ");
                    break;
            }
            _sb_join.Append(_joinTableName + " " + _joinTableNameShortcut);
            _sb_join.Append(" ON ");
            _sb_join.Append("[" + _tableNameLeft + "].[" + _columnNameLeft + "]");
            _sb_join.Append(" = ");
            _sb_join.Append("[" + _tableNameRight + "].[" + _columnNameRight + "]");
        }

        public override System.Data.IDataReader ExecuteReader(string _sql)
        {
            return this.ExecuteReader(_sql, false);
        }
        public override System.Data.IDataReader ExecuteReader(string _sql, Boolean _isStoredProcedure)
        {
            if (_isStoredProcedure == true)
            {
                this._command.CommandType = System.Data.CommandType.StoredProcedure;
            }
            else
            {
                this._command.CommandType = System.Data.CommandType.Text;
            }
            this._command.CommandText = _sql;
            Settings.Log.LogSQL(_sql, _command.Parameters);
            return this._command.ExecuteReader();
        }

        public override object ExecuteScalar(string _sql)
        {
            Settings.Log.LogSQL(_sql, _command.Parameters);
            return this.ExecuteScalar(_sql, false);
        }
        public override object ExecuteScalar(string _sql, Boolean _isStoredProcedure)
        {
            this.Connect();

            if (_isStoredProcedure == true)
            {
                this._command.CommandType = System.Data.CommandType.StoredProcedure;
            }
            else
            {
                this._command.CommandType = System.Data.CommandType.Text;
            }
            this._command.CommandText = _sql;
            Settings.Log.LogSQL(_sql, _command.Parameters);
            return this._command.ExecuteScalar();
        }

        public override void AddGroupBy(string _tableName, string _columnName)
        {
            if (this._sb_groupBy == null)
            {
                this._sb_groupBy = new System.Text.StringBuilder();
                this._sb_groupBy.Append(" GROUP BY ["+ _tableName + "].[" + _columnName + "]");
            }
            else
            {
                this._sb_groupBy.Append(", [" + _tableName + "].[" + _columnName + "]");
            }
        }

        public override void AddDistinct()
        {
            this._sb_distinct = "DISTINCT ";
        }

        //public override String AddAggregationFce(AggregationFunctionsEnum _fceType, string _columnName)
        //{
        //    String _sbAggreg = "";

        //    switch (_fceType)
        //    {
        //        case AggregationFunctionsEnum.Sum:
        //            _sbAggreg = "SUM(" + _columnName + ")";
        //            break;
        //        case AggregationFunctionsEnum.Min:
        //            _sbAggreg = "MIN(" + _columnName + ")";
        //            break;
        //        case AggregationFunctionsEnum.Max:
        //            _sbAggreg = "MAX(" + _columnName + ")";
        //            break;
        //        case AggregationFunctionsEnum.Avg:
        //            _sbAggreg = "AVG(" + _columnName + ")";
        //            break;
        //        case AggregationFunctionsEnum.Count:
        //            _sbAggreg = "COUNT(" + _columnName + ")";
        //            break;
        //        default:
        //            break;
        //    }
        //    return _sbAggreg;
        //}

        //public override String AddMathFce(string _columnNameLeft, string _columnNameRight, MathOperatorEnum _operator, Boolean _addParenthises)
        //{
        //    string _retVal = "";
        //    switch (_operator)
        //    {
        //        case MathOperatorEnum.Plus:
        //            _retVal = _columnNameLeft + " + " + _columnNameRight;
        //            break;
        //        case MathOperatorEnum.Minus:
        //            _retVal = _columnNameLeft + " - " + _columnNameRight;
        //            break;
        //        case MathOperatorEnum.Multiple:
        //            _retVal = _columnNameLeft + " * " + _columnNameRight;
        //            break;
        //        case MathOperatorEnum.Divide:
        //            _retVal = _columnNameLeft + " / " + _columnNameRight;
        //            break;
        //        default:
        //            break;
        //    }

        //    if (_retVal.Length > 0)
        //    {
        //        if (_addParenthises)
        //        {
        //            _retVal = "(" + _retVal + ")";
        //        }
        //    }
        //    return _retVal;
        //}

        public override string AddAggregationFce(AggregationFunctionsEnum _fceType, string _mathFce)
        {
            String _sbAggreg = "";

            switch (_fceType)
            {
                case AggregationFunctionsEnum.Sum:
                    _sbAggreg = "SUM(" + _mathFce + ")";
                    break;
                case AggregationFunctionsEnum.Min:
                    _sbAggreg = "MIN(" + _mathFce + ")";
                    break;
                case AggregationFunctionsEnum.Max:
                    _sbAggreg = "MAX(" + _mathFce + ")";
                    break;
                case AggregationFunctionsEnum.Avg:
                    _sbAggreg = "AVG(" + _mathFce + ")";
                    break;
                default:
                    break;
            }
            return _sbAggreg;
        }
        public override string AddAggregationFce(AggregationFunctionsEnum _fceType, string _tableName, string _columnName)
        {
            String _sbAggreg = "";

            switch (_fceType)
            {
                case AggregationFunctionsEnum.Sum:
                    _sbAggreg = "SUM([" + _tableName + "].[" + _columnName + "])";
                    break;
                case AggregationFunctionsEnum.Min:
                    _sbAggreg = "MIN([" + _tableName + "].[" + _columnName + "])";
                    break;
                case AggregationFunctionsEnum.Max:
                    _sbAggreg = "MAX([" + _tableName + "].[" + _columnName + "])";
                    break;
                case AggregationFunctionsEnum.Avg:
                    _sbAggreg = "AVG([" + _tableName + "].[" + _columnName + "])";
                    break;
                default:
                    break;
            }
            return _sbAggreg;
        }

        public override string AddMathFce(string _tableNameLeft, string _columnNameLeft, string _tableNameRight, string _columnNameRight, MathOperatorEnum _operator, bool _addParenthises)
        {
            string _retVal = "";
            switch (_operator)
            {
                case MathOperatorEnum.Plus:
                    _retVal ="["+ _tableNameLeft + "].[" + _columnNameLeft + "] + [" + _tableNameRight + "].[" + _columnNameRight + "]";
                    break;
                case MathOperatorEnum.Minus:
                    _retVal = "[" + _tableNameLeft + "].[" + _columnNameLeft + "] - [" + _tableNameRight + "].[" + _columnNameRight + "]";
                    break;
                case MathOperatorEnum.Multiple:
                    _retVal = "[" + _tableNameLeft + "].[" + _columnNameLeft + "] * [" + _tableNameRight + "].[" + _columnNameRight + "]";
                    break;
                case MathOperatorEnum.Divide:
                    _retVal = "[" + _tableNameLeft + "].[" + _columnNameLeft + "] / [" + _tableNameRight + "].[" + _columnNameRight + "]";
                    break;
                default:
                    break;
            }

            if (_retVal.Length > 0)
            {
                if (_addParenthises)
                {
                    _retVal = "(" + _retVal + ")";
                }
            }
            return _retVal;
        }

        public override string AddMathFce(string _columnNameLeft, string _columnNameRight, MathOperatorEnum _operator, bool _addParenthises)
        {
            string _retVal = "";
            switch (_operator)
            {
                case MathOperatorEnum.Plus:
                    _retVal ="["+ _columnNameLeft + "] + [" + _columnNameRight + "]";
                    break;
                case MathOperatorEnum.Minus:
                    _retVal = "[" + _columnNameLeft + "] - [" + _columnNameRight + "]";
                    break;
                case MathOperatorEnum.Multiple:
                    _retVal = "[" + _columnNameLeft + "] * [" + _columnNameRight + "]";
                    break;
                case MathOperatorEnum.Divide:
                    _retVal = "[" + _columnNameLeft + "] / [" + _columnNameRight + "]";
                    break;
                default:
                    break;
            }

            if (_retVal.Length > 0)
            {
                if (_addParenthises)
                {
                    _retVal = "(" + _retVal + ")";
                }
            }
            return _retVal;
        }

        public override string AddMathFce(string xText, bool _addParenthesis)
        {
            string _retVal = xText;
            if (_retVal.Length > 0)
            {
                if (_addParenthesis)
                {
                    _retVal = "(" + _retVal + ")";
                }
            }
            return _retVal;
        }

        public override IDataReader RunProcedure(string _procedureName, List<IDataParameter> _parameters)
        {
            throw new NotImplementedException();
        }


        /*
        public override System.Data.IDataReader RunProcedure(string _procedureName, System.Collections.Generic.List<System.Data.IDataParameter> _parameters)
        {
            this.CreateCommand();
            foreach (var _p in _parameters)
            {
                this._command.Parameters.Add(_p);
            }
            Singletons.RaiseEventLogSQL(this, "Procedure>" + _procedureName);

            return this.ExecuteReader(_procedureName, true);
        }
        */

        public override System.Data.IDataParameter CreateParameter()
        {
            return new System.Data.SqlClient.SqlParameter();
        }

        /*
        public override void CreateTable(string tableName, string columnName, string type, bool isKey, int maxCharSize, object defaultValue)
        {
            this.CreateCommand();
            var x = @"CREATE TABLE " + tableName + @" (" + columnName + @" " + GetColumnTypeFromClassType(type, isKey, maxCharSize) + ");";
            Settings.Log.LogSQL(x, null);
            this.ExecuteScalar(x);
        }
        */

        /*
        public override void CreateColumnInTable(string tableName, string columnName, string type, bool isKey, int maxCharSize, object defaultValue)
        {
            this.CreateCommand();
            var x = "ALTER TABLE " + tableName + " ADD COLUMN " + columnName + " " + GetColumnTypeFromClassType(type, isKey, maxCharSize) + ";";
            Settings.Log.LogSQL(x, null);
            this.ExecuteScalar(x);
        }
        */

        /*
        private string GetColumnTypeFromClassType(string classType, bool isKey, int maxCharSize)
        {
            switch (classType)
            {
                case "ColumnBoolean":
                    return "BOOLEAN";
                case "ColumnByte":
                    return "CHAR(1)";
                case "ColumnByteArray":
                    return "BLOB";
                case "ColumnDateTime":
                    return "DATETIME";
                case "ColumnDecimal":
                    return "DECIMAL(14,4)";
                case "ColumnInt16":
                    if (isKey == true)
                    {
                        return "SMALLINT AUTO_INCREMENT PRIMARY KEY";
                    }
                    else
                    {
                        return "SMALLINT";
                    }
                case "ColumnInt32":
                    if (isKey == true)
                    {
                        return "INTEGER AUTO_INCREMENT PRIMARY KEY";
                    }
                    else
                    {
                        return "INTEGER";
                    }
                case "ColumnInt64":
                    if (isKey == true)
                    {
                        return "BIGINT AUTO_INCREMENT PRIMARY KEY";
                    }
                    else
                    {
                        return "BIGINT";
                    }
                case "ColumnString":
                    return "VARCHAR(" + maxCharSize.ToString() + ")";
                default:
                    return "VARCHAR(250)";
            }
        }
        */
        
        /*public override System.Collections.Generic.List<String> GetIndexesInTable(String _tbl, JelinekPetr.Data.Provider.Common.Database.Database _db)
        {
            System.Collections.Generic.List<String> _idx = new System.Collections.Generic.List<string>();

            this.CreateCommand();
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _command.Parameters.Clear();
                //_command.CommandText = "SHOW INDEX FROM " + _tbl + ";";
                _command.CommandText = @"SELECT DISTINCT INDEX_NAME, TABLE_NAME,  TABLE_SCHEMA FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA='" + _db.Name + @"' AND TABLE_NAME = '"  +_tbl + @"';";
                //Console.WriteLine(_command.CommandText);
                var _dr = _command.ExecuteReader();
                while (_dr.Read())
                {
                    _idx.Add(_dr.GetString(0));
                }
                _dr.Close();
                _dr.Dispose();
            }
            return _idx;
        }*/

        /*public override void CreateIndexInTable(String _tbl, String _column, String _indexName)
        {
            this.CreateCommand();
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _command.Parameters.Clear();
                _command.CommandText = @"CREATE INDEX " + _indexName + @" ON " + _tbl + @"(" + _column + @");";

                //Console.WriteLine(_command.CommandText);
                _command.ExecuteScalar();
            }
        }*/

        /*public override void DropIndexInTable(string _indexName, String _column, String _tbl)
        {
            this.CreateCommand();
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _command.Parameters.Clear();
                _command.CommandText = @"DROP INDEX " + _indexName + @" FROM " + _tbl + ";";

                //Console.WriteLine(_command.CommandText);
                _command.ExecuteScalar();
            }
        }*/




        public override void AddLimit(int count)
        {
            this._limit = " TOP " + count.ToString() + " ";
        }

        public override object ReaderReturnNetType(object value)
        {
            return value;
        }
    }
}
