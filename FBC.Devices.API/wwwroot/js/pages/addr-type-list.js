function addrTypeListPage() {
    return {
        table: null,
        newItem: { name: '' },
        editItem: null,
        editModal: null,

        async init() {
            this.initTable();
            this.editModal = new bootstrap.Modal(document.getElementById('addrTypeEditModal'));
        },

        initTable() {
            const self = this;
            this.table = new Tabulator('#addr-type-table', {
                layout: 'fitColumns',
                pagination: true,
                paginationSize: 25,
                ajaxURL: '/api/addr-types',
                ajaxConfig: { headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` } },
                columns: [
                    { title: 'ID', field: 'id', width: 70, headerFilter: true },
                    { title: i18n.t('common.name'), field: 'name', headerFilter: true },
                    {
                        title: i18n.t('common.actions'), field: 'id', width: 120, hozAlign: 'center',
                        formatter: () => `<button class="btn btn-sm btn-outline-primary me-1"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger"><i class="bi bi-trash"></i></button>`,
                        cellClick: (e, cell) => {
                            if (e.target.closest('.btn-outline-primary')) self.openEdit(cell.getData());
                            if (e.target.closest('.btn-outline-danger')) self.deleteItem(cell.getData().id);
                        }
                    }
                ]
            });
        },

        async addItem() {
            if (!this.newItem.name) return;
            try {
                await apiFetch('/api/addr-types', { method: 'POST', body: JSON.stringify(this.newItem) });
                showToast(i18n.t('toast.saved'));
                this.newItem = { name: '' };
                this.table.replaceData();
            } catch { }
        },

        openEdit(data) {
            this.editItem = { ...data };
            this.editModal.show();
        },

        async saveEdit() {
            if (!this.editItem) return;
            try {
                await apiFetch(`/api/addr-types/${this.editItem.id}`, { method: 'PUT', body: JSON.stringify(this.editItem) });
                showToast(i18n.t('toast.saved'));
                this.editModal.hide();
                this.table.replaceData();
            } catch { }
        },

        async deleteItem(id) {
            if (!confirm(i18n.t('common.confirmDelete'))) return;
            try {
                await apiFetch(`/api/addr-types/${id}`, { method: 'DELETE' });
                showToast(i18n.t('toast.deleted'));
                this.table.replaceData();
            } catch { }
        }
    };
}
