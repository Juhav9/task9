using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DeviceManager.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Savonia.xUnit.Helpers;
using Savonia.xUnit.Helpers.Infrastructure;
using System;
using System.Collections.Generic;
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
    public async void Checkpoint04_Anonymous(string data, Tuple<int, string> expected)
    {
        // Act
        var response = await _anonymousClient.GetAsync(data);

        // Assert
        Assert.Equal(expected.Item1, (int)response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(expected.Item2, response.Headers.Location.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [JsonFileData("testdata.json", "devices", typeof(Tuple<string, int>), typeof(Tuple<bool, int, string>))]
    public async Task Checkpoint05_Devices(Tuple<string, int> data, Tuple<bool, int, string> expected)
    {
        // Arrange

        //Act
        var response = await _client.GetAsync($"{data.Item1}/{data.Item2}");

        // Assert
        Assert.Equal(expected.Item2, (int)response.StatusCode);
        if (false == expected.Item1)
        {
            Assert.NotNull(response.Headers.Location);
            Assert.Contains(expected.Item3, response.Headers.Location.OriginalString, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [JsonFileData("testdata.json", "delete", typeof(Tuple<string, int, int>), typeof(Tuple<bool, int, string>))]
    public async Task Checkpoint05_Delete(Tuple<string, int, int> data, Tuple<bool, int, string> expected)
    {
        // Arrange

        //Act
        var response = await _client.GetAsync($"{data.Item1}/{data.Item2}");
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(response);
        var form = content.GetForm();
        Assert.NotNull(form);
        form.Action = $"{data.Item1}/{data.Item3}";
        var submit = form.GetSubmitButton();
        var responsePost = await _client.SendAsync(
                form,
                submit);

        Assert.Equal(expected.Item2, (int)responsePost.StatusCode);
        Assert.NotNull(responsePost.Headers.Location);
        Assert.Contains(expected.Item3, responsePost.Headers.Location.OriginalString, StringComparison.OrdinalIgnoreCase);
        if (expected.Item1)
        {
            Assert.Null(await _db.Devices.FirstOrDefaultAsync(d => d.Id == data.Item2));
        }
        else
        {
            Assert.NotNull(await _db.Devices.FirstOrDefaultAsync(d => d.Id == data.Item2));
        }
    }
}