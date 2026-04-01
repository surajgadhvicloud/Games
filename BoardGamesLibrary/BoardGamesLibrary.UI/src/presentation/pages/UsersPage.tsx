import { useEffect, useState } from 'react';
import { useUsers } from '../hooks/useUsers';
import type { AppUser, CreateUserRequest, UpdateUserRequest } from '../../domain/entities/appUser';
import { UserRole, enumLabel } from '../../domain/enums';
import Table from '../components/Table';
import Pagination from '../components/Pagination';
import Modal from '../components/Modal';

const EMPTY: CreateUserRequest = { firstName: '', lastName: '', email: '', username: '', password: '', role: UserRole.DataEntry };

export default function UsersPage() {
  const { pagedResult, isLoading, error, fetchPage, create, update, clearError } = useUsers();
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<AppUser | null>(null);
  const [form, setForm] = useState<CreateUserRequest>(EMPTY);
  const [saving, setSaving] = useState(false);

  useEffect(() => { fetchPage(page); }, [page]);

  function openAdd() { setEditing(null); setForm(EMPTY); clearError(); setModalOpen(true); }
  function openEdit(u: AppUser) {
    setEditing(u);
    setForm({ firstName: u.firstName, lastName: u.lastName, email: u.email, username: u.username, password: '', role: u.role });
    clearError(); setModalOpen(true);
  }
  function closeModal() { setModalOpen(false); setEditing(null); }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    if (editing) {
      const payload: UpdateUserRequest = { ...form, password: form.password || null };
      await update(editing.id, payload);
    } else {
      await create(form);
    }
    setSaving(false);
    if (!error) closeModal();
  }

  const rows = pagedResult?.items ?? [];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-semibold text-gray-800">Users</h1>
        <button onClick={openAdd} className="bg-blue-600 text-white px-4 py-2 rounded text-sm hover:bg-blue-700">+ Add User</button>
      </div>
      {error && <p className="text-red-500 text-sm mb-4">{error}</p>}
      {isLoading && <p className="text-gray-400 text-sm mb-4">Loading…</p>}
      <Table rows={rows} rowKey={(r) => r.id} onRowClick={openEdit} columns={[
        { header: 'ID', render: (r) => r.id },
        { header: 'Username', render: (r) => <button className="text-blue-600 hover:underline">{r.username}</button> },
        { header: 'Name', render: (r) => `${r.firstName} ${r.lastName}` },
        { header: 'Email', render: (r) => r.email },
        { header: 'Role', render: (r) => enumLabel(UserRole, r.role) },
      ]} />
      <Pagination page={page} totalPages={pagedResult?.totalPages ?? 1} onPageChange={setPage} />

      <Modal isOpen={modalOpen} title={editing ? 'Edit User' : 'Add User'} onClose={closeModal}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Field label="First Name"><input required className={input} value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} /></Field>
            <Field label="Last Name"><input required className={input} value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} /></Field>
          </div>
          <Field label="Email"><input type="email" required className={input} value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></Field>
          <Field label="Username"><input required className={input} value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} /></Field>
          <Field label={editing ? 'Password (leave blank to keep)' : 'Password'}>
            <input type="password" required={!editing} className={input} value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
          </Field>
          <Field label="Role">
            <select required className={input} value={form.role} onChange={(e) => setForm({ ...form, role: +e.target.value as UserRole })}>
              <option value={UserRole.Admin}>Admin</option>
              <option value={UserRole.Manager}>Manager</option>
              <option value={UserRole.DataEntry}>DataEntry</option>
            </select>
          </Field>
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div className="flex justify-end gap-2 pt-2">
            <button type="button" onClick={closeModal} className="px-4 py-2 border rounded text-sm">Cancel</button>
            <button type="submit" disabled={saving} className="px-4 py-2 bg-blue-600 text-white rounded text-sm hover:bg-blue-700 disabled:opacity-50">{saving ? 'Saving…' : 'Save'}</button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (<div><label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>{children}</div>);
}
const input = 'w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500';
