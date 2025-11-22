# Automatic screen output by cable, only for Android

1.Dowload File.
---
Scrcpy

2.Put the downloaded file where you want.
---
Recommended: C:/scrcpy

3.Copy.
---
Go to scrcpy and find the file named: "run_scrcpy_monitor"& "USB scrcpy monitor".

Copy it.

4.Use key combinations (Windown + R).
---
Enter the following command into the run block
```
shell:startup
```

5.Paste.
---
Paste the 2 files you just copied into the Startup folder

6.Testing.(Windown + R)
---
Enter the following command into the run block.
```
C:\scrcpy\run_scrcpy_monitor.vbs
```
# Change the path if you put the file somewhere else

6.Other problems
---
File not working:

Please check the link again if it is different I recommend.

Open "usb_monitor file" with notepad

Edit the following line:
```
        $adbResult = & "C:\scrcpy\adb.exe" devices 2>$null
                          ↑
                        Change to the path you placed the file
```
