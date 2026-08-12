using Domain.Enum;
using Domain.Model;
using Domain.Model.Auth;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Seed;

public sealed class IdentitySeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public IdentitySeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    public async Task InitializeAsync()
    {
        // 1. Seed Roles
        if (!await _roleManager.RoleExistsAsync(AppRole.SuperAdmin))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = AppRole.SuperAdmin });
        }

        if (!await _roleManager.RoleExistsAsync(AppRole.User))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = AppRole.User });
        }

        // 2. Seed Users
        var superAdminEmail = "abdallhmamdouh079@gmail.com";
        var user1Email = "abdallh.dev87@gmail.com";
        var user2Email = "abdallhmamdouh463@gmail.com";
        var password = "Gymora@Secure2026!";

        var superAdmin = await _userManager.FindByEmailAsync(superAdminEmail);
        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                PersonName = "Super Admin",
                EmailConfirmed = true,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,

            };
            var result = await _userManager.CreateAsync(superAdmin, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(superAdmin, AppRole.SuperAdmin);
                await _userManager.AddToRoleAsync(superAdmin, AppRole.User);
            }
        }

        var user1 = await _userManager.FindByEmailAsync(user1Email);
        if (user1 == null)
        {
            user1 = new ApplicationUser
            {
                UserName = user1Email,
                Email = user1Email,
                PersonName = "User One",
                EmailConfirmed = true,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user1, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user1, AppRole.User);
            }
        }

        var user2 = await _userManager.FindByEmailAsync(user2Email);
        if (user2 == null)
        {
            user2 = new ApplicationUser
            {
                UserName = user2Email,
                Email = user2Email,
                PersonName = "User Two",
                EmailConfirmed = true,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user2, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user2, AppRole.User);
            }
        }

        // 3. Seed Subscription Plans and Plan Prices
        if (!await _context.SubscriptionPlan.AnyAsync())
        {
            var freePlan = new SubscriptionPlan
            {
                Name = "Free",
                Description = "Free basic plan",
                IsFree = true,
                MaxOwnedGyms = 1,
                MaxCoaches = 1,
                MaxMembers = 1,
                FeaturesJson = "[\"1 Gym\", \"1 Coaches\", \"1 Members\"]",
                CreatedOn = DateTime.UtcNow
            };

            var goldPlan = new SubscriptionPlan
            {
                Name = "Gold",
                Description = "Gold premium plan",
                IsFree = false,
                MaxOwnedGyms = 2,
                MaxCoaches = 2,
                MaxMembers = 2,
                FeaturesJson = "[\"2 Gyms\", \"2 Coaches\", \"2 Members\"]",
                CreatedOn = DateTime.UtcNow
            };

            _context.SubscriptionPlan.Add(freePlan);
            _context.SubscriptionPlan.Add(goldPlan);
            await _context.SaveChangesAsync();

            // Seed Prices
            var freePrice = new PlanPrice
            {
                PlanId = freePlan.Id,
                CountryCode = "US",
                CurrencyCode = "USD",
                DurationMonths = 12,
                Amount = 0,
                CreatedOn = DateTime.UtcNow,
            };

            var goldPriceMonthly = new PlanPrice
            {
                PlanId = goldPlan.Id,
                CountryCode = "US",
                CurrencyCode = "USD",
                DurationMonths = 1,
                Amount = 29.99m,
                CreatedOn = DateTime.UtcNow
            };

            var goldPriceYearly = new PlanPrice
            {
                PlanId = goldPlan.Id,
                CountryCode = "US",
                CurrencyCode = "USD",
                DurationMonths = 12,
                Amount = 299.99m,
                CreatedOn = DateTime.UtcNow
            };

            _context.PlanPrice.AddRange(freePrice, goldPriceMonthly, goldPriceYearly);
            await _context.SaveChangesAsync();
        }

        var goldPlanEntity = await _context.SubscriptionPlan.FirstOrDefaultAsync(x => x.Name == "Gold");
        var goldMonthlyPrice = await _context.PlanPrice.FirstOrDefaultAsync(x => x.PlanId == goldPlanEntity!.Id && x.DurationMonths == 1);

        // 4. Seed Payment Requests
        if (!await _context.PaymentRequest.AnyAsync() && goldPlanEntity != null && goldMonthlyPrice != null)
        {
            // Payment Request 1 (User 1 - Approved)
            var pr1 = new PaymentRequest
            {
                PlanId = goldPlanEntity.Id,
                PlanPriceId = goldMonthlyPrice.Id,
                OriginalAmount = 29.99m,
                DiscountAmount = 0m,
                FinalAmount = 29.99m,
                CurrencyCode = "USD",
                Status = PaymentRequestStatus.Approved,
                ReviewedBy = superAdmin.Id,
                ReviewedAt = DateTime.UtcNow,
                ReviewNotes = "Approved by seeder",
                CreatedById = user1.Id,
                CreatedOn = DateTime.UtcNow,
                StoredFilePath = "seeder-receipt-1.pdf",
                IsPublic = true
            };

            // Payment Request 2 (User 2 - Approved)
            var pr2 = new PaymentRequest
            {
                PlanId = goldPlanEntity.Id,
                PlanPriceId = goldMonthlyPrice.Id,
                OriginalAmount = 29.99m,
                DiscountAmount = 0m,
                FinalAmount = 29.99m,
                CurrencyCode = "USD",
                Status = PaymentRequestStatus.Approved,
                ReviewedBy = superAdmin.Id,
                ReviewedAt = DateTime.UtcNow,
                ReviewNotes = "Approved by seeder",
                CreatedById = user2.Id,
                CreatedOn = DateTime.UtcNow,
                StoredFilePath = "seeder-receipt-2.pdf",
                IsPublic = true
            };

            // Payment Request 3 (User 1 - Declined/Rejected)
            var pr3 = new PaymentRequest
            {
                PlanId = goldPlanEntity.Id,
                PlanPriceId = goldMonthlyPrice.Id,
                OriginalAmount = 29.99m,
                DiscountAmount = 0m,
                FinalAmount = 29.99m,
                CurrencyCode = "USD",
                Status = PaymentRequestStatus.Rejected,
                ReviewedBy = superAdmin.Id,
                ReviewedAt = DateTime.UtcNow,
                RejectionReason = "Declined by seeder",
                CreatedById = user1.Id,
                CreatedOn = DateTime.UtcNow,
                StoredFilePath = "seeder-receipt-3.pdf",
                IsPublic = true
            };

            // Payment Request 4 (User 2 - Declined/Rejected)
            var pr4 = new PaymentRequest
            {
                PlanId = goldPlanEntity.Id,
                PlanPriceId = goldMonthlyPrice.Id,
                OriginalAmount = 29.99m,
                DiscountAmount = 0m,
                FinalAmount = 29.99m,
                CurrencyCode = "USD",
                Status = PaymentRequestStatus.Rejected,
                ReviewedBy = superAdmin.Id,
                ReviewedAt = DateTime.UtcNow,
                RejectionReason = "Declined by seeder",
                CreatedById = user2.Id,
                CreatedOn = DateTime.UtcNow,
                StoredFilePath = "seeder-receipt-4.pdf",
                IsPublic = true
            };

            _context.PaymentRequest.AddRange(pr1, pr2, pr3, pr4);
            await _context.SaveChangesAsync();

            // Seed Owner Subscriptions for Approved Payment Requests
            var os1 = new OwnerSubscription
            {
                PlanId = goldPlanEntity.Id,
                PlanPriceId = goldMonthlyPrice.Id,
                PaymentRequestId = pr1.Id,
                AmountPaid = 29.99m,
                CurrencyCode = "USD",
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(25),
                CreatedById = user1.Id,
                CreatedOn = DateTime.UtcNow
            };

            var os2 = new OwnerSubscription
            {
                PlanId = goldPlanEntity.Id,
                PlanPriceId = goldMonthlyPrice.Id,
                PaymentRequestId = pr2.Id,
                AmountPaid = 29.99m,
                CurrencyCode = "USD",
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(28),
                CreatedById = user2.Id,
                CreatedOn = DateTime.UtcNow
            };

            _context.OwnerSubscription.AddRange(os1, os2);
            await _context.SaveChangesAsync();
        }

        // 5. Seed Gyms
        if (!await _context.Gym.AnyAsync())
        {
            var gym1 = new Gym
            {
                Name = "User 1 Gym Alpha",
                Description = "First gym owned by user 1",
                OwnerUserId = user1.Id,
                OwnerUser = user1,
                Status = GymStatus.Active,
                StoredFilePath = "seeder"
            };

            var gym2 = new Gym
            {
                Name = "User 1 Gym Beta",
                Description = "Second gym owned by user 1",
                OwnerUserId = user1.Id,
                OwnerUser = user1,
                Status = GymStatus.Active,
                StoredFilePath = "seeder"
            };

            var gym3 = new Gym
            {
                Name = "User 2 Gym Alpha",
                Description = "First gym owned by user 2",
                OwnerUserId = user2.Id,
                OwnerUser = user2,
                Status = GymStatus.Active,
                StoredFilePath = "seeder"
            };

            _context.Gym.AddRange(gym1, gym2, gym3);
            await _context.SaveChangesAsync();

            // Seed GymPerson Owner records
            var gp1 = new GymPerson
            {
                GymId = gym1.Id,
                UserId = user1.Id,
                PersonType = PersonType.Owner,
                Name = "User One",
                PhoneNumber = "1111111111",
                Email = user1.Email,
                AccessStatus = GymPersonAccessStatus.Active,
                CreatedById = user1.Id,
                CreatedOn = DateTime.UtcNow
            };

            var gp2 = new GymPerson
            {
                GymId = gym2.Id,
                UserId = user1.Id,
                PersonType = PersonType.Owner,
                Name = "User One",
                PhoneNumber = "1111111112",
                Email = user1.Email,
                AccessStatus = GymPersonAccessStatus.Active,
                CreatedById = user1.Id,
                CreatedOn = DateTime.UtcNow
            };

            var gp3 = new GymPerson
            {
                GymId = gym3.Id,
                UserId = user2.Id,
                PersonType = PersonType.Owner,
                Name = "User Two",
                PhoneNumber = "2222222221",
                Email = user2.Email,
                AccessStatus = GymPersonAccessStatus.Active,
                CreatedById = user2.Id,
                CreatedOn = DateTime.UtcNow
            };

            _context.GymPerson.AddRange(gp1, gp2, gp3);
            await _context.SaveChangesAsync();
        }
    }
}
