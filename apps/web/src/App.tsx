import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
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
            <NavLink
              to="/"
              style={{
                textDecoration: 'none',
                color: '#000',
                fontWeight: 600,
                fontSize: '1.25rem'
              }}
            >
              App2
            </NavLink>
            {/* Todos hidden; uncomment to show */}
            {/*
            <NavLink
              to="/todos"
              style={({ isActive }) => ({
                textDecoration: 'none',
                color: isActive ? '#357abd' : '#4a90e2',
                fontWeight: isActive ? 500 : 400,
                padding: '0.5rem 1rem'
              })}
            >
              Todos
            </NavLink>
            */}
            <NavLink
              to="/"
              style={({ isActive }) => ({
                textDecoration: 'none',
                color: isActive ? '#357abd' : '#4a90e2',
                fontWeight: isActive ? 500 : 400,
                padding: '0.5rem 1rem'
              })}
            >
              Locations
            </NavLink>
          </div>
        </nav>

        {/* Routes */}
        <Routes>
          {/* Locations is the default landing page */}
          <Route path="/" element={<LocationList />} />
          {/* Todos still available at /todos if needed */}
          <Route path="/todos" element={<TodosPage />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}
