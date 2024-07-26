using DeviceManager.Areas.Administration.Controllers;
using DeviceManager.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Savonia.xUnit.Helpers;
using Savonia.xUnit.Helpers.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace tests;

public class UnitTest : AppTestBase, IClassFixture<WebApplicationFactoryFixture<Program>>
{
    private readonly HttpClient _client;
    private readonly HttpClient _anonymousClient;

    public UnitTest(ITestOutputHelper testOutputHelper, WebApplicationFactoryFixture<Program> factoryFixture) : base(new string[] { "app.db" }, testOutputHelper)
    {
        _anonymousClient = factoryFixture.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        _client = factoryFixture.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandlerAdmin>(
                        "Test", options => { });
            });
        })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Theory]
    [JsonFileData("testdata.json", "auth", typeof(string), typeof(Tuple<int, string>))]
    public async void Checkpoint04_Anonymous(string data, Tuple<int, string> expected)
    {
        // Act
        var response = await _anonymousClient.GetAsync(data);

        // Assert
        Assert.Equal(expected.Item1, (int)response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(expected.Item2, response.Headers.Location.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Checkpoint04_ManageController()
    {
        var t = typeof(ManageController);
        Assert.True(Attribute.IsDefined(t, typeof(AuthorizeAttribute)));
        var attr = Attribute.GetCustomAttribute(t, typeof(AuthorizeAttribute));
        Assert.NotNull(attr);
        Assert.IsAssignableFrom<AuthorizeAttribute>(attr);
        var aa = (AuthorizeAttribute)attr;
        Assert.NotNull(aa);
    }

    [Fact]
    public void Checkpoint04_HomeController()
    {
        var t = typeof(HomeController);
        Assert.True(Attribute.IsDefined(t, typeof(AuthorizeAttribute)));
        Assert.True(Attribute.IsDefined(t, typeof(AreaAttribute)));
        var attr = Attribute.GetCustomAttribute(t, typeof(AuthorizeAttribute));
        Assert.NotNull(attr);
        Assert.IsAssignableFrom<AuthorizeAttribute>(attr);
        var aa = (AuthorizeAttribute)attr;
        Assert.Equal("Admins", aa.Roles);
        var attr2 = Attribute.GetCustomAttribute(t, typeof(AreaAttribute));
        Assert.NotNull(attr2);
        var ar = (AreaAttribute)attr2;
        Assert.Equal("Administration", ar.RouteValue);
    }

    [Theory]
    [JsonFileData("testdata.json", "users", typeof(string), typeof(Tuple<IEnumerable<Tuple<string, string>>, int, int>))]
    public async Task Checkpoint04_Users(string data, Tuple<IEnumerable<Tuple<string, string>>, int, int> expected)
    {
        // Arrange

        //Act
        var response = await _client.GetAsync(data);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(response);
        var table = content.QuerySelector("table");
        Assert.NotNull(table);
        var rows = table.QuerySelectorAll("tr");
        var links = table.QuerySelectorAll("a");
        // WriteLine(await response.Content.ReadAsStringAsync());
        Assert.Equal(expected.Item2, rows.Count());
        Assert.Equal(expected.Item3, links.Count());
        var rowContent = rows.Skip(1).Select(r => r.TextContent);
        // WriteLine(rowContent);
        Assert.All(expected.Item1, e => Assert.Contains(rowContent, r => r.Contains(e.Item1, StringComparison.OrdinalIgnoreCase) && r.Contains(e.Item2, StringComparison.OrdinalIgnoreCase)));
    }

    [Theory]
    [JsonFileData("testdata.json", "details", typeof(Tuple<string, string>), typeof(Tuple<string, string, IEnumerable<string>>))]
    public async Task Checkpoint04_Details(Tuple<string, string> data, Tuple<string, string, IEnumerable<string>> expected)
    {
        // Arrange

        //Act
        var response = await _client.GetAsync($"{data.Item1}/{data.Item2}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act 2
        var content = await HtmlHelpers.GetDocumentAsync(response);
        // Assert 2
        Assert.NotNull(content);
        var body = content.Body;
        Assert.NotNull(body);
        var textContent = body.TextContent;
        Assert.NotNull(textContent);
        Assert.Contains(expected.Item1, textContent);
        Assert.Contains(expected.Item2, textContent);
        Assert.All(expected.Item3, e => Assert.Contains(e, textContent));
    }

    [Theory]
    [JsonFileData("testdata.json", "delete", typeof(Tuple<string, string>), typeof(Tuple<string, string>))]
    public async Task Checkpoint04_Delete(Tuple<string, string> data, Tuple<string, string> expected)
    {
        // Arrange

        //Act
        var response = await _client.GetAsync($"{data.Item1}/{data.Item2}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(response);
        var form = content.GetForm();
        var submit = form.GetSubmitButton();
        var responsePost = await _client.SendAsync(
                form,
                submit);

        // Assert 2
        Assert.Equal(HttpStatusCode.Redirect, responsePost.StatusCode);
        Assert.NotNull(responsePost.Headers.Location);
        var responseRedirect = await _client.GetAsync($"{responsePost.Headers.Location.OriginalString}");
        Assert.True(responseRedirect.IsSuccessStatusCode);
        var resultPage = await HtmlHelpers.GetDocumentAsync(responseRedirect);
        Assert.NotNull(resultPage);
        var body = resultPage.Body;
        Assert.NotNull(body);
        var textContent = body.TextContent;
        Assert.DoesNotContain(expected.Item1, textContent);
        Assert.DoesNotContain(expected.Item2, textContent);
    }

    [Theory]
    [JsonFileData("testdata.json", "roles", typeof(Tuple<string, string>), typeof(Tuple<string, string, IEnumerable<string>, IEnumerable<string>>))]
    public async Task Checkpoint04_Roles(Tuple<string, string> data, Tuple<string, string, IEnumerable<string>, IEnumerable<string>> expected)
    {
        // Arrange

        //Act
        var response = await _client.GetAsync($"{data.Item1}/{data.Item2}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(response);
        Assert.NotNull(content);
        var form = content.GetForm();
        var submit = form.GetSubmitButton();
        var responsePost = await _client.SendAsync(
                form,
                submit);

        // Assert 2
        Assert.Equal(HttpStatusCode.Redirect, responsePost.StatusCode);
        Assert.NotNull(responsePost.Headers.Location);
        var responseRedirect = await _client.GetAsync($"{responsePost.Headers.Location.OriginalString}");
        Assert.True(responseRedirect.IsSuccessStatusCode);
        var resultPage = await HtmlHelpers.GetDocumentAsync(responseRedirect);
        Assert.NotNull(resultPage);
        var body = resultPage.Body;
        Assert.NotNull(body);
        var textContent = body.TextContent;
        Assert.Contains(expected.Item1, textContent);
        Assert.Contains(expected.Item2, textContent);
        if (expected.Item3 is not null)
        {
            Assert.All(expected.Item3, e => Assert.Contains(e, textContent));
        }
        if (expected.Item4 is not null)
        {
            Assert.All(expected.Item4, e => Assert.DoesNotContain(e, textContent));
        }
    }
}