 🏠 نظام إدارة دار المغتربات

 📖 الوصف
نظام متكامل لإدارة دار سكن المغتربات يشمل إدارة المقيمات، المدفوعات، التقارير، والإشعارات الآلية.

     البدء السريع

  المتطلبات المسبقة
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 أو VS Code

    التثبيت
1. استنساخ المستودع
2. استعادة الحزم: `dotnet restore`
3. تحديث قاعدة البيانات: `dotnet ef database update`
4. التشغيل: `dotnet run`

   🔐 الدخول
- **المستخدم**: `admin@mughtaribat.com`
- **كلمة المرور**: '!`

 
 📁 هيكل المشروع
MughtaribatHouse/
├── Controllers/   API
├── Data/ 
├── Models/ 
├── Pages/ 

MughtaribatHouse/
├── 📂 Controllers/       (API)
├── 📂 Data/               
├── 📂 Models/          
├── 📂 Pages/                Razor
├── 📂 Services/           
├── 📂 BackgroundServices/  
├── 📂 wwwroot/     
└── 📄 Program.cs    /////////////////////////////////////////////////////////////////
<img width="11519" height="5939" alt="my tables jpg" src="https://github.com/user-attachments/assets/4682339e-57b7-4a0a-9e9b-6a95355d0224" />




























├── Services/ # منطق الأعمال
└── wwwroot/ # ملفات ثابتة
