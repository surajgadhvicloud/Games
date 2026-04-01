import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './presentation/pages/LoginPage';
import ProtectedRoute from './presentation/routes/ProtectedRoute';
import Layout from './presentation/components/Layout';
import BoardGamesPage from './presentation/pages/BoardGamesPage';
import GameIssuesPage from './presentation/pages/GameIssuesPage';
import MembersPage from './presentation/pages/MembersPage';
import UsersPage from './presentation/pages/UsersPage';
import InventoryPage from './presentation/pages/InventoryPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<Layout />}>
            <Route path="/board-games" element={<BoardGamesPage />} />
            <Route path="/game-issues" element={<GameIssuesPage />} />
            <Route path="/members" element={<MembersPage />} />
            <Route path="/users" element={<UsersPage />} />
            <Route path="/inventory" element={<InventoryPage />} />
            <Route path="/dashboard" element={<Navigate to="/board-games" replace />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;



