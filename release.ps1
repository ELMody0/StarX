# StarX — نشر إصدار جديد بأمر واحد.
# يوحّد رقم VERSION مع التاج تلقائياً عشان التطبيق ماينبهش لتحديث وهمي.
#
#   .\release.ps1 1.3.0
#
# الخطوات: تحقق الرقم + نظافة الشغل + كتابة VERSION + commit + tag + push (الكود والتاج).
#
# التوقيع: الـ workflow يوقّع الحزمة بـ secrets.STARX_SIGNING_KEY
# (base64 لـ ECPrivateKey P-256) — أضِفه في GitHub → Settings → Secrets.

param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

$ver = $Version.Trim().TrimStart('v', 'V')
if ($ver -notmatch '^\d+\.\d+\.\d+$') {
    throw "Bad version '$Version'. Use X.Y.Z (e.g. 1.3.0)."
}

# التاج موجود قبل كده؟
$existing = git rev-parse --verify --quiet "refs/tags/v$ver"
if ($existing) {
    throw "Tag v$ver already exists. Pick a new version."
}

# فيه شغل مش محفوظ غير VERSION؟ لازم يتحفظ الأول عشان الـ Release يتبني عليه
$dirty = git status --porcelain | Where-Object { $_ -notmatch ' VERSION$' -and $_ -notmatch '^.. VERSION$' }
if ($dirty) {
    Write-Host "Uncommitted changes (other than VERSION):" -ForegroundColor Yellow
    $dirty | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    throw "Commit or stash your work first so the release builds from clean code."
}

# وحّد VERSION مع رقم الإصدار
$ver | Out-File -FilePath VERSION -Encoding ascii -NoNewline
Write-Host "VERSION = $ver" -ForegroundColor Green

git add VERSION
git commit -m "Release v$ver" | Out-Null
git tag "v$ver"
git push origin HEAD
git push origin "v$ver"

Write-Host "Pushed v$ver - the workflow will build + publish the Release." -ForegroundColor Green
