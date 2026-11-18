using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tacit.Brand.QuickGrid.Tests;

public sealed class PaginatorTests : BunitContext
{
	public PaginatorTests() => JSInterop.Mode = JSRuntimeMode.Loose;

	[Fact]
	public void Renders_polish_texts_by_default()
	{
		var cut = Render<EventsGrid>();

		Assert.Equal("47 pozycji", cut.Find(".summary").TextContent);
		Assert.Equal("Strona 1 z 5", cut.Find(".pagination-text").TextContent);
		Assert.Equal("Strony", cut.Find("nav").GetAttribute("aria-label"));
		Assert.Equal(
			["Pierwsza strona", "Poprzednia strona", "Następna strona", "Ostatnia strona"],
			cut.FindAll("nav a").Select(a => a.GetAttribute("aria-label")));
	}

	[Fact]
	public void Disabled_links_have_no_href_and_enabled_links_carry_the_page()
	{
		var cut = Render<EventsGrid>();

		var first = cut.Find(".go-first");
		Assert.Null(first.GetAttribute("href"));
		Assert.Equal("true", first.GetAttribute("aria-disabled"));
		Assert.Equal("http://localhost/?page=2", cut.Find(".go-next").GetAttribute("href"));
		Assert.Equal("http://localhost/?page=5", cut.Find(".go-last").GetAttribute("href"));
		Assert.Null(cut.Find(".go-next").GetAttribute("aria-disabled"));
	}

	[Fact]
	public void Reads_the_page_from_the_address_like_static_ssr()
	{
		Services.GetRequiredService<NavigationManager>().NavigateTo("/?page=3");

		var cut = Render<EventsGrid>();

		Assert.Equal("Strona 3 z 5", cut.Find(".pagination-text").TextContent);
		Assert.Equal("21", cut.Find("tbody td").TextContent);
		// Pierwsza strona bez parametru w adresie, jak w Paginator z QuickGrid.
		Assert.Equal("http://localhost/", cut.Find(".go-first").GetAttribute("href"));
		Assert.Equal("http://localhost/?page=2", cut.Find(".go-previous").GetAttribute("href"));
	}

	[Fact]
	public void Follows_navigation_like_interactive_mode()
	{
		var cut = Render<EventsGrid>();

		cut.InvokeAsync(() => Services.GetRequiredService<NavigationManager>().NavigateTo(cut.Find(".go-last").GetAttribute("href")!));

		cut.WaitForAssertion(() => Assert.Equal("Strona 5 z 5", cut.Find(".pagination-text").TextContent));
		Assert.Equal("41", cut.Find("tbody td").TextContent);
		Assert.Null(cut.Find(".go-next").GetAttribute("href"));
	}

	[Fact]
	public void Uses_the_grid_query_parameter_names()
	{
		var cut = Render<EventsGrid>(p => p.Add(g => g.QueryNames, new QueryParameterNameOptions("log-")));

		Assert.Equal("http://localhost/?log-page=2", cut.Find(".go-next").GetAttribute("href"));
	}

	[Fact]
	public void Texts_can_be_replaced_for_another_language()
	{
		var cut = Render<TacitPaginator>(p => p
			.Add(x => x.State, StateWith(total: 1))
			.Add(x => x.Label, "Pages")
			.Add(x => x.NextPageLabel, "Next page")
			.Add(x => x.Summary, count => count == 1 ? "1 item" : $"{count} items")
			.Add(x => x.PageText, (page, pages) => $"Page {page} of {pages}")
			.AddUnmatched("class", "extra"));

		Assert.Equal("paginator extra", cut.Find("div").ClassName);
		Assert.Equal("1 item", cut.Find(".summary").TextContent);
		Assert.Equal("Page 1 of 1", cut.Find(".pagination-text").TextContent);
		Assert.Equal("Pages", cut.Find("nav").GetAttribute("aria-label"));
		Assert.Equal("Next page", cut.Find(".go-next").GetAttribute("aria-label"));
	}

	[Fact]
	public void Renders_only_the_container_until_the_total_is_known()
	{
		var cut = Render<TacitPaginator>(p => p.Add(x => x.State, new PaginationState()));

		Assert.Empty(cut.FindAll("nav"));
		Assert.Equal("paginator", cut.Find("div").ClassName);
	}

	[Theory]
	[InlineData(0, "0 pozycji")]
	[InlineData(1, "1 pozycja")]
	[InlineData(2, "2 pozycje")]
	[InlineData(5, "5 pozycji")]
	[InlineData(12, "12 pozycji")]
	[InlineData(22, "22 pozycje")]
	[InlineData(114, "114 pozycji")]
	public void Default_summary_uses_polish_plural(int count, string expected)
	{
		Assert.Equal(expected, TacitPaginator.DefaultSummary(count));
	}

	[Theory]
	[InlineData("http://localhost/", "page", 0)]
	[InlineData("http://localhost/?page=4", "page", 3)]
	[InlineData("http://localhost/?page=0", "page", 0)]
	[InlineData("http://localhost/?page=abc", "page", 0)]
	[InlineData("http://localhost/?sort=Nazwa&log-page=2", "log-page", 1)]
	public void Reads_one_based_page_from_query(string uri, string name, int expected)
	{
		Assert.Equal(expected, TacitPaginator.PageIndexFromQuery(uri, name));
	}

	// Liczbę pozycji ustawia tylko QuickGrid (metoda wewnętrzna), więc test bez siatki ustawia ją refleksją.
	private static PaginationState StateWith(int total)
	{
		var state = new PaginationState();
		var set = typeof(PaginationState).GetMethod("SetTotalItemCountAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
		((Task)set.Invoke(state, [total])!).GetAwaiter().GetResult();
		return state;
	}
}
