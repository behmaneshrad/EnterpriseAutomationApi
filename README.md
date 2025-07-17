# EnterpriseAutomationApi

پروژه EnterpriseAutomation - نسخه ASP.NET Core همراه با Docker و EF Core

## 🐳 اجرای پروژه با Docker
برای اجرا و ساخت کانتینر از طریق Docker Compose، در ترمینال بزنید:

    docker compose up --build

## 🛠 مدیریت Migrations (Entity Framework Core)

### افزودن مایگریشن جدید
وارد پوشه `EnterpriseAutomation.Infrastructure` شوید و دستور زیر را بزنید:

    dotnet ef migrations add <MigrationName>

به‌جای `<MigrationName>` نام دلخواه مایگریشن را وارد کنید (مثلاً: `initdb`)

### اعمال تغییرات روی دیتابیس
برای به‌روزرسانی دیتابیس با آخرین مایگریشن:

    dotnet ef database update
