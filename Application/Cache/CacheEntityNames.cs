using Domain.Model;

namespace Application.Cache;

public static class CacheEntityNames
{
    public const string SubscriptionPlan = "subscription_plan";
    public const string PlanPrice = "plan_price";
    public const string Notification = "notification";
    public const string UserDevice = "user_device";

    public static string ForType<T>() where T : class
    {
        var type = typeof(T);
        if (type == typeof(SubscriptionPlan)) return SubscriptionPlan;
        if (type == typeof(PlanPrice)) return PlanPrice;
        if (type == typeof(Notification)) return Notification;
        if (type == typeof(UserDevice)) return UserDevice;
        return type.Name.ToLower();
    }
}
