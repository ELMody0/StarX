using System.Text.Json;

namespace StarX_Client;

/// <summary>نظام اللغة: عربي / English — يُحفظ في %AppData%/StarX/settings.json</summary>
internal static class Lang
{
    public const string Arabic = "ar";
    public const string English = "en";

    private static string _current = Load();

    public static string Current
    {
        get => _current;
        set
        {
            if (value != Arabic && value != English) value = Arabic;
            if (_current == value) return;
            _current = value;
            Save();
        }
    }

    public static bool IsArabic => _current == Arabic;

    private static readonly Dictionary<string, (string Ar, string En)> S = new()
    {
        ["settings_title"] = ("الإعدادات", "Settings"),
        ["key_ph"] = ("أدخل مفتاح التفعيل", "Enter activation key"),
        ["activate"] = ("تفعيل", "Activate"),
        ["lang_label"] = ("اللغة", "Language"),
        ["save"] = ("حفظ", "Save"),
        ["lang_ar"] = ("العربية", "العربية"),
        ["lang_en"] = ("English", "English"),
        ["pro_title"] = ("البرو ★", "Pro ★"),
        ["pro_note"] = ("مميزات البرو — قريباً", "Pro features — coming soon"),
        ["owner_title"] = ("لوحة المالك ♛", "Owner Panel ♛"),
        ["online_h"] = ("المتصلون الآن", "Online now"),
        ["lic_h"] = ("المفاتيح", "Keys"),
        ["create"] = ("إنشاء مفتاح", "Create key"),
        ["note_ph"] = ("ملاحظة (اختياري)", "Note (optional)"),
        ["refresh"] = ("تحديث", "Refresh"),
        ["delete_key"] = ("مسح المفتاح", "Delete key"),
        ["exit_tip"] = ("الخروج", "Logout"),
        ["st_owner"] = ("مالك ✓", "Owner ✓"),
        ["st_pro"] = ("برو ✓", "Pro ✓"),
        ["st_inactive"] = ("غير مفعل — أدخل مفتاح التفعيل", "Not activated — enter your key"),
        ["st_no_supabase"] = ("لم يتم ربط Supabase بعد", "Supabase not linked yet"),
        ["st_checking"] = ("جارٍ التحقق من الترخيص…", "Checking license…"),
        ["st_activating"] = ("جارٍ التفعيل…", "Activating…"),
        ["st_deactivating"] = ("جارٍ إلغاء التفعيل…", "Logging out…"),
        ["st_enter_key"] = ("أدخل المفتاح أولاً", "Enter the key first"),
        ["st_offline"] = ("وضع عدم الاتصال", "Offline mode"),
        ["st_no_grace"] = ("تعذر الاتصال وانتهت مهلة السماح — تحقق من الإنترنت", "No connection and grace expired — check internet"),
        ["st_creating"] = ("جارٍ الإنشاء…", "Creating…"),
        ["st_updating"] = ("جارٍ التحديث…", "Updating…"),
        ["st_select_first"] = ("اختر مفتاحاً من القائمة أولاً", "Select a key from the list first"),
        ["st_deleted"] = ("تم مسح المفتاح ✓", "Key deleted ✓"),
        ["st_created"] = ("تم إنشاء المفتاح ✓ انسخه الآن", "Key created ✓ copy it now"),
        ["st_expires"] = ("ينتهي:", "Expires:"),
        ["st_lifetime"] = ("دائم", "Lifetime"),
        ["st_online_n"] = ("متصلون", "Online"),
        ["st_keys_n"] = ("كل المفاتيح", "Keys"),
        ["r_invalid"] = ("المفتاح غير صحيح", "Invalid key"),
        ["r_revoked"] = ("المفتاح موقوف", "Key revoked"),
        ["r_expired"] = ("المفتاح منتهي الصلاحية", "Key expired"),
        ["r_device_limit"] = ("تم تجاوز عدد الأجهزة المسموح بها", "Device limit reached"),
        ["r_config"] = ("لم يتم ربط Supabase بعد", "Supabase not linked yet"),
        ["r_network"] = ("تعذر الاتصال بالسيرفر", "Cannot reach server"),
        ["r_forbidden"] = ("غير مصرح — مفتاح المالك فقط", "Forbidden — owner key only"),
        ["r_not_found"] = ("المفتاح غير موجود", "Key not found"),
        ["r_protected"] = ("لا يمكن مسح مفاتيح المالك", "Owner keys are protected"),
        ["r_unknown"] = ("فشل التفعيل", "Activation failed"),
        ["r_create_fail"] = ("فشل الإنشاء", "Creation failed"),
        ["r_delete_fail"] = ("فشل المسح", "Delete failed"),
        ["r_bad_response"] = ("استجابة غير صالحة", "Invalid response"),
        ["col_device"] = ("الجهاز", "Device"),
        ["col_key"] = ("المفتاح", "Key"),
        ["col_plan"] = ("الخطة", "Plan"),
        ["col_seen"] = ("آخر ظهور", "Last seen"),
        ["col_active"] = ("نشط", "Active"),
        ["col_devices"] = ("أجهزة", "Devices"),
        ["col_expires"] = ("ينتهي", "Expires"),
        ["col_note"] = ("ملاحظة", "Note"),
        ["plan_pro"] = ("برو", "Pro"),
        ["plan_owner"] = ("مالك", "Owner"),
        ["plan_free"] = ("مجاني", "Free"),
        ["yes"] = ("نعم", "Yes"),
        ["no"] = ("لا", "No"),
        ["online_now"] = ("متصل ✓ ", "Online ✓ "),
        ["dur_day"] = ("يوم", "1 day"),
        ["dur_2days"] = ("يومان", "2 days"),
        ["dur_3days"] = ("3 أيام", "3 days"),
        ["dur_week"] = ("أسبوع", "1 week"),
        ["dur_month"] = ("شهر", "1 month"),
        ["dur_year"] = ("سنة", "1 year"),
        ["dur_lifetime"] = ("دائم", "Lifetime"),
        ["mb_activate"] = ("StarX — التفعيل", "StarX — Activation"),
        ["mb_delete"] = ("StarX — مسح مفتاح", "StarX — Delete key"),
        ["confirm_delete"] = ("مسح هذا المفتاح نهائياً؟", "Delete this key permanently?"),
        ["upd_title"] = ("التحديثات", "Updates"),
        ["upd_current"] = ("الإصدار الحالي", "Current version"),
        ["upd_check"] = ("التحقق من التحديثات", "Check for updates"),
        ["upd_checking"] = ("جارٍ التحقق…", "Checking…"),
        ["upd_uptodate"] = ("أنت على أحدث إصدار ✓", "You're up to date ✓"),
        ["upd_failed"] = ("تعذر التحقق من التحديثات — حاول لاحقاً", "Unable to check for updates right now. Please try again later."),
        ["upd_rate"] = ("GitHub مشغول حالياً — حاول لاحقاً", "GitHub is busy right now — try again later."),
        ["upd_none"] = ("لا يوجد إصدار منشور بعد", "No published release yet"),
        ["upd_invalid"] = ("بيانات التحديث غير صالحة", "Invalid update data"),
        ["upd_available"] = ("يتوفر إصدار جديد", "New version available"),
        ["upd_update"] = ("تحديث", "Update"),
        ["upd_later"] = ("لاحقاً", "Later"),
        ["upd_latest"] = ("الأحدث", "Latest"),
        ["upd_starting"] = ("جارٍ بدء التحديث…", "Starting update…"),
        ["upd_no_updater"] = ("ملف المحدّث غير موجود بجانب التطبيق", "Updater not found next to the app"),
        ["upd_no_checksum"] = ("تعذر التحقق من سلامة الحزمة — تم الإلغاء", "Cannot verify package integrity — cancelled"),
        ["upd_notify_h"] = ("يتوفر تحديث StarX", "StarX Update Available"),
        ["pub_title"] = ("نشر تحديث جديد", "Publish new release"),
        ["pub_version_ph"] = ("رقم الإصدار (مثال 1.4.0)", "Version (e.g. 1.4.0)"),
        ["pub_token_ph"] = ("رمز GitHub (لا يُحفظ)", "GitHub token (not stored)"),
        ["pub_notes_ph"] = ("ملاحظات الإصدار (اختياري)", "Release notes (optional)"),
        ["pub_publish"] = ("نشر", "Publish"),
        ["pub_bad_version"] = ("رقم إصدار غير صالح (مثال 1.4.0)", "Invalid version (e.g. 1.4.0)"),
        ["pub_no_token"] = ("أدخل رمز GitHub", "Enter GitHub token"),
        ["pub_working"] = ("جارٍ النشر…", "Publishing…"),
        ["pub_stage"] = ("تجهيز الحزمة…", "Preparing package…"),
        ["pub_zip"] = ("ضغط الحزمة…", "Compressing…"),
        ["pub_hash"] = ("حساب البصمة…", "Hashing…"),
        ["pub_release"] = ("إنشاء الإصدار…", "Creating release…"),
        ["pub_upload"] = ("رفع الملفات…", "Uploading files…"),
        ["pub_done"] = ("تم النشر ✓", "Published ✓"),
        ["pub_exists"] = ("هذا الإصدار موجود بالفعل", "Release already exists"),
        ["pub_unauth"] = ("الرمز غير صالح (تحقق من الصلاحيات)", "Invalid token (check scopes)"),
        ["nav_home"] = ("الرئيسية", "Home"),
        ["nav_settings"] = ("الإعدادات", "Settings"),
        ["nav_pro"] = ("البرو", "Pro"),
        ["nav_owner"] = ("المالك", "Owner"),
    };

    public static string T(string key)
    {
        if (S.TryGetValue(key, out var v)) return IsArabic ? v.Ar : v.En;
        return key;
    }

    private static string Load() => AppSettings.GetString("lang", Arabic) == English ? English : Arabic;

    private static void Save() => AppSettings.Set("lang", _current);
}
