# Automatic screen output by cable, only for Android

1.Dowload File.
---
Scrcpy

2.Put the downloaded file where you want.
---
Recommended: C:/scrcpy

3.Testing.(Windown + R)
---
Enter the following command into the run block.
```
C:\scrcpy\run_scrcpy_monitor.vbs
```
# Change the path if you put the file somewhere else

4.Other problems
---
-File not working:
---
Please check the link again if it is different I recommend.

Open "usb_monitor file" with notepad

Edit the following line:
```
        $adbResult = & "C:\scrcpy\adb.exe" devices 2>$null
                          ↑
                        Change to the path you placed the file
```
