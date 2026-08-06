using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Enum;
using Domain.Events;
using Domain.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class CoachAssignmentService : BaseService<CoachAssignment, CoachAssignmentRDTO, CoachAssignmentCDTO, CoachAssignmentUDTO>, ICoachAssignmentService
{


    private readonly IGymPersonRepo _gymPersonRepo;

    public CoachAssignmentService(
        ICoachAssignmentRepo repo,
        IGymPersonRepo gymPersonRepo,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService,
        IPublishEndpoint publishEndpoint,
        CurrentUser currentUser,
        ILogger<CoachAssignmentService> logger)
        : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
    {
        _gymPersonRepo = gymPersonRepo;
    }

    protected override async Task BeforeAddAsync(CoachAssignmentCDTO dto, CancellationToken cancellationToken)
    {

        if (CurrentGymId != dto.GymId)
            throw new ForbiddenException("You are not authorized to perform this action.");

        GymPerson? member = await _gymPersonRepo.GetByIdAsync(dto.MemberId, false, cancellationToken);
        if (member is null)
            throw new NotFoundException($"Member with ID {dto.MemberId} was not found.");

        GymPerson? coach = await _gymPersonRepo.GetByIdAsync(dto.CoachStaffId, false, cancellationToken);
        if (coach is null || coach.PersonType == PersonType.Member)
            throw new NotFoundException($"Coach with ID {dto.CoachStaffId} was not found.");

    }



}
