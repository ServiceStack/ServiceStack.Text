
REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\..\ServiceStack.Contrib\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\..\ServiceStack.OrmLite\lib

COPY ..\src\ServiceStack.Text\bin\Signed\*.* ..\..\ServiceStack\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\*.* ..\..\ServiceStack.Redis\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\*.* ..\..\ServiceStack.OrmLite\lib\signed

COPY ..\src\ServiceStack.Text.SL5\bin\%BUILD%\*.* ..\..\ServiceStack\lib\sl5

MD ..\NuGet\lib\net35
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\NuGet\lib\net35

COPY ..\src\ServiceStack.Text\bin\Pcl\*.* ..\..\ServiceStack\lib\pcl
