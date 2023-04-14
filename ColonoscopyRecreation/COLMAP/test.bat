@echo off
set /a COUNTER=1
set /a MOD = %COUNTER% %% 2
echo %COUNTER% %MOD%
set /a COUNTER=%COUNTER% + 1
set /a MOD = %COUNTER% %% 2
echo %COUNTER% %MOD%