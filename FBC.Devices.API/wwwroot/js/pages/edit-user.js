function editUserPage() {
    return {
        user: null,
        allRoles: ['ViewDevices', 'Edit.Devices', 'Edit.DeviceGroups', 'Edit.DeviceTypes', 'Edit.DeviceAddrTypes'],
        selectedRoles: [],
        loading: true,
        saving: false,

        async init() {
            const id = this.getUserId();
            if (!id) return;
            await this.loadUser(id);
            this.loading = false;
        },

        getUserId() {
            const hash = window.location.hash;
            const match = hash.match(/edit-user\/(\d+)/);
            return match ? parseInt(match[1]) : null;
        },

        async loadUser(id) {
            try {
                this.user = await apiFetch(`/api/users/${id}`);
                if (this.user) {
                    this.selectedRoles = this.user.roles.filter(r => r !== 'SysAdmin');
                }
            } catch { }
        },

        toggleRole(role) {
            const idx = this.selectedRoles.indexOf(role);
            if (idx >= 0) {
                this.selectedRoles.splice(idx, 1);
            } else {
                this.selectedRoles.push(role);
            }
        },

        async save() {
            if (!this.user) return;
            this.saving = true;
            try {
                await apiFetch(`/api/users/${this.user.id}`, {
                    method: 'PUT',
                    body: JSON.stringify({
                        userName: this.user.userName,
                        newPassword: this.user.newPassword || null,
                        name: this.user.name,
                        isSysAdmin: this.user.isSysAdmin,
                        roles: this.user.isSysAdmin ? ['SysAdmin'] : this.selectedRoles
                    })
                });
                showToast(i18n.t('toast.saved'));
            } catch { }
            this.saving = false;
        },

        goBack() {
            window.location.hash = '#users';
        }
    };
}
