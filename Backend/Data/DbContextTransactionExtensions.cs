using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DbContextTransactionExtensions
{
    public static Task ExecuteInTransactionAsync(
        this ApplicationDbContext context,
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        return context.ExecuteInTransactionAsync(IsolationLevel.Unspecified, operation, cancellationToken);
    }

    public static async Task ExecuteInTransactionAsync(
        this ApplicationDbContext context,
        IsolationLevel isolationLevel,
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = isolationLevel == IsolationLevel.Unspecified
                ? await context.Database.BeginTransactionAsync(cancellationToken)
                : await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

            try
            {
                await operation();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public static Task<TResult> ExecuteInTransactionAsync<TResult>(
        this ApplicationDbContext context,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return context.ExecuteInTransactionAsync(IsolationLevel.Unspecified, operation, cancellationToken);
    }

    public static async Task<TResult> ExecuteInTransactionAsync<TResult>(
        this ApplicationDbContext context,
        IsolationLevel isolationLevel,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = isolationLevel == IsolationLevel.Unspecified
                ? await context.Database.BeginTransactionAsync(cancellationToken)
                : await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

            try
            {
                var result = await operation();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
