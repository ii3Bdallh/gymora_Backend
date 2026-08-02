using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Gymora.Contracts.Authentication;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepo userRepo, IUnitOfWork unitOfWork)
        {
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileRDTO> GetUserProfileAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByIdAsync(userId, false, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User profile not found.");
            }

            var roles = await _userRepo.GetUserRolesAsync(userId, cancellationToken);
            var platformRole = roles.FirstOrDefault() ?? "User";

            return new UserProfileRDTO(
                UserId: user.Id.ToString(),
                FirstName: user.FirstName ?? string.Empty,
                LastName: user.LastName ?? string.Empty,
                FullName: user.PersonName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                PhoneNumber: user.PhoneNumber,
                ProfilePictureUrl: user.ProfileImageUrl,
                CreatedAt: user.CreatedOn,
                PlatformRole: platformRole
            );
        }

        public async Task<UserProfileRDTO> UpdateUserProfileAsync(int userId, UserProfileUDTO updateDto, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByIdAsync(userId, true, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User profile not found.");
            }

            if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber) && user.PhoneNumber != updateDto.PhoneNumber)
            {
                bool isPhoneInUse = await _userRepo.IsPhoneNumberUsedByOtherUserAsync(updateDto.PhoneNumber, userId, cancellationToken);
                if (isPhoneInUse)
                {
                    throw new ConflictException("This phone number is already registered to another account.");
                }
                user.PhoneNumber = updateDto.PhoneNumber;
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.PersonName = $"{updateDto.FirstName} {updateDto.LastName}";
            
            if (!string.IsNullOrWhiteSpace(updateDto.ProfilePictureUrl))
            {
                // The URL string is directly passed and saved as per user instruction.
                user.ProfileImageUrl = updateDto.ProfilePictureUrl;
            }

            await _userRepo.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roles = await _userRepo.GetUserRolesAsync(userId, cancellationToken);
            var platformRole = roles.FirstOrDefault() ?? "User";

            return new UserProfileRDTO(
                UserId: user.Id.ToString(),
                FirstName: user.FirstName ?? string.Empty,
                LastName: user.LastName ?? string.Empty,
                FullName: user.PersonName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                PhoneNumber: user.PhoneNumber,
                ProfilePictureUrl: user.ProfileImageUrl,
                CreatedAt: user.CreatedOn,
                PlatformRole: platformRole
            );
        }
    }
}
