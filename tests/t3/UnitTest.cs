using AngleSharp.Html.Dom;
using DeviceManager.Controllers;
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
    private readonly ApplicationDbContext _db;

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
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandlerUser>(
                        "Test", options => { });
            });
        })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        string connectionstring = "Data Source=app.db";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(connectionstring);

        _db = new ApplicationDbContext(optionsBuilder.Options);
    }

    [Theory]
    [JsonFileData("testdata.json", "auth", typeof(string), typeof(Tuple<int, string>))]
    public async void Checkpoint03_Anonymous(string data, Tuple<int, string> expected)
    {
        // Act
        var response = await _anonymousClient.GetAsync(data);

        // Assert
        Assert.Equal(expected.Item1, (int)response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(expected.Item2, response.Headers.Location.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Checkpoint03_DevicesController()
    {
        var t = typeof(DevicesController);
        Assert.True(Attribute.IsDefined(t, typeof(AuthorizeAttribute)));
        var attr = Attribute.GetCustomAttribute(t, typeof(AuthorizeAttribute));
        Assert.NotNull(attr);
        Assert.IsAssignableFrom<AuthorizeAttribute>(attr);
        var aa = (AuthorizeAttribute)attr;
        Assert.NotNull(aa);
    }

    [Theory]
    [JsonFileData("testdata.json", "devices", typeof(string), typeof(Tuple<IEnumerable<Dictionary<string, string>>, int, int>))]
    public async Task Checkpoint03_Devices(string data, Tuple<IEnumerable<Dictionary<string, string>>, int, int> expected)
    {
        // Arrange

        //Act
        var response = await _client.GetAsync($"{data}");

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
        Assert.All(expected.Item1, e => Assert.Contains(rowContent, r => r.Contains(e["Name"], StringComparison.OrdinalIgnoreCase) && r.Contains(e["Description"], StringComparison.OrdinalIgnoreCase)));

        // has create link
        var allLinks = content.QuerySelectorAll("a");
        Assert.Contains(allLinks, l => l.HasAttribute(null, "href") && l.GetAttribute("href").Contains("create", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [JsonFileData("testdata.json", "crud", typeof(Tuple<string, string>), typeof(Tuple<int>))]
    public async Task Checkpoint03_CRUDDevice(Tuple<string, string> data, Tuple<int> expected)
    {
        // Arrange
        Random rnd = new Random();
        var device = new Dictionary<string, string>
                {
                    {"UserId", $"{data.Item2}"},
                    {"Name", $"a new device-{rnd.Next()}"},
                    {"Description", $"with desc-{rnd.Next()}"}
                };
        var editedName = $"name-edited-{rnd.Next()}";
        //Act
        var response = await _client.GetAsync($"{data.Item1}/create");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(response);
        var form = content.GetForm();
        var submit = form.GetSubmitButton();
        var responsePost = await _client.SendAsync(
                form,
                submit,
                new Dictionary<string, string>
                {
                    {"Name", device["Name"]},
                    {"Description", device["Description"]}
                });

        // Assert 2
        if (responsePost.StatusCode == HttpStatusCode.Redirect)
        {
            var responseRedirect = await _client.GetAsync($"{responsePost.Headers.Location.OriginalString}");
            var resultPage = await responseRedirect.Content.ReadAsStringAsync();
            // WriteLine(resultPage);

            Assert.True(responseRedirect.IsSuccessStatusCode);
            Assert.Contains(device["Name"], resultPage);
            Assert.Contains(device["Description"], resultPage);
        }
        else
        {
            var resultPage = await responsePost.Content.ReadAsStringAsync();
            // WriteLine(resultPage);

            Assert.True(responsePost.IsSuccessStatusCode);
            Assert.Contains(device["Name"], resultPage);
            Assert.Contains(device["Description"], resultPage);
        }

        // Arrange 3
        var dbDevice = await _db.Devices.FirstOrDefaultAsync(d => d.Name == device["Name"]);

        // Assert 3
        Assert.NotNull(dbDevice);

        // Act 4
        var responseGetEdit = await _client.GetAsync($"{data.Item1}/edit/{dbDevice.Id}");

        // Assert 4
        Assert.Equal(HttpStatusCode.OK, responseGetEdit.StatusCode);

        // Act 5
        var contentEdit = await HtmlHelpers.GetDocumentAsync(responseGetEdit);
        var readonlyInputs = contentEdit.QuerySelectorAll("input[readonly]");
        form = contentEdit.GetForm();
        submit = form.GetSubmitButton();
        var responsePostEdit = await _client.SendAsync(
                form,
                submit,
                new Dictionary<string, string>
                {
                    {"Name", editedName},
                    {"Description", device["Description"]}
                });
        if (responsePostEdit.StatusCode == HttpStatusCode.Redirect)
        {
            var responseRedirect = await _client.GetAsync($"{responsePostEdit.Headers.Location.OriginalString}");
            var resultPageEdit = await responseRedirect.Content.ReadAsStringAsync();
            // WriteLine(resultPage);

            Assert.True(responseRedirect.IsSuccessStatusCode);
            Assert.Contains(editedName, resultPageEdit);
            Assert.Contains(device["Description"], resultPageEdit);
        }
        else
        {
            var resultPageEdit = await responsePostEdit.Content.ReadAsStringAsync();
            // WriteLine(resultPage);

            Assert.True(responsePostEdit.IsSuccessStatusCode);
            Assert.Contains(editedName, resultPageEdit);
            Assert.Contains(device["Description"], resultPageEdit);
        }

        // Assert 5
        Assert.NotNull(readonlyInputs);
        Assert.NotEmpty(readonlyInputs);
        Assert.True(readonlyInputs.Count() >= expected.Item1);
        Assert.NotNull(await _db.Devices.FirstOrDefaultAsync(d => d.Name == editedName));

        // Act 6
        var responseGetDelete = await _client.GetAsync($"{data.Item1}/delete/{dbDevice.Id}");

        // Assert 6
        Assert.Equal(HttpStatusCode.OK, responseGetDelete.StatusCode);

        // Act 7
        var contentDelete = await HtmlHelpers.GetDocumentAsync(responseGetDelete);
        form = contentDelete.GetForm();
        submit = form.GetSubmitButton();
        var responsePostDelete = await _client.SendAsync(
                form,
                submit);
        Assert.Equal(HttpStatusCode.Redirect, responsePostDelete.StatusCode);
        Assert.NotNull(responsePostDelete.Headers.Location);
        var responseDeleteRedirect = await _client.GetAsync($"{responsePostDelete.Headers.Location.OriginalString}");
        var resultPageDelete = await responseDeleteRedirect.Content.ReadAsStringAsync();
        Assert.True(responseDeleteRedirect.IsSuccessStatusCode);
        Assert.DoesNotContain(editedName, resultPageDelete);
        Assert.DoesNotContain(device["Description"], resultPageDelete);


        // Assert 7
        Assert.Null(await _db.Devices.FirstOrDefaultAsync(d => d.Name == editedName));
    }
}