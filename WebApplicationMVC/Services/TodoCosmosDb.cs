using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Azure.Cosmos;

namespace WebApplicationMVC.Services
{
    public class TodoCosmosDb : ICosmosDb
    {
        private Database _database;
        private readonly IConfiguration _configuration;
        private Dictionary<int, Container> containers;   // кешированные подключения
        private String[] containersId;
        public TodoCosmosDb(IConfiguration configuration)
        {
            _configuration = configuration;
            _database = null!;
            containers = new();
            // Определяем сколько контейнеров в БД
            containersId = _configuration
                        .GetSection("CosmosDB")
                        .GetValue<String>("ContainerId")
                        .Split(';', StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0; i < containersId.Length; i++)
            {
                containers.Add(i, null!);  // на старте все подключения - null 
            };

        }

        public Database Database {
            get
            {
                if (_database == null)
                {
                    var CosmosConfig = _configuration.GetSection("CosmosDB");
                    String endpoint = CosmosConfig.GetValue<String>("Endpoint");
                    String apiKey = CosmosConfig.GetValue<String>("Key");
                    String databaseId = CosmosConfig.GetValue<String>("DatabaseId");
                    // подключение к учетной записи
                    CosmosClient cosmosClient =
                        new(endpoint, apiKey,
                            new CosmosClientOptions()
                            {
                                ApplicationName = "WebApplicationMVC"
                            });
                    // Выбор базы данных
                    _database =
                        cosmosClient
                        .CreateDatabaseIfNotExistsAsync(databaseId)
                        .Result;
                }
                return _database;
            }
        }

#pragma warning disable CS8603 // Possible null reference return.
        public Container this[int index] => index switch {
            0 => containers[index] 
                 ?? (containers[index] = 
                        Database
                        .CreateContainerIfNotExistsAsync(containersId[0], "/partitionKey")
                        .Result),
            _ => null
        };
#pragma warning restore CS8603 // Possible null reference return.

        // public Container this[String name] => null;
        // Д.З. Перенести выполнение запроса к контейнеру из контроллера в службу БД
    }
}
