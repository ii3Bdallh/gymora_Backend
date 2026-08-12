using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class BodyMeasurementRepo(ApplicationDbContext context, ILogger<BodyMeasurementRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseAuditableRepo<BodyMeasurement>(context, logger, queryCache, currentUser), IBodyMeasurementRepo
    {

    }
}
