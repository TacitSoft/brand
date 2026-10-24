# Dostępność serwisu Tacit

Cel: **WCAG 2.2, poziom AA** w serwisie Blazor, w przewodnikach (Starlight) i w referencji API (DocFX).

## Dlaczego WCAG 2.2 AA

| Przepis | Kogo dotyczy | Co z tego wynika dla Tacit |
|---|---|---|
| Ustawa z 4 kwietnia 2019 r. o dostępności cyfrowej stron internetowych i aplikacji mobilnych podmiotów publicznych | Podmioty publiczne | Nas wprost nie dotyczy. Dotyczy natomiast klientów z sektora publicznego: w zamówieniach często wymagają od dostawców zgodności z WCAG. |
| Ustawa z 26 kwietnia 2024 r. o zapewnianiu spełniania wymagań dostępności niektórych produktów i usług przez podmioty gospodarcze (polskie wdrożenie Europejskiego aktu o dostępności, EAA), stosowana od 28 czerwca 2025 r. | Firmy świadczące konsumentom m.in. usługi handlu elektronicznego. **Mikroprzedsiębiorcy świadczący usługi są wyłączeni.** | Katalog z przyciskiem „Zapytaj o ofertę” dla firm (B2B) najpewniej nie jest usługą handlu elektronicznego dla konsumentów. Obowiązek pojawi się, jeśli serwis zacznie sprzedawać konsumentom, a firma przestanie być mikroprzedsiębiorcą. **Do potwierdzenia z prawnikiem przed uruchomieniem sprzedaży online.** |
| Norma EN 301 549 | Techniczny punkt odniesienia dla EAA i zamówień publicznych | Wersja v3.2.1 odsyła do WCAG 2.1 AA. Wersja v4.1.1 (opublikowana we wrześniu 2026 r., ma być wiążąca od grudnia 2026 r.) odsyła do **WCAG 2.2 AA**. |

Wniosek: nawet jeśli dziś nie mamy obowiązku prawnego, celujemy w WCAG 2.2 AA. To ten sam poziom, którego oczekują klienci publiczni i który obowiązywałby nas po zmianie modelu sprzedaży. Dorobienie go później, w gotowym serwisie, kosztuje dużo więcej niż zaprojektowanie od razu.

W stopce serwisu umieszczamy link **„Deklaracja dostępności”**. Nas formalnie nie obowiązuje, ale klienci publiczni o nią pytają. Strona opisuje osiągnięty poziom, znane braki, datę ostatniego audytu i kontakt do zgłaszania problemów.

## Co zapewniają style (`tacit-tokens.css`, `tacit-site.css`, adaptery)

| Kryterium WCAG | Jak |
|---|---|
| 1.4.3 Kontrast (minimum), 1.4.11 Kontrast elementów nietekstowych | Każda para tekst/tło ma ≥ 4,5:1, a ramki pól, fokus i znaczniki stanu ≥ 3:1, w obu motywach i w nagłówku. Pilnuje tego `tools/check-contrast.mjs` (kod wyjścia 1 przy błędzie, do CI). Pomarańcz marki `#F5A623` ma na bieli tylko 2:1, dlatego w jasnym motywie jest wyłącznie ozdobą, a tekst „akcentowy” ma ciemny wariant `--tacit-color-accent-text`. |
| 1.4.1 Użycie koloru | Linki w tekście są podkreślone. Aktywna pozycja menu ma pasek, nie tylko inny kolor. Zaznaczony filtr ma ✓, błąd w formularzu ma ikonę, grubszą ramkę i treść komunikatu. |
| 1.4.4 Zmiana rozmiaru tekstu, 1.4.10 Dopasowanie do ekranu | Rozmiary w `rem`, układ działa od 320 px bez przewijania w poziomie. Długie nazwy typów .NET się łamią (`overflow-wrap`). Tabele mają własne przewijanie w kontenerze `.t-table`. |
| 1.4.12 Odstępy w tekście | Brak sztywnych wysokości przy tekście, więc zwiększone odstępy niczego nie ucinają. |
| 2.4.1 Pomijanie bloków | `.t-skip-link` „Przejdź do treści”. |
| 2.4.7 Widoczny fokus, 2.4.13 Wygląd fokusu | `:focus-visible`: obrys 3 px z odstępem 2 px, kontrast ≥ 3:1. Karta klikalna dostaje obrys na całej karcie. |
| 2.4.11 Fokus niezasłonięty | `scroll-padding-top` równy wysokości lepkiego nagłówka, także w Starlight i DocFX. |
| 2.5.8 Rozmiar celu (minimum) | Przyciski i linki nawigacji mają ≥ 44 px, gęste kontrolki ≥ 36 px (wymóg to 24 px). |
| 2.3.3 Animacje z interakcji | `prefers-reduced-motion` zeruje `--tacit-duration` i wyłącza unoszenie kart. |
| Ustawienia systemu | `prefers-contrast: more` przyciemnia tekst pomocniczy i linie. `forced-colors` (tryb kontrastu w Windows) dodaje ramki tam, gdzie elementy różniły się tylko tłem. `prefers-color-scheme` jest motywem domyślnym. |
| Wydruk | Zawsze jasny motyw. Linki zewnętrzne w treści dostają adres w nawiasie. |

## Co musi zapewnić znacznik (Razor, Markdown). Tego CSS nie załatwi.

- **Język**: `<html lang="…">` zgodny z językiem strony (3.1.1). Fragmenty w innym języku dostają `lang`, np. nazwy języków w przełączniku (`<a lang="ru">Русский</a>`) (3.1.2). Starlight robi to sam.
- **Struktura**: na stronie jest jeden `<h1>` i nagłówki bez przeskoków poziomu. Do tego landmarki `<header>`, `<nav aria-label>`, `<main id="main" tabindex="-1">` i `<footer>`. Jeśli jest kilka `<nav>`, każdy ma inną etykietę.
- **Tytuł strony**: przez `<PageTitle>`, unikalny, w formacie „Nazwa strony · Tacit” (2.4.2).
- **Obrazy**: `alt` opisuje treść. Grafika dekoracyjna i logo obok nazwy mają `alt=""`. Ikony SVG mają `aria-hidden="true" focusable="false"`.
- **Przyciski i linki**: `<button>` służy do akcji, `<a href>` do nawigacji, a `<div @onclick>` jest zakazany. Przycisk z samą ikoną dostaje `aria-label`. Przełączniki stanu (filtry, motyw) mają `aria-pressed`.
- **Karty**: link jest tylko w tytule (`.t-card__link`), a klikalność całej karty daje CSS. Nie owijaj całej karty w `<a>`.
- **Formularze**: każde pole ma `<label for>`. Podpowiedź i błąd są połączone z polem przez `aria-describedby`. Pole z błędem ma `aria-invalid="true"`, a wymagane `required`. Po nieudanym wysłaniu fokus trafia do podsumowania błędów lub do pierwszego błędnego pola (3.3.1, 3.3.3). Pól nie podpisujemy samym placeholderem.
- **Treść zmieniana dynamicznie**: liczba wyników filtrowania i komunikat „Skopiowano” trafiają do regionu `aria-live="polite"` (4.1.3).
- **Nawigacja w Blazorze**: `<FocusOnNavigate Selector="h1" />` w `Routes.razor`, żeby czytnik ekranu ogłosił nową stronę po enhanced navigation.
- **Tabele danych**: `<th scope="col|row">`, a w razie potrzeby `<caption>`. Na wąskie ekrany idą do `.t-table` z `role="region"`, `aria-label` i `tabindex="0"`.
- **Bloki kodu, które się przewijają**: `tabindex="0"`, żeby dało się je przewinąć klawiaturą (Starlight i Expressive Code robią to same).
- **Wideo**: napisy, a przy treści tylko wizualnej także transkrypcja (1.2.x).
- **Limit czasu, karuzele, autoodtwarzanie**: nie używamy. Jeśli już muszą być, to z pauzą (2.2.2).
- **Tekst**: prosty język, krótkie zdania, linki opisowe („Instalacja Tacit.Results”, a nie „kliknij tutaj”) (2.4.4).

## Jak sprawdzać

1. `node web/tools/check-contrast.mjs` po każdej zmianie tokenów (w CI).
2. **axe** (rozszerzenie przeglądarki albo `@axe-core/playwright` w testach E2E) na kluczowych stronach w obu motywach. Wynik: zero naruszeń „serious” i „critical”.
3. **Tylko klawiatura**: Tab i Shift+Tab przez całą stronę. Fokus jest zawsze widoczny i niezasłonięty, kolejność logiczna, nic nie więzi fokusu, a menu i przełączniki działają Enterem i Spacją.
4. **Powiększenie 200% i szerokość 320 px** (np. okno 1280 px przy 400%): bez poziomego przewijania strony i bez uciętej treści.
5. **Czytnik ekranu**: NVDA z Firefoksem lub Chrome na Windows, VoiceOver z Safari. Sprawdź nagłówki (H), landmarki (D), formularz z błędem i przełącznik motywu (czy ogłasza „wciśnięty”).
6. **Tryb kontrastu Windows** (Ustawienia → Ułatwienia dostępu → Motywy kontrastu): elementy interaktywne mają widoczne granice.
7. **Ustawienia systemu**: ograniczenie ruchu i ciemny motyw systemowy przy wyborze „Systemowy”.

Automaty wykrywają około jednej trzeciej problemów, dlatego kroki 3–5 są obowiązkowe przy każdej nowej stronie lub komponencie. Przed publicznym startem warto zamówić zewnętrzny audyt WCAG 2.2 i na jego podstawie wypełnić deklarację dostępności.
