import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className="min-h-screen bg-gray-100 p-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-semibold text-gray-800">Dashboard</h1>
          <button
            onClick={handleLogout}
            className="text-sm text-gray-600 hover:text-gray-900 underline"
          >
            Sign out
          </button>
        </div>
        <p className="text-gray-600">
          Welcome, <span className="font-medium">{user?.username}</span>! You are
          signed in as <span className="font-medium">{user?.role}</span>.
        </p>
        <p className="text-gray-400 mt-4 text-sm">More pages coming soon.</p>
      </div>
    </div>
  );
}
