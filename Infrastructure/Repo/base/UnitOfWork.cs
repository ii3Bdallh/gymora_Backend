
using System.Text.RegularExpressions;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Infrastructure.Configuration;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                HandleDatabaseException(ex);
                throw;
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }


private void HandleDatabaseException(Exception ex)
    {
        if (ex is DbUpdateException dbEx)
        {
            var innerException = dbEx.InnerException;

            if (innerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                var (indexName, duplicateValue) = ExtractDuplicateKeyInfo(sqlEx.Message);

                _logger.LogWarning(sqlEx,
                    "Duplicate key error captured in UnitOfWork. Index: {Index}, Value: {Value}",
                    indexName, duplicateValue);

                var message = !string.IsNullOrEmpty(duplicateValue)
                    ? $"The value '{duplicateValue}' already exists and must be unique."
                    : "This value already exists. Please use a different one.";

                throw new BadRequestException(message);
            }

            if (innerException != null && innerException.Message.ToLower().Contains("unique"))
            {
                _logger.LogWarning(dbEx, "Unique constraint violation captured in UnitOfWork. Raw: {Message}", innerException.Message);
                throw new BadRequestException($"This value already exists and must be unique. Details: {innerException.Message}");
            }

            _logger.LogError(dbEx, "Database update error captured in UnitOfWork.");
            throw new BadRequestException("A database error occurred while saving the record.");
        }

        _logger.LogError(ex, "Unexpected database error captured in UnitOfWork.");
        throw new BadRequestException("An unexpected error occurred. Please try again later.");
    }

    private static (string? IndexName, string? DuplicateValue) ExtractDuplicateKeyInfo(string sqlMessage)
    {
        // مثال على رسالة SQL Server:
        // "Cannot insert duplicate key row in object 'dbo.Users' with unique index 'IX_Users_Email'. The duplicate key value is (test@test.com)."
        // var indexMatch = Regex.Match(sqlMessage, @"unique index '([^']+)'");
        var valueMatch = Regex.Match(sqlMessage, @"duplicate key value is \(([^)]+)\)");

        // var indexName = indexMatch.Success ? indexMatch.Groups[1].Value : null;
        string? indexName =  null;

        var duplicateValue = valueMatch.Success ? valueMatch.Groups[1].Value : null;

        return (indexName, duplicateValue);
    }
    public void Dispose()
    {
        _context.Dispose();
        _currentTransaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
}