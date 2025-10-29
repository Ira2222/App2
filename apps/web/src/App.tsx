import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import TodosPage from './pages/TodosPage';
import LocationList from './features/locations/LocationList';

export default function App() {
  return (
    <BrowserRouter>
      <div style={{ fontFamily: 'system-ui, sans-serif' }}>
        {/* Navigation */}
        <nav
          style={{
            padding: '1rem 2rem',
            backgroundColor: '#f8f9fa',
            borderBottom: '1px solid #dee2e6'
          }}
        >
          <div
            style={{
              display: 'flex',
              gap: '1.5rem',
              alignItems: 'center',
              maxWidth: 1200,
              margin: '0 auto'
            }}
          >
            <Link
              to="/"
              style={{
                textDecoration: 'none',
                color: '#000',
                fontWeight: 600,
                fontSize: '1.25rem'
              }}
            >
              App2
            </Link>
            <Link
              to="/"
              style={{
                textDecoration: 'none',
                color: '#4a90e2',
                padding: '0.5rem 1rem'
              }}
            >
              Todos
            </Link>
            <Link
              to="/locations"
              style={{
                textDecoration: 'none',
                color: '#4a90e2',
                padding: '0.5rem 1rem'
              }}
            >
              Locations
            </Link>
          </div>
        </nav>

        {/* Routes */}
        <Routes>
          <Route path="/" element={<TodosPage />} />
          <Route path="/locations" element={<LocationList />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}
