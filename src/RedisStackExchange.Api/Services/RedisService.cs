using StackExchange.Redis;

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
                if(_database == null)
                    _database = _multiplexer.GetDatabase(_dbIndex);
                return _database;
            }

        }

    }
}
