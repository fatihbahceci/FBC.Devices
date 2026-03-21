// ===== API Fetch Utility =====
async function apiFetch(url, options = {}) {
    const token = localStorage.getItem('jwt_token');
    const headers = { 'Content-Type': 'application/json', ...options.headers };
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const res = await fetch(url, { ...options, headers });

    if (res.status === 401) {
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('user_info');
        Alpine.store('app').logout();
        return null;
    }

    if (!res.ok) {
        const text = await res.text();
        showToast(text || i18n.t('toast.error'), 'danger');
        throw new Error(text);
    }

    return res.status === 204 ? null : await res.json();
}

// ===== Toast Notification =====
function showToast(message, type = 'success') {
    const id = 'toast-' + Date.now();
    const html = `
        <div id="${id}" class="toast align-items-center text-bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`;
    document.getElementById('toast-container').insertAdjacentHTML('beforeend', html);
    const toast = new bootstrap.Toast(document.getElementById(id), { delay: 3000 });
    toast.show();
    setTimeout(() => document.getElementById(id)?.remove(), 4000);
}

// ===== Alpine Store =====
document.addEventListener('alpine:init', () => {
    Alpine.store('app', {
        page: 'home',
        user: null,
        sidebarOpen: true,
        darkMode: false,

        init() {
            const userInfo = localStorage.getItem('user_info');
            if (userInfo) {
                this.user = JSON.parse(userInfo);
            }
            // Restore theme preference
            const savedTheme = localStorage.getItem('theme');
            if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
                this.darkMode = true;
                document.documentElement.setAttribute('data-bs-theme', 'dark');
            }
            this.handleHash();
            window.addEventListener('hashchange', () => this.handleHash());
        },

        get isAuthenticated() {
            return this.user !== null;
        },

        hasRole(role) {
            if (!this.user) return false;
            if (this.user.isSysAdmin) return true;
            return this.user.roles?.includes(role) ?? false;
        },

        handleHash() {
            const hash = window.location.hash.slice(1) || 'home';
            const [page] = hash.split('/');
            this.page = page;

            if (!this.isAuthenticated && page !== 'login') {
                window.location.hash = '#login';
                return;
            }
        },

        navigate(page) {
            window.location.hash = '#' + page;
        },

        async login(username, password) {
            try {
                const res = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ username, password })
                });
                if (!res.ok) return false;
                const data = await res.json();
                localStorage.setItem('jwt_token', data.token);
                localStorage.setItem('user_info', JSON.stringify({
                    userName: data.userName,
                    name: data.name,
                    isSysAdmin: data.isSysAdmin,
                    roles: data.roles
                }));
                this.user = JSON.parse(localStorage.getItem('user_info'));
                window.location.hash = '#home';
                return true;
            } catch {
                return false;
            }
        },

        logout() {
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('user_info');
            this.user = null;
            window.location.hash = '#login';
        },

        toggleSidebar() {
            this.sidebarOpen = !this.sidebarOpen;
        },

        toggleTheme() {
            this.darkMode = !this.darkMode;
            document.documentElement.setAttribute('data-bs-theme', this.darkMode ? 'dark' : 'light');
            localStorage.setItem('theme', this.darkMode ? 'dark' : 'light');
        }
    });
});

// Load i18n on startup
(async () => {
    await i18n.load('en');
})();
