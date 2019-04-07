@echo off

cd %1
set s=%2 -i %3 %4 %5
set s=%s:$$=%
set s=%s:"=%
set s=%s:'="%
echo %s%
ffmpeg %s%

echo %errorlevel%