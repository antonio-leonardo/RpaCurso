using Rpa.Curso.CrossCutting;
using Rpa.Curso.Domain.Interfaces.Repositories;
using Rpa.Curso.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace Rpa.Curso.Infrastructure.Repositories
{
    public class UnitOfWork : LoggingBase, IUnitOfWork
    {
        private readonly DataContext _context;
        private IDbContextTransaction _currentTransaction;
        public UnitOfWork(DataContext context)
        {
            Log.Info("Constructor UnitOfWork");
            _context = context;
            _currentTransaction = null;
        }
        public async Task<IDbContextTransaction> GetContextTransactionAsync()
        {
            Log.Info("Obtém transação corrente");
            if (_currentTransaction == null)
                _currentTransaction = await _context.Database.BeginTransactionAsync();

            return _currentTransaction;
        }

        public async Task RollBackAsync()
        {
            Log.Info("Faz roll-back");
            await _context.DisposeAsync();
        }

        public async Task<int> SaveAllAsync()
        {
            Log.Info("Faz commit");
            return await _context.SaveChangesAsync();
        }
    }
}