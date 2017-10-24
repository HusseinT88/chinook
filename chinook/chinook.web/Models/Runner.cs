using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace chinook.web.Models
{
    public class Runner : IRunner
    {
        public string  ConnectionString { get; set; }

        public Runner(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public IEnumerable<dynamic> ExecuteDynamic(string sql, params object[] args)
        {
            var reader = OpenReader(sql, args);
            while (reader.Read())
            {
                yield return reader.ToSingle();
            }
            reader.Dispose();
        }
        public dynamic ExecuteToSingleDynamic(string command, params object[] args)
        {
            return ExecuteDynamic(command, args).FirstOrDefault();
        }
        public T ExecuteSingle<T>(string sql, params object[] args) where T : new()
        {
            return Execute<T>(sql,args).FirstOrDefault();
        }
        public IEnumerable<T> Execute<T>(string sql, params object[] args) where T : new()
        {
            var reader = OpenReader(sql, args);
            while (reader.Read())
            {
                yield return reader.ToSingle<T>();
            }
            reader.Dispose();
        }
        public NpgsqlDataReader OpenReader(string sql, params object[] args)
        {
            var connection = new NpgsqlConnection(ConnectionString);
            var command = BuildCommand(sql, args);
            command.Connection = connection;
            connection.Open();
            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }
        public async Task<NpgsqlDataReader> OpenReaderAsync(string sql, params object[] args)
        {
            var connection = new NpgsqlConnection(ConnectionString);
            var command = BuildCommand(sql, args);
            command.Connection = connection;
            await connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return reader as NpgsqlDataReader;
        }
        public async Task<IList<int>> TransactAsync(params NpgsqlCommand[] commands)
        {
            var result = new List<int>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (var trx = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var command in commands)
                        {
                            command.Connection = connection;
                            command.Transaction = trx;
                            result.Add(await command.ExecuteNonQueryAsync());
                        }
                        await trx.CommitAsync();
                    }
                    catch (NpgsqlException e)
                    {
                        await trx.RollbackAsync();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return result;
        }
        public IList<int> Transact(params NpgsqlCommand[] commands)
        {
            var result = new List<int>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var trx = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var command in commands)
                        {
                            command.Connection = connection;
                            command.Transaction = trx;
                            result.Add(command.ExecuteNonQuery());
                        }
                        trx.Commit();
                    }
                    catch (NpgsqlException e)
                    {
                        trx.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                return result;
            }
        }
        public NpgsqlCommand BuildCommand(string sql, object[] args)
        {
            var cmd = new NpgsqlCommand{ CommandText = sql };
            if (args == null) return cmd;
            foreach (var arg in args)
            {
                cmd.AddParameter(arg);
            }
            return cmd;
        }        
    }
}