# Tacit.Brand

Marka Tacit dla serwisu Blazor: tokeny `--tacit-*`, fonty, style komponentów `.t-*`, skrypt motywu, `<ThemeSwitch />` i wspólne komponenty stron modułów. Źródło: repo `TacitSoft/brand`, katalog `web/` (ADR-C-0026). Komponenty dla stron modułów wynikają z ADR-C-0031 pkt 9: strony z kilku bibliotek mają wyglądać jak jedna aplikacja.

```xml
<PackageReference Include="Tacit.Brand" Version="0.2.*" />
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

Pełny wzór `App.razor`, `MainLayout.razor` i układu obszaru modułów jest w repo w `src/Tacit.Brand/App.razor.example`. Opis klas i zasad: `web/README.md`, dostępność: `web/DOSTEPNOSC.md`. Wszystkie komponenty w działaniu pokazuje aplikacja `samples/Tacit.Brand.Sample` w repo.

## Komponenty

Komponenty renderują sam znacznik z klasami `.t-*` i nie mają własnych stylów ani skryptów. Działają w statycznym SSR i w Interactive Server. Atrybuty spoza parametrów (`class`, `id`, `@onclick`, `aria-*`) przechodzą na główny element, a `class` jest doklejana do klas `.t-*`. Domyślne teksty są po polsku. W aplikacji wielojęzycznej przekazuj je z `IStringLocalizer`.

| Komponent | Klasy | Do czego |
|---|---|---|
| `<TacitTable>` | `.t-table`, `.t-empty` | Tabela danych przewijana w poziomie i osiągalna klawiaturą, ze stanem pustym |
| `<TacitField>` | `.t-field`, `.t-label`, `.t-hint`, `.t-error` | Pole formularza: etykieta, podpowiedź i błąd połączone z kontrolką, zgodne z `EditForm` |
| `<TacitValidationSummary>` | `.validation-errors` | Podsumowanie błędów z linkami do pól, z fokusem po nieudanym wysłaniu |
| `<TacitButton>` | `.t-btn` | Przycisk (`<button>`) albo link w wyglądzie przycisku (`<a>`) |
| `<TacitCallout>` | `.t-callout` | Komunikat, opcjonalnie region na żywo |
| `<TacitBreadcrumbs>` | `.t-breadcrumbs` | Okruszki z bieżącą stroną |
| `<TacitNav>` | `.t-sidenav` | Menu obszaru modułów z grupami |
| `<TacitEmpty>` | `.t-empty` | Stan pusty |

### Formularz

```razor
<EditForm Model="model" FormName="client" OnValidSubmit="Save">
    <DataAnnotationsValidator />
    <TacitValidationSummary />

    <TacitField For="() => model.Name" Label="Nazwa" Hint="Widoczna na ekranie zgody." Required Context="field">
        <InputText @bind-Value="model.Name" class="t-input" @attributes="field.Attributes" />
    </TacitField>

    <TacitField For="() => model.Type" Label="Rodzaj klienta" Context="field">
        <InputSelect @bind-Value="model.Type" class="t-select" @attributes="field.Attributes">…</InputSelect>
    </TacitField>

    <TacitButton Type="submit" Variant="TacitButtonVariant.Primary" Busy="saving">Zapisz</TacitButton>
</EditForm>
```

- `field.Attributes` daje kontrolce `id`, `aria-describedby` (podpowiedź i błąd), `aria-required` i `aria-invalid`. Kontrolkę wybierasz sam: `InputText`, `InputSelect`, `InputTextArea`, `InputCheckbox` albo zwykły `<input>`.
- `Context="field"` jest potrzebny, bo `EditForm` zajmuje już nazwę `context`.
- `Required` daje gwiazdkę i `aria-required`, ale nie atrybut `required`, bo ten włącza walidację przeglądarki obok `EditForm`. Samą regułę daje `[Required]` na modelu.
- Identyfikator kontrolki powstaje z typu modelu i nazwy pola (`clientmodel-name`). Gdy ten sam model jest na stronie dwa razy, podaj `Id`.
- Bez `EditForm` podaj `Id` zamiast `For`, a błąd w parametrze `Error`. Ten parametr przyjmuje też błąd z odpowiedzi serwera przy `EditForm`.
- `<TacitValidationSummary />` linkuje błędy pól z `<TacitField For>` do kontrolek. W trybie interaktywnym po nieudanym wysłaniu dostaje fokus (WCAG 3.3.1). W statycznym SSR fokus nie przechodzi sam, ale podsumowanie stoi na górze formularza.

### Tabela

```razor
<TacitTable Items="clients" Label="Klienci OAuth" RowKey="c => c.Id">
    <Header>
        <th scope="col">Nazwa</th>
        <th scope="col">Identyfikator</th>
        <th scope="col"><span class="t-visually-hidden">Akcje</span></th>
    </Header>
    <Row>
        <th scope="row">@context.Name</th>
        <td class="t-mono">@context.ClientId</td>
        <td><TacitButton Href="@($"clients/{context.Id}")" Variant="TacitButtonVariant.Ghost" Small>Edytuj<span class="t-visually-hidden"> @context.Name</span></TacitButton></td>
    </Row>
    <Empty>Nie masz jeszcze klientów. <a href="clients/new">Dodaj pierwszego</a>.</Empty>
</TacitTable>
```

`Label` nazywa region przewijania dla czytnika ekranu. `Caption` dodaje widoczny podpis. Bez elementów (także przy `Items="null"`) komponent pokazuje `.t-empty` zamiast pustej tabeli. Nagłówki kolumn to `<th scope="col">`, a komórka nazywająca wiersz to `<th scope="row">`. Linki „Edytuj” w wierszach dostają ukryte uzupełnienie, żeby na liście linków czytnika różniły się od siebie (WCAG 2.4.4).

### Przycisk, komunikat, okruszki

```razor
<TacitButton Variant="TacitButtonVariant.Danger" @onclick="Delete">Usuń klienta</TacitButton>
<TacitButton Href="clients/new" Variant="TacitButtonVariant.Primary">Nowy klient</TacitButton>

<TacitCallout Tone="TacitTone.Success" Live="TacitLive.Polite">@status</TacitCallout>

<TacitBreadcrumbs Items="@([new("Backoffice", "backoffice"), new("Klienci OAuth")])" />
```

- `TacitButton` z `Href` renderuje `<a>` (nawigacja), a bez niego `<button type="button">` (akcja). `Busy` ustawia `aria-busy` i pokazuje kółko ładowania, ale nie blokuje kliknięć, więc w obsłudze zdarzenia zignoruj drugie kliknięcie. Przycisk z samą ikoną dostaje `aria-label`.
- `TacitCallout` bez `Title` pokazuje tytuł tonu („Informacja”, „Gotowe”, „Ostrzeżenie”, „Błąd”), bo rodzaju komunikatu nie wolno oddawać samym kolorem. `Live="Polite"` daje `role="status"`, a `Live="Assertive"` daje `role="alert"`. Region na żywo musi istnieć przed zmianą treści, więc renderuj komponent stale i zmieniaj tylko treść. Gdy komunikat ma się pojawiać i znikać, owiń go w stale renderowany `<div role="status">` i wstawiaj w nim `<TacitCallout>` warunkowo (tak robi sample).
- W `TacitBreadcrumbs` ostatnia pozycja to bieżąca strona: bez linku, z `aria-current="page"`.

### Menu obszaru

```razor
@* BackofficeLayout.razor: układ obszaru, który host podaje w AddModuleBlazor(...) *@
@inherits LayoutComponentBase
@layout MainLayout  @* nagłówek, <main class="t-wrap t-main"> i stopka serwisu *@

<div class="t-area">
    <TacitNav class="t-area__nav" Label="Backoffice" Items="items" />
    <div class="t-stack">@Body</div>
</div>

@code {
    private readonly TacitNavItem[] items =
    [
        new("Pulpit", "backoffice", Match: NavLinkMatch.All),
        new("Klienci OAuth", "backoffice/clients", Group: "Tożsamość"),
        new("Klucze", "backoffice/keys", Group: "Kryptografia"),
    ];
}
```

`.t-area` układa menu obok treści od 960 px, a na węższym ekranie nad treścią. Pozycje bez grupy są na górze, a grupy idą w kolejności pierwszego wystąpienia. Bieżącą stronę oznacza `NavLink` (`aria-current="page"`). `Label` musi różnić się od etykiety nawigacji głównej w nagłówku.

Pozycje menu modułów daje `<ModuleNavigation>` z Tacit.Modulus.Blazor (ADR-C-0031 pkt 5): wybiera pozycje obszaru, które użytkownik może zobaczyć, i układa je według kolejności. Bez treści renderuje własny, prostszy znacznik (`<nav><ul>`, grupa jako `<span>` z zagnieżdżoną listą), bez klas `.t-sidenav`. Żeby menu modułów wyglądało jak menu pisane ręcznie, przekaż widoczne pozycje do `<TacitNav>`:

```razor
<ModuleNavigation Area="backoffice" Context="groups">
    <TacitNav class="t-area__nav" Label="Backoffice"
              Items="groups.SelectMany(g => g.Items).Select(i => new TacitNavItem(i.Title, i.Href, i.Group, i.Match))" />
</ModuleNavigation>
```

`TacitNavItem` ma pola pozycji z `ModuleNavigationBuilder` bez obszaru, kolejności i polityki autoryzacji, bo te rozstrzyga `ModuleNavigation`. `TacitNav` stawia pozycje bez grupy na górze, a `ModuleNavigation` umieszcza grupę według jej najniższej pozycji, więc kolejność może się różnić, gdy pozycja bez grupy ma większe `Order` niż pozycje grup. Gdy nic nie jest widoczne, `TacitNav` nie renderuje niczego. Znacznik:

```html
<nav class="t-sidenav" aria-label="Backoffice">
  <ul><li><a href="backoffice" aria-current="page">Pulpit</a></li></ul>
  <div>
    <p class="t-sidenav__title" id="t-sidenav-g1">Tożsamość</p>
    <ul aria-labelledby="t-sidenav-g1"><li><a href="backoffice/clients">Klienci OAuth</a></li></ul>
  </div>
</nav>
```

## Hex tylko w tokenach

Paczka dokłada do builda odbiorcy (także przechodnio, przez paczkę modułu UI) sprawdzenie zasady z ADR-C-0026: kolor wpisany jako hex jest tylko w `tacit-tokens.css`. Hex w wartości deklaracji w `*.razor.css` albo `*.cshtml.css` daje ostrzeżenie **TBRAND001** z plikiem i linią. Komentarze, napisy w cudzysłowie, selektory `#id` i całe `url(…)` są pomijane.

```xml
<!-- błąd zamiast ostrzeżenia, np. w Directory.Build.props biblioteki z UI -->
<WarningsAsErrors>$(WarningsAsErrors);TBRAND001</WarningsAsErrors>

<!-- wyłączenie -->
<TacitBrandHexCheck>false</TacitBrandHexCheck>
```
