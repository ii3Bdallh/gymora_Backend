using Domain.Model.Base;

namespace Domain.Model;


// جدول الأب
public class Parent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int NumberOfChildren { get; set; } // العداد اللي هيتحدث فوراً
}

// جدول الابن
public class Child
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ParentId { get; set; }
}
