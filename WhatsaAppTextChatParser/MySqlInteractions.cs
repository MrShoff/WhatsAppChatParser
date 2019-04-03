using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class MySqlInteractions
{
    #region Properties
    private MySqlConnection connection;
    public string Server { get; set; }
    public string Database { get; set; }
    public string Uid { get; set; }
    public string Password { get; set; }
    public string Port { get; set; }
    #endregion

    #region Initialization
    public MySqlInteractions()
    {
        Initialize();
    }

    //Initialize values
    private void Initialize()
    {
        Server = "localhost";
        Database = "whatsapp";
        Port = "3306";
        Uid = "WhatsAppParser";
        Password = "SecretPasskey12";
        string connectionString;
        connectionString = "SERVER=" + Server + ";" + "PORT=" + Port + ";" + "DATABASE=" + Database + ";"
            + "UID=" + Uid + ";" + "PASSWORD=" + Password + ";";
        connection = new MySqlConnection(connectionString);
    }
    #endregion

    #region Private Functions
    //open connection to database
    private bool OpenConnection()
    {
        try
        {
            connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            //When handling errors, you can your application's response based 
            //on the error number.
            //The two most common error numbers when connecting are as follows:
            //0: Cannot connect to server.
            //1045: Invalid user name and/or password.
            switch (ex.Number)
            {
                case 0:
                    UserTextFeedback.ConsoleOut("Cannot connect to server.  Contact administrator");
                    break;
                case 1045:
                    UserTextFeedback.ConsoleOut("Invalid username/password, please try again");
                    break;
                default:
                    UserTextFeedback.ConsoleOut(ex.ToString());
                    break;
            }
            return false;
        }
    }

    //Close connection
    private bool CloseConnection()
    {
        try
        {
            connection.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            UserTextFeedback.ConsoleOut(ex.Message);
            return false;
        }
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Execute a select query against your MySQL server.
    /// </summary>
    /// <param name="query">The select query.</param>
    /// <param name="numberOfColumns">The number of columns expected in the return value. Determines the size of the return array.</param>
    /// <param name="parameters">The collection of parameters used in the query.</param>
    /// <returns></returns>
    public List<object[]> Select(string query, uint numberOfColumns, MySqlParameter[] parameters = null)
    {
        //Create a list to store the result
        List<object[]> results = new List<object[]>();

        //Open connection
        if (OpenConnection())
        {
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            cmd.CommandTimeout = 300;
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            //Read the data and store them in the list
            while (dataReader.Read())
            {
                List<object> row = new List<object>();
                for (int i = 0; i < numberOfColumns; i++)
                {
                    row.Add(dataReader.GetValue(i));
                }
                results.Add(row.ToArray());
            }

            //close Data Reader
            dataReader.Close();

            //close Connection
            CloseConnection();

            //return list to be displayed
            return results;
        }
        else
        {
            return results;
        }
    }

    /// <summary>
    /// Insert data into MySQL. Make sure to use parameterized insert query to avoid injection.
    /// </summary>
    /// <param name="query">The insert query.</param>
    /// <param name="parameters">The collection of parameters used in the insert.</param>
    /// <returns></returns>
    public int Insert(string query, MySqlParameter[] parameters)
    {
        MySqlCommand sqlCmd = new MySqlCommand(query, connection);
        sqlCmd.Parameters.AddRange(parameters);

        OpenConnection();

        int results = sqlCmd.ExecuteNonQuery();

        CloseConnection();

        return results;
    }

    /// <summary>
    /// Update data in MySQL. Make sure to use parameterized update query to avoid injection.
    /// </summary>
    /// <param name="query">The update query.</param>
    /// <param name="parameters">The collection of parameters used in the update.</param>
    /// <returns></returns>
    public int Update(string query, MySqlParameter[] parameters)
    {
        MySqlCommand sqlCmd = new MySqlCommand(query, connection);
        sqlCmd.Parameters.Add(parameters);

        OpenConnection();

        int results = sqlCmd.ExecuteNonQuery();

        CloseConnection();

        return results;
    }

    /// <summary>
    /// Delete data in MySQL. Make sure to use parameterized delete query to avoid injection.
    /// </summary>
    /// <param name="query">The delete query.</param>
    /// <param name="parameters">The collection of parameters used in the delete.</param>
    /// <returns></returns>
    public int Delete(string query, MySqlParameter[] parameters)
    {
        MySqlCommand sqlCmd = new MySqlCommand(query, connection);
        sqlCmd.Parameters.Add(parameters);

        OpenConnection();

        int results = sqlCmd.ExecuteNonQuery();

        CloseConnection();

        return results;
    }
    #endregion
}

