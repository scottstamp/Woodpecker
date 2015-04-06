using System;
using System.Data;

using MySql.Data.MySqlClient;

using Woodpecker.Core;

namespace Woodpecker.Storage
{
    /// <summary>
    /// A reuseable instance of a database connection, for accessing/writing data into the database.
    /// </summary>
    public class Database
    {
        #region Fields
        /// <summary>
        /// The MySqlConnection object of this connection. This object is private.
        /// </summary>
        private MySqlConnection Connection;
        /// <summary>
        /// The MySqlDataAdapter object of this connection, required for inserting data etc. This object is private.
        /// </summary>
        private MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
        /// <summary>
        /// The MySqlCommand object of this connection, used for executing commands at the database. This object is private.
        /// </summary>
        private MySqlCommand Command = new MySqlCommand();
        /// <summary>
        /// A boolean indicating if the Database object should be closed after the next query.
        /// </summary>
        public bool closeAfterNextQuery;
        /// <summary>
        /// The connection string for connections. This string is static.
        /// </summary>
        public static string connectionString;
        
        private bool _Ready = false;
        /// <summary>
        /// Gets the current readystate. (connected yes/no)
        /// </summary>
        public bool Ready
        {
            get { return this._Ready; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the Database object, with the options to open the database upon constructing, and/or to make the Database object tidy up (close connection and dispose resources) after the first query.
        /// </summary>
        /// <param name="openNow">Indicates if the database connection should be opened already.</param>
        /// <param name="closeAfterNextQuery">Indicates if the Database object should close the connection and dispose resources after the first query.</param>
        public Database(bool openNow, bool closeAfterFirstQuery)
        {
            if (openNow)
                this.Open();
            this.closeAfterNextQuery = closeAfterFirstQuery;
        }
        #endregion

        #region Methods
        #region Opening and closing database
        /// <summary>
        /// Attempts to open a connection to the database and prepares for use.
        /// </summary>
        public void Open()
        {
            // Attempt to connect to the database and handle exceptions
            try
            {
                this.Connection = new MySqlConnection(connectionString);
                this.Connection.Open();
                this.Command.Connection = this.Connection;
                this.dataAdapter.SelectCommand = this.Command;
                this._Ready = true;
            }
            catch (MySqlException ex) // Error while connecting
            {
                Logging.Log(ex.Message, Logging.logType.commonError);
            }
        }
        /// <summary>
        /// Closes the connection to the database, if connected. All resources are disposed.
        /// </summary>
        public void Close()
        {
            if (this._Ready)
            {
                this.Connection.Close();
                this.Connection = null;
                this.dataAdapter = null;
                this.Command = null;
                this.closeAfterNextQuery = false;
                this._Ready = false;
            }
        }
        #endregion

        #region Data access
        /// <summary>
        /// Returns a DataSet object containing requested data of various tables.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public DataSet getDataSet(string Query)
        {
            DataSet dReturn = new DataSet();
            try
            {
                this.Command.CommandText = Query;
                this.dataAdapter.Fill(dReturn);
            }
            catch (Exception ex) { Logging.Log(ex.Message, Logging.logType.commonError); }
            if (this.closeAfterNextQuery)
                this.Close();

            return dReturn;
        }
        /// <summary>
        /// Returns a DataTable object containing requested data of a single table.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public DataTable getTable(string Query)
        {
            //Logging.Log("Retrieving datatable; " + Query);
            DataTable dReturn = new DataTable();
            try
            {
                this.Command.CommandText = Query;
                this.dataAdapter.Fill(dReturn);
            }
            catch (Exception ex) { Logging.Log(ex.Message, Logging.logType.commonError); }
            if (this.closeAfterNextQuery)
                this.Close();

            return dReturn;
        }
        /// <summary>
        /// Returns a DataRow object containing requested data of a single row of a single table.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public DataRow getRow(string Query)
        {
            DataRow dReturn = null;
            try
            {
                DataSet tmpSet = new DataSet();
                this.Command.CommandText = Query;
                this.dataAdapter.Fill(tmpSet);
                dReturn = tmpSet.Tables[0].Rows[0];
            }
            catch { }

            if (this.closeAfterNextQuery)
                this.Close();

            return dReturn;
        }
        /// <summary>
        /// Retrieves a single field value from the database and returns it as a string.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public string getString(string Query)
        {
            string s = "";
            try
            {
                this.Command.CommandText = Query;
                s = this.Command.ExecuteScalar().ToString();
            }
            catch { }
            if (this.closeAfterNextQuery)
                this.Close();

            return s;
        }
        /// <summary>
        /// Retrieves a single field value from the database and returns it as an integer.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public int getInteger(string Query)
        {
            int i = 0;
            try
            {
                this.Command.CommandText = Query;
                i = int.Parse(this.Command.ExecuteScalar().ToString());
            }
            catch { }
            if (this.closeAfterNextQuery)
                this.Close();

            return i;
        }
        /// <summary>
        /// Returns a boolean indicating if there were results for a certain query.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public bool findsResult(string Query)
        {
            bool Found = false;
            try
            {
                this.Command.CommandText = Query;
                MySqlDataReader dReader = this.Command.ExecuteReader();
                Found = dReader.HasRows;
                dReader.Close();
            }
            catch (Exception ex) { Logging.Log(ex.Message, Logging.logType.commonError); }
            if (this.closeAfterNextQuery)
                this.Close();

            return Found;
        }
        #endregion

        #region Other
        /// <summary>
        /// Adds a parameter with a value to the current parameter collection, for use in queries. A '@' symbol is placed infront of the parameter key automatically.
        /// </summary>
        /// <param name="Parameter">The parameter key to add. '@' symbol is added infront.</param>
        /// <param name="Value">The value of the parameter, can be any type.</param>
        public void addParameterWithValue(string Parameter, object Value)
        {
            this.Command.Parameters.AddWithValue("@" + Parameter, Value);
        }
        public void addRawParameter(MySqlParameter Parameter)
        {
            this.Command.Parameters.Add(Parameter);
        }
        /// <summary>
        /// Clears all parameters from the parameter collection.
        /// </summary>
        public void clearParameters()
        {
            this.Command.Parameters.Clear();
        }
        /// <summary>
        /// Attempts to open a connection to the database to execute a query.
        /// </summary>
        /// <param name="Query">The query string to execute.</param>
        public void runQuery(string Query)
        {
            try
            {
                this.Command.CommandText = Query;
                this.Command.ExecuteNonQuery();
            }
            catch (Exception ex) { Logging.Log(ex.Message, Logging.logType.commonError); }
            if (this.closeAfterNextQuery)
                this.Close();
        }
        #endregion
        #endregion

    }
}
