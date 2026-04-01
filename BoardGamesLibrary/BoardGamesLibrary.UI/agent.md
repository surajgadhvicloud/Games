# Agent Instructions — BoardGamesLibrary UI

This file is the single source of truth for every coding decision in this project.
All contributors and AI agents **must** follow these rules without exception.

---

## Tech Stack

| Tool | Version | Purpose |
|------|---------|---------|
| React | 19 | UI framework |
| TypeScript | ~5.9 | Language (strict mode) |
| Vite | 8 | Build tool & dev server |
| Tailwind CSS | 3 | Utility-first styling |
| React Router | 7 | Client-side routing |
| Axios | 1 | HTTP client |
| Zustand | 5 | Global state management |

---

## Folder Structure (Clean Architecture)

```
src/
  domain/               ← Pure TypeScript. ZERO framework or library imports.
    entities/           ← Interfaces/types for domain objects
    interfaces/         ← Repository ports (I-prefixed interfaces)

  infrastructure/       ← Implements domain interfaces. May import Axios.
    http/               ← Axios client setup
    repositories/       ← Concrete repository implementations

  application/          ← Use cases and global state. May import Zustand + domain + infrastructure.
    store/              ← Zustand stores (one store per feature/domain)

  presentation/         ← Everything React. May import from all other layers.
    components/         ← Shared/reusable UI components
    hooks/              ← Selector hooks that wrap Zustand stores
    pages/              ← Full-page route components
    routes/             ← Route guards and layout wrappers

  App.tsx               ← Router setup only. No business logic.
  main.tsx              ← Entry point. Mounts <App />.
  index.css             ← Tailwind directives only (@tailwind base/components/utilities).
```

---

## Dependency Rule (STRICT)

Each layer may **only** import from layers listed below it. Never upward.

```
presentation  →  application  →  infrastructure  →  domain
```

| Layer | May import from | Must NOT import from |
|-------|----------------|----------------------|
| `domain/` | nothing | everything else |
| `infrastructure/` | `domain/` | `application/`, `presentation/` |
| `application/` | `domain/`, `infrastructure/` | `presentation/` |
| `presentation/` | all layers | — |

> **Rule:** Components never import a Zustand store directly.
> They always go through a `presentation/hooks/use*.ts` selector hook.

---

## Naming Conventions

### Files
| Type | Convention | Example |
|------|-----------|---------|
| Domain entity/interface | `camelCase.ts` | `user.ts`, `boardGame.ts` |
| Repository interface | `I{Feature}Repository.ts` | `IAuthRepository.ts` |
| Repository class file | `{feature}Repository.ts` | `authRepository.ts` |
| Zustand store | `{feature}Store.ts` | `authStore.ts` |
| Presentation hook | `use{Feature}.ts` | `useAuth.ts` |
| Page component | `{Name}Page.tsx` | `LoginPage.tsx`, `DashboardPage.tsx` |
| Shared component | `{Name}.tsx` | `Button.tsx`, `InputField.tsx` |
| Route guard | `{Name}Route.tsx` | `ProtectedRoute.tsx` |

### Symbols
- **Interfaces**: PascalCase, repository ports prefixed with `I` (`IAuthRepository`)
- **Classes**: PascalCase (`AuthRepository`)
- **Zustand store hooks**: exported as `use{Feature}Store` (`useAuthStore`)
- **Presentation hooks**: exported as `use{Feature}` (`useAuth`)
- **Components/Pages**: PascalCase default export matching the file name
- **Types imported only as types**: always use `import type { ... }`

---

## Domain Layer Rules

- **No imports** from React, Axios, Zustand, or any npm package.
- Only TypeScript `interface` and `type` definitions.
- Every entity file in `domain/entities/` defines the shape of one domain object.
- Every file in `domain/interfaces/` defines a port (interface) that infrastructure must implement.
- Use `interface` over `type` for objects; use `type` for unions/aliases.

**Example entity (`domain/entities/boardGame.ts`):**
```ts
export interface BoardGame {
  id: number;
  gameName: string;
  version: string;
  minPlayers: number;
  maxPlayers: number;
  price: number;
  imageUrl: string | null;
}
```

**Example port (`domain/interfaces/IBoardGameRepository.ts`):**
```ts
import type { BoardGame } from '../entities/boardGame';

export interface IBoardGameRepository {
  getAll(): Promise<BoardGame[]>;
  getById(id: number): Promise<BoardGame>;
}
```

---

## Infrastructure Layer Rules

- One file for the shared Axios instance: `infrastructure/http/apiClient.ts`.
  - Reads `VITE_API_URL` from `import.meta.env` (falls back to `http://localhost:5000`).
  - Attaches `Authorization: Bearer <token>` from `localStorage.getItem('accessToken')`.
  - Do **not** create additional Axios instances.
- One file per repository: `infrastructure/repositories/{feature}Repository.ts`.
  - The class implements the domain port interface.
  - Export a singleton instance (not the class): `export const authRepository = new AuthRepository();`
  - API response shapes (raw JSON) are defined as private `interface` inside the repository file — they must NOT leak into domain or application layers.

---

## Application Layer Rules

- One Zustand store per feature: `application/store/{feature}Store.ts`.
- Store interface name: `{Feature}State` (e.g. `AuthState`).
- Store export name: `use{Feature}Store` (e.g. `useAuthStore`).
- Stores contain: **state fields** + **use-case methods** (async actions).
- Stores handle their own loading/error state (`isLoading: boolean`, `error: string | null`).
- Stores call repository singletons directly (imported from infrastructure).
- localStorage persistence is done inside the store action, not in the repository.
- Include a `clearError()` action in every store that has an `error` field.
- **No React imports** in the store file.

**Store skeleton:**
```ts
import { create } from 'zustand';
import type { SomeEntity } from '../../domain/entities/someEntity';
import { someRepository } from '../../infrastructure/repositories/someRepository';

interface SomeState {
  items: SomeEntity[];
  isLoading: boolean;
  error: string | null;
  fetchAll: () => Promise<void>;
  clearError: () => void;
}

export const useSomeStore = create<SomeState>((set) => ({
  items: [],
  isLoading: false,
  error: null,
  fetchAll: async () => {
    set({ isLoading: true, error: null });
    try {
      const items = await someRepository.getAll();
      set({ items, isLoading: false });
    } catch {
      set({ error: 'Failed to load.', isLoading: false });
    }
  },
  clearError: () => set({ error: null }),
}));
```

---

## Presentation Layer Rules

### Hooks (`presentation/hooks/`)
- Every store must have a corresponding selector hook.
- The hook selects only the fields the UI needs — do **not** expose the entire store.
- Hooks are the **only** way components access store state.

```ts
// presentation/hooks/useSomeFeature.ts
import { useSomeStore } from '../../application/store/someStore';

export function useSomeFeature() {
  return useSomeStore((state) => ({
    items: state.items,
    isLoading: state.isLoading,
    error: state.error,
    fetchAll: state.fetchAll,
    clearError: state.clearError,
  }));
}
```

### Pages (`presentation/pages/`)
- One file per route. Named `{Name}Page.tsx`.
- Default export only. Name matches file name.
- Import state via `presentation/hooks/`, never directly from a store.
- No business logic in pages — pages orchestrate components and hooks only.
- Use `useEffect` to load data on mount; call `clearError()` before any action.

### Components (`presentation/components/`)
- Reusable UI-only components (no store imports, no routing logic).
- Props typed with a `{ComponentName}Props` interface in the same file.
- Default export.

### Routes (`presentation/routes/`)
- Route guards using `<Outlet />` pattern (React Router v7).
- Read auth state from `useAuth()` hook.
- Redirect unauthenticated users to `/login` with `<Navigate replace />`.

### App.tsx
- Contains `<BrowserRouter>`, `<Routes>`, and `<Route>` declarations only.
- No state, no hooks, no business logic.
- Import pages from `presentation/pages/`, routes from `presentation/routes/`.

---

## Routing Conventions

| Path | Component | Guard |
|------|-----------|-------|
| `/login` | `LoginPage` | Public |
| `/dashboard` | `DashboardPage` | `ProtectedRoute` |
| `*` | Redirect to `/login` | — |

- All protected routes are nested inside `<Route element={<ProtectedRoute />}>`.
- Add new routes in `App.tsx` only.

---

## Styling Rules

- Use **Tailwind CSS utility classes** only. Do not write custom CSS unless absolutely unavoidable.
- `src/index.css` contains only `@tailwind base; @tailwind components; @tailwind utilities;` — do not add global styles here.
- `src/App.css` is unused — do not add styles there.
- No inline `style={{}}` props.
- Responsive design: use Tailwind's `sm:`, `md:`, `lg:` prefixes.
- No Tailwind design decisions are final in the initial build — visual polish is a separate pass.

---

## Environment Variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `VITE_API_URL` | `http://localhost:5000` | Base URL for all API calls |

- All env vars must be prefixed with `VITE_` to be accessible in the browser.
- Access via `import.meta.env.VITE_*` — never `process.env`.
- Dev values go in `.env`. Never commit secrets.

---

## API Integration

- All HTTP calls go through `infrastructure/http/apiClient.ts`.
- API base URL: `http://localhost:5000` (dev). CORS is enabled on the API for `http://localhost:5173`.
- Auth endpoints:

| Method | Path | Purpose |
|--------|------|---------|
| POST | `/api/auth/login` | Login → returns accessToken + refreshToken |
| POST | `/api/auth/revoke` | Logout (requires Bearer token) |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/reset-password` | Change password (requires Bearer token) |

- Token storage: `accessToken`, `refreshToken`, `username`, `role` in `localStorage`.
- Tokens are attached automatically by the Axios request interceptor.

---

## Auth & Roles

Three roles exist: `Admin`, `Manager`, `DataEntry`.
- All authenticated routes must be wrapped in `ProtectedRoute`.
- If role-based UI gating is needed, read `user.role` from `useAuth()`.

---

## Adding a New Feature — Checklist

When adding a new feature (e.g. "BoardGames list page"), follow this order:

1. **`domain/entities/{feature}.ts`** — define the entity interface
2. **`domain/interfaces/I{Feature}Repository.ts`** — define the port
3. **`infrastructure/repositories/{feature}Repository.ts`** — implement the port using `apiClient`
4. **`application/store/{feature}Store.ts`** — create Zustand store with loading/error state
5. **`presentation/hooks/use{Feature}.ts`** — selector hook over the store
6. **`presentation/pages/{Name}Page.tsx`** — page component using the hook
7. **`presentation/components/`** — any shared sub-components
8. **`App.tsx`** — add the new route

---

## TypeScript Rules

- `"strict": true` is enabled — never disable it.
- Always use `import type { ... }` for type-only imports (required by `verbatimModuleSyntax`).
- No `any`. Use `unknown` if type is truly unknown, then narrow it.
- Prefer `interface` over `type` for object shapes.
- API response shapes (raw JSON) are private to the repository file — never exported.
- **Do NOT use TypeScript `enum`** — `erasableSyntaxOnly` is enabled. Use `const` objects with `as const` instead:

```ts
// ✅ Correct pattern (src/domain/enums/index.ts)
export const UserType = { Regular: 0, Premium: 1 } as const;
export type UserType = (typeof UserType)[keyof typeof UserType];

// ❌ Forbidden
export enum UserType { Regular = 0, Premium = 1 }
```

- Use the exported `enumLabel(obj, value)` helper from `domain/enums/index.ts` to get a display name from a numeric value.

---

## Routing

| Path | Page | Guard | Notes |
|------|------|-------|-------|
| `/login` | `LoginPage` | Public | |
| `/board-games` | `BoardGamesPage` | `ProtectedRoute` + `Layout` | Default home |
| `/game-issues` | `GameIssuesPage` | `ProtectedRoute` + `Layout` | |
| `/members` | `MembersPage` | `ProtectedRoute` + `Layout` | |
| `/users` | `UsersPage` | `ProtectedRoute` + `Layout` | |
| `/inventory` | `InventoryPage` | `ProtectedRoute` + `Layout` | |
| `/dashboard` | Redirect to `/board-games` | | |
| `*` | Redirect to `/login` | | |

All protected routes are nested inside `<Route element={<ProtectedRoute />}><Route element={<Layout />}>`.

---

## Shared Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `Layout` | `presentation/components/Layout.tsx` | Sidebar + `<Outlet />` main area |
| `Sidebar` | `presentation/components/Sidebar.tsx` | Collapsible nav (☰ hamburger), nav links with icons, sign-out |
| `Modal` | `presentation/components/Modal.tsx` | Generic overlay modal — `isOpen`, `title`, `onClose`, `children` |
| `ImageUpload` | `presentation/components/ImageUpload.tsx` | File input + data-URL preview box |
| `Table<T>` | `presentation/components/Table.tsx` | Generic typed table — `columns`, `rows`, `rowKey`, `onRowClick` |
| `Pagination` | `presentation/components/Pagination.tsx` | Prev/Next page controls |

### Sidebar navigation items
Add new pages to the `NAV_ITEMS` array in `Sidebar.tsx` and add a corresponding icon in `navIcon()`.

### Table columns pattern
```tsx
columns={[
  { header: 'ID', render: (r) => r.id },
  { header: 'Name', render: (r) => <button className="text-blue-600 hover:underline">{r.name}</button> },
]}
```
Clicking a row fires `onRowClick(row)` — use this to open the edit modal.

### Page pattern (Add + Edit modals)
Each feature page follows this pattern:
1. `useEffect` → `fetchPage(page)` on mount and page change
2. **Add button** → sets `editing = null`, opens modal
3. **Row click** → sets `editing = row`, populates form, opens modal
4. `handleSubmit` → calls `create(form)` or `update(id, form)` based on `editing`
5. Modal closes only when `error` is null after save

### Stores use `pagedResult` not `items`
All feature stores expose `pagedResult: PagedResult<T> | null` (not a flat `items` array).
Access items via `pagedResult?.items ?? []`.

---

## Domain Enums (src/domain/enums/index.ts)

| Const | Values |
|-------|--------|
| `UserType` | `Regular=0`, `Premium=1` |
| `UserRole` | `Admin=1`, `DataEntry=2`, `Manager=3` |
| `GameCondition` | `Mint=0`, `Lost=1`, `Broken=2`, `CompleteNotMint=3` |
| `GameIssueStatus` | `Active=0`, `Returned=1`, `Overdue=2` |

---

## Scripts

```bash
npm run dev       # Start dev server at http://localhost:5173
npm run build     # TypeScript check + production build
npm run lint      # ESLint
npm run preview   # Serve production build locally
```

> Always run `npm run build` before committing to ensure zero TypeScript errors.

