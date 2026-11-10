using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tacit.Brand.Tests;

public sealed class ButtonTests : BunitContext
{
	[Fact]
	public void Renders_button_with_type_button_by_default()
	{
		var cut = Render<TacitButton>(p => p.AddChildContent("Zapisz"));

		var button = cut.Find("button");
		Assert.Equal("button", button.GetAttribute("type"));
		Assert.Equal("t-btn", button.ClassName);
	}

	[Fact]
	public void Renders_link_when_href_is_set_and_merges_user_class()
	{
		var cut = Render<TacitButton>(p => p
			.Add(b => b.Href, "clients/new")
			.Add(b => b.Variant, TacitButtonVariant.Primary)
			.Add(b => b.Small, true)
			.AddUnmatched("class", "extra")
			.AddChildContent("Nowy klient"));

		var link = cut.Find("a");
		Assert.Equal("clients/new", link.GetAttribute("href"));
		Assert.Equal("t-btn t-btn--primary t-btn--sm extra", link.ClassName);
		Assert.Empty(cut.FindAll("button"));
	}

	[Fact]
	public void Busy_sets_aria_busy()
	{
		var cut = Render<TacitButton>(p => p.Add(b => b.Busy, true));

		Assert.Equal("true", cut.Find("button").GetAttribute("aria-busy"));
	}
}

public sealed class CalloutTests : BunitContext
{
	[Fact]
	public void Has_default_title_so_tone_is_not_conveyed_by_colour_alone()
	{
		var cut = Render<TacitCallout>(p => p.Add(c => c.Tone, TacitTone.Danger).AddChildContent("Nie zapisano."));

		Assert.Equal("t-callout t-callout--danger", cut.Find("div").ClassName);
		Assert.Equal("Błąd", cut.Find(".t-callout__title").TextContent);
		Assert.Null(cut.Find("div").GetAttribute("role"));
	}

	[Theory]
	[InlineData(TacitLive.Polite, "status")]
	[InlineData(TacitLive.Assertive, "alert")]
	public void Live_sets_role(TacitLive live, string role)
	{
		var cut = Render<TacitCallout>(p => p.Add(c => c.Live, live));

		Assert.Equal(role, cut.Find(".t-callout").GetAttribute("role"));
	}

	[Fact]
	public void Empty_title_hides_title()
	{
		var cut = Render<TacitCallout>(p => p.Add(c => c.Title, ""));

		Assert.Empty(cut.FindAll(".t-callout__title"));
	}
}

public sealed class TableTests : BunitContext
{
	private sealed record Client(int Id, string Name);

	[Fact]
	public void Renders_scrollable_region_reachable_by_keyboard()
	{
		var cut = Render<TacitTable<Client>>(p => p
			.Add(t => t.Items, [new Client(1, "Portal"), new Client(2, "Sklep")])
			.Add(t => t.Label, "Klienci OAuth")
			.Add(t => t.Caption, "Zarejestrowani klienci")
			.Add(t => t.Header, "<th scope=\"col\">Nazwa</th>")
			.Add(t => t.Row, c => $"<th scope=\"row\">{c.Name}</th>"));

		var region = cut.Find(".t-table");
		Assert.Equal("region", region.GetAttribute("role"));
		Assert.Equal("Klienci OAuth", region.GetAttribute("aria-label"));
		Assert.Equal("0", region.GetAttribute("tabindex"));
		Assert.Equal("Zarejestrowani klienci", cut.Find("caption").TextContent);
		Assert.Equal(2, cut.FindAll("tbody tr").Count);
	}

	[Fact]
	public void Renders_empty_state_instead_of_empty_table()
	{
		var cut = Render<TacitTable<Client>>(p => p
			.Add(t => t.Items, [])
			.Add(t => t.Label, "Klienci OAuth")
			.Add(t => t.EmptyText, "Nie masz jeszcze klientów.")
			.Add(t => t.Row, c => c.Name));

		Assert.Empty(cut.FindAll("table"));
		Assert.Equal("Nie masz jeszcze klientów.", cut.Find(".t-empty").TextContent.Trim());
	}
}

public sealed class BreadcrumbsTests : BunitContext
{
	[Fact]
	public void Last_item_is_current_page_without_link()
	{
		var cut = Render<TacitBreadcrumbs>(p => p.Add(b => b.Items,
			[new TacitBreadcrumb("Klienci OAuth", "clients"), new TacitBreadcrumb("Portal", "clients/1")]));

		Assert.Equal("Okruszki", cut.Find("nav").GetAttribute("aria-label"));
		var items = cut.FindAll("li");
		Assert.Equal("clients", items[0].QuerySelector("a")!.GetAttribute("href"));
		Assert.Equal("page", items[1].GetAttribute("aria-current"));
		Assert.Null(items[1].QuerySelector("a"));
	}
}

public sealed class NavTests : BunitContext
{
	[Fact]
	public void Groups_items_in_order_with_labelled_lists_and_marks_current_page()
	{
		Services.GetRequiredService<NavigationManager>().NavigateTo("clients");

		var cut = Render<TacitNav>(p => p
			.Add(n => n.Label, "Backoffice")
			.Add(n => n.Items,
			[
				new TacitNavItem("Klienci OAuth", "clients", "Tożsamość"),
				new TacitNavItem("Pulpit", "", Match: NavLinkMatch.All),
				new TacitNavItem("Klucze", "keys", "Kryptografia"),
				new TacitNavItem("Zgody", "consents", "Tożsamość"),
			]));

		var nav = cut.Find("nav.t-sidenav");
		Assert.Equal("Backoffice", nav.GetAttribute("aria-label"));

		var lists = cut.FindAll("nav > ul, nav > div > ul");
		Assert.Equal(["Pulpit"], lists[0].QuerySelectorAll("a").Select(a => a.TextContent));
		Assert.Equal(["Klienci OAuth", "Zgody"], lists[1].QuerySelectorAll("a").Select(a => a.TextContent));
		Assert.Equal(["Klucze"], lists[2].QuerySelectorAll("a").Select(a => a.TextContent));

		var titleId = lists[1].GetAttribute("aria-labelledby");
		Assert.Equal("Tożsamość", cut.Find($"#{titleId}").TextContent);

		var current = cut.Find("a[aria-current=page]");
		Assert.Equal("Klienci OAuth", current.TextContent);
	}

	[Fact]
	public void Renders_nothing_without_items()
	{
		var cut = Render<TacitNav>(p => p.Add(n => n.Label, "Backoffice").Add(n => n.Items, []));

		Assert.Empty(cut.FindAll("nav"));
	}
}

public sealed class FieldTests : BunitContext
{
	[Fact]
	public void Connects_label_hint_and_control()
	{
		var cut = Render<ClientForm>();

		var input = cut.Find("#clientmodel-name");
		Assert.Equal("clientmodel-name", cut.Find("label").GetAttribute("for"));
		Assert.Equal("clientmodel-name-hint", input.GetAttribute("aria-describedby"));
		Assert.Equal("true", input.GetAttribute("aria-required"));
		Assert.False(input.HasAttribute("required"));
		Assert.False(input.HasAttribute("aria-invalid"));
		Assert.True(cut.Find("label").HasAttribute("data-required"));
		Assert.Empty(cut.FindAll(".validation-errors"));
	}

	[Fact]
	public void Failed_submit_shows_errors_linked_to_fields_and_summary_with_links()
	{
		var cut = Render<ClientForm>();
		cut.Find("#clientmodel-uri").Change("nie-adres");

		cut.Find("form").Submit();

		var name = cut.Find("#clientmodel-name");
		Assert.Equal("true", name.GetAttribute("aria-invalid"));
		Assert.Equal("clientmodel-name-hint clientmodel-name-error", name.GetAttribute("aria-describedby"));
		Assert.Equal("Podaj nazwę.", cut.Find("#clientmodel-name-error").TextContent);

		var summary = cut.Find(".validation-errors");
		Assert.Equal("-1", summary.GetAttribute("tabindex"));
		Assert.Equal("Popraw 2 pola:", summary.QuerySelector("strong")!.TextContent);
		var links = summary.QuerySelectorAll("a").Select(a => a.GetAttribute("href")).ToList();
		Assert.Equal(["/#clientmodel-name", "/#clientmodel-uri"], links);
		Assert.False(cut.Instance.Submitted);
	}

	[Fact]
	public void Error_parameter_works_without_edit_form()
	{
		var cut = Render<TacitField>(p => p
			.Add(f => f.Id, "q")
			.Add(f => f.Label, "Szukaj")
			.Add(f => f.Error, "Wpisz co najmniej 2 znaki.")
			.Add(f => f.ChildContent, ctx => b =>
			{
				b.OpenElement(0, "input");
				b.AddMultipleAttributes(1, ctx.Attributes);
				b.CloseElement();
			}));

		var input = cut.Find("input");
		Assert.Equal("q", input.Id);
		Assert.Equal("q-error", input.GetAttribute("aria-describedby"));
		Assert.Equal("true", input.GetAttribute("aria-invalid"));
		Assert.Equal("Wpisz co najmniej 2 znaki.", cut.Find("#q-error").TextContent);
	}

	[Fact]
	public void Requires_for_or_id()
	{
		Assert.Throws<InvalidOperationException>(() => Render<TacitField>(p => p.Add(f => f.Label, "Nazwa")));
	}

	[Theory]
	[InlineData(1, "Popraw 1 pole:")]
	[InlineData(3, "Popraw 3 pola:")]
	[InlineData(5, "Popraw 5 pól:")]
	[InlineData(12, "Popraw 12 pól:")]
	[InlineData(22, "Popraw 22 pola:")]
	public void Summary_title_uses_polish_plural(int count, string expected)
	{
		Assert.Equal(expected, TacitValidationSummary.DefaultTitle(count));
	}
}
