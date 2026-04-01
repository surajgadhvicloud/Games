import { useEffect, useState } from 'react';
import { useBoardGames } from '../hooks/useBoardGames';
import type { BoardGame, CreateBoardGameRequest, UpdateBoardGameRequest } from '../../domain/entities/boardGame';
import Table from '../components/Table';
import Pagination from '../components/Pagination';
import Modal from '../components/Modal';
import ImageUpload from '../components/ImageUpload';

const EMPTY_FORM: CreateBoardGameRequest = {
  gameName: '', version: '', minPlayers: 2, maxPlayers: 4, price: 0, imageUrl: null,
};

export default function BoardGamesPage() {
  const { pagedResult, isLoading, error, fetchPage, create, update, clearError } = useBoardGames();
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<BoardGame | null>(null);
  const [form, setForm] = useState<CreateBoardGameRequest>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);

  useEffect(() => { fetchPage(page); }, [page]);

  function openAdd() { setEditing(null); setForm(EMPTY_FORM); clearError(); setModalOpen(true); }
  function openEdit(g: BoardGame) {
    setEditing(g);
    setForm({ gameName: g.gameName, version: g.version, minPlayers: g.minPlayers, maxPlayers: g.maxPlayers, price: g.price, imageUrl: g.imageUrl });
    clearError();
    setModalOpen(true);
  }
  function closeModal() { setModalOpen(false); setEditing(null); }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    if (editing) {
      await update(editing.id, form as UpdateBoardGameRequest);
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
        <h1 className="text-xl font-semibold text-gray-800">Board Games</h1>
        <button onClick={openAdd} className="bg-blue-600 text-white px-4 py-2 rounded text-sm hover:bg-blue-700">
          + Add Game
        </button>
      </div>

      {error && <p className="text-red-500 text-sm mb-4">{error}</p>}
      {isLoading && <p className="text-gray-400 text-sm mb-4">Loading…</p>}

      <Table
        rows={rows}
        rowKey={(r) => r.id}
        onRowClick={openEdit}
        columns={[
          { header: 'ID', render: (r) => r.id },
          { header: 'Game Name', render: (r) => <button className="text-blue-600 hover:underline">{r.gameName}</button> },
          { header: 'Version', render: (r) => r.version },
          { header: 'Players', render: (r) => `${r.minPlayers}–${r.maxPlayers}` },
          { header: 'Price', render: (r) => `₹${r.price.toFixed(2)}` },
          { header: 'Image', render: (r) => r.imageUrl ? <img src={r.imageUrl} alt="" className="w-10 h-10 object-contain" /> : '—' },
        ]}
      />
      <Pagination page={page} totalPages={pagedResult?.totalPages ?? 1} onPageChange={setPage} />

      <Modal isOpen={modalOpen} title={editing ? 'Edit Board Game' : 'Add Board Game'} onClose={closeModal}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Field label="Game Name"><input required className={input} value={form.gameName} onChange={(e) => setForm({ ...form, gameName: e.target.value })} /></Field>
          <Field label="Version"><input required className={input} value={form.version} onChange={(e) => setForm({ ...form, version: e.target.value })} /></Field>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Min Players"><input type="number" min={1} required className={input} value={form.minPlayers} onChange={(e) => setForm({ ...form, minPlayers: +e.target.value })} /></Field>
            <Field label="Max Players"><input type="number" min={1} required className={input} value={form.maxPlayers} onChange={(e) => setForm({ ...form, maxPlayers: +e.target.value })} /></Field>
          </div>
          <Field label="Price (₹)"><input type="number" min={0} step="0.01" required className={input} value={form.price} onChange={(e) => setForm({ ...form, price: +e.target.value })} /></Field>
          <ImageUpload label="Game Image" value={form.imageUrl ?? null} onChange={(url) => setForm({ ...form, imageUrl: url })} />
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
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      {children}
    </div>
  );
}

const input = 'w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500';
