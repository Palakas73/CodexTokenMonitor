# Rebuild the multi-resolution Windows icon using only Windows/.NET drawing APIs.
# Geometry matches src/CodexTokenMonitor.App/Assets/CodexTokenMonitor.svg.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskAssets = Join-Path $taskRoot 'src/CodexTokenMonitor.App/Assets'
$taskPreview = Join-Path $taskRoot 'artifacts/icon-preview.png'
New-Item -ItemType Directory -Path $taskAssets, (Split-Path $taskPreview -Parent) -Force | Out-Null
$taskSizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$taskFrames = @()
foreach ($taskSize in $taskSizes) {
    $taskBitmap = New-Object System.Drawing.Bitmap($taskSize, $taskSize)
    $taskGraphics = [System.Drawing.Graphics]::FromImage($taskBitmap)
    $taskPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $taskFill = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#0f172a'))
    $taskBorder = New-Object System.Drawing.Pen([System.Drawing.ColorTranslator]::FromHtml('#334155'), 2)
    $taskTrack = New-Object System.Drawing.Pen([System.Drawing.ColorTranslator]::FromHtml('#334155'), 12)
    $taskArc = New-Object System.Drawing.Pen([System.Drawing.ColorTranslator]::FromHtml('#2dd4bf'), 12)
    $taskNeedle = New-Object System.Drawing.Pen([System.Drawing.ColorTranslator]::FromHtml('#f8fafc'), 8)
    $taskWhite = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#f8fafc'))
    $taskMemory = New-Object System.IO.MemoryStream
    try {
        $taskGraphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $taskGraphics.Clear([System.Drawing.Color]::Transparent)
        $taskGraphics.ScaleTransform($taskSize / 128.0, $taskSize / 128.0)
        $taskPath.AddArc(4, 4, 56, 56, 180, 90)
        $taskPath.AddArc(68, 4, 56, 56, 270, 90)
        $taskPath.AddArc(68, 68, 56, 56, 0, 90)
        $taskPath.AddArc(4, 68, 56, 56, 90, 90)
        $taskPath.CloseFigure()
        $taskGraphics.FillPath($taskFill, $taskPath)
        $taskGraphics.DrawPath($taskBorder, $taskPath)
        foreach ($taskPen in @($taskTrack, $taskArc, $taskNeedle)) {
            $taskPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
            $taskPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        }
        $taskGraphics.DrawArc($taskTrack, 24, 24, 80, 80, 135, 270)
        $taskGraphics.DrawArc($taskArc, 24, 24, 80, 80, 135, 216)
        $taskGraphics.DrawLine($taskNeedle, 64, 66, 84, 46)
        $taskGraphics.FillEllipse($taskWhite, 58, 60, 12, 12)
        $taskBitmap.Save($taskMemory, [System.Drawing.Imaging.ImageFormat]::Png)
        $taskFrames += ,($taskMemory.ToArray())
        if ($taskSize -eq 256) { $taskBitmap.Save($taskPreview, [System.Drawing.Imaging.ImageFormat]::Png) }
    } finally {
        $taskMemory.Dispose(); $taskGraphics.Dispose(); $taskBitmap.Dispose(); $taskPath.Dispose()
        $taskFill.Dispose(); $taskWhite.Dispose(); $taskBorder.Dispose(); $taskTrack.Dispose(); $taskArc.Dispose(); $taskNeedle.Dispose()
    }
}
$taskStream = [System.IO.File]::Create((Join-Path $taskAssets 'CodexTokenMonitor.ico'))
$taskWriter = New-Object System.IO.BinaryWriter($taskStream)
try {
    $taskWriter.Write([uint16]0); $taskWriter.Write([uint16]1); $taskWriter.Write([uint16]$taskSizes.Count)
    $taskOffset = 6 + 16 * $taskSizes.Count
    for ($taskIndex = 0; $taskIndex -lt $taskSizes.Count; $taskIndex++) {
        $taskDimension = $taskSizes[$taskIndex] % 256
        $taskWriter.Write([byte]$taskDimension); $taskWriter.Write([byte]$taskDimension)
        $taskWriter.Write([byte]0); $taskWriter.Write([byte]0)
        $taskWriter.Write([uint16]1); $taskWriter.Write([uint16]32)
        $taskWriter.Write([uint32]$taskFrames[$taskIndex].Length); $taskWriter.Write([uint32]$taskOffset)
        $taskOffset += $taskFrames[$taskIndex].Length
    }
    foreach ($taskFrame in $taskFrames) { $taskWriter.Write([byte[]]$taskFrame) }
} finally { $taskWriter.Dispose(); $taskStream.Dispose() }
Write-Output 'Application icon generated (16-256 px).'
