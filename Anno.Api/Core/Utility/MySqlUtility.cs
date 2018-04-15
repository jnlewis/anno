using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace AnnoAPI.Core.Utility
{
	public class MySqlUtility
    {
        private string connectionString = null;

        public MySqlUtility(string connectionString)
        {
            this.connectionString = connectionString;
        }
        
        /// <summary>
        /// Execute the SQL statement
        /// </summary>
        /// <param name="strQuery"></param>
        /// <param name="strConnectionString"></param>
        /// <returns></returns>
        public DataTable ExecuteAsDataTable(string sql)
        {
            DataTable data = null;
            MySqlConnection conn = null;
            MySqlDataAdapter adapter = null;

            try
            {
                data = new DataTable();
                conn = new MySqlConnection(this.connectionString);
                adapter = new MySqlDataAdapter(sql, conn);

                conn.Open();
                adapter.Fill(data);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
                
                if (adapter != null)
                    adapter.Dispose();
            }

            return data;
        }

        /// <summary>
        /// Executes an SQL statement.
        /// </summary>
        /// <param name="sql">SQL statement.</param>
        public void Execute(string sql)
        {
            long id = -1;
            Execute(sql, out id);
        }
        
        /// <summary>
        /// Executes an SQL statement and output the newly inserted incremental ID.
        /// </summary>
        /// <param name="sql">SQL statement.</param>
        /// <param name="id">The newly created incremental ID.</param>
        public void Execute(string sql, out long id)
        {
            MySqlConnection conn = null;
            MySqlCommand command = null;

            try
            {
                conn = new MySqlConnection(this.connectionString);
                conn.Open();

                command = conn.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();

                id = command.LastInsertedId;
            }
            catch
            {
                throw;
            }
            finally
            {
                if(conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }

                if (command != null)
                    command.Dispose();
            }
        }

        /// <summary>
        /// Execute a list of SQL statements as transaction
        /// </summary>
        /// <param name="queries">The list of SQL Statements to execute.</param>
        /// <param name="connectionString">The database connection string.</param>
        public void ExecuteTransaction(string[] sqlArray)
        {
            MySqlConnection conn = null;
            MySqlTransaction tran = null;
            MySqlCommand command = null;

            try
            {
                conn = new MySqlConnection(connectionString);
                conn.Open();
                tran = conn.BeginTransaction();

                command = new MySqlCommand("", conn, tran);

                foreach (string sql in sqlArray)
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }

                if (command != null)
                    command.Dispose();
            }
        }
        
	}
}

