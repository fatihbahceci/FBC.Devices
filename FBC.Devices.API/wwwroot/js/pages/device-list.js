function deviceListPage() {
    return {
        table: null,
        groups: [],
        types: [],
        newDevice: { name: '', deviceGroupId: null, deviceTypeId: null },

        async init() {
            await this.loadDropdowns();
            this.initTable();
        },

        async loadDropdowns() {
            try {
                this.groups = await apiFetch('/api/device-groups') || [];
                this.types = await apiFetch('/api/device-types') || [];
            } catch { }
        },

        initTable() {
            const self = this;
            this.table = new Tabulator('#device-table', {
                layout: 'fitColumns',
                responsiveLayout: 'hide',
                pagination: true,
                paginationSize: 25,
                paginationSizeSelector: [10, 25, 50, 100],
                movableColumns: true,
                ajaxURL: '/api/devices',
                ajaxConfig: { headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` } },
                ajaxResponse: function (url, params, response) {
                    return response.items || [];
                },
                columns: [
                    { title: 'ID', field: 'id', width: 70, headerFilter: true },
                    { title: i18n.t('device.name'), field: 'name', headerFilter: true },
                    { title: i18n.t('device.description'), field: 'description', headerFilter: true },
                    { title: i18n.t('device.group'), field: 'deviceGroup.name', headerFilter: true },
                    { title: i18n.t('device.type'), field: 'deviceType.name', headerFilter: true },
                    { title: i18n.t('device.model'), field: 'deviceModel', headerFilter: true },
                    { title: i18n.t('device.serialNumber'), field: 'serialNumber', headerFilter: true },
                    { title: i18n.t('device.location'), field: 'location', headerFilter: true },
                    { title: i18n.t('device.isActive'), field: 'isActive', formatter: 'tickCross', hozAlign: 'center', width: 80 },
                    {
                        title: i18n.t('common.actions'), field: 'id', width: 120, hozAlign: 'center',
                        formatter: () => `<button class="btn btn-sm btn-outline-primary me-1"><i class="bi bi-pencil"></i></button><button class="btn btn-sm btn-outline-danger"><i class="bi bi-trash"></i></button>`,
                        cellClick: (e, cell) => {
                            if (e.target.closest('.btn-outline-primary')) {
                                window.location.hash = `#edit-device/${cell.getData().id}`;
                            }
                            if (e.target.closest('.btn-outline-danger')) {
                                self.deleteDevice(cell.getData().id);
                            }
                        }
                    }
                ]
            });
        },

        async addDevice() {
            if (!this.newDevice.name) return;
            try {
                await apiFetch('/api/devices', {
                    method: 'POST',
                    body: JSON.stringify({
                        name: this.newDevice.name,
                        deviceGroupId: this.newDevice.deviceGroupId || null,
                        deviceTypeId: this.newDevice.deviceTypeId || null,
                        isActive: true,
                        addresses: []
                    })
                });
                showToast(i18n.t('toast.saved'));
                this.newDevice = { name: '', deviceGroupId: null, deviceTypeId: null };
                this.table.replaceData();
            } catch { }
        },

        async deleteDevice(id) {
            if (!confirm(i18n.t('common.confirmDelete'))) return;
            try {
                await apiFetch(`/api/devices/${id}`, { method: 'DELETE' });
                showToast(i18n.t('toast.deleted'));
                this.table.replaceData();
            } catch { }
        }
    };
}
