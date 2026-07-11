using Application.Interface.Service;
using Application.Interface.Service.Entity;
using Application.Interface.Service.Shared;
using Application.Service;
using Application.Service.Entity;
using Application.Service.shared;
using Application.Service.Shared; // تأكد من اسم الـ namespace الخاص بالـ Storage الجديد
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Google.Cloud.Storage.V1;


namespace Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services, IConfiguration configuration) // قمنا بتمرير الـ configuration هنا
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

            // 2. إنشاء وتجسبل الـ StorageClient كـ Singleton لرفع الملفات
            var storageClient = StorageClient.Create(googleCredential);
            services.AddSingleton(storageClient);

            services.AddSingleton(googleCredential);
            // --- تسجيل الخدمات (DI Registration) ---

            // Auth & Core Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();

            // Notification Service
            services.AddScoped<INotificationService, NotificationService>();


            return services;
        }
    }
}