import { useEffect, useState } from 'react';

type Todo = {
  id: number;
  title: string;
  description?: string | null;
  isCompleted: boolean;
};

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? '';

export default function App() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      try {
        const response = await fetch(`${API_BASE}/api/todos`, {
          headers: { Accept: 'application/json' }
        });

        if (!response.ok) {
          throw new Error(`Request failed with status ${response.status}`);
        }

        const data: Todo[] = await response.json();
        setTodos(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unknown error');
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
