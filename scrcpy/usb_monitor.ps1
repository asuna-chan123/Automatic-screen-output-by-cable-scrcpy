Write-Host "USB monitor started. Waiting for Android device..." -ForegroundColor Green

Register-WmiEvent -Class Win32_DeviceChangeEvent -SourceIdentifier "USBMonitor" -Action {
    Write-Host "USB event detected." -ForegroundColor Yellow

    try {
        # Gọi adb devices
        $adbResult = & "C:\scrcpy\adb.exe" devices 2>$null

        # Tách từng dòng, chỉ lấy dòng kết thúc bằng 'device'
        $lines = $adbResult -split "`r?`n"
        $deviceLines = $lines | Where-Object { $_ -match "device\s*$" }

        if ($deviceLines) {
            Write-Host "ADB device detected." -ForegroundColor Cyan

            # Nếu chưa có scrcpy nào đang chạy thì mới mở thêm
            $scrcpyProc = Get-Process scrcpy -ErrorAction SilentlyContinue
            if (-not $scrcpyProc) {
                Write-Host "Starting scrcpy..." -ForegroundColor Cyan
                Start-Process "C:\scrcpy\run_scrcpy.bat"
            } else {
                Write-Host "scrcpy is already running, skip." -ForegroundColor DarkYellow
            }
        } else {
            Write-Host "No ADB device connected." -ForegroundColor DarkGray
        }
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
    }
} | Out-Null

while ($true) { Start-Sleep -Seconds 1 }
