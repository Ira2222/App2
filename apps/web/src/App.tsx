import { useEffect, useState } from 'react';
import { createApiClient, type TodoDto, ApiError } from './api';

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? '';
const api = createApiClient({ baseUrl: API_BASE });

export default function App() {
  const [todos, setTodos] = useState<TodoDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<number | null>(null);

  // Create form state
  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [creating, setCreating] = useState(false);

  // Edit form state
  const [editTitle, setEditTitle] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [editCompleted, setEditCompleted] = useState(false);

  useEffect(() => {
    loadTodos();
  }, []);

  async function loadTodos() {
    try {
      setLoading(true);
      setError(null);
      const data = await api.getTodos();
      setTodos(data);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(`${err.message} (Status: ${err.status})`);
      } else {
        setError(err instanceof Error ? err.message : 'Unknown error');
      }
    } finally {
      setLoading(false);
    }
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!newTitle.trim()) return;

    try {
      setCreating(true);
      setError(null);
      const created = await api.createTodo({
        title: newTitle,
        description: newDescription || null
      });
      setTodos([...todos, created]);
      setNewTitle('');
      setNewDescription('');
    } catch (err) {
      if (err instanceof ApiError) {
        setError(`Failed to create todo: ${err.message}`);
      } else {
        setError(err instanceof Error ? err.message : 'Unknown error');
      }
    } finally {
      setCreating(false);
    }
  }

  function startEdit(todo: TodoDto) {
    setEditingId(todo.id);
    setEditTitle(todo.title);
    setEditDescription(todo.description || '');
    setEditCompleted(todo.isCompleted);
  }

  function cancelEdit() {
    setEditingId(null);
    setEditTitle('');
    setEditDescription('');
    setEditCompleted(false);
  }

  async function handleUpdate(id: number) {
    if (!editTitle.trim()) return;

    try {
      setError(null);
      const updated = await api.updateTodo(id, {
        title: editTitle,
        description: editDescription || null,
        isCompleted: editCompleted
      });

      if (updated) {
        setTodos(todos.map(t => t.id === id ? updated : t));
        cancelEdit();
      } else {
        setError('Todo not found');
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setError(`Failed to update todo: ${err.message}`);
      } else {
        setError(err instanceof Error ? err.message : 'Unknown error');
      }
    }
  }

  async function handleToggleComplete(todo: TodoDto) {
    try {
      setError(null);
      const updated = await api.updateTodo(todo.id, {
        title: todo.title,
        description: todo.description || null,
        isCompleted: !todo.isCompleted
      });

      if (updated) {
        setTodos(todos.map(t => t.id === todo.id ? updated : t));
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setError(`Failed to toggle todo: ${err.message}`);
      } else {
        setError(err instanceof Error ? err.message : 'Unknown error');
      }
    }
  }

  async function handleDelete(id: number) {
    if (!confirm('Are you sure you want to delete this todo?')) return;

    try {
      setError(null);
      const deleted = await api.deleteTodo(id);
      if (deleted) {
        setTodos(todos.filter(t => t.id !== id));
      } else {
        setError('Todo not found');
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setError(`Failed to delete todo: ${err.message}`);
      } else {
        setError(err instanceof Error ? err.message : 'Unknown error');
      }
    }
  }

  return (
    <main style={{ margin: '2rem auto', maxWidth: 640, fontFamily: 'system-ui, sans-serif' }}>
      <header style={{ marginBottom: '1.5rem' }}>
        <h1>App2 Todos</h1>
        <p>Full CRUD operations with ASP.NET Core Minimal API backend.</p>
      </header>

      {error && (
        <div
          role="alert"
          style={{
            padding: '1rem',
            marginBottom: '1rem',
            backgroundColor: '#fee',
            border: '1px solid #fcc',
            borderRadius: '0.5rem',
            color: '#c33'
          }}
        >
          {error}
        </div>
      )}

      {/* Create Form */}
      <form
        onSubmit={handleCreate}
        style={{
          padding: '1.5rem',
          marginBottom: '2rem',
          border: '2px solid #4a90e2',
          borderRadius: '0.5rem',
          backgroundColor: '#f8fbff'
        }}
      >
        <h2 style={{ marginTop: 0, fontSize: '1.25rem' }}>Create New Todo</h2>
        <div style={{ marginBottom: '1rem' }}>
          <label htmlFor="title" style={{ display: 'block', marginBottom: '0.25rem', fontWeight: 500 }}>
            Title *
          </label>
          <input
            id="title"
            type="text"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            placeholder="Enter todo title"
            required
            disabled={creating}
            style={{
              width: '100%',
              padding: '0.5rem',
              fontSize: '1rem',
              border: '1px solid #ccc',
              borderRadius: '0.25rem',
              boxSizing: 'border-box'
            }}
          />
        </div>
        <div style={{ marginBottom: '1rem' }}>
          <label htmlFor="description" style={{ display: 'block', marginBottom: '0.25rem', fontWeight: 500 }}>
            Description
          </label>
          <textarea
            id="description"
            value={newDescription}
            onChange={(e) => setNewDescription(e.target.value)}
            placeholder="Optional description"
            disabled={creating}
            rows={3}
            style={{
              width: '100%',
              padding: '0.5rem',
              fontSize: '1rem',
              border: '1px solid #ccc',
              borderRadius: '0.25rem',
              fontFamily: 'inherit',
              boxSizing: 'border-box'
            }}
          />
        </div>
        <button
          type="submit"
          disabled={creating || !newTitle.trim()}
          style={{
            padding: '0.5rem 1.5rem',
            fontSize: '1rem',
            fontWeight: 500,
            color: 'white',
            backgroundColor: creating ? '#ccc' : '#4a90e2',
            border: 'none',
            borderRadius: '0.25rem',
            cursor: creating ? 'not-allowed' : 'pointer'
          }}
        >
          {creating ? 'Creating...' : 'Create Todo'}
        </button>
      </form>

      {/* Todos List */}
      {loading ? (
        <p>Loading todos...</p>
      ) : todos.length === 0 ? (
        <p style={{ color: '#666', fontStyle: 'italic' }}>No todos yet. Create one above!</p>
      ) : (
        <ul style={{ listStyle: 'none', padding: 0, display: 'grid', gap: '0.75rem' }}>
          {todos.map(todo => (
            <li
              key={todo.id}
              style={{
                padding: '1rem',
                border: '1px solid #ddd',
                borderRadius: '0.5rem',
                backgroundColor: todo.isCompleted ? '#f0f8f0' : 'white'
              }}
            >
              {editingId === todo.id ? (
                // Edit mode
                <div>
                  <div style={{ marginBottom: '0.75rem' }}>
                    <label style={{ display: 'block', marginBottom: '0.25rem', fontWeight: 500, fontSize: '0.875rem' }}>
                      Title *
                    </label>
                    <input
                      type="text"
                      value={editTitle}
                      onChange={(e) => setEditTitle(e.target.value)}
                      style={{
                        width: '100%',
                        padding: '0.5rem',
                        fontSize: '1rem',
                        border: '1px solid #ccc',
                        borderRadius: '0.25rem',
                        boxSizing: 'border-box'
                      }}
                    />
                  </div>
                  <div style={{ marginBottom: '0.75rem' }}>
                    <label style={{ display: 'block', marginBottom: '0.25rem', fontWeight: 500, fontSize: '0.875rem' }}>
                      Description
                    </label>
                    <textarea
                      value={editDescription}
                      onChange={(e) => setEditDescription(e.target.value)}
                      rows={2}
                      style={{
                        width: '100%',
                        padding: '0.5rem',
                        fontSize: '1rem',
                        border: '1px solid #ccc',
                        borderRadius: '0.25rem',
                        fontFamily: 'inherit',
                        boxSizing: 'border-box'
                      }}
                    />
                  </div>
                  <div style={{ marginBottom: '1rem' }}>
                    <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
                      <input
                        type="checkbox"
                        checked={editCompleted}
                        onChange={(e) => setEditCompleted(e.target.checked)}
                        style={{ width: '1.25rem', height: '1.25rem', cursor: 'pointer' }}
                      />
                      <span style={{ fontWeight: 500, fontSize: '0.875rem' }}>Completed</span>
                    </label>
                  </div>
                  <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button
                      onClick={() => handleUpdate(todo.id)}
                      disabled={!editTitle.trim()}
                      style={{
                        padding: '0.375rem 1rem',
                        fontSize: '0.875rem',
                        fontWeight: 500,
                        color: 'white',
                        backgroundColor: '#28a745',
                        border: 'none',
                        borderRadius: '0.25rem',
                        cursor: editTitle.trim() ? 'pointer' : 'not-allowed'
                      }}
                    >
                      Save
                    </button>
                    <button
                      onClick={cancelEdit}
                      style={{
                        padding: '0.375rem 1rem',
                        fontSize: '0.875rem',
                        fontWeight: 500,
                        color: '#333',
                        backgroundColor: '#e9ecef',
                        border: '1px solid #ccc',
                        borderRadius: '0.25rem',
                        cursor: 'pointer'
                      }}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                // View mode
                <div>
                  <div style={{ display: 'flex', alignItems: 'flex-start', gap: '0.75rem', marginBottom: '0.75rem' }}>
                    <input
                      type="checkbox"
                      checked={todo.isCompleted}
                      onChange={() => handleToggleComplete(todo)}
                      style={{ marginTop: '0.25rem', width: '1.25rem', height: '1.25rem', cursor: 'pointer' }}
                      title={todo.isCompleted ? 'Mark as incomplete' : 'Mark as complete'}
                    />
                    <div style={{ flex: 1 }}>
                      <strong
                        style={{
                          textDecoration: todo.isCompleted ? 'line-through' : 'none',
                          color: todo.isCompleted ? '#666' : '#000'
                        }}
                      >
                        {todo.title}
                      </strong>
                      {todo.description && (
                        <p style={{ margin: '0.5rem 0 0', color: '#555', fontSize: '0.875rem' }}>
                          {todo.description}
                        </p>
                      )}
                    </div>
                  </div>
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      paddingTop: '0.75rem',
                      borderTop: '1px solid #eee'
                    }}
                  >
                    <span style={{ fontSize: '0.75rem', color: '#666' }}>
                      Created: {new Date(todo.createdAt).toLocaleDateString()}
                      {todo.completedAt && ` • Completed: ${new Date(todo.completedAt).toLocaleDateString()}`}
                    </span>
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      <button
                        onClick={() => startEdit(todo)}
                        style={{
                          padding: '0.25rem 0.75rem',
                          fontSize: '0.875rem',
                          color: '#4a90e2',
                          backgroundColor: 'transparent',
                          border: '1px solid #4a90e2',
                          borderRadius: '0.25rem',
                          cursor: 'pointer'
                        }}
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleDelete(todo.id)}
                        style={{
                          padding: '0.25rem 0.75rem',
                          fontSize: '0.875rem',
                          color: '#dc3545',
                          backgroundColor: 'transparent',
                          border: '1px solid #dc3545',
                          borderRadius: '0.25rem',
                          cursor: 'pointer'
                        }}
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
