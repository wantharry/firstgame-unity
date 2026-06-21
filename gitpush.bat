@echo off
echo === Removing stale git lock ===
del /f /q "C:\Users\openclaw\firstgame\.git\index.lock" 2>nul

echo === Setting git identity ===
cd /d "C:\Users\openclaw\firstgame"
git config user.email "wantharry@gmail.com"
git config user.name "Harry"

echo === Removing large assets from git tracking (if previously added) ===
git rm -r --cached Assets/FurnishedCabin/ 2>nul
git rm --cached Assets/FurnishedCabin.meta 2>nul
git rm -r --cached Assets/ALP_Assets/ 2>nul
git rm --cached Assets/ALP_Assets.meta 2>nul
git rm --cached Assets/Resources/BillingMode.json.meta 2>nul

echo === Staging required files ===
git add .gitignore
git add ASSETS.md
git add Assets/Scripts/Bootstrap.cs
git add Assets/Scripts/GameManager.cs
git add Assets/Scripts/CharacterAnimator.cs
git add Assets/Scripts/PlayerController.cs
git add Assets/Scenes/SampleScene.unity
git add Packages/manifest.json

echo === Files to be committed ===
git status

echo === Committing ===
git commit -m "Furnished Cabin: openable door, key disappears on use, player -30pct size, crosshair; add ASSETS.md"

echo === Pushing ===
git push origin master

echo.
echo === Done! Press any key to close ===
pause
