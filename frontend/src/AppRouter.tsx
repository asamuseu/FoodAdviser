import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import App from './App';
import { ProtectedRoute } from './components';
import { useAuth } from './contexts/AuthContext';
import { InventoryPage, LoginPage, ReceiptsPage, RecipesPage, RegisterPage } from './pages';

function NavBar() {
  const { isAuthenticated, user, logout } = useAuth();

  return (
    <nav className="navbar">
      <div className="navbar-links">
        <NavLink to="/" end>
          Home
        </NavLink>
        <NavLink to="/inventory">Inventory</NavLink>
        <NavLink to="/receipts">Receipts</NavLink>
        <NavLink to="/recipes">Recipes</NavLink>
      </div>
      <div className="navbar-auth">
        {isAuthenticated ? (
          <>
            <span className="navbar-user">{user?.email}</span>
            <button className="navbar-btn" onClick={logout}>
              Sign Out
            </button>
          </>
        ) : (
          <>
            <NavLink to="/login">Sign In</NavLink>
            <NavLink to="/register" className="navbar-btn-primary">Register</NavLink>
          </>
        )}
      </div>
    </nav>
  );
}

export default function AppRouter() {
  return (
    <BrowserRouter>
      <NavBar />
      <Routes>
        <Route path="/" element={<App />} />
        <Route
          path="/inventory"
          element={
            <ProtectedRoute>
              <InventoryPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/receipts"
          element={
            <ProtectedRoute>
              <ReceiptsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/recipes"
          element={
            <ProtectedRoute>
              <RecipesPage />
            </ProtectedRoute>
          }
        />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Routes>
    </BrowserRouter>
  );
}
