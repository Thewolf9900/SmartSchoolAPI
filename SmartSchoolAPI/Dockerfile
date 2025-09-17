# المرحلة 1: بناء المشروع
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات .csproj واستعادة الحزم
COPY ["SmartSchoolAPI/SmartSchoolAPI.csproj", "SmartSchoolAPI/"]
RUN dotnet restore "SmartSchoolAPI/SmartSchoolAPI.csproj"

# نسخ بقية الكود
COPY . .
WORKDIR "/src/SmartSchoolAPI"

# بناء ونشر التطبيق في مجلد واحد
# ✨ التعديل الأول: أزلنا /p:UseAppHost=false للسماح بإنشاء الملف التنفيذي
RUN dotnet publish "SmartSchoolAPI.csproj" -c Release -o /app/publish

# المرحلة 2: إنشاء الصورة النهائية
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# ✨ التعديل الثاني: تغيير نقطة الدخول لتشغيل الملف التنفيذي مباشرة
ENTRYPOINT ["./SmartSchoolAPI"]
