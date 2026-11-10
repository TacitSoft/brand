using Microsoft.AspNetCore.Components.Forms;

namespace Tacit.Brand;

/// <summary>
/// Dane, które <see cref="TacitField"/> przekazuje kontrolce. Najprościej rozłożyć <see cref="Attributes"/>:
/// <c>&lt;InputText class="t-input" @attributes="field.Attributes" /&gt;</c> przy <c>Context="field"</c>.
/// </summary>
/// <param name="Id">Identyfikator kontrolki, na który wskazuje etykieta.</param>
/// <param name="DescribedBy">Identyfikatory podpowiedzi i błędu dla aria-describedby, null gdy ich nie ma.</param>
/// <param name="Invalid">Czy pole ma błąd.</param>
/// <param name="Attributes">id, aria-describedby, aria-required i aria-invalid gotowe do <c>@attributes</c>.</param>
public sealed record TacitFieldContext(
	string Id,
	string? DescribedBy,
	bool Invalid,
	IReadOnlyDictionary<string, object> Attributes);

/// <summary>
/// Pola <see cref="TacitField"/> zarejestrowane w formularzu, w kolejności renderowania. Trzymane w
/// <see cref="EditContext.Properties"/>, żeby <see cref="TacitValidationSummary"/> mógł linkować błędy do kontrolek.
/// </summary>
internal sealed class TacitFieldRegistry
{
	private readonly List<(FieldIdentifier Field, string Id)> fields = [];

	public IReadOnlyList<(FieldIdentifier Field, string Id)> Fields => fields;

	/// <summary>Pole doszło, zmieniło identyfikator albo zniknęło. Podsumowanie renderuje się przed polami, więc musi o tym wiedzieć.</summary>
	public event Action? Changed;

	public static TacitFieldRegistry For(EditContext context)
	{
		if (context.Properties.TryGetValue(typeof(TacitFieldRegistry), out var existing))
		{
			return (TacitFieldRegistry)existing;
		}

		var registry = new TacitFieldRegistry();
		context.Properties[typeof(TacitFieldRegistry)] = registry;
		return registry;
	}

	public static TacitFieldRegistry? Find(EditContext context) =>
		context.Properties.TryGetValue(typeof(TacitFieldRegistry), out var existing) ? (TacitFieldRegistry)existing : null;

	public void Set(FieldIdentifier field, string id)
	{
		var index = fields.FindIndex(f => f.Field.Equals(field));
		if (index >= 0)
		{
			if (fields[index].Id == id)
			{
				return;
			}
			fields[index] = (field, id);
		}
		else
		{
			fields.Add((field, id));
		}
		Changed?.Invoke();
	}

	public void Remove(FieldIdentifier field)
	{
		if (fields.RemoveAll(f => f.Field.Equals(field)) > 0)
		{
			Changed?.Invoke();
		}
	}
}
