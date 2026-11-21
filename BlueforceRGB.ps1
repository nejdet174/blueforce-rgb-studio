Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
$files = @(
    (Join-Path $dir "src\HidKeyboard.cs"),
    (Join-Path $dir "src\ModernControls.cs"),
    (Join-Path $dir "src\MainForm.cs")
)

Add-Type -Path $files -ReferencedAssemblies System.Windows.Forms, System.Drawing

[System.Windows.Forms.Application]::EnableVisualStyles()
[System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)
$form = New-Object BlueforceRGB.MainForm
[System.Windows.Forms.Application]::Run($form)
