using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;


namespace FRDB_SQLite
{
    public class SqliteConnection
    {
        #region 1. Fields

        public SQLiteConnection _conn;
        public SQLiteCommand _cmd;
        public SQLiteDataAdapter _adapt;

        private String _commandText;    
        private CommandType _commandType;
        private String _errorMessage;
        private String[] _paramCollection;
        private Object[] _valueCollection;

        #endregion

        #region 2. Properties

        public String CommandText
        {
            get { return _commandText; }
            set { _commandText = value; }
        }

        public CommandType CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }

        public String ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public String[] ParamCollection
        {
            get { return _paramCollection; }
            set { _paramCollection = value; }
        }

        public Object[] ValueCollection
        {
            get { return _valueCollection; }
            set { _valueCollection = value; }
        }

        #endregion

        #region 3. Contructors

        public SqliteConnection()
        {
            this._conn = new SQLiteConnection();
            this._cmd = _conn.CreateCommand();
            this._adapt = new SQLiteDataAdapter();
        }

        public SqliteConnection(String connString)
        {
            this._conn = new SQLiteConnection();
            this._conn.ConnectionString = connString;
            this._cmd = _conn.CreateCommand();
            this._adapt = new SQLiteDataAdapter();
        }

        #endregion

        #region 4. Methods

        #region 4.1 Connection

        public Boolean CheckConnection(String connString)
        {
            try
            {
                _conn = new SQLiteConnection(connString);
                _conn.Open();
                _conn.Close();

                return true;
            }
            catch(SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return false;
            }
        }

        public void OpenConnect(String connString)
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    _conn.ConnectionString = connString;//this is connection string 
                    _conn.Open();
                }
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
            }
        }

        public void OpenConnect()
        {
            try
            {
                if (_conn.State == ConnectionState.Closed)
                {
                    //conn.ConnectionString = connString;//this is connection string 
                    _conn.Open();
                }
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
            }
        }

        public void CloseConnect()
        {
            try
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Clone();
                }
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
            }
        }

        #endregion

        #region 4.2 DatatTable and DataSet

        public DataSet GetDataSet(String query)
        {
            DataSet ds = new DataSet();
            OpenConnect();

            try
            {
                _cmd = new SQLiteCommand();
                _cmd.CommandText = query;
                _cmd.Connection = _conn;
                _cmd.CommandType = CommandType.Text;

                if (this._paramCollection != null)
                {
                    AddParam(_cmd);
                }

                _adapt = new SQLiteDataAdapter(_cmd);
                _adapt.Fill(ds);
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return null;
            }

            CloseConnect();

            return ds;
        }

        public DataSet GetDataSet(String query, String tableName)
        {
            DataSet ds = new DataSet();
            OpenConnect();

            try
            {
                _cmd = new SQLiteCommand();
                _cmd.CommandText = query;
                _cmd.Connection = _conn;
                _cmd.CommandType = CommandType.Text;

                if (this._paramCollection != null)
                {
                    AddParam(_cmd);
                }

                _adapt = new SQLiteDataAdapter(_cmd);
                _adapt.Fill(ds, tableName);
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return null;
            }

            CloseConnect();
            return ds;
        }

        public DataTable GetDataTable(String query)
        {
            DataTable dt = new DataTable();
            OpenConnect();

            try
            {
                _cmd = new SQLiteCommand();
                _cmd.CommandText = query;
                _cmd.Connection = _conn;
                _cmd.CommandType = CommandType.Text;

                if (this._paramCollection != null)
                {
                    AddParam(_cmd);
                }

                _adapt = new SQLiteDataAdapter(_cmd);
                _adapt.Fill(dt);    
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return null;
            }

            CloseConnect();
            return dt;
        }

        public DataTable GetDataTable(String query, String tableName)
        {
            DataTable dt = new DataTable(tableName);
            OpenConnect();

            try
            {
                _cmd = new SQLiteCommand();
                _cmd.CommandText = query;
                _cmd.Connection = _conn;
                _cmd.CommandType = CommandType.Text;

                if (this._paramCollection != null)
                {
                    AddParam(_cmd);
                }

                _adapt = new SQLiteDataAdapter(_cmd);
                _adapt.Fill(dt);
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return null;
            }

            CloseConnect();
            return dt;
        }

        public bool DropTable(String tableName)
        {
            try
            {
                OpenConnect();
                _commandText = "DROP TABLE IF EXISTS " + tableName;
                _commandType = CommandType.Text;
                int result = ExecuteNonQuery();
                CloseConnect();

                if (result < 0) return false;
            }
            catch (SQLiteException ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

            return true;
        }


        #endregion

        #region 4.3 Create, Insert, Update, Delete

        public bool CreateTable(String query)
        {
            try
            {
                OpenConnect();
                this._commandText = query;
                this._commandType = CommandType.Text;
                int result = ExecuteNonQuery();
                CloseConnect();

                if (result < 0)
                {
                    return false;
                }
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return false;
            }

            return true;
        }

        public Object GetValueField(String query)
        { 
            OpenConnect();
            
            try
            {
                _commandText = query;
                _paramCollection = null;

                return ExecuteScalar();
            }
            catch(SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return null;
            }

            //CloseConnect();
            //return null;
        }

        public bool Update(String query)
        {
            try
            {
                OpenConnect();

                _commandText = query;
                _commandType = CommandType.Text;
                int result = ExecuteNonQuery();

                CloseConnect();

                if (result < 0) return false;
            }
            catch (SQLiteException ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

            return true;
        }

        public int GetSchemeID(string schemeName)
        {
            OpenConnect();
            int id;

            try
            {
                _commandText = "SELECT ID FROM SystemScheme WHERE SchemeName=" + "'" + schemeName + "'";
                _paramCollection = null;
                id = Convert.ToInt16(ExecuteScalar());
            }
            catch (SQLiteException ex)
            {
                ErrorMessage = ex.Message;
                return -1;
            }

            return id;
        }

        #endregion

        #endregion

        #region 5. Privates

        private int ExecuteNonQuery()
        {
            int result = 0;

            try
            {
                _cmd = new SQLiteCommand();
                _cmd.CommandText = _commandText;
                _cmd.Connection = _conn;
                _cmd.CommandType = _commandType;

                if (_paramCollection != null)
                {
                    AddParam(_cmd);
                }

                result = _cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                this._errorMessage = ex.Message;
                return result = -1;
            }
            finally
            {
                _cmd.Dispose();
            }

            return result; //Result is the number of rows which ExecuteNonQuery did
        }

        private Object ExecuteScalar()
        {
            Object objValue = null;

            try
            {
                _cmd = new SQLiteCommand();
                _cmd.CommandText = _commandText;
                _cmd.Connection = _conn;

                if (_paramCollection != null)
                {
                    AddParam(_cmd);
                }

                objValue = _cmd.ExecuteScalar();
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
                return null;
            }
            finally
            {
                _cmd.Dispose();
            }

            return objValue;
        }

        private void AddParam(SQLiteCommand cmd)
        {
            try
            {
                for (int i = 0; i < _paramCollection.Length; i++)
                {
                    cmd.Parameters.AddWithValue(_paramCollection[i], _valueCollection[i]);
                }
            }
            catch (SQLiteException ex)
            {
                this._errorMessage = ex.Message;
            }
        }

        #endregion
    }
}
