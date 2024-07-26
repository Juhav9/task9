using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Data;

// TODO: modify to use DeviceManagerAppUser as identity user
public class ApplicationDbContext : IdentityDbContext<DeviceManagerAppUser, IdentityRole, string>
{
    /*
     * See documents:
     * - Add navigation properties, https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-6.0#add-navigation-properties
     * - Add all User navigation properties, https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-6.0#add-all-user-navigation-properties
     */

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // TODO: implement required DbSets to enable access to Devices table
    public virtual DbSet<Device> Devices { get; set; } = null!;
    public virtual DbSet<DeviceManagerAppUser> AppUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TODO: set Device properties and navigation property
        modelBuilder.Entity<Device>(d=>
        {
            d.HasKey(d=>d.Id);
            d.Property(d => d.UserId).HasMaxLength(450);
            d.Property(d=>d.Name).HasMaxLength(50);
            d.Property(d => d.Description);
            d.Property(d => d.DateAdded);

            d.HasOne(d=>d.User)
                .WithMany(e=>e.Devices)
                .HasForeignKey(e=>e.UserId)
                .IsRequired();
        });
        // TODO: set DeviceManagerAppUser navigation property to UserRole join table
        modelBuilder.Entity<DeviceManagerAppUser>(d =>
		{
            d.HasMany(e => e.UserRoles)
			    .WithOne()
				.HasForeignKey(e => e.UserId)
				.IsRequired();

            d.HasMany(e=>e.Devices)
                .WithOne(e=>e.User)
                .HasForeignKey(e=>e.UserId)
                .IsRequired();
		});
                    
    }
}
