using Application.Interface;
using Application.Interface.Repo.Shared;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Service;

public class TransactionManager : ITransactionManager
{
    private readonly ApplicationDbContext _context;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

    public TransactionManager(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            _transaction = await _context.Database.BeginTransactionAsync(ct);
        }
        catch (Exception) when (!_context.Database.IsRelational())
        {
            _transaction = null;
        }
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default)
    {
        try
        {
            _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, ct);
        }
        catch (Exception) when (!_context.Database.IsRelational())
        {
            _transaction = null;
        }
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            await _transaction.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            await _transaction.RollbackAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
