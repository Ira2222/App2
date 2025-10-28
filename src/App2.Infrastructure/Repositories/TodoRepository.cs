using App2.Domain.Entities;
using App2.Domain.Repositories;
using App2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace App2.Infrastructure.Repositories;

public sealed class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _dbContext;

    public TodoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Todo>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Todos.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Todo?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Todos.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Todo> AddAsync(Todo todo, CancellationToken cancellationToken)
    {
        _dbContext.Todos.Add(todo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return todo;
    }

    public async Task<Todo> UpdateAsync(Todo todo, CancellationToken cancellationToken)
    {
        _dbContext.Todos.Update(todo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return todo;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var todo = await _dbContext.Todos.FindAsync(new object[] { id }, cancellationToken);
        if (todo is null)
        {
            return false;
        }

        _dbContext.Todos.Remove(todo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
