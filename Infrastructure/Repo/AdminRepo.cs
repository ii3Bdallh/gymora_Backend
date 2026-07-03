using Application.Interface.Repo;
using Application.Interface.Service;
using Domain.Model;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.DTO;

namespace Infrastructure.Repo.Entity
{
    public class AdminRepo : IAdminRepo
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminRepo> _logger;

        public AdminRepo(
            AppDbContext appDbContext,
            UserManager<AppUser> userManager,
            INotificationService notificationService,
            ILogger<AdminRepo> logger)
        {
            _context = appDbContext;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<AppUser>> GetAllAdminsWithDeviceTokensAsync(CancellationToken cancellationToken = default)
        {
            var adminRoleIds = await _context.Roles
                .Where(r => r.Name == nameof(RoleType.Owner) || r.Name == nameof(RoleType.Admin))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            var adminUserIds = await _context.UserRoles
                .Where(ur => adminRoleIds.Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var admins = await _context.Users
                .Where(u => adminUserIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            return admins;
        }

        public async Task<AppUser?> GetAdminByIdWithDeviceTokenAsync(int adminId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == adminId, cancellationToken);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(nameof(RoleType.Owner)) || roles.Contains(nameof(RoleType.Admin)))
                return user;

            return null;
        }

 



 
    }
}
