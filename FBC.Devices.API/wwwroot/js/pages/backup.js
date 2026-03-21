function backupPage() {
    return {
        backups: [],
        loading: false,
        creating: false,
        selectedBackups: [],

        async init() {
            await this.loadBackups();
        },

        async loadBackups() {
            this.loading = true;
            try {
                this.backups = await apiFetch('/api/backups') || [];
                this.selectedBackups = [];
            } catch { }
            this.loading = false;
        },

        async createBackup() {
            this.creating = true;
            try {
                await apiFetch('/api/backups/create', { method: 'POST' });
                showToast('Backup created successfully');
                await this.loadBackups();
            } catch { }
            this.creating = false;
        },

        downloadBackup(fileName) {
            const token = localStorage.getItem('jwt_token');
            const a = document.createElement('a');
            a.href = `/api/backups/download/${encodeURIComponent(fileName)}`;
            a.download = fileName;
            // For auth, we open in new tab (cookie not needed, direct file endpoint)
            window.open(`/api/backups/download/${encodeURIComponent(fileName)}`, '_blank');
        },

        toggleSelect(fileName) {
            const idx = this.selectedBackups.indexOf(fileName);
            if (idx >= 0) this.selectedBackups.splice(idx, 1);
            else this.selectedBackups.push(fileName);
        },

        toggleSelectAll() {
            if (this.selectedBackups.length === this.backups.length) {
                this.selectedBackups = [];
            } else {
                this.selectedBackups = this.backups.map(b => b.fileName);
            }
        },

        get allSelected() {
            return this.backups.length > 0 && this.selectedBackups.length === this.backups.length;
        },

        async deleteSelected() {
            if (this.selectedBackups.length === 0) return;
            if (!confirm(`Are you sure you want to delete ${this.selectedBackups.length} backup(s)?`)) return;
            try {
                await apiFetch('/api/backups/delete', {
                    method: 'POST',
                    body: JSON.stringify({ fileNames: this.selectedBackups })
                });
                showToast(`${this.selectedBackups.length} backup(s) deleted`);
                await this.loadBackups();
            } catch { }
        },

        formatSize(bytes) {
            if (bytes < 1024) return bytes + ' B';
            if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
            return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
        },

        formatDate(dateStr) {
            return new Date(dateStr).toLocaleString();
        }
    };
}
