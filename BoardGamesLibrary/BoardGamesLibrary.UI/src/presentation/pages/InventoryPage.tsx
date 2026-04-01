import { useEffect, useState } from 'react';
import { useInventory } from '../hooks/useInventory';
import type { Inventory, UpdateInventoryRequest } from '../../domain/entities/inventory';
import Table from '../components/Table';
import Pagination from '../components/Pagination';
import Modal from '../components/Modal';

export default function InventoryPage() {
  const { pagedResult, isLoading, error, fetchPage, update, clearError } = useInventory();
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Inventory | null>(null);
  const [form, setForm] = useState<UpdateInventoryRequest>({ isMissingOrBroken: false, totalInventory: 1, availableInventory: 1 });
  const [saving, setSaving] = useState(false);

  useEffect(() => { fetchPage(page); }, [page]);

  function openEdit(inv: Inventory) {
    setEditing(inv);
    setForm({ isMissingOrBroken: inv.isMissingOrBroken, totalInventory: inv.totalInventory, availableInventory: inv.availableInventory });
    clearError();
    setModalOpen(true);
  }
  function closeModal() { setModalOpen(false); setEditing(null); }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!editing) return;
    setSaving(true);
    await update(editing.boardGameId, form);
    setSaving(false);
    if (!error) closeModal();
  }

  const rows = pagedResult?.items ?? [];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-semibold text-gray-800">Inventory</h1>
      </div>

      {error && <p className="text-red-500 text-sm mb-4">{error}</p>}
      {isLoading && <p className="text-gray-400 text-sm mb-4">Loading…</p>}

      <Table
        rows={rows}
        rowKey={(r) => r.id}
        onRowClick={openEdit}
        columns={[
          { header: 'ID', render: (r) => r.id },
          { header: 'Board Game ID', render: (r) => r.boardGameId },
          { header: 'Total', render: (r) => r.totalInventory },
          { header: 'Available', render: (r) => r.availableInventory },
          { header: 'Missing/Broken', render: (r) => r.isMissingOrBroken ? '⚠️ Yes' : 'No' },
        ]}
      />
      <Pagination page={page} totalPages={pagedResult?.totalPages ?? 1} onPageChange={setPage} />

      <Modal isOpen={modalOpen} title="Edit Inventory" onClose={closeModal}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="flex items-center gap-2">
            <input type="checkbox" id="missingOrBroken" checked={form.isMissingOrBroken} onChange={(e) => setForm({ ...form, isMissingOrBroken: e.target.checked })} />
            <label htmlFor="missingOrBroken" className="text-sm font-medium text-gray-700">Missing or Broken</label>
          </div>
          <Field label="Total Inventory"><input type="number" min={1} required className={input} value={form.totalInventory} onChange={(e) => setForm({ ...form, totalInventory: +e.target.value })} /></Field>
          <Field label="Available Inventory"><input type="number" min={0} required className={input} value={form.availableInventory} onChange={(e) => setForm({ ...form, availableInventory: +e.target.value })} /></Field>
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
