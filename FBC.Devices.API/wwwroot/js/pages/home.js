function homePage() {
    return {
        devices: [],
        searchFilter: '',
        selectedFields: [],
        criteria: [],
        loading: false,
        skip: 0,
        take: 5,
        totalCount: 0,
        pingStatuses: {},

        async init() {
            await this.loadCriteria();
            await this.search();
            await this.loadPingStatuses();
        },

        async loadCriteria() {
            try {
                this.criteria = await apiFetch('/api/search/criteria') || [];
                // Default: select key fields (Name, Description, SerialNumber, Note, Addr)
                this.selectedFields = this.criteria
                    .filter(c => ['Name', 'Description', 'SerialNumber', 'Note', 'Addr'].includes(c.fieldName))
                    .map(c => c.index);
            } catch { }
        },

        async search() {
            this.loading = true;
            try {
                const fields = this.selectedFields.join(',');
                const params = new URLSearchParams();
                if (this.searchFilter) params.set('filter', this.searchFilter);
                if (fields) params.set('fields', fields);
                params.set('skip', this.skip);
                params.set('take', this.take);
                const result = await apiFetch(`/api/search/devices?${params}`);
                if (result) {
                    this.devices = result.items || [];
                    this.totalCount = result.totalCount || 0;
                }
            } catch { }
            this.loading = false;
        },

        async loadPingStatuses() {
            try {
                this.pingStatuses = await apiFetch('/api/status/ping-results') || {};
            } catch { }
        },

        getAddrStatus(addrId) {
            return this.pingStatuses[addrId] || null;
        },

        getStatusBadgeClass(addrId, periodicPingCheck) {
            if (!periodicPingCheck) return 'badge-ping-default';
            const status = this.getAddrStatus(addrId);
            if (!status) return 'badge-ping-default';
            return status.isSuccess ? 'badge-ping-success' : 'badge-ping-fail';
        },

        getStatusText(addrId, periodicPingCheck) {
            if (!periodicPingCheck) return i18n.t('status.disabled');
            const status = this.getAddrStatus(addrId);
            if (!status) return i18n.t('status.disabled');
            return status.isSuccess ? i18n.t('status.active') : i18n.t('status.passive');
        },

        async ping(address) {
            try {
                const result = await apiFetch('/api/status/ping', {
                    method: 'POST',
                    body: JSON.stringify({ address })
                });
                if (result) {
                    showToast(`Ping ${address}: ${result.status} (${result.roundtripTime}ms)`,
                        result.success ? 'success' : 'warning');
                }
            } catch { }
        },

        copyToClipboard(text) {
            navigator.clipboard.writeText(text);
            showToast('Copied to clipboard', 'info');
        },

        get totalPages() {
            return Math.ceil(this.totalCount / this.take);
        },

        get currentPage() {
            return Math.floor(this.skip / this.take) + 1;
        },

        get pageNumbers() {
            const pages = [];
            const total = this.totalPages;
            const current = this.currentPage;
            let start = Math.max(1, current - 2);
            let end = Math.min(total, start + 4);
            start = Math.max(1, end - 4);
            for (let i = start; i <= end; i++) pages.push(i);
            return pages;
        },

        get pagingSummary() {
            if (this.totalCount === 0) return 'No records found';
            return `Displaying page ${this.currentPage} of ${this.totalPages} (total ${this.totalCount} records)`;
        },

        async goToPage(page) {
            this.skip = (page - 1) * this.take;
            await this.search();
        },

        async nextPage() {
            if (this.currentPage < this.totalPages) {
                this.skip += this.take;
                await this.search();
            }
        },

        async prevPage() {
            if (this.skip > 0) {
                this.skip -= this.take;
                await this.search();
            }
        }
    };
}
