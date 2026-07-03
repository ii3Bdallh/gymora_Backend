using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Domain.Enum;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Data;
using Infrastructure.Repo.Base;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    /// <summary>
    /// Topic repository implementation for data access operations
    /// </summary>
    public class TopicRepo : BaseRepo<Topic> , ITopicRepo
    {
        public TopicRepo(AppDbContext context, ILogger logger, QueryCache queryCache)
                  : base(context, logger, queryCache)
        {
        }
    }
}
