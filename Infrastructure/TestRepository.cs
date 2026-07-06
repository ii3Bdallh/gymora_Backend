using Domain.Events;
using Domain.Model;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class TestRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public TestRepository(ApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task AddChildAndUpdateCountAsync(int parentId, string childName)
    {
        // 1. جلب الأب للتأكد من وجوده ولتحديث العداد
        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.Id == parentId);
        if (parent == null) throw new Exception("Parent not found!");

        // 2. إنشاء الابن الجديد وإضافته للـ Context
        var child = new Child
        {
            Name = childName,
            ParentId = parentId
        };
        _context.Children.Add(child);

        // 3. تحديث العداد فوراً (Strong Consistency - فوق الـ SaveChanges)
        parent.NumberOfChildren += 1;

        // 4. حفظ الـ Event في جدول الـ Outbox (مش هيتبعت فوراً، هيستنى الـ SaveChanges)
        // هنفترض إيميل الأب ثابت للتجربة parent@example.com
        await _publishEndpoint.Publish(new ChildAddedEvent(parent.Id, child.Name, "Abdallhmamdouh079@gmail.com"));

        // 5. حفظ كل شيء في نفس الـ Transaction
        // إضافة الابن + تحديث العداد + حفظ الـ Event في الـ Outbox = كتلة واحدة تنجح معاً أو تفشل معاً
        await _context.SaveChangesAsync();
    }
}
