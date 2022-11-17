using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLibrary
{
    public class Connection
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _factory;

        public Connection(DbProviderFactory factory, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException($"{connectionString} is not a valid ConnectionString");

            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            _connectionString = connectionString;
            _factory = factory;
        }

        public int ExecuteNonQuery(Command cmd)
        {
            using (DbConnection dbConnection = CreateConnection())
            {
                using (DbCommand sqlCommand = CreateCommand(cmd, dbConnection))
                {
                    dbConnection.Open();
                    return sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(Command cmd)
        {
            using (DbConnection dbConnection = CreateConnection())
            {
                using (DbCommand sqlCommand = CreateCommand(cmd, dbConnection))
                {
                    dbConnection.Open();
                    object o = sqlCommand.ExecuteScalar();
                    return (o is DBNull) ? null : o;
                }
            }
        }

        public IEnumerable<TResult> ExecuteReader<TResult>(Command cmd, Func<IDataReader, TResult> converter)
        {
            using (DbConnection connection = CreateConnection())
            {
                using (DbCommand sqlCommand = CreateCommand(cmd, connection))
                {
                    connection.Open();
                    using (IDataReader dataReader = sqlCommand.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            yield return converter(dataReader);
                        }
                    }
                }
            }
        }

        public DataTable GetDataTable(Command cmd)
        {
            using (DbConnection connection = CreateConnection())
            {
                using (DbCommand sqlCommand = CreateCommand(cmd, connection))
                {
                    using (DbDataAdapter dataAdapter = _factory.CreateDataAdapter())
                    {
                        dataAdapter.SelectCommand = sqlCommand;
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        return dataTable;
                    }

                }
            }
        }

        public DataSet GetDataSet(Command cmd)
        {
            using (DbConnection connection = CreateConnection())
            {
                using (DbCommand sqlCommand = CreateCommand(cmd, connection))
                {
                    using (DbDataAdapter dataAdapter = _factory.CreateDataAdapter())
                    {
                        dataAdapter.SelectCommand = sqlCommand;
                        DataSet dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);

                        return dataSet;
                    }

                }
            }
        }

        private DbConnection CreateConnection()
        {
            DbConnection dbConnection = _factory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;

            return dbConnection;
        }

        private static DbCommand CreateCommand(Command command, DbConnection dbConnection)
        {
            DbCommand sqlCommand = dbConnection.CreateCommand();
            sqlCommand.CommandText = command.Query;

            if (command.IsStoredProcedure)
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

            foreach (KeyValuePair<string, object> kvp in command.Parameters)
            {
                DbParameter sqlParameter = sqlCommand.CreateParameter();
                sqlParameter.ParameterName = kvp.Key;
                sqlParameter.Value = kvp.Value;

                sqlCommand.Parameters.Add(sqlParameter);
            }

            return sqlCommand;
        }
    }
}
