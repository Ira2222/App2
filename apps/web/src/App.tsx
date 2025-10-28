import { useEffect, useState } from 'react';
import { createApiClient, type TodoDto, ApiError } from './api';

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? '';
const api = createApiClient({ baseUrl: API_BASE });

export default function App() {
  const [todos, setTodos] = useState<TodoDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      try {
        const data = await api.getTodos();
        setTodos(data);
      } catch (err) {
        if (err instanceof ApiError) {
          setError(`${err.message} (Status: ${err.status})`);
        } else {
          setError(err instanceof Error ? err.message : 'Unknown error');
        }
      }
    }

    load();
  }, []);

  return (
    <main style={{ margin: '2rem auto', maxWidth: 640, fontFamily: 'system-ui, sans-serif' }}>
      <header style={{ marginBottom: '1.5rem' }}>
        <h1>App2 Todos</h1>
        <p>Data served from the ASP.NET Core Minimal API.</p>
      </header>

      {error && <p role="alert">Failed to load todos: {error}</p>}

      <ul style={{ listStyle: 'none', padding: 0, display: 'grid', gap: '0.75rem' }}>
        {todos.map(todo => (
          <li key={todo.id} style={{ padding: '1rem', border: '1px solid #ddd', borderRadius: '0.5rem' }}>
            <strong>{todo.title}</strong>
            {todo.description ? <p>{todo.description}</p> : null}
            <p>Status: {todo.isCompleted ? 'Complete' : 'Pending'}</p>
          </li>
        ))}
      </ul>
    </main>
  );
}
