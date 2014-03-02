
REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.Text\bin\%BUILD%\ServiceStack.Text.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\ServiceStack.Text.* ..\..\ServiceStack.Contrib\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib

COPY ..\src\ServiceStack.Text\bin\Signed\ServiceStack.Text.* ..\..\ServiceStack\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib\signed

COPY ..\src\ServiceStack.Text.SL5\bin\%BUILD%\ServiceStack.Text.* ..\..\ServiceStack\lib\sl5

COPY ..\src\ServiceStack.Text\bin\Pcl\ServiceStack.Text.* ..\..\ServiceStack\lib\pcl
