using MySql.Data.MySqlClient;
using System.Data;
using DotNetEnv;

namespace BasculasPG.DataAccess
{
    public class MySqlConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(IConfiguration configuration)
        {
            // Cargar las variables del archivo .env
            Env.Load();

            // Obtener la IP del servidor desde el archivo .env
            var serverIp = Environment.GetEnvironmentVariable("MYSQL_SERVER") ?? "0.0.0.0";

            // Obtener la cadena de conexión desde appsettings.json
            var baseConnectionString = configuration.GetConnectionString("mysql");

            // Reemplazar la parte del servidor en la cadena de conexión
            _connectionString = baseConnectionString.Replace("{Server}", serverIp);
        }

        public IDbConnection CreateConnection()
        {
            // Retorna una conexión MySQL usando la cadena de conexión construida
            return new MySqlConnection(_connectionString);
        }
    }
}