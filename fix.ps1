$f = Get-Content "D:\StarX\StarX_Client\LuxuryControls.cs" -Raw
$f = $f -replace "new RadialGradientBrush\(", "StarXTheme.GetRadialBrush("
$f = $f -replace "new LinearGradientBrush\(", "StarXTheme.GetVerticalGradient("
$f = $f -replace "new LinearGradientBrush\(new RectangleF\(", "StarXTheme.GetVerticalGradient(new Rectangle("
Set-Content "D:\StarX\StarX_Client\LuxuryControls.cs" -Value $f
