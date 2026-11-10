using Microsoft.AspNetCore.Components.Routing;

namespace Tacit.Brand;

/// <summary>Wariant przycisku <see cref="TacitButton"/>.</summary>
public enum TacitButtonVariant
{
	/// <summary>Drugorzędny: obramowany, tło powierzchni.</summary>
	Default,

	/// <summary>Główna akcja strony. Najwyżej jeden na widok.</summary>
	Primary,

	/// <summary>Tekstowy, bez ramki.</summary>
	Ghost,

	/// <summary>Akcja nieodwracalna, np. usunięcie.</summary>
	Danger,
}

/// <summary>Ton komunikatu <see cref="TacitCallout"/>.</summary>
public enum TacitTone
{
	Info,
	Success,
	Warning,
	Danger,
}

/// <summary>Czy komunikat jest regionem na żywo, który czytnik ekranu ogłasza po zmianie treści.</summary>
public enum TacitLive
{
	/// <summary>Zwykły komunikat, obecny od wczytania strony.</summary>
	None,

	/// <summary>role="status": ogłoszenie po zakończeniu bieżącej wypowiedzi czytnika (wynik zapisu, liczba wyników).</summary>
	Polite,

	/// <summary>role="alert": ogłoszenie od razu. Tylko dla błędów, które przerywają pracę.</summary>
	Assertive,
}

/// <summary>Pozycja okruszków. Ostatnia pozycja listy to bieżąca strona i jej adres jest pomijany.</summary>
/// <param name="Text">Tekst pozycji.</param>
/// <param name="Href">Adres, null dla pozycji bez linku.</param>
public sealed record TacitBreadcrumb(string Text, string? Href = null);

/// <summary>
/// Pozycja menu obszaru <see cref="TacitNav"/>. Pola odpowiadają pozycji z <c>ModuleNavigationBuilder</c>
/// (Tacit.Modulus.Blazor) bez obszaru, kolejności i polityki: te rozstrzyga ten, kto buduje listę.
/// </summary>
/// <param name="Title">Tytuł widoczny w menu.</param>
/// <param name="Href">Adres strony, względny do <c>&lt;base href&gt;</c>.</param>
/// <param name="Group">Grupa w menu, null dla pozycji bez grupy (na górze menu).</param>
/// <param name="Match">Kiedy pozycja jest bieżąca. Dla strony głównej obszaru użyj <see cref="NavLinkMatch.All"/>.</param>
public sealed record TacitNavItem(string Title, string Href, string? Group = null, NavLinkMatch Match = NavLinkMatch.Prefix);
