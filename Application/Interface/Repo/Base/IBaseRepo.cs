using System.Linq.Expressions;
using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Interface;
using Domain.Model.Base;

namespace Application.Interface.Repo
{




    public interface IBaseRepo<T> where T : class, IBaseEntity
    {

        // جلب كل العناصر (للقوائم الصغيرة)
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, Func<IQueryable<T>, IQueryable<T>>? include = null);

        // بناء الـ Query الأساسية مع الفلاتر والسيرش والـ Includes
        IQueryable<T> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IQueryable<T>>? include = null);

        // جلب صفحة معينة (Pagination) ودعم الـ Includes للـ Children
        Task<PaginatedRes<T>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IQueryable<T>>? include = null);

        // جلب عنصر واحد بـ ID مع الـ Includes بتوعه
        Task<T?> GetByIdAsync(
        int id,
        bool isActive = true,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        Func<IQueryable<T>, IQueryable<T>>? include = null);

        // جلب عنصر واحد بـ ID مع الـ Includes بتوعه
        Task<T?> GetByIdIgnoringSecurityAsync(
        int id,
        bool isActive = true,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        Func<IQueryable<T>, IQueryable<T>>? include = null);

        // عمليات التعديل جوه الـ Memory (بدون SaveChanges)
        Task<T> AddAsync(T item, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T item, CancellationToken cancellationToken = default);

        // تحديث الـ Parent ومزامنة الـ Children بتوعه (إضافة/تعديل/مسح للـ Children)
        // Task UpdateWithChildrenAsync<TChild>(T parentItem, Expression<Func<T, IEnumerable<TChild>>> childCollectionExpression) where TChild : class;

        // الحذف المؤقت (IsActive = false)
        Task<T> DeleteAsync(T item, CancellationToken cancellationToken = default);

        // الحذف المؤقت للـ Parent وكل الـ Children المحددين معاه
        // Task<T?> DeleteWithChildrenAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<T, IEnumerable<object>>>[] childCollections);

        // الحذف النهائي من الداتا بيز
        Task<T> HardDeleteAsync(T item, CancellationToken cancellationToken = default);
    }
}