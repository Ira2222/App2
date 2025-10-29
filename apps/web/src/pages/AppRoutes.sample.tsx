/**
 * Sample React Router v6 setup
 * This is for reference. The main App.tsx already includes this pattern.
 *
 * Minimal wiring example:
 */

import { BrowserRouter, Routes, Route, NavLink } from "react-router-dom";
import LocationList from "../features/locations/LocationList";
// import TodosPage from "./TodosPage";

export function SampleApp() {
  return (
    <BrowserRouter>
      <div style={{ fontFamily: "system-ui, sans-serif" }}>
        {/* Navigation with active state styling */}
        <nav style={{ padding: "1rem", backgroundColor: "#f8f9fa" }}>
          <NavLink
            to="/"
            style={({ isActive }) => ({
              marginRight: "1rem",
              color: isActive ? "#357abd" : "#4a90e2",
              fontWeight: isActive ? 500 : 400,
              textDecoration: "none",
            })}
          >
            Locations
          </NavLink>
          {/* Uncomment to add Todos route */}
          {/*
          <NavLink
            to="/todos"
            style={({ isActive }) => ({
              color: isActive ? "#357abd" : "#4a90e2",
              fontWeight: isActive ? 500 : 400,
              textDecoration: "none",
            })}
          >
            Todos
          </NavLink>
          */}
        </nav>

        {/* Routes */}
        <Routes>
          {/* Locations is the default landing page */}
          <Route path="/" element={<LocationList />} />
          {/* Todos still available at /todos if needed */}
          {/* <Route path="/todos" element={<TodosPage />} /> */}
        </Routes>
      </div>
    </BrowserRouter>
  );
}

/**
 * Key points:
 *
 * 1. BrowserRouter wraps all routes
 * 2. NavLink automatically gets isActive when the route matches
 * 3. Pass a function to `style` prop to conditionally style active links
 * 4. Routes and Route are nested inside BrowserRouter
 * 5. path="/" is the default fallback
 */
