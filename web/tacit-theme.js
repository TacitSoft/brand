/*
 * Przełącznik motywu Tacit (serwis Blazor).
 *
 * Dołącz w <head> jako zwykły, blokujący skrypt, PRZED arkuszami stylów:
 *     <script src="tacit-theme.js"></script>
 * Musi się wykonać przed pierwszym malowaniem strony, inaczej przy wybranym motywie innym niż systemowy
 * strona mignie w złych kolorach. Nie dawaj mu defer ani async.
 *
 * Wybór: "light" | "dark" | "system". "system" usuwa atrybut data-theme i zostawia decyzję CSS-owi
 * (prefers-color-scheme), więc bez JS strona też ma poprawny motyw, tylko bez ręcznego przełącznika.
 *
 * Serwis, przewodniki Starlight (/docs/) i referencja DocFX (/api/) leżą w jednej domenie (ADR-C-0026, pkt 4),
 * więc dzielą localStorage. Wybór zapisujemy pod kluczami wszystkich trzech narzędzi, żeby użytkownik
 * ustawiał motyw raz. W drugą stronę (zmiana w dokumentacji → serwis) działa tacit-theme-sync.js.
 *
 * Znacznik przełącznika (renderuje go komponent ThemeSwitch.razor z paczki Tacit.Brand):
 *     <div class="t-segmented t-theme-switch" role="group" aria-label="Motyw" data-tacit-theme-switch hidden>
 *       <button type="button" data-tacit-theme="light" aria-pressed="false">…Jasny</button>
 *       <button type="button" data-tacit-theme="dark" aria-pressed="false">…Ciemny</button>
 *       <button type="button" data-tacit-theme="system" aria-pressed="false">…Systemowy</button>
 *     </div>
 * Przełącznik ma atrybut hidden i pokazuje go dopiero ten skrypt, żeby bez JS nie było martwych przycisków.
 */
(() => {
	const KEYS = {
		tacit: 'tacit-theme',          // "light" | "dark" | "system"
		starlight: 'starlight-theme',  // "light" | "dark" | brak klucza = auto
		docfx: 'theme',                // "light" | "dark" | "auto"
	};
	const VALID = ['light', 'dark', 'system'];
	const root = document.documentElement;

	function storage(fn) {
		try {
			return fn(window.localStorage);
		} catch {
			return null; // tryb prywatny, zablokowane ciasteczka: działamy bez zapamiętywania
		}
	}

	function read() {
		return storage((s) => {
			const own = s.getItem(KEYS.tacit);
			if (VALID.includes(own)) return own;
			// Użytkownik mógł wybrać motyw najpierw w dokumentacji.
			const sl = s.getItem(KEYS.starlight);
			if (sl === 'light' || sl === 'dark') return sl;
			const dfx = s.getItem(KEYS.docfx);
			if (dfx === 'light' || dfx === 'dark') return dfx;
			return null;
		}) ?? 'system';
	}

	function write(pref) {
		storage((s) => {
			s.setItem(KEYS.tacit, pref);
			if (pref === 'system') s.removeItem(KEYS.starlight);
			else s.setItem(KEYS.starlight, pref);
			s.setItem(KEYS.docfx, pref === 'system' ? 'auto' : pref);
		});
	}

	function apply(pref) {
		if (pref === 'light' || pref === 'dark') root.setAttribute('data-theme', pref);
		else root.removeAttribute('data-theme');
		syncSwitches(pref);
	}

	function syncSwitches(pref = read()) {
		for (const sw of document.querySelectorAll('[data-tacit-theme-switch]')) {
			sw.hidden = false;
			for (const btn of sw.querySelectorAll('[data-tacit-theme]')) {
				btn.setAttribute('aria-pressed', String(btn.dataset.tacitTheme === pref));
			}
		}
	}

	// Natychmiast, jeszcze w <head>: ustawia atrybut przed malowaniem.
	apply(read());

	// Delegacja na document: działa też dla przełącznika dorenderowanego później (enhanced navigation).
	document.addEventListener('click', (event) => {
		const btn = event.target.closest?.('[data-tacit-theme]');
		if (!btn || !VALID.includes(btn.dataset.tacitTheme)) return;
		write(btn.dataset.tacitTheme);
		apply(btn.dataset.tacitTheme);
	});

	// Zmiana w innej karcie tej samej domeny (np. w otwartej dokumentacji).
	window.addEventListener('storage', (event) => {
		if (Object.values(KEYS).includes(event.key)) apply(read());
	});

	function afterLoad() {
		syncSwitches();
		// Blazor enhanced navigation podmienia DOM bez przeładowania strony: przywróć stan przycisków
		// i atrybut na <html> (synchronizacja DOM może go nadpisać wartością z serwera).
		window.Blazor?.addEventListener?.('enhancedload', () => apply(read()));
	}

	if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', afterLoad, { once: true });
	else afterLoad();
})();
