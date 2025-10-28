using App2.Domain.Entities;

namespace App2.Domain.Repositories;

public interface ITodoRepository
{
    Task<IReadOnlyList<Todo>> GetAllAsync(CancellationToken cancellationToken);
    Task<Todo?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Todo> AddAsync(Todo todo, CancellationToken cancellationToken);
    Task<Todo> UpdateAsync(Todo todo, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
