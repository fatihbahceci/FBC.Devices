function loginPage() {
    return {
        username: '',
        password: '',
        error: '',
        loading: false,

        async submit() {
            this.loading = true;
            this.error = '';
            const success = await Alpine.store('app').login(this.username, this.password);
            if (!success) {
                this.error = i18n.t('login.error');
            }
            this.loading = false;
        }
    };
}
