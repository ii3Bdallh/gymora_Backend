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
using Domain.Enum;

namespace Infrastructure.Repo
{
    public class PaymentRequestRepo(ApplicationDbContext context, ILogger<PaymentRequestRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseRepo<PaymentRequest>(context, logger, queryCache, currentUser), IPaymentRequestRepo
    {
        public async Task<bool> HasPendingRequestAsync(int UserId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(x =>
                x.CreatedById == UserId &&
                x.Status == PaymentRequestStatus.Pending &&
                x.IsActive, ct);
        }
    }
}