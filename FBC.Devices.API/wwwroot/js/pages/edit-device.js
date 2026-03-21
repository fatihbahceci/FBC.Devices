function editDevicePage() {
    return {
        device: null,
        groups: [],
        types: [],
        addrTypes: [],
        loading: true,
        saving: false,

        async init() {
            const id = this.getDeviceId();
            if (!id) return;
            await Promise.all([this.loadDropdowns(), this.loadDevice(id)]);
            this.loading = false;
        },

        getDeviceId() {
            const hash = window.location.hash;
            const match = hash.match(/edit-device\/(\d+)/);
            return match ? parseInt(match[1]) : null;
        },

        async loadDropdowns() {
            try {
                const [groups, types, addrTypes] = await Promise.all([
                    apiFetch('/api/device-groups'),
                    apiFetch('/api/device-types'),
                    apiFetch('/api/addr-types')
                ]);
                this.groups = groups || [];
                this.types = types || [];
                this.addrTypes = addrTypes || [];
            } catch { }
        },

        async loadDevice(id) {
            try {
                this.device = await apiFetch(`/api/devices/${id}`);
            } catch { }
        },

        addAddress() {
            if (!this.device) return;
            this.device.deviceAddresses.push({
                id: 0,
                addrTypeId: this.addrTypes[0]?.id || 0,
                addr: 'http://',
                username: '',
                password: '',
                periodicPingCheck: false,
                _showPassword: false
            });
        },

        removeAddress(index) {
            if (!confirm(i18n.t('common.confirmDelete'))) return;
            this.device.deviceAddresses.splice(index, 1);
        },

        async save() {
            if (!this.device) return;
            this.saving = true;
            try {
                await apiFetch(`/api/devices/${this.device.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        name: this.device.name,
                        description: this.device.description,
                        deviceGroupId: this.device.deviceGroupId || null,
                        deviceTypeId: this.device.deviceTypeId || null,
                        deviceModel: this.device.deviceModel,
                        serialNumber: this.device.serialNumber,
                        location: this.device.location,
                        note: this.device.note,
                        isActive: this.device.isActive,
                        addresses: this.device.deviceAddresses.map(a => ({
                            id: a.id || 0,
                            addrTypeId: a.addrTypeId,
                            addr: a.addr,
                            username: a.username,
                            password: a.password,
                            periodicPingCheck: a.periodicPingCheck
                        }))
                    })
                });
                showToast(i18n.t('toast.saved'));
            } catch { }
            this.saving = false;
        },

        goBack() {
            window.location.hash = '#devices';
        }
    };
}
