using Microsoft.EntityFrameworkCore.Storage;

namespace Rpa.Curso.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveAllAsync();
        Task RollBackAsync();
        Task<IDbContextTransaction> GetContextTransactionAsync();
    }
}