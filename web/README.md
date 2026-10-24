# Style WWW Tacit

Wspólne style serwisu Blazor, przewodników Starlight i referencji API DocFX ([ADR-C-0026](../../adr/docs/adr/ADR-C-0026-dokumentacja-przewodniki-w-starlight-5-jezykow-referencja-api-z-docfx-wspolne-tokeny-marki.md), pkt 3). Wygląd wywodzi się z prototypu katalogu Tacit, z kolorami marki zamiast prototypowej zieleni.

Podgląd wszystkich komponentów w obu motywach: `preview.html`, opublikowany pod https://claude.ai/artifact/VbkkVaRMQ4FVmSFH6iPQ1b. Plik nie ma własnego `<!doctype>`, bo dokłada go publikacja.

## Pliki

| Plik | Co to jest | Kto używa |
|---|---|---|
| `tacit-tokens.css` | **Jedyne źródło wartości**: kolory (jasne i ciemne przez `light-dark()`), fonty, skala tekstu, odstępy, promienie, wysokość nagłówka, rozmiary celów | wszyscy |
| `tacit-fonts.css` + `fonts/` | `@font-face` dla rodzin „Tacit Display”, „Tacit Sans” i „Tacit Mono”, hostowanych lokalnie | wszyscy |
| `tacit-site.css` | Reset, typografia, układ i komponenty `.t-*` | Blazor |
| `tacit-theme.js` | Przełącznik jasny/ciemny/systemowy, bez mignięcia przy ładowaniu, ze wspólnym zapisem dla serwisu i dokumentacji | Blazor |
| `tacit-theme-sync.js` | Przenosi wybór motywu z dokumentacji do serwisu | Starlight, DocFX |
| `adapters/starlight.css` | `--tacit-*` → `--sl-*`, granatowy nagłówek, fonty nagłówków | Starlight |
| `adapters/docfx.css` | `--tacit-*` → zmienne Bootstrap 5, granatowy nagłówek | DocFX modern |
| `package.json` | Definicja paczki npm `@tacitsoft/brand` | Starlight, DocFX |
| `../src/Tacit.Brand/` | Paczka NuGet `Tacit.Brand` (Razor Class Library): pliki z `web/` jako zasoby statyczne, komponent `ThemeSwitch.razor`, wzór `App.razor.example` z `App.razor` i `MainLayout.razor` | Blazor |
| `tools/check-contrast.mjs` | Sprawdza kontrast WCAG wszystkich par tokenów w obu motywach | CI |
| `DOSTEPNOSC.md` | Wymagania dostępności: przepisy, co dają style, co musi zrobić znacznik, jak testować | wszyscy |

Kolejność dołączania jest wszędzie ta sama: **fonty → tokeny → style lub adapter**.

## Zasady

- Kolor, font i odstęp zawsze bierzemy z `var(--tacit-*)`. Hex wpisujemy tylko w `tacit-tokens.css` (ADR-C-0026). Dotyczy to też `*.razor.css`.
- Komponenty używają **tokenów semantycznych** (`--tacit-color-fg`, `--tacit-color-primary`…), nie skal (`--tacit-navy-600`). Dzięki temu motyw ciemny działa bez dodatkowego kodu.
- Zmiana koloru w tokenach wymaga przejścia `node web/tools/check-contrast.mjs`.
- Pomarańcz marki to w jasnym motywie wyłącznie ozdoba, bo na bieli ma kontrast 2:1. Jako kolor tekstu służy `--tacit-color-accent-text`.
- Style `tacit-site.css` są w warstwach `@layer tacit.*`, więc style izolowane komponentów wygrywają bez `!important`.

## Motyw jasny i ciemny

- Domyślnie motyw idzie za systemem (`prefers-color-scheme`). Działa to bez JS.
- Wybór użytkownika ustawia `data-theme="light|dark"` na `<html>`. DocFX używa `data-bs-theme` i tokeny rozumieją oba atrybuty.
- Tokeny kolorów mają postać `light-dark(jasny, ciemny)`, więc wystarczy zmienić `color-scheme`. Nagłówek ma `color-scheme: dark` i w obu motywach jest granatowy, tak samo w serwisie, w Starlight i w DocFX.
- Serwis i dokumentacja leżą w jednej domenie, więc wybór zapisujemy pod kluczami wszystkich trzech narzędzi: `tacit-theme`, `starlight-theme` i `theme` (DocFX). Motyw ustawiony w jednym miejscu obowiązuje we wszystkich. Nazwy kluczy Starlight i DocFX to szczegół implementacyjny tych narzędzi, więc sprawdzaj je przy ich aktualizacji.
- `light-dark()` wymaga przeglądarek z 2024 r. (Chrome/Edge 123, Firefox 120, Safari 17.5).

## Fonty

| Rodzina | Łacina | Cyrylica |
|---|---|---|
| Tacit Display (nagłówki) | Bricolage Grotesque | Onest |
| Tacit Sans (tekst) | Public Sans | Onest |
| Tacit Mono (kod) | JetBrains Mono | JetBrains Mono |

Bricolage Grotesque i Public Sans z prototypu nie mają cyrylicy, a przewodniki są także po rosyjsku. Cyrylicę dostarcza więc Onest przez `unicode-range`, a przeglądarka pobiera tylko potrzebne pliki. Wszystkie fonty są na licencji SIL OFL 1.1 (`fonts/OFL-*.txt`) i są hostowane lokalnie, bez Google Fonts: nie przekazujemy adresów IP odwiedzających podmiotowi trzeciemu i nie zależymy od zewnętrznego CDN.

## Dystrybucja

Z `web/` powstają dwie paczki o tej samej wersji, publikowane w **GitHub Packages** organizacji TacitSoft:

| Paczka | Feed | Dla kogo | Zawartość |
|---|---|---|---|
| npm `@tacitsoft/brand` | `https://npm.pkg.github.com` | Starlight, DocFX | Wszystkie pliki CSS i JS, adaptery, fonty |
| NuGet `Tacit.Brand` | `https://nuget.pkg.github.com/TacitSoft/index.json` | Serwis Blazor | Tokeny, fonty, `tacit-site.css`, `tacit-theme.js`, ikony, komponent `<ThemeSwitch />` |

Pliki leżą tylko w `web/`. Projekt `src/Tacit.Brand` ich nie kopiuje, tylko podlinkowuje jako static web assets.

**Wydanie nowej wersji**: `git tag v0.2.0 && git push origin v0.2.0`. Workflow `.github/workflows/release.yml` sprawdza kontrast, buduje obie paczki i publikuje je. Wersja pochodzi z tagu (ADR-C-0003), a publikacja jest tylko po tagu (ADR-C-0008). Każdy push i PR też buduje paczki i zostawia je jako artefakt workflow, ale ich nie publikuje.

Semver: zmiana wartości tokenu to wersja patch albo minor. Usunięcie lub zmiana nazwy tokenu, klasy `.t-*` albo pliku to wersja major, bo psuje style odbiorców.

### Dostęp do paczek

GitHub Packages wymaga uwierzytelnienia przy pobieraniu paczek npm i NuGet, także publicznych.

- **Lokalnie**: klasyczny Personal Access Token z uprawnieniem `read:packages`. Tokeny fine-grained nie działają z feedami npm i NuGet. Token trzymaj w zmiennej środowiskowej, a nie w repo.
- **W Actions innego repo**: w ustawieniach **każdej z paczek** (Package settings → Manage Actions access) dodaj to repo z rolą Read. Wtedy wystarczy `GITHUB_TOKEN` i `permissions: packages: read` w workflow.

npm, plik `.npmrc` w repo odbiorcy:

```ini
@tacitsoft:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${NODE_AUTH_TOKEN}
```

NuGet, plik `nuget.config` w repo odbiorcy:

```xml
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="tacitsoft" value="https://nuget.pkg.github.com/TacitSoft/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
    <packageSource key="tacitsoft"><package pattern="Tacit.*" /></packageSource>
  </packageSourceMapping>
</configuration>
```

Hasło do źródła dodaj poza repo: `dotnet nuget update source tacitsoft --username <login> --password <PAT> --store-password-in-clear-text`. W CI zrób to samo z `GITHUB_TOKEN`.

## Podłączenie

### Blazor

```xml
<PackageReference Include="Tacit.Brand" Version="0.1.*" />
```

W `_Imports.razor` dodaj `@using Tacit.Brand`. Pliki są pod `_content/Tacit.Brand/…`, a ikony pod `_content/Tacit.Brand/icons/`. `MapStaticAssets` i `@Assets[...]` dają im kompresję i fingerprinting, więc bundler nie jest potrzebny. Wzór `App.razor` i layoutu jest w `src/Tacit.Brand/App.razor.example`.

Usuń Bootstrapa z szablonu Blazora. Klasy `.t-*` z nim nie kolidują, ale dwa systemy stylów to dwa źródła prawdy.

### Starlight

Paczka trafia do bundla Vite: Vite rozwiązuje `url(fonts/…)` z `node_modules`, dopisuje hash i kopiuje fonty do wyniku.

```js
// astro.config.mjs
import { readFileSync } from 'node:fs';

const themeSync = readFileSync(new URL(import.meta.resolve('@tacitsoft/brand/tacit-theme-sync.js')), 'utf8');

starlight({
  customCss: [
    '@tacitsoft/brand/tacit-fonts.css',
    '@tacitsoft/brand/tacit-tokens.css',
    '@tacitsoft/brand/adapters/starlight.css',
  ],
  head: [{ tag: 'script', content: themeSync }],
})
```

Expressive Code (bloki kodu) ma osobne motywy kolorystyczne, które ustawia się w `expressiveCode.themes`.

### DocFX (szablon modern)

DocFX nie ma bundlera, więc pliki z paczki npm kopiuje do szablonu skrypt uruchamiany przed `docfx build`:

```js
// scripts/copy-brand.mjs
import { cpSync } from 'node:fs';

const brand = new URL('.', import.meta.resolve('@tacitsoft/brand/package.json'));
const target = new URL('../docfx/template/public/', import.meta.url);
for (const f of ['tacit-fonts.css', 'tacit-tokens.css', 'tacit-theme-sync.js', 'fonts/']) {
  cpSync(new URL(f, brand), new URL(f, target), { recursive: true });
}
cpSync(new URL('adapters/docfx.css', brand), new URL('docfx.css', target));
```

Skopiowane pliki dopisz do `.gitignore`. Potem:

```css
/* template/public/main.css */
@import url("tacit-fonts.css");
@import url("tacit-tokens.css");
@import url("docfx.css");
```

```js
// template/public/main.js
import './tacit-theme-sync.js';
```

## Komponenty (`tacit-site.css`)

| Klasa | Zastosowanie |
|---|---|
| `.t-page`, `.t-wrap`, `.t-main` | Szkielet strony, kontener szerokości, główny obszar |
| `.t-skip-link` | „Przejdź do treści” |
| `.t-header`, `.t-header__inner`, `.t-header__end`, `.t-brand`, `.t-nav` | Granatowy nagłówek z nawigacją (`aria-current="page"` albo `.active` z `NavLink`) |
| `.t-segmented` | Przełącznik segmentowy: motyw (`aria-pressed`) i język (`aria-current`) |
| `.t-btn` + `--primary`, `--ghost`, `--danger`, `--sm`, `--block` | Przyciski. Stan ładowania przez `aria-busy="true"` |
| `.t-hero`, `.t-lead`, `.t-eyebrow` | Nagłówek strony głównej, lead, etykieta z pomarańczowym znacznikiem |
| `.t-grid`, `.t-split`, `.t-stack`, `.t-cluster`, `.t-prose` | Układy: siatka kart 1→2→3, treść z boczną kolumną, odstępy, szpalta czytelnicza |
| `.t-card` + `__head`, `__link`, `__foot` | Karta. Link tylko w tytule, cała karta klikalna |
| `.t-panel`, `.t-facts`, `.t-links`, `.t-kicker` | Panele boczne, lista klucz–wartość, lista powiązań, etykieta wersalikami |
| `.t-badge` + `--accent`, `--success`, `--danger`, `.t-tags` | Odznaki i tagi |
| `.t-chips`, `.t-chip`, `.t-chip__count` | Filtry z `aria-pressed` |
| `.t-field`, `.t-label`, `.t-hint`, `.t-error`, `.t-input`, `.t-select`, `.t-textarea`, `.t-check`, `.t-search` | Formularze. Obsługują też klasy z `EditForm` (`.invalid`, `.validation-message`, `.validation-errors`) |
| `.t-callout` + `--success`, `--warning`, `--danger` | Komunikaty |
| `.t-table` | Tabela w ramce, przewijana na wąskich ekranach |
| `.t-command` | Polecenie do skopiowania (`dotnet add package …`) |
| `.t-breadcrumbs`, `.t-empty`, `.t-footer` | Okruszki, stan pusty, stopka |
| `.t-visually-hidden`, `.t-muted`, `.t-mono`, `.t-small`, `.t-nowrap` | Narzędzia |
