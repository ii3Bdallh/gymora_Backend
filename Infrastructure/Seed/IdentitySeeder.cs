using Domain.Model.Auth;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seed;

public sealed class IdentitySeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IdentitySeeder(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        if (!await _roleManager.RoleExistsAsync(RoleConstants.SuperAdmin))
        {
            await _roleManager.CreateAsync(
                new ApplicationRole { Name = RoleConstants.SuperAdmin });
        }



        if (!await _roleManager.RoleExistsAsync(RoleConstants.User))
        {
            await _roleManager.CreateAsync(
                new ApplicationRole { Name = RoleConstants.User });
        }


    }
}
