using Application.Interface.Repo;
using Domain.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Repo.Base;
using Domain.Model.Auth;
using Google;
using Infrastructure.Persistence;
using Infrastructure.Cache;
using Application.Model;

namespace Infrastructure.Repo
{
    public class CouponRedemptionRepo(ApplicationDbContext context, ILogger<CouponRedemptionRepo> logger, QueryCache queryCache , CurrentUser currentUser)
    : BaseAuditableRepo<CouponRedemption>(context, logger, queryCache, currentUser), ICouponRedemptionRepo
    {
    }
}