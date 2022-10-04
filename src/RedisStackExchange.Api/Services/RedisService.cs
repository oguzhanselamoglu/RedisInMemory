using StackExchange.Redis;
using System.Text.Json;


namespace RedisStackExchange.Api.Services
{
    public class RedisService
    {
        private readonly string _host;
        private readonly string _port;
        private readonly int _dbIndex;

        private string _connectionString => _host + ":" + _port;
        private readonly IConnectionMultiplexer _multiplexer;
        public RedisService(IConfiguration configuration)
        {
            _host = configuration["Redis:Host"];
            _port = configuration["Redis:Port"];
            _dbIndex = Convert.ToInt32(configuration["Redis:DbIndex"]);
            _multiplexer = Connect();
        }

        public IConnectionMultiplexer Connect()
        {
            return ConnectionMultiplexer.Connect(_connectionString);
        }
        private IDatabase _database;
        public IDatabase Database
        {
            get
            {
                if (_database == null)
                    _database = _multiplexer.GetDatabase(_dbIndex);
                return _database;
            }

        }

        public T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value);
        }
        public bool Contains(string key)
        {
            return Database.KeyExists(key);
        }
        public T? Get<T>(string key) where T : class
        {
            string? redisValue = Contains(key) ? Database.StringGet(key) : string.Empty;

            if (!string.IsNullOrEmpty(redisValue))
                return Deserialize<T>(redisValue);

            return null;
        }

        public void Add<T>(string key, T value)
        {

            Database.StringSet(key, Serialize(value));
        }

    }
}
