const i18n = {
    locale: 'en',
    translations: {},

    async load(locale) {
        try {
            const res = await fetch(`/lang/${locale}.json`);
            this.translations = await res.json();
            this.locale = locale;
        } catch (e) {
            console.error('Failed to load translations:', e);
        }
    },

    t(key) {
        return this.translations[key] || key;
    }
};
