namespace Ecom.ProductService.Core.Models.Connection.RedisConnection
{
    public class RedisConnection
    {
        public string RedisConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }
}
