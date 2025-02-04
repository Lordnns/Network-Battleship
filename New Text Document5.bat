@echo off
setlocal EnableDelayedExpansion

REM === Initialization ===
set "LOG_FILE=push_log.txt"
set /a part=1
set /a LIMIT=2000000000  REM 2GB in bytes

REM === Reset Git Stage to Avoid Conflicts ===
git reset >> %LOG_FILE% 2>&1

set /a TOTAL=0
set /a ADDED_FILES=0

echo Processing files for part %part%... >> %LOG_FILE%
echo Processing files for part %part%...

REM === List All Files Recursively ===
for /r %%F in (*) do (
    set "FILE=%%F"

    REM === Skip Files Ignored by .gitignore ===
    git check-ignore -q "!FILE!" >nul 2>&1
    if !errorlevel! == 0 (
        echo [SKIP] Ignored by .gitignore: !FILE! >> %LOG_FILE%
        set "FILE="
    )

    REM === Skip Locked Files Without Stopping ===
    >nul 2>&1 ( >>"!FILE!" (call )) || (
        echo [SKIP] Locked file: !FILE! >> %LOG_FILE%
        set "FILE="
    )

    REM === Process Only Valid Files ===
    if not "!FILE!"=="" (
        set /a FILE_SIZE=%%~zF
        set /a TEST_TOTAL=!TOTAL! + !FILE_SIZE!

        if !TEST_TOTAL! LEQ %LIMIT% (
            git add "!FILE!" >> %LOG_FILE% 2>&1
            if errorlevel 1 (
                echo [ERROR] Failed to add file: !FILE! >> %LOG_FILE%
            ) else (
                set /a TOTAL+=!FILE_SIZE!
                set /a ADDED_FILES+=1
                echo Added: !FILE! (Total: !TOTAL! bytes) >> %LOG_FILE%
            )
        ) else (
            REM === Commit and Push if Size Limit Reached ===
            if !ADDED_FILES! GTR 0 (
                echo [INFO] Size limit reached. Committing part %part%... >> %LOG_FILE%
                git commit -m "Part !part! commit" >> %LOG_FILE% 2>&1
                if errorlevel 1 (
                    echo [ERROR] Commit failed. >> %LOG_FILE%
                ) else (
                    git push origin main >> %LOG_FILE% 2>&1
                    if errorlevel 1 (
                        echo [ERROR] Push failed. >> %LOG_FILE%
                    ) else (
                        echo [SUCCESS] Part !part! pushed successfully! >> %LOG_FILE%
                        set /a part+=1
                        git reset >> %LOG_FILE% 2>&1
                        set /a TOTAL=0
                        set /a ADDED_FILES=0

                        REM === Add the Current File to the Next Commit ===
                        git add "!FILE!" >> %LOG_FILE% 2>&1
                        if errorlevel 1 (
                            echo [ERROR] Failed to add file after push: !FILE! >> %LOG_FILE%
                        ) else (
                            set /a TOTAL+=!FILE_SIZE!
                            set /a ADDED_FILES+=1
                            echo Added to new commit: !FILE! (Total: !TOTAL! bytes) >> %LOG_FILE%
                        )
                    )
                )
            )
        )
    )
)

REM === Final Commit and Push if Files are Left ===
if !ADDED_FILES! GTR 0 (
    git commit -m "Final part !part! commit" >> %LOG_FILE% 2>&1
    git push origin main >> %LOG_FILE% 2>&1
)

echo All files processed successfully! >> %LOG_FILE%
pause
exit /b
