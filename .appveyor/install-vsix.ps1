$vsixPath = "$($env:USERPROFILE)\sqlite-wp81-winrt-3081002.vsix"
(New-Object Net.WebClient).DownloadFile('https://visualstudiogallery.msdn.microsoft.com/5d97faf6-39e3-4048-a0bc-adde2af75d1b/file/132406/15/sqlite-wp81-winrt-3081002.vsix', $vsixPath)
"`"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\VSIXInstaller.exe`" /q /a $vsixPath" | out-file ".\install-vsix.cmd" -Encoding ASCII
& .\install-vsix.cmd
