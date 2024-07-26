using DeviceManager.Data;
using DeviceManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Savonia.xUnit.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;

namespace tests;

public class UnitTest : AppTestBase
{
    public UnitTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Checkpoint01_Device()
    {

        Device d = new Device();
        Assert.NotNull(d);
        Type t = d.GetType();
        Assert.NotNull(t);
        var pn = t.GetProperty("Name");
        Assert.NotNull(pn);
        Assert.NotNull(t.GetProperty("Id"));
        Assert.NotNull(t.GetProperty("UserId"));
        Assert.NotNull(t.GetProperty("Description"));
        Assert.NotNull(t.GetProperty("DateAdded"));

        Assert.True(Attribute.IsDefined(pn, typeof(RequiredAttribute)));
        Assert.True(Attribute.IsDefined(pn, typeof(MaxLengthAttribute)));
        var attr = Attribute.GetCustomAttribute(pn, typeof(MaxLengthAttribute));
        Assert.NotNull(attr);
        MaxLengthAttribute mlAttr = (MaxLengthAttribute)attr;
        Assert.NotNull(mlAttr);
        Assert.Equal(50, mlAttr.Length);
    }

    [Fact]
    public void Checkpoint01_DeviceManagerAppUser()
    {
        DeviceManagerAppUser a = new DeviceManagerAppUser();
        Assert.NotNull(a);
        Type t = a.GetType();
        Assert.NotNull(t);
        var pd = t.GetProperty("Devices");
        Assert.NotNull(pd);
        var pu = t.GetProperty("UserRoles");
        Assert.NotNull(pu);
        Assert.True(typeof(IdentityUser).IsAssignableFrom(typeof(DeviceManagerAppUser)));
    }

    [Fact]
    public void Checkpoint01_DeviceViewModel()
    {

        DeviceViewModel d = new DeviceViewModel();
        Assert.NotNull(d);
        Type t = d.GetType();
        Assert.NotNull(t);
        var pn = t.GetProperty("Name");
        Assert.NotNull(pn);
        Assert.NotNull(t.GetProperty("Id"));
        Assert.NotNull(t.GetProperty("UserId"));
        Assert.NotNull(t.GetProperty("Description"));
        Assert.NotNull(t.GetProperty("DateAdded"));

        Assert.True(Attribute.IsDefined(pn, typeof(RequiredAttribute)));
        Assert.True(Attribute.IsDefined(pn, typeof(MaxLengthAttribute)));
        var attr = Attribute.GetCustomAttribute(pn, typeof(MaxLengthAttribute));
        Assert.NotNull(attr);
        MaxLengthAttribute mlAttr = (MaxLengthAttribute)attr;
        Assert.NotNull(mlAttr);
        Assert.Equal(50, mlAttr.Length);
    }

    [Fact]
    public void Checkpoint01_ApplicationDbContext_1()
    {
        // Arrange
        Type t = typeof(ApplicationDbContext);

        // Act
        var pn = t.GetProperty("Devices");

        // Assert
        Assert.NotNull(pn);
        Assert.Single(t.GetConstructors());
        Assert.True(typeof(IdentityDbContext<DeviceManagerAppUser, IdentityRole, string>).IsAssignableFrom(typeof(ApplicationDbContext)));
    }

    [Fact]
    public async void Checkpoint01_ApplicationDbContext_2()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseInMemoryDatabase("data");

        // Act
        var db = new ApplicationDbContext(optionsBuilder.Options);

        // Assert
        Assert.NotNull(db);
        var devices = await db.Devices.ToListAsync();
        Assert.NotNull(devices);
    }
}