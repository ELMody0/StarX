# StarX

تطبيق سطح مكتب لويندوز لـ **تنظيف ذكي + تراخيص + تحديث ذاتي** — بواجهة عربية/إنجليزية فاخرة (بدون إطار، دوك بأسلوب iOS).

**الإصدار الحالي:** [VERSION](VERSION) · **المنصات:** Windows x64 · **الترخيص:** راجع LICENSE (غير موجود بعد)

---

## المميزات

| الميزة | الوصف |
|---|---|
| 🔍 **تنظيف ذكي** | 12 فئة ملفات مؤقتة + سلة المحذوفات + وكيل محلي بدرجة ثقة 0–100 + استثناءات شجرية |
| ☁️ **تنظيف بالإنترنت** | قواعد تقييم سحابية من Supabase + رفع إحصاءات المسح (اختياري) |
| 🔑 **تراخيص** | خطط `free` / `pro` / `owner` عبر Supabase RPC + HWID + مهلة offline 7 أيام |
| 👑 **لوحة المالك** | إنشاء/حذف مفاتيح + متابعة الأجهزة + نشر تحديثات |
| 🔄 **تحديث ذاتي** | فحص GitHub Releases كل 30 دقيقة + محدّث مستقل مع SHA-256 + **توقيع ECDSA** + rollback |
| 🌍 **تدويل** | عربي (افتراضي) / إنجليزي — ~200 مفتاح ترجمة |

---

## المتطلبات

- **للتطوير:** [.NET 10 SDK](https://dotnet.microsoft.com/download) + Windows x64
- **للتشغيل (Release):** لا شيء — حزم self-contained (بدون تثبيت .NET)
- **للتراخيص/السحابة:** مشروع [Supabase](https://supabase.com) + تنفيذ ملفات SQL في `supabase/migrations/`

---

## التشغيل السريع

```powershell
# بناء Debug
dotnet build StarX_Client/StarX_Client.csproj -c Debug

# تشغيل
.\StarX_Client\bin\Debug\net10.0-windows\StarX.exe

# بناء Release نهائي (self-contained)
dotnet publish StarX_Client/StarX_Client.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true -o dist/StarX
dotnet publish StarXUpdater/StarXUpdater.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true -o dist/StarXUpdater
Copy-Item dist/StarXUpdater/StarXUpdater.exe dist/StarX/ -Force
```

---

## هيكل المشروع

```
StarX_Client\          ← التطبيق الرئيسي (StarX.exe) — WinForms / .NET 10
  Form1.cs             ← قلب التطبيق: تنقل، ترخيص، تنظيف، تحديث، لوحة المالك
  CleanerEngine.cs     ← محرك الفحص والحذف (12 فئة)
  SmartAdvisor.cs      ← الوكيل الذكي المحلي (+ قواعد سحابية اختيارية)
  CleanerCloud.cs      ← مزامنة Supabase للتنظيف
  LicenseManager.cs    ← تراخيص Supabase + كاش DPAPI
  UpdateManager.cs     ← كشف/تشغيل التحديث
  PublishManager.cs    ← نشر Release من لوحة المالك
  LuxuryControls.cs    ← مكتبة واجهة مخصصة (17 كنترول)
  StarXTheme.cs        ← ألوان/أنصاف أقطار/مسافات
  Lang.cs              ← ترجمة ar/en
StarXUpdater\          ← محدّث مستقل (عملية منفصلة)
  UpdaterForm.cs       ← تحميل ← توقيع+SHA ← backup+manifest ← استبدال ← rollback
Shared\UpdateSecurity.cs ← توقيع/تحقق ECDSA P-256
supabase\migrations\   ← SQL: تراخيص + تنظيف سحابي + rate limit
release.ps1            ← نشر بأمر واحد (VERSION + tag + push)
.github/workflows/release.yml ← CI: بناء + توقيع + ZIP + Release
SECURITY.md            ← تقرير التحليل الأمني
RELEASE.md             ← دليل النشر والتحديث بالتفصيل
```

---

## إعداد Supabase

1. أنشئ مشروعاً على Supabase.
2. نفّذ بالترتيب في **SQL Editor**:
   - `supabase/migrations/20260923_license_security.sql`
   - `supabase/migrations/20260923_clean_cloud.sql`
3. ضع `URL` و `anon key` في `StarX_Client\SupabaseConfig.cs`  
   (أو متغيرات البيئة `STARX_SUPABASE_URL` / `STARX_SUPABASE_ANON_KEY`).
4. أنشئ مفتاح **owner** في جدول `licenses` (يدوياً أو عبر SQL) لتفعيل لوحة المالك.

> خط الدفاع الأساسي في السيرفر — تعديل العميل لا يمنح ترخيصاً.

---

## التوقيع والتحديث

| الملف | الدور |
|---|---|
| `Shared/UpdateSecurity.cs` | المفتاح **العام** مضمّن (يتحقق العميل والمحدّث) |
| `STARX_SIGNING_KEY` (Secret) | المفتاح **الخاص** في GitHub Actions — **لا يُرفع أبداً** |
| `StarX-v*.zip.sha256` | بصمة النقل |
| `StarX-v*.zip.sig` | توقيع ECDSA على البصمة — **إلزامي في Release** |

أضِف السر قبل أي نشر:

```text
GitHub → Settings → Secrets and variables → Actions → New repository secret
Name:  STARX_SIGNING_KEY
Value: <محتوى signing/starx_update_private.key>   # متجاهل من git
```

النشر: `.\release.ps1 1.4.0` — التفاصيل في [RELEASE.md](RELEASE.md).

---

## الإعدادات المحلية

| الملف | المسار |
|---|---|
| الإعدادات | `%AppData%\StarX\settings.json` |
| الترخيص (مشفّر DPAPI) | `%AppData%\StarX\license.json` |
| سجل التنظيف | `%AppData%\StarX\clean_history.json` |
| سجل الأعطال | `logs\crash.log` بجانب التطبيق |
| نسخ احتياطي التحديث | `%LOCALAPPDATA%\StarXUpdate\backup-*` |

مفاتيح مهمة في الإعدادات: `lang`, `closeToTray`, `cleanCloud`, `cleanExcludes`, `updateSkippedVersion`.

---

## الأمان

راجع **[SECURITY.md](SECURITY.md)** للتحليل الكامل. أبرز ما هو مُنفَّذ:

- توقيع ECDSA على حزم التحديث + تحقق مزدوج
- SHA-256 إجباري قبل التثبيت + نسخ احتياطي + re-hash عند rollback
- رفض `file://` في Release + تحقق هوية PID + حماية Zip-Slip
- منع خطة `owner` في وضع offline grace
- كاش الترخيص مشفّر DPAPI (CurrentUser)
- مسح توكن GitHub دائماً بعد النشر
- rate limit + RLS على دوال Supabase

---

## بناء واختبار

```powershell
dotnet build StarX_Client/StarX_Client.csproj -c Release
dotnet build StarX_Client/StarX_Client.csproj -c Debug
```

لا توجد اختبارات آلية بعد — التحقق اليدوي موثّق في `RELEASE.md`.

---

## روابط سريعة

- [RELEASE.md](RELEASE.md) — النشر والتحديث
- [SECURITY.md](SECURITY.md) — التحليل الأمني
- [supabase/migrations/](supabase/migrations/) — SQL
