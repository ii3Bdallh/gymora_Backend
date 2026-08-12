using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Model;
using AutoMapper;
using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Application.Service
{
    public class SessionExerciseService : ISessionExerciseService
    {
        private readonly ISessionExerciseRepo _repo;
        private readonly ISessionRepo _sessionRepo;
        private readonly IExerciseRepo _exerciseRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CurrentUser _currentUser;

        public SessionExerciseService(
            ISessionExerciseRepo repo,
            ISessionRepo sessionRepo,
            IExerciseRepo exerciseRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            CurrentUser currentUser)
        {
            _repo = repo;
            _sessionRepo = sessionRepo;
            _exerciseRepo = exerciseRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        private async Task VerifySessionOwnershipAsync(int sessionId, CancellationToken cancellationToken)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId, false, cancellationToken);
            if (session == null)
                throw new NotFoundException($"Session with ID {sessionId} was not found.");

            if (session.CreatedById != _currentUser.UserId && !_currentUser.IsSuperAdmin)
                throw new ForbiddenException("You do not have access to modify this session.");
        }

        public async Task<IEnumerable<SessionExerciseRDTO>> AddRangeAsync(IEnumerable<SessionExerciseCDTO> dtos, CancellationToken cancellationToken)
        {
            // 1. Verify session ownership once per unique SessionId
            var uniqueSessionIds = dtos.Select(d => d.SessionId).Distinct().ToList();
            foreach (var sessionId in uniqueSessionIds)
            {
                await VerifySessionOwnershipAsync(sessionId, cancellationToken);
            }

            // 2. Load all unique exercises at once to verify they exist and get their names
            var exerciseIds = dtos.Where(d => d.ExerciseId.HasValue).Select(d => d.ExerciseId!.Value).Distinct().ToList();
            var exercisesDict = new Dictionary<int, Exercise>();
            if (exerciseIds.Any())
            {
                var exercises = await _exerciseRepo.DbSet
                    .Where(e => exerciseIds.Contains(e.Id))
                    .ToListAsync(cancellationToken);

                if (exercises.Count != exerciseIds.Count)
                {
                    var foundIds = exercises.Select(e => e.Id).ToHashSet();
                    var missingId = exerciseIds.First(id => !foundIds.Contains(id));
                    throw new NotFoundException($"Exercise with ID {missingId} was not found.");
                }

                exercisesDict = exercises.ToDictionary(e => e.Id);
            }

            // 3. Map and save entities
            var addedEntities = new List<SessionExercise>();
            foreach (var dto in dtos)
            {
                var entity = _mapper.Map<SessionExercise>(dto);

                // Auto-fill exercise name if it's not provided but ExerciseId is set
                if (entity.ExerciseId.HasValue && string.IsNullOrEmpty(entity.ExerciseName))
                {
                    if (exercisesDict.TryGetValue(entity.ExerciseId.Value, out var exercise))
                    {
                        entity.ExerciseName = exercise.Name;
                    }
                }

                var added = await _repo.AddAsync(entity, cancellationToken);
                addedEntities.Add(added);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<IEnumerable<SessionExerciseRDTO>>(addedEntities);
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            var exercises = await _repo.DbSet.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
            if (exercises.Count != ids.Count())
                throw new NotFoundException("One or more session exercises were not found.");

            // Verify session ownership once per unique SessionId
            var sessionIds = exercises.Select(e => e.SessionId).Distinct().ToList();
            foreach (var sessionId in sessionIds)
            {
                await VerifySessionOwnershipAsync(sessionId, cancellationToken);
            }

            foreach (var entity in exercises)
            {
                await _repo.DeleteAsync(entity, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
