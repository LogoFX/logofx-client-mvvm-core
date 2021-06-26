SET package_name=LogoFX.Client.Mvvm.Core
SET package_version=2.2.0
cd ../../src
nuget restore
cd ../devops
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& './build/build.ps1'"
cd ./pack
call ./pack.bat
cd ../publish
call ./copy.bat %package_name% %package_version% %1
cd ../install
call uninstall-global-single.bat %package_name% %package_version%
cd ..