using System.ComponentModel.DataAnnotations;

namespace Tacit.Brand.Sample;

public sealed class ClientStore
{
	private readonly List<Client> clients =
	[
		new(1, "Portal partnera", "portal-partner", ClientKind.Confidential, "https://partner.example.com/callback"),
		new(2, "Aplikacja mobilna", "mobile-app", ClientKind.Public, "com.example.app:/callback"),
		new(3, "Panel raportów", "reports", ClientKind.Confidential, "https://reports.example.com/signin-oidc"),
	];

	private readonly Lock gate = new();

	public IReadOnlyList<Client> All()
	{
		lock (gate) { return [.. clients]; }
	}

	public Client Add(ClientModel model)
	{
		lock (gate)
		{
			var client = new Client(clients.Count == 0 ? 1 : clients.Max(c => c.Id) + 1, model.Name!, model.ClientId!, model.Kind, model.RedirectUri!);
			clients.Add(client);
			return client;
		}
	}

	public void Remove(int id)
	{
		lock (gate) { clients.RemoveAll(c => c.Id == id); }
	}
}

public sealed record Client(int Id, string Name, string ClientId, ClientKind Kind, string RedirectUri);

public enum ClientKind
{
	Confidential,
	Public,
}

public sealed class ClientModel
{
	[Required(ErrorMessage = "Podaj nazwę klienta.")]
	[StringLength(60, ErrorMessage = "Nazwa może mieć najwyżej 60 znaków.")]
	public string? Name { get; set; }

	[Required(ErrorMessage = "Podaj identyfikator klienta.")]
	[RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Identyfikator może mieć tylko małe litery, cyfry i myślniki.")]
	public string? ClientId { get; set; }

	public ClientKind Kind { get; set; }

	[Required(ErrorMessage = "Podaj adres przekierowania.")]
	[Url(ErrorMessage = "Podaj pełny adres, np. https://app.example.com/callback.")]
	public string? RedirectUri { get; set; }

	public string? Notes { get; set; }
}
