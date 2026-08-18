using Application.DTO.Pagintion;
using Domain.Model;
using Domain.Model.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IUsersRepo 
    {
            
        // إرجاع IQueryable خام بدون أي فلترة (Active/Includes/Ordering)
        // تستخدم في الـ Services اللي عايزة تبني الـ Query بنفسها (زي BaseReadService)
        // IQueryable<ApplicationUser> GetQueryable(bool trackChanges = false);
        // جلب كل العناصر (للقوائم الصغيرة)
        Task<IEnumerable<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default, Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null);

        // بناء الـ Query الأساسية مع الفلاتر والسيرش والـ Includes
        IQueryable<ApplicationUser> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null);

        // جلب صفحة معينة (Pagination) ودعم الـ Includes للـ Children
        Task<PaginatedRes<ApplicationUser>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null);

        // جلب عنصر واحد بـ ID مع الـ Includes بتوعه
        Task<ApplicationUser?> GetByIdAsync(
        int id,
        bool isActive = true,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null);

        Task<ApplicationUser?> GetByIdDetailsAsync(
            int id,
            CancellationToken cancellationToken = default);

        // جلب عنصر واحد بـ ID مع الـ Includes بتوعه
        Task<ApplicationUser?> GetByIdIgnoringSecurityAsync(
        int id,
        bool isActive = true,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null);

    }
}