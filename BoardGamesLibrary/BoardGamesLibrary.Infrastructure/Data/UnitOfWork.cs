using BoardGamesLibrary.Application.Interfaces;

namespace BoardGamesLibrary.Infrastructure.Data;

public sealed class UnitOfWork(BoardGamesDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}