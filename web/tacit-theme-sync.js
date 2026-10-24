/*
 * Synchronizacja motywu z dokumentacji (Starlight, DocFX) do serwisu Blazor.
 *
 * Starlight i DocFX mają własne przełączniki motywu i własne klucze w localStorage. Ten skrypt patrzy,
 * kiedy narzędzie zmienia motyw na <html>, i przepisuje jego wybór do kluczy pozostałych narzędzi,
 * tak jak robi to tacit-theme.js w serwisie. Dzięki temu motyw wybrany w /docs/ obowiązuje też w / i /api/.
 *
 * Starlight: dodaj jako skrypt w `head` w astro.config.mjs (patrz adapters/starlight.css, nagłówek pliku).
 * DocFX:     dołącz w template/public/main.js przez `import './tacit-theme-sync.js'`.
 */
(() => {
	const root = document.documentElement;

	function storage(fn) {
		try {
			return fn(window.localStorage);
		} catch {
			return null;
		}
	}

	// Odczytuje wybór z klucza narzędzia, w którym jesteśmy. "system" = auto.
	function currentPreference() {
		return storage((s) => {
			if (root.hasAttribute('data-bs-theme')) {
				const v = s.getItem('theme');
				return v === 'light' || v === 'dark' ? v : 'system';
			}
			const v = s.getItem('starlight-theme');
			return v === 'light' || v === 'dark' ? v : 'system';
		});
	}

	function mirror() {
		const pref = currentPreference();
		if (!pref) return;
		storage((s) => {
			s.setItem('tacit-theme', pref);
			if (pref === 'system') s.removeItem('starlight-theme');
			else s.setItem('starlight-theme', pref);
			s.setItem('theme', pref === 'system' ? 'auto' : pref);
		});
	}

	// Obie biblioteki przy każdej zmianie wołają setAttribute na <html>, także gdy wartość się nie zmienia
	// (np. „auto” przy systemowym ciemnym), a MutationObserver widzi każde takie wywołanie.
	new MutationObserver(mirror).observe(root, { attributes: true, attributeFilter: ['data-theme', 'data-bs-theme'] });
})();
