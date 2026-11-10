namespace Tacit.Brand;

/// <summary>Składanie atrybutu class: klasy komponentu plus klasa podana przez użytkownika.</summary>
internal static class Css
{
	public static string Join(params string?[] classes) =>
		string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));

	/// <summary>Klasa z atrybutów przekazanych komponentowi (class="…"), żeby nie nadpisała klas .t-*.</summary>
	public static string? From(IReadOnlyDictionary<string, object>? attributes) =>
		attributes is not null && attributes.TryGetValue("class", out var value) ? Convert.ToString(value) : null;
}
