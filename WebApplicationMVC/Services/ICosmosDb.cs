using Microsoft.Azure.Cosmos;

namespace WebApplicationMVC.Services
{
    public interface ICosmosDb
    {
        Database Database { get; }
        Container this[int index] { get; }
    }
}
