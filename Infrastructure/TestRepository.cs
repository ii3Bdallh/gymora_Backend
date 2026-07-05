using Domain.Events;
using Domain.Model;
using Infrastructure.Persistence;

namespace Infrastructure;

public class TestRepository
{
    private readonly ApplicationDbContext _context;

    public TestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task TestEvent(
        int userId,
        string message)
    {
        var test = new TestEntity
        {
            UserId = userId,
            Message = message
        };

        test.AddDomainEvent(
            new TestEvent(
                userId,
                "Abdallhmamdouh079@gmail.com",
                message));

        await _context.Tests.AddAsync(test);

        await _context.SaveChangesAsync();
    }
}
