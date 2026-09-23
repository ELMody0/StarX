# StarX — النشر والتحديث التلقائي

## فكرة النظام

- `VERSION` (في جذر المشروع) هو مصدر الإصدار المفرد، ويُنسخ بجانب `StarX.exe`.
- التحديثات تُوزع عبر **GitHub Releases** — لا سيرفر خاص.
- `StarX.exe` لا يستبدل نفسه أبداً — `StarXUpdater.exe` منفصل يتولى التحميل والتحقق والتثبيت وإعادة التشغيل.

## النشر من لوحة المالك (الطريقة السهلة)

1. افتح التطبيق وفعّل بمفتاح المالك → أيقونة التاج → **لوحة المالك**.
2. انزل لقسم **نشر تحديث جديد**: اكتب رقم الإصدار (مثال `1.4.0`).
3. الصق **GitHub token** بصلاحية `repo` (يُستخدم مرة واحدة ولا يُحفظ).
4. اكتب ملاحظات الإصدار (اختياري) → **نشر**.
5. التطبيق يجهّز الحزمة من مجلد التشغيل (بدون `*.pdb` وبدون `logs/`)، ويكتب `VERSION` الجديد،
   ويحسب SHA-256، وينشئ الـ Release ويرفع `StarX-v1.4.0.zip` + ملف البصمة — والمستخدمون يصلهم إشعار تلقائياً.

## النشر عبر Git Tag (الطريقة الاحترافية)

```powershell
# من جذر المشروع — أمر واحد يوحّد VERSION + commit + tag + push:
.\release.ps1 1.4.0
# الـ workflow يبني C# + C++ ويجهز ZIP + SHA-256 وينشر Release تلقائياً
```

> مهم: انشر دائماً عبر `release.ps1` (مش `git tag` يدوي) عشان رقم `VERSION`
> يفضل مساوياً لآخر Release — غير كده نسخ التطوير هتنبه لتحديث وهمي.

> الإصدارات المنشورة **Self-contained single-file + ReadyToRun وبدون ملفات `.pdb`**:
> الـ `StarX.exe` جواه الرانتايم ويشتغل على أي ويندوز x64 **بدون تثبيت .NET**،
> والكود مترجم Native مسبقاً وبدون رموز تصحيح — عكس هندسته أصعب بكثير.
> خط الدفاع الأساسي يفضل في السيرفر (التحقق من المفاتيح عبر Supabase RPC) —
> تعديل نسخة العميل لا يمنح ترخيصاً.
> لو ظهرت رسالة `You must install .NET Desktop Runtime` فده معناه إن النسخة
> المشغلة قديمة (framework-dependent) — حمّل آخر Release أو ثبّت
> `.NET Desktop Runtime` من صفحة تحميل دوت نت.

## نسخة محلية للمشاركة (بدون GitHub)

```powershell
dotnet publish StarX_Client/StarX_Client.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/StarX
dotnet publish StarXUpdater/StarXUpdater.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/StarXUpdater
Copy-Item dist/StarXUpdater/StarXUpdater.exe dist/StarX/ -Force
# مجلد dist/StarX جاهز للنسخ والتشغيل على أي جهاز بدون .NET
```

ملفات الإصدار المطلوبة (يتحقق منها التطبيق والمحدّث):

```text
StarX-v1.4.0.zip
StarX-v1.4.0.zip.sha256   (سطر واحد: "<hash>  StarX-v1.4.0.zip")
StarX-v1.4.0.zip.sig      (توقيع ECDSA base64 — إلزامي في Release)
```

## تجربة محلية v1.0.0 → v1.1.0 (بدون GitHub)

> ملاحظة: بناء `StarX_Client` يبني `StarXUpdater` تلقائياً وينسخه بجانب `StarX.exe` —
> رسالة "ملف المحدّث غير موجود" تعني أن النسخ لم يحدث (ابنِ مرة أخرى).

```powershell
# 1) ابنِ التطبيق والمحدّث Release
dotnet build StarX_Client/StarX_Client.csproj -c Release
dotnet build StarXUpdater/StarXUpdater.csproj -c Release

# 2) جهّز مجلد تثبيت وهمي v1.0.0
$inst = "$env:TEMP\StarXLocal\v100"; New-Item -ItemType Directory -Force -Path $inst | Out-Null
"dummy-old" | Out-File "$inst\StarX.exe" -NoNewline   # سيُستبدل لاحقاً
"1.0.0" | Out-File "$inst\VERSION" -NoNewline
Copy-Item StarXUpdater/bin/Release/net10.0-windows/StarXUpdater.* $inst/ -Force

# 3) جهّز حزمة v1.1.0 (بملف StarX.exe حقيقي صغير — انسخ أي exe يغلق بسرعة)
$pkg = "$env:TEMP\StarXLocal\pkg"; New-Item -ItemType Directory -Force -Path $pkg | Out-Null
Copy-Item <exe-صغير> "$pkg\StarX.exe" -Force
"1.1.0" | Out-File "$pkg\VERSION" -NoNewline
"data" | Out-File "$pkg\extra.txt" -NoNewline
Compress-Archive -Path "$pkg\*" -DestinationPath "$env:TEMP\StarXLocal\StarX-v1.1.0.zip" -Force
$hash = (Get-FileHash "$env:TEMP\StarXLocal\StarX-v1.1.0.zip" -Algorithm SHA256).Hash.ToLower()

# 4) شغّل المحدّث (PID أي عملية قصيرة — مثال: ابدأ notepad ثم خذ الـ PID)
StarXUpdater/bin/Release/net10.0-windows/StarXUpdater.exe --pid <PID> --install-dir $inst --download-url "file:///$($env:TEMP -replace '\\','/')/StarXLocal/StarX-v1.1.0.zip" --version "1.1.0" --sha256 $hash

# 5) تحقق: "$inst\VERSION" == "1.1.0" و StarX.exe الجديد اشتغل ثم أُغلق
```

> ملاحظة: `file://` مدعوم للاختبار المحلي فقط — إصدارات GitHub الحقيقية دائماً `https://`.

## الأمان

- لا يُثبَّت أي ملف قبل تحقق SHA-256 **و** التوقيع ECDSA (`.zip.sig`، مفتاح عام مضمّن) — SHA لسلامة النقل، والتوقيع ضد اختطاع الحساب.
- نسخة احتياطية كاملة قبل الاستبدال + استرجاع تلقائي عند الفشل.
- مجلد `logs/` وبيانات `%AppData%/StarX` لا تُمس أبداً.
- لا رموز GitHub في التطبيق — رمز النشر يُدخل يدوياً ولا يُحفظ.
- السجلات: `<install>/logs/updater.log` (بدون أي أسرار).

## استكشاف الأخطاء

| المشكلة | السبب المحتمل |
|---|---|
| لا يظهر تحديث | لا يوجد Release أحدث، أو اسم الأصل مخالف `StarX-vX.Y.Z.zip`، أو بلا ملف `.sha256` |
| فشل التحقق | تلف التحميل — أعد المحاولة |
| المحدّث ينتظر | StarX ما زال يعمل — أغلقه |
| 401/403 عند النشر | الرمز منتهي أو بلا صلاحية `repo` |
| 422 عند النشر | وسم بهذا الرقم موجود — استخدم رقماً جديداً |
