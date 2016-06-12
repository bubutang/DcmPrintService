using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Transactions;
using System.Reflection;


namespace PrintSystem.Common.SQLServer
{
    public enum QCConnectionType
    {
        PrintSystem = 0
    }

    public class ConnectionManager
    {
        private ConnectionStringSettings printsystemConnectionStringSettings = null;

        private static ConnectionManager _connectionManager = null;

        public ConnectionManager()
        {
            try
            {
                printsystemConnectionStringSettings = ConfigurationManager.ConnectionStrings["PrintSystem_Connection"];

                if (printsystemConnectionStringSettings == null)
                {

                }

            }
            catch (ConfigurationErrorsException configurationErrorsException)
            {
            }
        }

        public static ConnectionManager GetInstance()
        {
            if (_connectionManager == null)
            {
                _connectionManager = new ConnectionManager();
            }
            return _connectionManager;
        }

        /// <summary>
        /// get a connection string by connection type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string Get(QCConnectionType type)
        {
            if (type == QCConnectionType.PrintSystem)
            {
                return printsystemConnectionStringSettings.ConnectionString;
            }
            return "";
        }
    }

    public class SQLServerHelper
    {
        /// <summary>
        /// Query DB by Sql and return the Dataset result
        /// </summary>
        /// <param name="queryString">SQL</param>
        /// <returns></returns>
        public static DataSet ExecuteQuery(SqlWrapper _SqlWrapper, QCConnectionType connectionType)
        {
            DataSet dataset = new DataSet();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionManager.GetInstance().Get(connectionType)))
                {
                    SqlDataAdapter dadp = new SqlDataAdapter();
                    SqlCommand cmd = new SqlCommand(_SqlWrapper.SqlString, conn);
                    cmd.CommandType = _SqlWrapper.CommandType;
                    if (_SqlWrapper.Parameter != null)
                    {
                        foreach (SqlParameter para in _SqlWrapper.Parameter)
                        {
                            para.Value = para.Value == null ? DBNull.Value : para.Value;
                            cmd.Parameters.Add(para);
                        }
                    }
                    dadp.SelectCommand = cmd;
                    dadp.Fill(dataset);
                }
            }
            catch (SqlException sqlException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

            return dataset;
        }

        /// <summary>
        /// Query DB by Sql and return the Dataset result
        /// </summary>
        /// <param name="queryString">SQL</param>
        /// <returns></returns>
        public static DataSet ExecuteQuery(SqlWrapper _SqlWrapper, string connectionStr)
        {
            DataSet dataset = new DataSet();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionStr))
                {
                    SqlDataAdapter dadp = new SqlDataAdapter();
                    SqlCommand cmd = new SqlCommand(_SqlWrapper.SqlString, conn);
                    cmd.CommandType = _SqlWrapper.CommandType;
                    if (_SqlWrapper.Parameter != null)
                    {
                        foreach (SqlParameter para in _SqlWrapper.Parameter)
                        {
                            para.Value = para.Value == null ? DBNull.Value : para.Value;
                            cmd.Parameters.Add(para);
                        }
                    }
                    dadp.SelectCommand = cmd;
                    dadp.Fill(dataset);
                }
            }
            catch (SqlException sqlException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

            return dataset;
        }


        public static int ExecuteNonQuery(SqlWrapper SqlWrapper, QCConnectionType connectionType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionManager.GetInstance().Get(connectionType)))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = SqlWrapper.SqlString;
                    cmd.Connection = conn;
                    cmd.CommandType = SqlWrapper.CommandType;
                    conn.Open();
                    if (SqlWrapper.Parameter != null && SqlWrapper.Parameter.Length > 0)
                    {
                        foreach (SqlParameter para in SqlWrapper.Parameter)
                        {
                            para.Value = para.Value == null ? DBNull.Value : para.Value;
                            cmd.Parameters.Add(para);
                        }
                    }
                    int iret = cmd.ExecuteNonQuery();
                    if (SqlWrapper.Parameter != null && SqlWrapper.Parameter.Length > 0)
                    {
                        foreach (SqlParameter para in SqlWrapper.Parameter)
                        {
                            if (para.Direction == ParameterDirection.ReturnValue)
                            {
                                iret = Convert.ToInt16(para.Value.ToString());
                            }
                        }
                    }
                    return iret;
                }
            }
            catch (SqlException sqlException)
            {
                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Execute a SQL statement, such as Insert, Update or Delete
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(List<SqlWrapper> lstSql, QCConnectionType connectionType)
        {
            try
            {
                int iret = 0;

                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionManager.GetInstance().Get(connectionType)))
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        conn.Open();
                        foreach (SqlWrapper sql in lstSql)
                        {
                            try
                            {
                                cmd.CommandText = sql.SqlString;
                                cmd.CommandType = sql.CommandType;
                                cmd.Parameters.Clear();
                                if (sql.Parameter != null)
                                {
                                    foreach (SqlParameter para in sql.Parameter)
                                    {
                                        para.Value = para.Value == null ? DBNull.Value : para.Value;
                                        cmd.Parameters.Add(para);
                                    }
                                }
                                iret += cmd.ExecuteNonQuery();
                            }
                            catch (SqlException sqlException)
                            {
                                return 0;
                            }
                            catch (Exception e)
                            {
                                return 0;
                            }
                        }
                    }
                    scope.Complete();
                    return iret;
                }
            }
            catch (Exception e)
            {
                return -1;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SqlWrapper
    {
        public SqlWrapper()
        {
            // Default command type
            CommandType = CommandType.Text;
        }
        public SqlWrapper(string _Sql, SqlParameter[] _Para)
            : this()
        {
            this.SqlString = _Sql;
            this.Parameter = _Para;
        }

        public string SqlString { get; set; }
        public SqlParameter[] Parameter { get; set; }
        public CommandType CommandType { get; set; }


    }
}
