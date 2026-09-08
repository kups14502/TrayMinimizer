# Builds TrayMinimizer.exe. Stops a running instance first, then relaunches it.
#
# Compiles against .NET Framework 4, which ships with every supported version of Windows, so the
# result is a single executable with nothing to install alongside it.

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe  = Join-Path $root 'TrayMinimizer.exe'
$src  = Get-ChildItem $root -Filter *.cs | Select-Object -ExpandProperty FullName

# The v4.0.30319 folder name is fixed for the whole .NET Framework 4.x line, but fall back to a
# search so an unusual Windows directory still builds.
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    $csc = Get-ChildItem (Join-Path $env:WINDIR 'Microsoft.NET') -Recurse -Filter csc.exe -ErrorAction SilentlyContinue |
           Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $csc -or -not (Test-Path $csc)) {
    throw "csc.exe not found. TrayMinimizer builds against .NET Framework 4, which ships with Windows."
}

$running = @(Get-Process TrayMinimizer -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe })
if ($running) {
    # Signal a clean exit so any parked windows get restored. Killing the process would strand them.
    Write-Host 'Stopping running instance...'
    try {
        $evt = [System.Threading.EventWaitHandle]::OpenExisting('Local\TrayMinimizer.Quit')
        [void]$evt.Set()
        $evt.Close()
        $running | Wait-Process -Timeout 8 -ErrorAction SilentlyContinue
    } catch {
        Write-Warning 'Quit signal unavailable, forcing stop (any parked windows will stay hidden).'
    }
    Get-Process TrayMinimizer -ErrorAction SilentlyContinue | Where-Object { $_.Id -in $running.Id } |
        Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
}

& $csc /nologo /target:winexe /platform:x64 /optimize+ /warn:4 `
    /out:$exe `
    /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll `
    $src

if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)" }

Write-Host ("Built {0} ({1:N0} bytes)" -f $exe, (Get-Item $exe).Length)
if ($running) { Start-Process -FilePath $exe; Write-Host 'Relaunched.' }
