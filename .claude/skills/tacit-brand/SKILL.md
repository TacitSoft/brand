---
name: tacit-brand
description: How to build and review Blazor pages and layouts with Tacit.Brand — loading the brand assets in App.razor (tacit-theme.js, tacit-fonts.css, tacit-tokens.css, tacit-site.css, ThemeSwitch), the --tacit-* design tokens and the .t-* classes, the module area layout (.t-area, .t-area__nav) with TacitNav and TacitNavItem, feeding TacitNav from Tacit.Modulus.Blazor ModuleNavigation, and the module page components TacitTable, TacitField (with EditForm and field.Attributes), TacitValidationSummary, TacitButton (TacitButtonVariant, Busy, Href), TacitCallout (TacitTone, TacitLive), TacitBreadcrumbs (TacitBreadcrumb) and TacitEmpty. Covers the rule that hex colours live only in tacit-tokens.css and the TBRAND001 build warning, semantic tokens versus palette tokens, scoped *.razor.css of a module, the npm package @tacitsoft/brand for Starlight and DocFX, and what the markup must do for WCAG 2.2 AA (labels, aria-describedby, th scope, live regions, skip link, focus after navigation). Use it when writing or reviewing a Blazor layout, a module page or a form in a Tacit service or a library with UI, when a page needs a table, a form field, a validation summary, a button, a notice, breadcrumbs, an empty state or an area menu, when styling a component, when Bootstrap classes or a hard-coded colour appear, and when someone asks why TBRAND001 fires, why the module menu does not look like the rest of the app, why the skip link jumps to the home page, or why a validation summary does not move focus.
---
# Using Tacit.Brand

This skill covers **consuming** `Tacit.Brand`: hosts and libraries that render Tacit pages in Blazor.
- Component usage and parameters: [`src/Tacit.Brand/README.md`](../../../src/Tacit.Brand/README.md). The XML doc comments and the `.razor` files take precedence over this skill.
- Every `.t-*` class, the tokens, theming and distribution: [`web/README.md`](../../../web/README.md).
- Accessibility duties and how to test them: [`web/DOSTEPNOSC.md`](../../../web/DOSTEPNOSC.md).
- A full `App.razor`, `MainLayout.razor` and area layout: [`src/Tacit.Brand/App.razor.example`](../../../src/Tacit.Brand/App.razor.example). A running app with every component: `samples/Tacit.Brand.Sample`.
- Decisions: ADR-C-0026 (shared brand tokens, hex only in tokens) and ADR-C-0031 point 9 (module pages from several libraries look like one app) in `TacitSoft/adr`.

## Mental model

- **One source of values.** Colours, fonts, spacing, radii and sizes are CSS custom properties `--tacit-*` in `tacit-tokens.css`. Everything else uses `var(--tacit-…)`. Colours are `light-dark(…)` pairs, so dark mode costs no extra code.
- **One stylesheet of components.** `tacit-site.css` holds the reset, layout and the `.t-*` classes, inside `@layer tacit.*`, so a component's scoped CSS wins without `!important`.
- **Components are markup only.** The Razor components render `.t-*` markup with the right `aria-*` wiring. They have no styles or scripts of their own and work in static SSR and Interactive Server. Unmatched attributes (`id`, `class`, `aria-*`, `@onclick`) go to the root element; `class` is appended to the `.t-*` classes.
- **The host loads the brand once.** A module library ships only its own scoped `*.razor.css`; it never links the brand stylesheets or a second design system.
- Default texts are Polish. In a multilingual app pass texts from `IStringLocalizer` (`Label`, `EmptyText`, `Title`, `ThemeSwitch` labels).

## Setting up a host

```xml
<PackageReference Include="Tacit.Brand" Version="0.2.*" />
```

```razor
@* _Imports.razor *@
@using Tacit.Brand

@* App.razor, <head>: the theme script first and blocking (no flash), then fonts → tokens → site *@
<script src="_content/Tacit.Brand/tacit-theme.js"></script>
<link rel="stylesheet" href="@Assets["_content/Tacit.Brand/tacit-fonts.css"]" />
<link rel="stylesheet" href="@Assets["_content/Tacit.Brand/tacit-tokens.css"]" />
<link rel="stylesheet" href="@Assets["_content/Tacit.Brand/tacit-site.css"]" />
```

- Remove Bootstrap and `app.css` styling from the Blazor template. Two design systems mean two sources of truth.
- Icons are under `_content/Tacit.Brand/icons/`. `MapStaticAssets` and `@Assets[...]` fingerprint and compress the files; no bundler is needed.
- `MainLayout` follows `App.razor.example`: `.t-page`, a `.t-skip-link` first, `.t-header` with `<nav class="t-nav" aria-label="…">` and `<ThemeSwitch />`, `<main id="main" tabindex="-1" class="t-wrap t-main">`, `.t-footer`.
- The skip link's `href` is the **current path plus `#main`**, because with `<base href="/">` a bare `#main` goes to the home page. `App.razor.example` shows it with `NavigationManager`.
- `Routes.razor` has `<FocusOnNavigate RouteData="routeData" Selector="h1" />`, so a screen reader announces each page after enhanced navigation.
- Starlight and DocFX use the npm package `@tacitsoft/brand` with its adapters, not the NuGet package. Setup is in `web/README.md`.

## Which component for what

| Need | Use | Notes |
|---|---|---|
| A data table | `<TacitTable Items Label RowKey>` with `Header`, `Row`, `Empty` | `Label` names the scroll region. Column headers `<th scope="col">`, the naming cell `<th scope="row">`. Shows `.t-empty` when there are no items, also for `Items="null"`. |
| A form field | `<TacitField For Label Hint Required Context="field">` around any input with `@attributes="field.Attributes"` | Gives the control `id`, `aria-describedby`, `aria-required`, `aria-invalid`. `Context="field"` because `EditForm` takes `context`. Without `EditForm`: `Id` instead of `For`, error in `Error`. |
| The error list above a form | `<TacitValidationSummary />` instead of `<ValidationSummary />` | Links field errors to controls; takes focus after a failed interactive submit. |
| An action | `<TacitButton @onclick Variant>` | Renders `<button type="button">`. `Type="submit"` in a form. At most one `Primary` per view. `Danger` for irreversible actions. |
| Navigation styled as a button | `<TacitButton Href>` | Renders `<a>`. |
| A notice | `<TacitCallout Tone Title>` | Without `Title` it shows the tone's title, because the kind of message must not be colour only. |
| A status that changes (saved, N results) | `<TacitCallout Live="TacitLive.Polite">` rendered **always**, changing only its content | `Polite` → `role="status"`, `Assertive` → `role="alert"` (only for errors that stop work). |
| Breadcrumbs | `<TacitBreadcrumbs Items>` with `TacitBreadcrumb(Text, Href)` | The last item is the current page: no link, `aria-current="page"`. |
| An area menu | `<TacitNav class="t-area__nav" Label Items>` with `TacitNavItem` | Groups in order of first appearance, ungrouped items on top. Renders nothing without items. |
| No data | `<TacitEmpty>` | Say what to do next, with a link. |
| Anything else (cards, badges, chips, panels, facts) | Plain markup with the `.t-*` classes from `web/README.md` | Do not invent a parallel class or component. |

## Area layout and the module menu

A host's area layout (the one passed to `AddModuleBlazor(...)` in Tacit.Modulus.Blazor) puts the menu next to the content:

```razor
@inherits LayoutComponentBase
@layout MainLayout

<div class="t-area">
    <ModuleNavigation Area="backoffice" Context="groups">
        <TacitNav class="t-area__nav" Label="Backoffice"
                  Items="groups.SelectMany(g => g.Items).Select(i => new TacitNavItem(i.Title, i.Href, i.Group, i.Match))" />
    </ModuleNavigation>
    <div class="t-stack">@Body</div>
</div>
```

- `<ModuleNavigation>` **without child content** renders its own plain `<nav><ul>` markup without `.t-sidenav`, so module menus look different from the rest of the app. Always pass the visible items to `<TacitNav>` as above.
- `ModuleNavigation` decides visibility, authorization and order; `TacitNavItem` deliberately has no area, order or policy.
- Ordering can differ from `ModuleNavigation`'s own when an ungrouped item has a higher `Order` than grouped ones, because `TacitNav` puts ungrouped items on top.
- `.t-area` is two columns (15rem menu + content) from 60em **only when a `.t-area__nav` child is present**; otherwise the content takes the full width. Below 60em the menu sits above the content.
- `Label` must differ from the header navigation's label. Two area menus on one page need different `IdPrefix` values.
- Hand-written menus use the same `<TacitNav>` with a `TacitNavItem[]`. The area's home page uses `Match: NavLinkMatch.All`.

## Styling rules

- Colours, fonts and spacing come from `var(--tacit-*)`. **A hex colour is written only in `tacit-tokens.css`**, never in a component's `*.razor.css` or `*.cshtml.css`.
- Use **semantic** tokens (`--tacit-color-fg`, `--tacit-color-fg-muted`, `--tacit-color-primary`, `--tacit-color-surface`, `--tacit-color-line`, `--tacit-color-danger`, `--tacit-color-accent-text`…), not palette steps (`--tacit-navy-600`). Palette steps do not switch with the theme.
- The brand orange is decoration only in the light theme (2:1 on white). For accent-coloured text use `--tacit-color-accent-text`.
- Need a colour that has no token? It belongs in `tacit-tokens.css` in the `brand` repo, checked with `node web/tools/check-contrast.mjs` — not a local hex or `rgb(...)` in a component.
- Override a `.t-*` class from scoped CSS or an unlayered rule; no `!important` is needed because the brand styles are layered.

### TBRAND001

The package adds a build step (also transitively, through a module UI package) that warns **TBRAND001** for every hex colour in a declaration value in `*.razor.css` / `*.cshtml.css`, with file and line. Comments, quoted strings, `url(...)` and `#id` selectors are ignored.

- Fix it by replacing the hex with a token. Do not silence it by writing `rgb()`, `hsl()` or a named colour instead — that breaks the same rule.
- A library with UI should make it an error: `<WarningsAsErrors>$(WarningsAsErrors);TBRAND001</WarningsAsErrors>` in `Directory.Build.props`.
- `<TacitBrandHexCheck>false</TacitBrandHexCheck>` turns it off; only for projects that are not Tacit UI (for example a third-party theme you vendor).

## Common mistakes

1. **Rendering `<ModuleNavigation />` with no content** in a Tacit area layout. The menu loses `.t-sidenav` styling and grouping. Wrap `<TacitNav>` as shown above.
2. **Hard-coded colours** in scoped CSS, including `rgb()`, `hsl()` and named colours used to get around TBRAND001. Use a semantic token.
3. **Palette tokens** (`--tacit-navy-600`) in components. They do not follow the dark theme. Use semantic tokens.
4. **Loading the brand stylesheets, Bootstrap or another CSS framework from a module library.** The host loads the brand once; a module ships only scoped CSS.
5. **`<label>`/`<input>`/`<span class="validation-message">` written by hand** next to `EditForm` instead of `<TacitField>`. The hint and error are then not connected through `aria-describedby`, and `aria-invalid` is missing.
6. **Forgetting `@attributes="field.Attributes"`** on the control inside `<TacitField>`. The label's `for` then points at nothing.
7. **Adding the `required` attribute** to a control. It turns on browser validation next to `EditForm`. `Required` on `TacitField` gives the asterisk and `aria-required`; the rule comes from `[Required]` on the model.
8. **Two forms with the same model on one page** without `Id` on `TacitField`. The generated ids (`clientmodel-name`) collide.
9. **Using `<ValidationSummary />`.** It has no links to fields and does not take focus. Use `<TacitValidationSummary />`.
10. **Rendering a live `TacitCallout` conditionally** (`@if (saved) { <TacitCallout Live=…> }`). A live region must exist before its content changes, or screen readers say nothing. Render it always, or wrap it in an always-rendered `<div role="status">`.
11. **`<div @onclick>` or `<a @onclick>` for an action.** Use `<TacitButton>` (a `<button>`). Use `Href` only for navigation.
12. **Relying on `Busy` to stop double submits.** It sets `aria-busy` and a spinner but does not block clicks; ignore the second click in the handler.
13. **Repeated "Edit" links in table rows** without a hidden suffix. Add `<span class="t-visually-hidden"> @item.Name</span>` so links differ in a screen reader's link list (WCAG 2.4.4).
14. **A table with `<td>` headers or without `Label`.** Use `<th scope="col">` and `<th scope="row">`, and give `TacitTable` a `Label`.
15. **A bare `href="#main"` skip link** or `#id` links in an app with `<base href="/">`. They navigate to the home page. Prefix the current path, as `TacitValidationSummary` does for its field links.
16. **Icon-only buttons without `aria-label`**, and decorative images or logos next to their name without `alt=""`.
17. **Writing a new table, field or button component in a module library.** ADR-C-0031 point 9 wants module pages to look like one app; extend Tacit.Brand instead.

## Accessibility duties left to the page

The components cover labels, descriptions, live regions, landmarks and `aria-current`. The page author still owns:
- one `<h1>` per page and no skipped heading levels,
- a unique `<PageTitle>` in the form "Page name · Tacit",
- `lang` on the page and on fragments in another language,
- descriptive link texts,
- a different `aria-label` on every `<nav>`,
- `aria-pressed` on toggle buttons and filters.

Check new pages by keyboard only, at 200% zoom and 320 px width, with axe (zero serious/critical findings), and with a screen reader — steps in `web/DOSTEPNOSC.md`.

## Scope

Out of scope of the package: WebAssembly/Auto render modes are not tested; there are no icon, modal, tab, toast or date-picker components — build those from `.t-*` classes or propose them for Tacit.Brand. Removing or renaming a token, a `.t-*` class, a component or a parameter is a major version of the packages.

## Review checklist

- [ ] The host loads `tacit-theme.js` (blocking, first) and then fonts → tokens → site, through `@Assets[...]`; no Bootstrap or second design system.
- [ ] Module libraries ship only scoped `*.razor.css` and link no global stylesheet.
- [ ] No hex, `rgb()`, `hsl()` or named colour in scoped CSS; only semantic `var(--tacit-color-…)` tokens. TBRAND001 is an error in libraries with UI.
- [ ] Area layouts use `.t-area` with `<TacitNav class="t-area__nav">`; module menus pass `<ModuleNavigation Context="groups">` items to `<TacitNav>`.
- [ ] Forms use `<TacitField>` with `field.Attributes` on the control and `<TacitValidationSummary />`; no `required` attribute; `Id` given when a model appears twice.
- [ ] Tables use `<TacitTable>` with `Label`, `RowKey`, `<th scope>` cells and an `Empty` text that says what to do next.
- [ ] Actions are `<TacitButton>` (`<button>`), navigation is `Href` (`<a>`); at most one `Primary` per view; handlers guard against double clicks.
- [ ] Changing statuses are always-rendered live regions; `Assertive` only for blocking errors.
- [ ] The skip link and in-page links include the current path; `FocusOnNavigate` targets `h1`.
- [ ] Each page has one `<h1>`, a unique `<PageTitle>`, distinct `<nav>` labels, and passes the keyboard, zoom and axe checks in `web/DOSTEPNOSC.md`.
