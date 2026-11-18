# Tacit.Brand.QuickGrid

Komponenty marki Tacit dla [QuickGrid](https://learn.microsoft.com/aspnet/core/blazor/components/quickgrid) w .NET 11. Uzupełnia paczkę `Tacit.Brand`: style siatki (`Theme="tacit"`, ramka `.t-datagrid`) są w `tacit-site.css` z `Tacit.Brand`, a ta paczka daje komponenty, których nie da się zastąpić samym CSS.

```
dotnet add package Tacit.Brand.QuickGrid
```

Paczka jest osobna, bo wymaga `Microsoft.AspNetCore.Components.QuickGrid` i net11.0. `Tacit.Brand` zostaje bez tej zależności i na net10.0.

## TacitPaginator

Zamiennik `<Paginator />`. Paginator z QuickGrid ma teksty wpisane po angielsku („Page 2 of 5”, „Go to next page”) i nie da się ich zmienić. Na stronie w innym języku czytnik ekranu czyta je wymową tego języka (WCAG 3.1.2). `TacitPaginator` ma ten sam znacznik i to samo zachowanie, ale każdy tekst jest parametrem.

```razor
@using Tacit.Brand.QuickGrid

<div class="t-datagrid">
    <div class="t-table" role="region" aria-label="Dziennik zdarzeń" tabindex="0">
        <QuickGrid Items="events" Theme="tacit" Pagination="pagination">…</QuickGrid>
    </div>
    <TacitPaginator State="pagination" />
</div>

@code {
    private readonly PaginationState pagination = new() { ItemsPerPage = 20 };
}
```

- Nawigacja między stronami to linki z numerem strony w adresie (`?page=3`), tak jak w QuickGrid z .NET 11. Działa w statycznym SSR i w Interactive Server, a adres strony można udostępnić. Pierwsza strona nie ma parametru w adresie.
- Nieaktywne linki (pierwsza i poprzednia strona na początku, następna i ostatnia na końcu) nie mają `href`, więc nie trafiają na listę linków czytnika ani do kolejności Tab. Paginator z QuickGrid zostawia im `href` i chowa je tylko przez `tabindex="-1"`.
- Tekst „Strona 2 z 5” jest w regionie `aria-live="polite"`, więc czytnik ogłasza zmianę strony.
- Do czasu, aż siatka poda liczbę pozycji, komponent renderuje pusty kontener.
- Gdy siatka ma `QueryParameterNameOptions` (np. dwie siatki na jednej stronie), przekaż ten sam obiekt do `TacitPaginator`. QuickGrid nie udostępnia nazwy parametru strony na zewnątrz.

### Teksty i języki

Domyślne teksty są po polsku. W aplikacji wielojęzycznej przekaż je z `IStringLocalizer`:

| Parametr | Domyślnie | Gdzie |
| --- | --- | --- |
| `Label` | „Strony” | `aria-label` nawigacji |
| `FirstPageLabel`, `PreviousPageLabel`, `NextPageLabel`, `LastPageLabel` | „Pierwsza strona”, „Poprzednia strona”, „Następna strona”, „Ostatnia strona” | `aria-label` i `title` linków |
| `Summary` | „47 pozycji” | `Func<int, string>`: liczba wszystkich pozycji |
| `PageText` | „Strona 2 z 5” | `Func<int, int, string>`: bieżąca strona i liczba stron (obie od 1) |

`Summary` i `PageText` są funkcjami, bo odmiana zależy od liczby i języka (po polsku: 1 pozycja, 2 pozycje, 5 pozycji, 22 pozycje).

```razor
@inject IStringLocalizer<SharedResources> L

<TacitPaginator State="pagination"
                Label="@L["Pages"]"
                FirstPageLabel="@L["First page"]" PreviousPageLabel="@L["Previous page"]"
                NextPageLabel="@L["Next page"]" LastPageLabel="@L["Last page"]"
                Summary="@(count => L["{0} items", count])"
                PageText="@((page, pages) => L["Page {0} of {1}", page, pages])" />
```

Gdy te same teksty powtarzają się na wielu stronach, zbierz je we własnym komponencie aplikacji, który owija `TacitPaginator`.
