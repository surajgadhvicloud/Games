import { useEffect, useState } from 'react';
import { useGameIssues } from '../hooks/useGameIssues';
import { useMembers } from '../hooks/useMembers';
import { useBoardGames } from '../hooks/useBoardGames';
import type { GameIssue, CreateGameIssueRequest, UpdateGameIssueRequest } from '../../domain/entities/gameIssue';
import { GameCondition, GameIssueStatus, enumLabel } from '../../domain/enums';
import Table from '../components/Table';
import Pagination from '../components/Pagination';
import Modal from '../components/Modal';
import ImageUpload from '../components/ImageUpload';

const EMPTY_ADD: CreateGameIssueRequest = { boardGameId: 0, userId: 0, conditionGivenOut: GameCondition.Mint };
const EMPTY_EDIT: UpdateGameIssueRequest = { returnDateUtc: null, conditionGivenIn: null, photoUrlAfterReturn: null };

function toDateInput(iso: string | null | undefined): string {
  if (!iso) return '';
  return iso.split('T')[0];
}

export default function GameIssuesPage() {
  const { pagedResult, isLoading, error, fetchPage, create, update, clearError } = useGameIssues();
  const { pagedResult: bgResult, fetchPage: fetchBg } = useBoardGames();
  const { pagedResult: mResult, fetchPage: fetchMembers } = useMembers();
  const [page, setPage] = useState(1);
  const [addOpen, setAddOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editing, setEditing] = useState<GameIssue | null>(null);
  const [addForm, setAddForm] = useState<CreateGameIssueRequest>(EMPTY_ADD);
  const [editForm, setEditForm] = useState<UpdateGameIssueRequest>(EMPTY_EDIT);
  const [saving, setSaving] = useState(false);

  useEffect(() => { fetchPage(page); fetchBg(1, 100); fetchMembers(1, 100); }, [page]);

  function openAdd() { setAddForm(EMPTY_ADD); clearError(); setAddOpen(true); }
  function openEdit(g: GameIssue) {
    setEditing(g);
    setEditForm({ returnDateUtc: g.returnDateUtc, conditionGivenIn: g.conditionGivenIn, photoUrlAfterReturn: g.photoUrlAfterReturn });
    clearError(); setEditOpen(true);
  }

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    await create(addForm);
    setSaving(false);
    if (!error) setAddOpen(false);
  }

  async function handleEdit(e: React.FormEvent) {
    e.preventDefault();
    if (!editing) return;
    setSaving(true);
    await update(editing.id, editForm);
    setSaving(false);
    if (!error) { setEditOpen(false); setEditing(null); }
  }

  const rows = pagedResult?.items ?? [];
  const boardGames = bgResult?.items ?? [];
  const members = mResult?.items ?? [];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-semibold text-gray-800">Game Issues</h1>
        <button onClick={openAdd} className="bg-blue-600 text-white px-4 py-2 rounded text-sm hover:bg-blue-700">+ Add Issue</button>
      </div>
      {error && <p className="text-red-500 text-sm mb-4">{error}</p>}
      {isLoading && <p className="text-gray-400 text-sm mb-4">Loading…</p>}
      <Table rows={rows} rowKey={(r) => r.id} onRowClick={openEdit} columns={[
        { header: 'ID', render: (r) => r.id },
        { header: 'Board Game ID', render: (r) => r.boardGameId },
        { header: 'Member ID', render: (r) => r.userId },
        { header: 'Start', render: (r) => toDateInput(r.startDateUtc) },
        { header: 'End', render: (r) => toDateInput(r.endDateUtc) },
        { header: 'Return', render: (r) => toDateInput(r.returnDateUtc) || '—' },
        { header: 'Status', render: (r) => enumLabel(GameIssueStatus, r.status) },
        { header: 'Overdue (₹)', render: (r) => r.overdueCharges > 0 ? `₹${r.overdueCharges}` : '—' },
      ]} />
      <Pagination page={page} totalPages={pagedResult?.totalPages ?? 1} onPageChange={setPage} />

      {/* Add Modal */}
      <Modal isOpen={addOpen} title="Add Game Issue" onClose={() => setAddOpen(false)}>
        <form onSubmit={handleAdd} className="space-y-4">
          <Field label="Board Game">
            <select required className={input} value={addForm.boardGameId} onChange={(e) => setAddForm({ ...addForm, boardGameId: +e.target.value })}>
              <option value={0}>Select…</option>
              {boardGames.map((g) => <option key={g.id} value={g.id}>{g.gameName} — {g.version}</option>)}
            </select>
          </Field>
          <Field label="Member">
            <select required className={input} value={addForm.userId} onChange={(e) => setAddForm({ ...addForm, userId: +e.target.value })}>
              <option value={0}>Select…</option>
              {members.map((m) => <option key={m.id} value={m.id}>{m.firstName} {m.lastName}</option>)}
            </select>
          </Field>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Start Date"><input type="date" className={input} onChange={(e) => setAddForm({ ...addForm, startDateUtc: e.target.value ? e.target.value + 'T00:00:00Z' : null })} /></Field>
            <Field label="End Date"><input type="date" className={input} onChange={(e) => setAddForm({ ...addForm, endDateUtc: e.target.value ? e.target.value + 'T00:00:00Z' : null })} /></Field>
          </div>
          <Field label="Condition Given Out">
            <select required className={input} value={addForm.conditionGivenOut} onChange={(e) => setAddForm({ ...addForm, conditionGivenOut: +e.target.value as GameCondition })}>
              {conditionOptions()}
            </select>
          </Field>
          <ImageUpload label="Photo Before Issue" value={addForm.photoUrlBeforeIssue ?? null} onChange={(url) => setAddForm({ ...addForm, photoUrlBeforeIssue: url })} />
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div className="flex justify-end gap-2 pt-2">
            <button type="button" onClick={() => setAddOpen(false)} className="px-4 py-2 border rounded text-sm">Cancel</button>
            <button type="submit" disabled={saving} className="px-4 py-2 bg-blue-600 text-white rounded text-sm hover:bg-blue-700 disabled:opacity-50">{saving ? 'Saving…' : 'Save'}</button>
          </div>
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal isOpen={editOpen} title="Edit Game Issue (Return)" onClose={() => { setEditOpen(false); setEditing(null); }}>
        <form onSubmit={handleEdit} className="space-y-4">
          <Field label="Return Date"><input type="date" className={input} value={toDateInput(editForm.returnDateUtc)} onChange={(e) => setEditForm({ ...editForm, returnDateUtc: e.target.value ? e.target.value + 'T00:00:00Z' : null })} /></Field>
          <Field label="Condition Given In">
            <select className={input} value={editForm.conditionGivenIn ?? ''} onChange={(e) => setEditForm({ ...editForm, conditionGivenIn: e.target.value !== '' ? +e.target.value as GameCondition : null })}>
              <option value="">Not returned yet</option>
              {conditionOptions()}
            </select>
          </Field>
          <ImageUpload label="Photo After Return" value={editForm.photoUrlAfterReturn ?? null} onChange={(url) => setEditForm({ ...editForm, photoUrlAfterReturn: url })} />
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div className="flex justify-end gap-2 pt-2">
            <button type="button" onClick={() => { setEditOpen(false); setEditing(null); }} className="px-4 py-2 border rounded text-sm">Cancel</button>
            <button type="submit" disabled={saving} className="px-4 py-2 bg-blue-600 text-white rounded text-sm hover:bg-blue-700 disabled:opacity-50">{saving ? 'Saving…' : 'Save'}</button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

function conditionOptions() {
  return [
    <option key={GameCondition.Mint} value={GameCondition.Mint}>Mint</option>,
    <option key={GameCondition.CompleteNotMint} value={GameCondition.CompleteNotMint}>Complete (Not Mint)</option>,
    <option key={GameCondition.Broken} value={GameCondition.Broken}>Broken</option>,
    <option key={GameCondition.Lost} value={GameCondition.Lost}>Lost</option>,
  ];
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (<div><label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>{children}</div>);
}
const input = 'w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500';
