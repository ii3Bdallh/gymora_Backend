using Application.Interface.Repo;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo;

public class TrainerCertificateRepo(ApplicationDbContext context, ILogger<TrainerCertificateRepo> logger, QueryCache queryCache)
    : BaseRepo<TrainerCertificate>(context, logger, queryCache), ITrainerCertificateRepo
{
}
