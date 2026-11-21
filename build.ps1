$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
& $csc /nologo /optimize+ /target:winexe /out:"$dir\BlueforceRGB.exe" `
    /r:System.Windows.Forms.dll `
    /r:System.Drawing.dll `
    "$dir\src\Program.cs" `
    "$dir\src\HidKeyboard.cs" `
    "$dir\src\ModernControls.cs" `
    "$dir\src\MainForm.cs"
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded: $dir\BlueforceRGB.exe"
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
