nuget setapikey ______________________________________________ -Source https://api.nuget.org/v3/index.json
set ver=0.0.3
set slnFolder=D:\dev\big\LINQPadPlus
set nugetFolder=C:\Users\admin\.nuget\packages
set nugetUrl=https://api.nuget.org/v3/index.json


set prjName=LINQPadPlus
rem ===================
set nupkgFile=%slnFolder%\%prjName%\bin\Release\%prjName%.%ver%.nupkg
rmdir /s /q %nugetFolder%\%prjName%\%ver%
del /q %nupkgFile%
cd /d %slnFolder%\%prjName%
dotnet pack -p:version=%ver%
nuget add %nupkgFile% -source %nugetFolder% -expand
nuget push %nupkgFile% -Source %nugetUrl%
del /q %nupkgFile%



cd %slnFolder%
