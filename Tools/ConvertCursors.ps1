<#
.SYNOPSIS
    把 Assets\Cursors\grab.png / grabbing.png 转换为成品 .cur 文件。
.DESCRIPTION
    用法：在解决方案根目录执行：
        pwsh Tools\ConvertCursors.ps1
    或自定义热点：
        pwsh Tools\ConvertCursors.ps1 -GrabHotX 13 -GrabHotY 6 -GrabbingHotX 13 -GrabbingHotY 10
    生成结果直接覆盖 Assets\Cursors\grab.cur / grabbing.cur，然后提交到 Git 即可。
#>
param(
    [int]$GrabHotX     = 13,
    [int]$GrabHotY     = 6,
    [int]$GrabbingHotX = 13,
    [int]$GrabbingHotY = 10,
    [int]$CursorSize   = 32,
    [switch]$Mirror                 # 水平镜像（拇指朝左）
)

Add-Type -AssemblyName PresentationCore, WindowsBase

function ConvertToCur([string]$pngPath, [string]$curPath, [int]$hotX, [int]$hotY, [int]$size, [bool]$mirror = $false) {
    # 1. 加载 PNG
    $decoder = [System.Windows.Media.Imaging.PngBitmapDecoder]::new(
        [System.IO.File]::OpenRead($pngPath),
        [System.Windows.Media.Imaging.BitmapCreateOptions]::PreservePixelFormat,
        [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad)
    [System.Windows.Media.Imaging.BitmapSource]$src = $decoder.Frames[0]

    # 水平镜像（拇指朝左）
    if ($mirror) {
        $src  = [System.Windows.Media.Imaging.TransformedBitmap]::new(
            $src, [System.Windows.Media.ScaleTransform]::new(-1, 1, ($src.PixelWidth / 2.0), 0))
        $hotX = $size - 1 - $hotX
    }

    # 2. 等比缩放到 size×size
    $scale = [Math]::Min($size / $src.PixelWidth, $size / $src.PixelHeight)
    $sw    = [int][Math]::Round($src.PixelWidth  * $scale)
    $sh    = [int][Math]::Round($src.PixelHeight * $scale)

    $visual = [System.Windows.Media.DrawingVisual]::new()
    $dc     = $visual.RenderOpen()
    $ox = [int](($size - $sw) / 2)
    $oy = [int](($size - $sh) / 2)

    if ($scale -ne 1) {
        $scaled = [System.Windows.Media.Imaging.TransformedBitmap]::new(
            $src, [System.Windows.Media.ScaleTransform]::new($scale, $scale))
        $dc.DrawImage($scaled, [System.Windows.Rect]::new($ox, $oy, $sw, $sh))
    } else {
        $dc.DrawImage($src, [System.Windows.Rect]::new($ox, $oy, $sw, $sh))
    }
    $dc.Close()

    $rtb = [System.Windows.Media.Imaging.RenderTargetBitmap]::new(
        $size, $size, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    $rtb.Render($visual)

    # 3. 转 Bgra32 并提取像素
    $bgra = [System.Windows.Media.Imaging.FormatConvertedBitmap]::new(
        $rtb, [System.Windows.Media.PixelFormats]::Bgra32, $null, 0)
    $stride = $size * 4
    $pixels = New-Object byte[] ($size * $stride)
    $bgra.CopyPixels($pixels, $stride, 0)

    # 4. 翻转为 bottom-up（DIB 要求）
    $flipped = New-Object byte[] ($pixels.Length)
    for ($r = 0; $r -lt $size; $r++) {
        [Array]::Copy($pixels, ($size - 1 - $r) * $stride, $flipped, $r * $stride, $stride)
    }

    # 5. 写 .cur（32bpp，AND mask 全 0 → alpha 控制透明度）
    $maskSz = [int]($size * $size / 8)
    $imgSz  = 40 + $flipped.Length + $maskSz
    $f      = [System.Collections.Generic.List[byte]]::new()

    function I32([int]$v)    { $f.AddRange([System.BitConverter]::GetBytes($v)) }
    function I16([int]$v)    { $f.AddRange([System.BitConverter]::GetBytes([int16]$v)) }
    function B([byte]$v)     { $f.Add($v) }

    I16 0; I16 2; I16 1                         # ICONDIR
    B $size; B $size; B 0; B 0                  # ICONDIRENTRY width/height/colorCount/reserved
    I16 $hotX; I16 $hotY                        # hotspot
    I32 $imgSz; I32 22                          # dwBytesInRes / dwImageOffset
    I32 40; I32 $size; I32 ($size * 2)          # BITMAPINFOHEADER biSize/biWidth/biHeight
    I16 1; I16 32                               # biPlanes / biBitCount
    for ($i = 0; $i -lt 6; $i++) { I32 0 }     # compression/sizeImage/ppm/colors
    $f.AddRange($flipped)                       # 像素数据
    $f.AddRange([byte[]](,0 * $maskSz))         # AND mask 全 0

    [System.IO.File]::WriteAllBytes($curPath, $f.ToArray())
    Write-Host "  $([System.IO.Path]::GetFileName($pngPath)) ($($src.PixelWidth)x$($src.PixelHeight)) -> $([System.IO.Path]::GetFileName($curPath)) ($($f.Count) bytes)"
}

$base = Join-Path $PSScriptRoot "..\Mine.Wpf.Controls\Assets\Cursors"
Write-Host "Converting cursors (${CursorSize}x${CursorSize}), mirror=$($Mirror.IsPresent)..."
ConvertToCur "$base\grab.png"     "$base\grab.cur"     $GrabHotX     $GrabHotY     $CursorSize $Mirror.IsPresent
ConvertToCur "$base\grabbing.png" "$base\grabbing.cur" $GrabbingHotX $GrabbingHotY $CursorSize $Mirror.IsPresent
Write-Host "Done. Commit the .cur files."
