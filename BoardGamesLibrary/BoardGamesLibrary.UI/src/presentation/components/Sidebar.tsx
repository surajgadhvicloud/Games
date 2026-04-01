import { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const NAV_ITEMS = [
  { label: 'Board Games', to: '/board-games' },
  { label: 'Game Issues', to: '/game-issues' },
  { label: 'Members', to: '/members' },
  { label: 'Users', to: '/users' },
  { label: 'Inventory', to: '/inventory' },
];

export default function Sidebar() {
  const [open, setOpen] = useState(true);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate('/login', { replace: true });
  }

  return (
    <aside
      className={`flex flex-col bg-gray-900 text-white transition-all duration-200 ${
        open ? 'w-56' : 'w-14'
      } min-h-screen`}
    >
      {/* Hamburger */}
      <div className="flex items-center h-14 px-3 border-b border-gray-700">
        <button
          onClick={() => setOpen((v) => !v)}
          className="p-1 rounded hover:bg-gray-700 focus:outline-none"
          aria-label="Toggle menu"
        >
          <span className="block w-5 h-0.5 bg-white mb-1" />
          <span className="block w-5 h-0.5 bg-white mb-1" />
          <span className="block w-5 h-0.5 bg-white" />
        </button>
        {open && (
          <span className="ml-3 font-semibold text-sm truncate">Board Games Library</span>
        )}
      </div>

      {/* Nav links */}
      <nav className="flex-1 py-4">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 text-sm transition-colors ${
                isActive
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-300 hover:bg-gray-700 hover:text-white'
              }`
            }
          >
            <span className="w-5 h-5 flex-shrink-0 text-center text-base">
              {navIcon(item.label)}
            </span>
            {open && <span className="truncate">{item.label}</span>}
          </NavLink>
        ))}
      </nav>

      {/* User + logout */}
      <div className="px-3 py-4 border-t border-gray-700">
        {open && (
          <p className="text-xs text-gray-400 truncate mb-2">
            {user?.username} · {user?.role}
          </p>
        )}
        <button
          onClick={handleLogout}
          className="flex items-center gap-2 text-gray-300 hover:text-white text-sm"
        >
          <span>⏻</span>
          {open && <span>Sign out</span>}
        </button>
      </div>
    </aside>
  );
}

function navIcon(label: string): string {
  switch (label) {
    case 'Board Games': return '🎲';
    case 'Game Issues': return '📋';
    case 'Members': return '👥';
    case 'Users': return '👤';
    case 'Inventory': return '📦';
    default: return '•';
  }
}
