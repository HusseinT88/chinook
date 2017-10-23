using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace chinook.web.Models
{
    public interface IRunner
    {
        string ConnectionString { get; set; }
        IEnumerable<object> ExecuteDynamic(string sql, params object[] args);
        dynamic ExecuteToSingleDynamic(string command, params object[] args);
        T ExecuteSingle<T>(string sql, params object[] args) where T : new();
        IEnumerable<T> Execute<T>(string sql, params object[] args) where T : new();
        NpgsqlDataReader OpenReader(string sql, params object[] args);
        Task<NpgsqlDataReader> OpenReaderAsync(string sql, params object[] args);
        Task<IList<int>> TransactAsync(params NpgsqlCommand[] commands);
        IList<int> Transact(params NpgsqlCommand[] commands);
        NpgsqlCommand BuildCommand(string sql, object[] args);
    }
}