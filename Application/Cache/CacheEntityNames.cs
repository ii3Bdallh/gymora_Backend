using Domain.Model;

namespace Application.Cache;

public static class CacheEntityNames
{
    public const string SubscriptionPlan = "subscription_plan";
    public const string PlanPrice = "plan_price";
    public const string Notification = "notification";
    public const string UserDevice = "user_device";

    // Script will Add After Here CacheEntityNames
    public const string Gym = "gym";

    public const string CouponRedemption = "couponredemption";

    public const string OwnerSubscription = "ownersubscription";

    public const string Coupon = "coupon";

    public const string PaymentRequest = "paymentrequest";


    public static string ForType<T>() where T : class
    {
        var type = typeof(T);
        if (type == typeof(SubscriptionPlan)) return SubscriptionPlan;
        if (type == typeof(PlanPrice)) return PlanPrice;
        if (type == typeof(Notification)) return Notification;
        if (type == typeof(UserDevice)) return UserDevice;

        // Script will Add After Here ForType
        if (type == typeof(Gym)) return Gym;

        if (type == typeof(CouponRedemption)) return CouponRedemption;

        if (type == typeof(OwnerSubscription)) return OwnerSubscription;

        if (type == typeof(Coupon)) return Coupon;

        if (type == typeof(PaymentRequest)) return PaymentRequest;

        return type.Name.ToLower();
    }
}