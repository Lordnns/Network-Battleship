@echo off
setlocal EnableDelayedExpansion

REM === Initialization ===
set "LOG_FILE=push_log.txt"
set /a part=1
set /a LIMIT=2000000000  REM 2GB in bytes

REM === Initialize Git Repository ===
git init >> %LOG_FILE% 2>&1
git branch -M main >> %LOG_FILE% 2>&1

set /p REMOTE_URL="Enter the Git remote repository URL: "
git remote add origin %REMOTE_URL% >> %LOG_FILE% 2>&1

REM === Commit and Push .gitignore First ===
if exist ".gitignore" (
    echo Adding and committing .gitignore... >> %LOG_FILE%
    git add .gitignore >> %LOG_FILE% 2>&1
    git commit -m "Add .gitignore" >> %LOG_FILE% 2>&1
    git push -u origin main >> %LOG_FILE% 2>&1
)

:PROCESS
set /a TOTAL=0
set /a ADDED_FILES=0

echo Processing files for part %part%... >> %LOG_FILE%
echo Processing files for part %part%...

REM === List All Files Recursively ===
for /r %%F in (*) do (
    set "FILE=%%F"

    REM === Debug: Show All Files ===
    echo [DEBUG] Found file: !FILE! >> %LOG_FILE%
    echo [DEBUG] Found file: !FILE!

    REM === Skip .git Directory and .gitignore ===
    echo !FILE! | findstr /I "\\.git\\" >nul
    if !errorlevel! == 0 (
        echo [SKIP] Skipping .git directory: !FILE! >> %LOG_FILE%
        rem Skip this file and continue with the next one
    ) else if "!FILE!"==".gitignore" (
        echo [SKIP] Skipping .gitignore: !FILE! >> %LOG_FILE%
        rem Skip this file and continue with the next one
    ) else (
        REM === Process File ===
        set /a FILE_SIZE=%%~zF
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
            goto :PROCESS
        )
    )
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

