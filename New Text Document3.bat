@echo off
setlocal EnableDelayedExpansion

REM === Initialization ===
set "LOG_FILE=push_log.txt"
set /a part=1
set /a LIMIT=2000000000  REM 2GB in bytes

REM === Reset Git Stage to Avoid Conflicts ===
git reset >> %LOG_FILE% 2>&1

:PROCESS
set /a TOTAL=0
set /a ADDED_FILES=0

echo Processing files for part %part%... >> %LOG_FILE%
echo Processing files for part %part%...

REM === List All Files Recursively ===
for /r %%F in (*) do (
    set "FILE=%%F"
    echo %%F

    REM === Skip Files Ignored by .gitignore ===
    git check-ignore -q "!FILE!"
    if !errorlevel! == 0 (
        echo [SKIP] Ignored by .gitignore: !FILE! >> %LOG_FILE%
        goto :nextfile
    )
    
    set /a FILE_SIZE=%%~zF

    echo Processing: !FILE! (!FILE_SIZE! bytes)
    echo Processing: !FILE! (!FILE_SIZE! bytes) >> %LOG_FILE%

    set /a TEST_TOTAL=!TOTAL! + !FILE_SIZE!

    if !TEST_TOTAL! LEQ %LIMIT% (
        git add "!FILE!" >> %LOG_FILE% 2>&1
        if errorlevel 1 (
            echo [ERROR] Failed to add file: !FILE! >> %LOG_FILE%
            goto :error
        )
        set /a TOTAL+=!FILE_SIZE!
        set /a ADDED_FILES+=1
        echo Added: !FILE! (Total: !TOTAL! bytes) >> %LOG_FILE%
    ) else (
        echo [INFO] Size limit reached. Committing part %part%... >> %LOG_FILE%

        REM === Commit and Push Current Batch ===
        if !ADDED_FILES! GTR 0 (
            git commit -m "Part !part! commit" >> %LOG_FILE% 2>&1
            if errorlevel 1 (
                echo [ERROR] Commit failed. >> %LOG_FILE%
                goto :error
            )
            git push origin main >> %LOG_FILE% 2>&1
            if errorlevel 1 (
                echo [ERROR] Push failed. >> %LOG_FILE%
                goto :error
            )
            set /a part+=1
        )

        REM === Start New Batch ===
        set /a TOTAL=0
        set /a ADDED_FILES=0

        REM === Add the skipped file in the next batch ===
        set "RETRY_FILE=!FILE!"
        goto :RETRY
    )

    :nextfile
)

:RETRY
if defined RETRY_FILE (
    set "FILE=!RETRY_FILE!"
    for %%A in ("!FILE!") do set /a FILE_SIZE=%%~zA
    if "!FILE_SIZE!"=="" set /a FILE_SIZE=0

    git add "!FILE!" >> %LOG_FILE% 2>&1
    if errorlevel 1 (
        echo [ERROR] Failed to add file on retry: !FILE! >> %LOG_FILE%
        goto :error
    )
    set /a TOTAL+=!FILE_SIZE!
    set /a ADDED_FILES+=1
    set "RETRY_FILE="
    goto :PROCESS
)

REM === Final Commit and Push ===
if !ADDED_FILES! GTR 0 (
    git commit -m "Final part !part! commit" >> %LOG_FILE% 2>&1
    git push origin main >> %LOG_FILE% 2>&1
)

echo All files processed successfully! >> %LOG_FILE%
pause
exit /b

:error
echo [ERROR] An issue occurred. Check the log below:
type %LOG_FILE%
pause
exit /b

