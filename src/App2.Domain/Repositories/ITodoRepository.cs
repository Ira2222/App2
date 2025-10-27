using App2.Domain.Entities;

namespace App2.Domain.Repositories;

public interface ITodoRepository
{
    Task<IReadOnlyList<Todo>> GetAllAsync(CancellationToken cancellationToken);
    Task<Todo> AddAsync(Todo todo, CancellationToken cancellationToken);
}
