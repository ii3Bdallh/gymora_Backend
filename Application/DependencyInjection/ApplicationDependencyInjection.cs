using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services , IConfiguration configuration)
    {
        // --- الإعدادات المشتركة لـ Firebase ---
        var firebaseCredentialPath = configuration["FirebaseConfig:CredentialFilePath"];

        if (string.IsNullOrWhiteSpace(firebaseCredentialPath) || !File.Exists(firebaseCredentialPath))
        {
            throw new InvalidOperationException($"Firebase credential file is missing or not configured correctly at: {firebaseCredentialPath}");
        }

        // إنشاء الـ Credential لمرة واحدة للهاردوير بالكامل
        var googleCredential = GoogleCredential.FromFile(firebaseCredentialPath);

        // 1. تهيئة الـ FirebaseApp المركزي للإشعارات (إذا لم يكن مهيأ مسبقاً)
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = googleCredential
            });
        }



        services.AddSingleton(googleCredential);
        // --- تسجيل الخدمات (DI Registration) ---

        services.AddApplicationServices();

        return services;
    }
}
    