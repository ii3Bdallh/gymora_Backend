
namespace Domain.Events;


// الـ Event اللي هيتبعت في الخلفية بعد إضافة الابن بنجاح
public record ChildAddedEvent(int ParentId, string ChildName, string ParentEmail);
