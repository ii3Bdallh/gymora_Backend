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
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repo
{
    public class CouponRepo(ApplicationDbContext context, ILogger<CouponRepo> logger, QueryCache queryCache)
    : BaseRepo<Coupon>(context, logger, queryCache), ICouponRepo
    {
        public  Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return  DbSet
                .FirstOrDefaultAsync(x => x.Code.ToUpper() == code.ToUpper(), ct);
        }


    }
}