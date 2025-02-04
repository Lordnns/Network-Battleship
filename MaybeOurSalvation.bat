@echo off
setlocal EnableDelayedExpansion

REM === Initialization ===
set "LOG_FILE=push_log.txt"
set /a part=1
set /a LIMIT=2000000000  REM 2GB in bytes (2GB)

REM === Reset Git Stage to Avoid Conflicts ===
git reset >> "%LOG_FILE%" 2>&1

REM Track total size of added files in this batch
set /a TOTAL=0
REM Track number of files added in this batch
set /a ADDED_FILES=0

echo Processing files for part %part%... >> "%LOG_FILE%"
echo Processing files for part %part%...

REM === Recursively list all files ===
for /r %%F in (*) do (
    set "FILE=%%F"

    REM === Skip if .gitignore excludes it ===
    git check-ignore -q "!FILE!" >nul 2>&1
    if !errorlevel!==0 (
        echo [SKIP] Ignored by .gitignore: !FILE! >> "%LOG_FILE%"
        set "FILE="
    )

    REM === Skip if file is locked ===
    >nul 2>&1 ( >>"!FILE!" (call )) || (
        echo [SKIP] Locked file: !FILE! >> "%LOG_FILE%"
        set "FILE="
    )

    REM === Process only if FILE is still valid ===
    if not "!FILE!"=="" (
        set /a FILE_SIZE=%%~zF
        set /a TEST_TOTAL=!TOTAL! + !FILE_SIZE!

        REM === Check if adding this file stays under limit ===
        if !TEST_TOTAL! LEQ %LIMIT% (
            git add "!FILE!" >> "%LOG_FILE%" 2>&1
            if errorlevel 1 (
                echo [ERROR] Failed to add file: !FILE! >> "%LOG_FILE%"
            ) else (
                set /a TOTAL+=!FILE_SIZE!
                set /a ADDED_FILES+=1
                echo Added: !FILE! (Total: !TOTAL! bytes) >> "%LOG_FILE%"
            )
        ) else (
            REM === If limit reached, commit & push current batch (if any files added)
            if !ADDED_FILES! GTR 0 (
                echo [INFO] Size limit reached. Committing part !part%! >> "%LOG_FILE%"
                git commit -m "Part !part! commit" >> "%LOG_FILE%" 2>&1
                if errorlevel 1 (
                    echo [ERROR] Commit failed! >> "%LOG_FILE%"
                ) else (
                    git push origin main >> "%LOG_FILE%" 2>&1
                    if errorlevel 1 (
                        echo [ERROR] Push failed! >> "%LOG_FILE%"
                    ) else (
                        echo [SUCCESS] Part !part! pushed successfully! >> "%LOG_FILE%"
                        set /a part+=1
                        git reset >> "%LOG_FILE%" 2>&1
                        set /a TOTAL=0
                        set /a ADDED_FILES=0

                        REM === After successful push, add this file to new batch
                        git add "!FILE!" >> "%LOG_FILE%" 2>&1
                        if errorlevel 1 (
                            echo [ERROR] Failed to add file after push: !FILE! >> "%LOG_FILE%"
                        ) else (
                            set /a TOTAL+=!FILE_SIZE!
                            set /a ADDED_FILES+=1
                            echo Added to new batch: !FILE! (Total: !TOTAL! bytes) >> "%LOG_FILE%"
                        )
                    )
                )
            )
        )
    )
)

REM === Final commit & push if any files remain
if !ADDED_FILES! GTR 0 (
    git commit -m "Final part !part! commit" >> "%LOG_FILE%" 2>&1
    git push origin main >> "%LOG_FILE%" 2>&1
)

echo All files processed successfully! >> "%LOG_FILE%"
type "%LOG_FILE%"
pause
exit /b

