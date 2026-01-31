import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import App from './App';
import { InventoryPage, ReceiptsPage, RecipesPage } from './pages';

function NavBar() {
  return (
    <nav className="navbar">
      <NavLink to="/" end>
        Home
      </NavLink>
      <NavLink to="/inventory">Inventory</NavLink>
      <NavLink to="/receipts">Receipts</NavLink>
      <NavLink to="/recipes">Recipes</NavLink>
    </nav>
  );
}

export default function AppRouter() {
  return (
    <BrowserRouter>
      <NavBar />
      <Routes>
        <Route path="/" element={<App />} />
        <Route path="/inventory" element={<InventoryPage />} />
        <Route path="/receipts" element={<ReceiptsPage />} />
        <Route path="/recipes" element={<RecipesPage />} />
      </Routes>
    </BrowserRouter>
  );
}
