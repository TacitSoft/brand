# Tacit.Brand

Marka Tacit dla serwisu Blazor: tokeny `--tacit-*`, fonty, style komponentów `.t-*`, skrypt motywu i komponent `<ThemeSwitch />`. Źródło: repo `TacitSoft/brand`, katalog `web/` (ADR-C-0026).

```xml
<PackageReference Include="Tacit.Brand" Version="0.1.*" />
```

```razor
@* _Imports.razor *@
@using Tacit.Brand
```

```razor
@* App.razor, <head>: najpierw skrypt motywu (blokująco), potem style *@
<script src="_content/Tacit.Brand/tacit-theme.js"></script>
<link rel="stylesheet" href="@Assets["_content/Tacit.Brand/tacit-fonts.css"]" />
<link rel="stylesheet" href="@Assets["_content/Tacit.Brand/tacit-tokens.css"]" />
<link rel="stylesheet" href="@Assets["_content/Tacit.Brand/tacit-site.css"]" />
```

Ikony są pod `_content/Tacit.Brand/icons/` (`favicon.ico`, `icon-rounded.svg`, `icon-rounded-180.png`, `-192`, `-512`).

Pełny wzór `App.razor` i `MainLayout.razor` (link „Przejdź do treści”, landmarki, przełącznik języka) jest w repo w `src/Tacit.Brand/App.razor.example`. Opis klas i zasad: `web/README.md`, dostępność: `web/DOSTEPNOSC.md`.
