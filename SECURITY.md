# StarX — تقرير التحليل الأمني

**النطاق:** أنظمة الترخيص (Supabase RPC) والتحديث الذاتي (GitHub Releases + StarXUpdater) وخط أنابيب النشر.
**التاريخ:** 2026-09-23
**قيد التدقيق:** ~~مصادر SQL/RLS الخاصة بدوال الترخيص السبعة غير موجودة في المستودع~~ — أُضيفت الآن في `supabase/migrations/20260923_license_security.sql` (يجب مراجعتها وتطبيقها على قاعدة بياناتك الحالية قبل الاعتماد عليها).

---

## 1. نموذج التهديد

### الأصول
| الأصل | الموقع | الأهمية |
|---|---|---|
| مفتاح الترخيص `pro`/`owner` | `%AppData%\StarX\license.json` | عالي |
| مفتاح المالك | نفس الملف + `_ownerKey` في Form1 | حرج |
| مفتاح GitHub PAT | `txtToken` + رؤوس HTTP أثناء النشر | حرج (مؤقت) |
| نزاهة التحديثات | GitHub Releases + `.sha256` | حرج |
| ثقة التحديث الصامت | `AutoUpdateAsync` كل 30 دقيقة | عالٍ |

### الخصوم
1. **قراصنة الترخيص** — تعديل العميل/الكاش/حجب الشبكة
2. **MITM** — اعتراض TLS إلى GitHub/Supabase (بلا pinning)
3. **حساب GitHub مخترق** — رفع zip + بصمة متطابقة خبيثين
4. **رمز PAT مسرّب** — نشر إصدار خبيث
5. **مهاجم محلي** — استبدال `StarXUpdater.exe` أو تعديل `%TEMP%` backup
6. **مهاجم RPC** — استدعاء مباشر بمفتاح `anon` المكشوف

---

## 2. نظام الترخيص

### مفتاح anon
- مضمّن في `SupabaseConfig.cs` — **publishable key** وليس سراً حسب تصميم Supabase.
- أي شخص يمكنه استدعاء: `validate_license`, `release_license`, `ping`, `device_ping`, `create_key`, `owner_overview`, `delete_key`.
- الأمان يعتمد كلياً على: `SECURITY DEFINER` + RLS صارمة + rate limit — **كلها خارج المستودع → غير مُدقّقة**.

### تعديل العميل (patching)
- تخطّي `_plan` يفتح **واجهات** لوحة المالك/النشر فقط.
- كل عملية حسّاسة تمرّ عبر RPC — تعديل العميل وحده **لا يمنح** مفاتيح حقيقية **بشرط** صحة الـ RPC.

### سماح عدم الاتصال (`OfflineGraceDays = 7`)
- `license.json` **غير موقّع/غير مشفّر**: `plan` + `validatedAt` قابلان للتعديل.
- سيناريو: تعديل `plan=owner` + `validatedAt=الآن` + حجب نطاق Supabase → `reason` يبدأ بـ `network` → ترخيص owner فعّال 7 أيام، ثم إعادة ضبط التاريخ.
- **تصحيح مُطبَّق:** منع `owner` داخل offline grace (pro فقط).

### تزوير HWID
- `MachineGuid` من Registry أو `SHA256(Machine|User)` — قابل للتعديل بامتيازات المستخدم/Admin.
- لا TPM/attestation.

### إساءة RPC
| الدالة | الخطر |
|---|---|
| `validate_license` | مِرآة صحة المفاتيح (reason متمايزة) + تسجيل HWIDs |
| `release_license` | إخلاء جهاز ضحية إن لم يتحقق السيرفر من الملكية |
| `create_key` / `owner_overview` / `delete_key` | تسريب `owner_key` = تحكم كامل — لا backoff في العميل |
| `ping` / `device_ping` | مفتاح كامل كل 60 ثانية — تسريب متكرر |

---

## 3. نظام التحديث

### SHA-256 من نفس Release — لا يحمي من اختطاع GitHub
- `ReleaseInfo.Sha256Url` = `AssetName + ".sha256"` **ضمن نفس الإصدار**.
- مهاجم يسيطر على الحساب يرفع **两者 معاً**.
- التحقق يمنع **تلف النقل** فقط، ليس سلامة سلسلة التوريد.
- **لا توقيع رقمي** (Ed25519/minisign/Sigstore/Authenticode).

### TLS
- `HttpClient` افتراضي، بلا certificate pinning — MITM يحتاج CA مزروعة محلياً.

### الكتم (Suppression)
- `settings.json` — إخفاء إشعارات تحديث (denial of patches)، ليس RCE.
- الفحص اليدوي يتجاهل الكتم.

### وسائط المحدّث (CLI)
| الوسيط | القيد | الثغرة |
|---|---|---|
| `--pid` | `> 0` | لا يتحقق أنه `StarX.exe` |
| `--install-dir` | موجود فقط | أي مجلد قابل للكتابة |
| `--download-url` | `https` / **`file`** / http loopback | **`file://` مقبول** (مُقيَّد في Release بتصحيح مُطبَّق) |
| `--sha256` | 64 hex | لا يضمن المصداقية بلا توقيع مستقل |

### Zip-Slip
- حماية جيدة: `GetFullPath` + `StartsWith(root + separator)`.
- لا فحص symlink/junction بعد `WipeInstall` (TOCTOU محلي).

### فحص مجلد التطوير
- الجذر فقط (`.pdb`/`.csproj`) — **ليس عائقاً أمنياً** (يُحذف بسهولة).

### النسخ الاحتياطي/الاسترجاع
- backup في `%TEMP%` القابل للكتابة — TOCTOU أثناء فشل التثبيت.
- `VerifyInstall` يفحص وجود `StarX.exe` + `VERSION` **بلا re-hash**.

### استبدال `StarXUpdater.exe`
- `FindUpdater` يثق بأي ملف بجانب التطبيق — **بلا تحقق Authenticode**.
- من يستطيع الكتابة يستبدل المحدّث غير المشغول → كود عند التحديث الصامت.

---

## 4. خط أنابيب النشر

- `txtToken` بـ `UseSystemPasswordChar` — جيد بصرياً؛ يبقى في `TextBox.Text` (نص CLR).
- **تصحيح مُطبَّق:** مسح دائم في `finally` (وليس عند النجاح فقط).
- PAT صلاحية `repo` كاملة — يُفضَّل fine-grained `contents: write` فقط.
- `StageInstallDir` يستثني `.pdb`/`logs`/`backup`/`.bak` فقط — **بلا allowlist**؛ نشر من جهاز المالك = مهاجم محلي يحقن ملفات في الحزمة الرسمية.
- CI على تاجات `v*.*.*` بلا حماية environment.

---

## 5. النتائج مرتبة حسب الخطورة

### [CRITICAL] سلسلة تحديث غير مُكدَّنة
SHA من نفس الـ Release، بلا توقيع مستقل → اختطاع الحساب = تحديث صامت خبيث لكل العميلين خلال ≤30 دقيقة.
**الحالة:** ✅ **توقيع ECDSA P-256** على بصمة الحزمة (`StarX-v*.zip.sig`) + تحقق مزدوج في `UpdateManager` و`UpdaterForm` (فشل إغلاق في Release بلا توقيع صالح). يتطلب ضبط `secrets.STARX_SIGNING_KEY` على GitHub. اختُبر محلياً: توقيع صالح ينجح، توقيع/بصمة خاطئة يمنعان التثبيت.

### [CRITICAL] offline grace يثق بـ `license.json` غير الموقّع
**الإصلاح (مُطبَّق):** منع `owner` في grace؛ كاش الترخيص مشفّر DPAPI (`dpapi:`) في `LicenseManager`؛ لا إعادة ضبط `validatedAt` بلا اتصال ناجح.

### [HIGH] غياب SQL/RLS للـ RPC + anon مكشوف
**الحالة:** ✅ migrations مكتوبة في `supabase/migrations/20260923_license_security.sql` (SECURITY DEFINER + revoke مباشر + rate limit في قاعدة البيانات). يجب تطبيقها على Supabase ومراجعتها مع قاعدة البيانات الحالية.

### [HIGH] مفتاح المالك كسرّ RPC وحيد في ملف عادي
**الإصلاح:** passphrase طويل + Argon2 على السيرفر؛ عدم تخزينه مع كاش الترخيص العادي.

### [HIGH] حماية الميزات بـ `_plan` محلياً
**الإصلاح:** كل عملية حسّاسة عبر RPC إجباري؛ الواجهة عرض فقط.

### [HIGH] استبدال `StarXUpdater.exe` بلا تحقق توقيع
**الإصلاح:** Authenticode أو hash المحدّث من manifest موقّع.

### [HIGH] backup في `%TEMP%` بلا re-hash عند الاسترجاع
**الصلاحية (مُخطط):** إعادة SHA لكل ملف عند rollback؛ backup خارج `%TEMP%` أو ACL صارم.

### [MEDIUM] تزوير HWID سهل
**الإصلاح:** مزيج أقوى (CPU/disk/MAC/TPM) + مراجعة سحابية عند تغيّر كبير.

### [MEDIUM] مِرآة المفاتيح عبر `validate_license` + لا backoff
**الإصلاح:** توحيد رسائل الخطأ للمستخدم النهائي؛ تأخير تصاعدي؛ audit log.

### [MEDIUM] `file://` + CLI حر في المحدّث
**الإصلاح (مُطبَّق):** رفض `file://` في بناء Release (`#if !DEBUG`).

### [MEDIUM] staging بلا allowlist + PAT كامل
**الإصلاح (مُطبَّق جزئياً):** مسح التوكن في `finally`؛ allowlist للملفات الرسمية؛ PAT محدود.

### [MEDIUM] سباقات PID
**الإصلاح:** التحقق `MainModule.FileName` == `StarX.exe`.

### [LOW] كتم التحديث عبر settings · لا pinning · عرض مفاتيح كاملة في لوحة المالك · مبالغة `RELEASE.md`

---

## 6. خارطة الطريق

### انتصارات سريعة (بعضها مُطبَّق)
| # | الإجراء | الحالة |
|---|---|---|
| 1 | مسح التوكن دائماً في `finally` | ✅ مُطبَّق |
| 2 | منع `owner` في offline grace | ✅ مُطبَّق |
| 3 | رفض `file://` في Release | ✅ مُطبَّق |
| 4 | تنظيف استثناءات staging (امتدادات خطرة) | ✅ مُطبَّق |
| 5 | re-hash عند rollback + backup خارج %TEMP% | ✅ مُطبَّق |
| 6 | هوية PID (`StarX.exe`) | ✅ مُطبَّق |
| 7 | توقيع حزم التحديث (ECDSA P-256) + تحقق في العميل والمحدّث | ✅ مُطبَّق (المفتاح العام في `Shared/UpdateSecurity.cs`؛ الخاص في `STARX_SIGNING_KEY` أو `signing/*.key` — gitignored) |
| 8 | نشر SQL/RLS للترخيص + rate limit | ✅ migrations جاهزة — **يلزم تطبيقها على Supabase** |
| 9 | allowlist كامل للنشر + PAT محدود | ⬜ جزئي (امتدادات خطرة مستثناة) |
| 10 | تصحيح ادعاءات `RELEASE.md` الأمنية | ✅ مُطبَّق |
| 11 | re-hash عند rollback + backup في LocalAppData | ✅ مُطبَّق |
| 12 | هوية PID (`StarX.exe`) | ✅ مُطبَّق |
| 13 | DPAPI لـ `license.json` | ✅ مُطبَّق (`LicenseManager` — `dpapi:` prefix + توافق مع الملفات القديمة) |

### الأولوية المتبقية
1. **تطبيق** `supabase/migrations/20260923_license_security.sql` على Supabase ومراجعته
2. **PAT محدود** (fine-grained contents:write) + allowlist كامل للنشر
3. ضبط `secrets.STARX_SIGNING_KEY` قبل أول Release جديد

---

## ملاحظة على ميزة التنظيف السحابي (2026-09-23)

- الدالتان `get_clean_rules` / `report_clean_run` مُنشأتان بـ `SECURITY DEFINER` + `revoke` من `public` ثم `grant` للـ anon فقط.
- `clean_runs`: لا قراءة مباشرة للـ anon (insert فقط عبر الدالة).
- `clean_rules`: قراءة فقط للمستخدمين `enabled`.
- لا تُرسل مسارات ملفات المنظّف — إحصاءات مجمّعة + HWID فقط.
