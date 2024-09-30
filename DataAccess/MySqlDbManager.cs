using Dapper;
using System.Data;

namespace BasculasPG.DataAccess
{
    public class MySqlDbManager 
    {
        private readonly MySqlConnectionFactory _connectionFactory;

        public MySqlDbManager(MySqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Ejecuta una consulta SELECT y retorna una lista de objetos
        public async Task<IEnumerable<T>> DapperExecuteQuery<T>(string query, object parameters = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<T>(query, parameters);
            }
        }

        // Ejecuta un comando INSERT, UPDATE o DELETE
        public async Task<int> DapperExecuteCommand(string query, object parameters = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.ExecuteAsync(query, parameters);
            }
        }

        // Ejecuta múltiples comandos dentro de una transacción
        public async Task ExecuteTransaction(Func<IDbConnection, IDbTransaction, Task> action)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await action(connection, transaction);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
