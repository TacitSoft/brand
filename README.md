# TacitSoft / brand

Jedno źródło prawdy dla znaku Tacit: przykładnica (T-square). Nazwa zaczyna się na „T” jak Tacit, a znak jest narzędziem do rysowania prostych, powtarzalnych linii, co wynika z misji firmy: *„Dajemy narzędzia, dzięki którym profesjonalne oprogramowanie powstaje prosto i przewidywalnie”*.

## Warianty

| Wariant | Źródło | Kiedy |
|---|---|---|
| **square** (pełny kwadrat, bez zaokrągleń) | `icon-square.svg` | Avatary i miejsca, które same przycinają narożniki: GitHub (organizacja), nuget.org (profil), Atlassian. |
| **rounded** (zaokrąglone narożniki, `rx=24` przy 128) | `icon-rounded.svg` | Ikona paczki NuGet, favicon, apple-touch-icon, manifest PWA. |

Nie wgrywaj wariantu `rounded` jako avatara, bo narożniki zostaną zaokrąglone dwa razy (ciemne rogi i ramka).

## Pliki

| Plik | Użycie |
|---|---|
| `icon-square-1024.png`, `-512`, `-256` | Avatar GitHub i nuget.org (wgrywaj 1024 lub 512) |
| `icon-rounded-128.png` | Ikona paczki NuGet (`PackageIcon`), kopia trafia do `assets/icon.png` w `dotnet-library-template` |
| `favicon.ico` (16/32/48) | Favicon |
| `icon-rounded-180.png` | `apple-touch-icon` |
| `icon-rounded-192.png`, `-512` | Manifest PWA / Android |
| `icon-rounded-16/32/48.png` | Pojedyncze rozmiary, gdy potrzebny jest sam PNG |
| `nuget/icon.png` | Ikona paczek NuGet pod stałym adresem (`PackageIconUrl`). Kopia `icon-rounded-128.png` |

Repo jest publiczne: trzyma tylko to, co i tak jest widoczne na zewnątrz. Adres `https://raw.githubusercontent.com/TacitSoft/brand/main/nuget/icon.png` jest wpisany w wydane paczki (`iconUrl` w `.nuspec`), więc **ścieżka jest stała**: podmieniaj zawartość pliku, nie zmieniaj jego nazwy ani położenia.

Przy 16 px podziałka znika, bo jest za drobna, i tak ma być. Czytelne zostaje „T” z pomarańczową końcówką.

## Kolory

| Rola | Hex |
|---|---|
| Tło (granat) | `#1F3A5F` |
| Znak | `#F2F4F7` |
| Akcent (końcówka) | `#F5A623` |

## Zmiana znaku

SVG jest źródłem, a PNG i ICO się z niego generuje. Po zmianie SVG wygeneruj ponownie wszystkie rozmiary, skopiuj `icon-rounded-128.png` do `nuget/icon.png` i zaktualizuj kopię `assets/icon.png` w `TacitSoft/dotnet-library-template`. Kopia w szablonie jest świadoma: paczka musi mieć plik w repo w chwili `dotnet pack`, a tego repo nie podpinamy jako zależności builda.

## Style WWW

Katalog `web/` zawiera wspólne style serwisu Blazor i dokumentacji (Starlight, DocFX) zgodnie z ADR-C-0026. Są tam tokeny `--tacit-*` (kolory z tabeli wyżej, motyw jasny i ciemny), lokalnie hostowane fonty, komponenty `.t-*`, przełącznik motywu i adaptery dla obu narzędzi dokumentacji. Opis i sposób podłączenia: [`web/README.md`](web/README.md), wymagania dostępności (WCAG 2.2 AA): [`web/DOSTEPNOSC.md`](web/DOSTEPNOSC.md).

Style są dystrybuowane jako dwie paczki w GitHub Packages, wydawane tagiem `vX.Y.Z`: npm `@tacitsoft/brand` (Starlight, DocFX) i NuGet `Tacit.Brand` (Blazor, z `src/Tacit.Brand`). Szczegóły i konfiguracja dostępu: [`web/README.md`](web/README.md#dystrybucja).
