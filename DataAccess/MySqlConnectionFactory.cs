using MySql.Data.MySqlClient;
using System.Data;

namespace BasculasPG.DataAccess
{
    public class MySqlConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(IConfiguration configuration)
        {
            // Obtiene la cadena de conexión desde el archivo appsettings.json
            _connectionString = configuration.GetConnectionString("mysql");
        }

        public IDbConnection CreateConnection()
        {
            // Retorna una conexión MySQL usando la cadena de conexión
            return new MySqlConnection(_connectionString);
        }
    }
}
