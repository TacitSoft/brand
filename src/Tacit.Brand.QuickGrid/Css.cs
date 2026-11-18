namespace Tacit.Brand.QuickGrid;

/// <summary>
/// Składanie atrybutu class, kopia Tacit.Brand.Css. Bez InternalsVisibleTo, bo paczka zależy od Tacit.Brand
/// w wersji „co najmniej”, a nowsza wersja może zmienić typy wewnętrzne.
/// </summary>
internal static class Css
{
	public static string Join(params string?[] classes) =>
		string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));

	public static string? From(IReadOnlyDictionary<string, object>? attributes) =>
		attributes is not null && attributes.TryGetValue("class", out var value) ? Convert.ToString(value) : null;
}
