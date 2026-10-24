// Sprawdza kontrast par kolorów z tacit-tokens.css w obu motywach według WCAG 2.2.
// Uruchom po każdej zmianie koloru:   node web/tools/check-contrast.mjs
// Kończy się kodem 1, gdy któraś para nie spełnia progu, więc nadaje się do CI.
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const css = readFileSync(join(dirname(fileURLToPath(import.meta.url)), '..', 'tacit-tokens.css'), 'utf8')
	.replace(/\/\*[\s\S]*?\*\//g, '');

// Bierzemy tylko główny blok :root z tokenami (ten z --tacit-brand-navy), bez wariantów @media.
const block = css.split(/(?=:root\s*\{)/).find((part) => part.includes('--tacit-brand-navy'));
const tokens = Object.fromEntries([...block.matchAll(/(--tacit-[\w-]+)\s*:\s*([^;]+);/g)].map((m) => [m[1], m[2].trim()]));

function resolve(value, mode, depth = 0) {
	if (depth > 20) throw new Error(`Cykl w tokenach: ${value}`);
	value = value.trim();
	const ld = value.match(/^light-dark\((.*)\)$/s);
	if (ld) {
		const [light, dark] = splitArgs(ld[1]);
		return resolve(mode === 'light' ? light : dark, mode, depth + 1);
	}
	const v = value.match(/^var\((--[\w-]+)\)$/);
	if (v) {
		if (!(v[1] in tokens)) throw new Error(`Brak tokenu ${v[1]}`);
		return resolve(tokens[v[1]], mode, depth + 1);
	}
	if (/^#[0-9a-f]{6}$/i.test(value)) return value.toUpperCase();
	throw new Error(`Nieobsługiwana wartość: ${value}`);
}

function splitArgs(s) {
	let level = 0;
	for (let i = 0; i < s.length; i++) {
		if (s[i] === '(') level++;
		else if (s[i] === ')') level--;
		else if (s[i] === ',' && level === 0) return [s.slice(0, i), s.slice(i + 1)];
	}
	throw new Error(`light-dark bez dwóch argumentów: ${s}`);
}

function luminance(hex) {
	const [r, g, b] = [1, 3, 5].map((i) => parseInt(hex.slice(i, i + 2), 16) / 255)
		.map((c) => (c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4));
	return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

function contrast(a, b) {
	const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
	return (hi + 0.05) / (lo + 0.05);
}

const TEXT = 4.5; // WCAG 1.4.3: zwykły tekst
const UI = 3; // WCAG 1.4.11: elementy interfejsu, fokus, ikony niosące informację

// [kolor pierwszego planu, tło, próg, opis]
const pairs = [
	['fg', 'bg', TEXT, 'tekst na tle strony'],
	['fg', 'surface', TEXT, 'tekst na karcie'],
	['fg', 'surface-sunken', TEXT, 'tekst na tle zagłębionym'],
	['fg-muted', 'bg', TEXT, 'tekst pomocniczy na tle strony'],
	['fg-muted', 'surface', TEXT, 'tekst pomocniczy na karcie'],
	['fg-muted', 'surface-sunken', TEXT, 'tekst pomocniczy na tle zagłębionym'],
	['link', 'bg', TEXT, 'link na tle strony'],
	['link', 'surface', TEXT, 'link na karcie'],
	['link-hover', 'surface', TEXT, 'link po najechaniu'],
	['on-primary', 'primary', TEXT, 'przycisk główny'],
	['on-primary', 'primary-hover', TEXT, 'przycisk główny po najechaniu'],
	['on-primary-soft', 'primary-soft', TEXT, 'tag, odznaka, zaznaczony filtr'],
	['accent-text', 'surface', TEXT, 'tekst akcentu na karcie'],
	['accent-text', 'accent-soft', TEXT, 'odznaka „Nowość”'],
	['success', 'surface', TEXT, 'tekst sukcesu'],
	['success', 'success-soft', TEXT, 'komunikat sukcesu'],
	['danger', 'surface', TEXT, 'komunikat błędu pod polem'],
	['danger', 'bg', TEXT, 'komunikat błędu na tle strony'],
	['danger', 'danger-soft', TEXT, 'komunikat błędu w ramce'],
	['fg', 'danger-soft', TEXT, 'tekst w ramce błędu'],
	['surface', 'danger', TEXT, 'przycisk niebezpieczny'],
	['warning', 'warning-soft', TEXT, 'komunikat ostrzeżenia'],
	['info', 'info-soft', TEXT, 'komunikat informacyjny'],
	['fg', 'info-soft', TEXT, 'tekst w komunikacie'],
	['code-fg', 'code-bg', TEXT, 'blok kodu'],
	['fg', 'code-inline-bg', TEXT, 'kod w tekście'],
	['fg', 'selection', TEXT, 'zaznaczony tekst'],
	['line-strong', 'surface', UI, 'ramka pola formularza na karcie'],
	['line-strong', 'bg', UI, 'ramka pola formularza na tle strony'],
	['focus', 'bg', UI, 'pierścień fokusu na tle strony'],
	['focus', 'surface', UI, 'pierścień fokusu na karcie'],
	['primary', 'bg', UI, 'krawędź przycisku głównego'],
	['danger', 'surface', UI, 'czerwona ramka błędnego pola'],
];

// Nagłówek jest ciemny w obu motywach (color-scheme: dark), więc liczymy go raz, w wariancie ciemnym.
const headerPairs = [
	['header-fg', 'header-bg', TEXT, 'nazwa i aktywny link w nagłówku'],
	['header-muted', 'header-bg', TEXT, 'linki w nagłówku'],
	['header-accent', 'header-bg', UI, 'znacznik aktywnej sekcji'],
	['focus', 'header-bg', UI, 'fokus w nagłówku'],
	['on-primary', 'primary', TEXT, 'wybrany przycisk przełącznika w nagłówku'],
	['primary', 'header-bg', UI, 'tło wybranego przycisku przełącznika na granacie'],
];

let failures = 0;
function check(mode, list, label = mode) {
	console.log(`\n${label}`);
	for (const [fg, bg, min, what] of list) {
		const a = resolve(`var(--tacit-color-${fg})`, mode);
		const b = resolve(`var(--tacit-color-${bg})`, mode);
		const ratio = contrast(a, b);
		const ok = ratio >= min;
		if (!ok) failures++;
		console.log(`${ok ? 'OK  ' : 'BŁĄD'} ${ratio.toFixed(2).padStart(5)}:1 (min ${min})  ${fg} na ${bg}  ${a}/${b}  ${what}`);
	}
}

check('light', pairs, 'Motyw jasny');
check('dark', pairs, 'Motyw ciemny');
check('dark', headerPairs, 'Nagłówek (oba motywy)');

console.log(failures ? `\n${failures} par poniżej progu WCAG.` : '\nWszystkie pary spełniają WCAG 2.2 AA.');
process.exitCode = failures ? 1 : 0;
