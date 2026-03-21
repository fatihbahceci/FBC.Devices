function userListPage() {
    return {
        table: null,
        newUser: { userName: '', password: '', name: '' },

        async init() {
            this.initTable();
        },

        initTable() {
            const self = this;
            this.table = new Tabulator('#user-table', {
                layout: 'fitColumns',
                pagination: true,
                paginationSize: 25,
                ajaxURL: '/api/users',
                ajaxConfig: { headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` } },
                ajaxResponse: function (url, params, response) {
                    return response.items || [];
                },
                columns: [
                    { title: 'ID', field: 'id', width: 70, headerFilter: true },
                    { title: i18n.t('user.username'), field: 'userName', headerFilter: true },
                    { title: i18n.t('user.name'), field: 'name', headerFilter: true },
                    { title: i18n.t('user.isSysAdmin'), field: 'isSysAdmin', formatter: 'tickCross', hozAlign: 'center', width: 100 },
                    {
                        title: i18n.t('common.actions'), field: 'id', width: 120, hozAlign: 'center',
                        formatter: () => `<button class="btn btn-sm btn-outline-primary me-1"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger"><i class="bi bi-trash"></i></button>`,
                        cellClick: (e, cell) => {
                            if (e.target.closest('.btn-outline-primary')) {
                                window.location.hash = `#edit-user/${cell.getData().id}`;
                            }
                            if (e.target.closest('.btn-outline-danger')) {
                                self.deleteUser(cell.getData().id);
                            }
                        }
                    }
                ]
            });
        },

        async addUser() {
            if (!this.newUser.userName || !this.newUser.password || !this.newUser.name) return;
            try {
                await apiFetch('/api/users', {
                    method: 'POST',
                    body: JSON.stringify({
                        userName: this.newUser.userName,
                        password: this.newUser.password,
                        name: this.newUser.name,
                        isSysAdmin: false,
                        roles: []
                    })
                });
                showToast(i18n.t('toast.saved'));
                this.newUser = { userName: '', password: '', name: '' };
                this.table.replaceData();
            } catch { }
        },

        async deleteUser(id) {
            if (!confirm(i18n.t('common.confirmDelete'))) return;
            try {
                await apiFetch(`/api/users/${id}`, { method: 'DELETE' });
                showToast(i18n.t('toast.deleted'));
                this.table.replaceData();
            } catch { }
        }
    };
}
