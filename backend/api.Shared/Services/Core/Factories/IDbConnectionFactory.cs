using System.Data;

namespace api.Shared.Services.Core.Factories
{
    public interface IDbConnectionFactory
    {
        IDbConnection Get();
    }
}