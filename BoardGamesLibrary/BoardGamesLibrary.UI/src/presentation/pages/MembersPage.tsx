import { useEffect, useState } from 'react';
import { useMembers } from '../hooks/useMembers';
import type { Member, CreateMemberRequest, UpdateMemberRequest } from '../../domain/entities/member';
import { UserType, enumLabel } from '../../domain/enums';
import Table from '../components/Table';
import Pagination from '../components/Pagination';
import Modal from '../components/Modal';

const EMPTY: CreateMemberRequest = { firstName: '', middleName: '', lastName: '', address: '', phoneNumber: '', email: '', typeOfUser: UserType.Regular };

export default function MembersPage() {
  const { pagedResult, isLoading, error, fetchPage, create, update, clearError } = useMembers();
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Member | null>(null);
  const [form, setForm] = useState<CreateMemberRequest>(EMPTY);
  const [saving, setSaving] = useState(false);

  useEffect(() => { fetchPage(page); }, [page]);

  function openAdd() { setEditing(null); setForm(EMPTY); clearError(); setModalOpen(true); }
  function openEdit(m: Member) {
    setEditing(m);
    setForm({ firstName: m.firstName, middleName: m.middleName ?? '', lastName: m.lastName, address: m.address, phoneNumber: m.phoneNumber, email: m.email, typeOfUser: m.typeOfUser });
    clearError(); setModalOpen(true);
  }
  function closeModal() { setModalOpen(false); setEditing(null); }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    if (editing) { await update(editing.id, form as UpdateMemberRequest); }
    else { await create(form); }
    setSaving(false);
    if (!error) closeModal();
  }

  const rows = pagedResult?.items ?? [];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-semibold text-gray-800">Members</h1>
        <button onClick={openAdd} className="bg-blue-600 text-white px-4 py-2 rounded text-sm hover:bg-blue-700">+ Add Member</button>
      </div>
      {error && <p className="text-red-500 text-sm mb-4">{error}</p>}
      {isLoading && <p className="text-gray-400 text-sm mb-4">Loading…</p>}
      <Table rows={rows} rowKey={(r) => r.id} onRowClick={openEdit} columns={[
        { header: 'ID', render: (r) => r.id },
        { header: 'Name', render: (r) => <button className="text-blue-600 hover:underline">{r.firstName} {r.lastName}</button> },
        { header: 'Email', render: (r) => r.email },
        { header: 'Phone', render: (r) => r.phoneNumber },
        { header: 'Type', render: (r) => enumLabel(UserType, r.typeOfUser) },
      ]} />
      <Pagination page={page} totalPages={pagedResult?.totalPages ?? 1} onPageChange={setPage} />

      <Modal isOpen={modalOpen} title={editing ? 'Edit Member' : 'Add Member'} onClose={closeModal}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Field label="First Name"><input required className={input} value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} /></Field>
            <Field label="Last Name"><input required className={input} value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} /></Field>
          </div>
          <Field label="Middle Name (optional)"><input className={input} value={form.middleName ?? ''} onChange={(e) => setForm({ ...form, middleName: e.target.value })} /></Field>
          <Field label="Address"><input required className={input} value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} /></Field>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Phone"><input required className={input} value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} /></Field>
            <Field label="Email"><input type="email" required className={input} value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></Field>
          </div>
          <Field label="Member Type">
            <select required className={input} value={form.typeOfUser} onChange={(e) => setForm({ ...form, typeOfUser: +e.target.value as UserType })}>
              <option value={UserType.Regular}>Regular</option>
              <option value={UserType.Premium}>Premium</option>
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
