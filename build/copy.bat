
REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Contrib\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\Stripe\lib
COPY "..\src\ServiceStack.Text\bin\%BUILD%\portable45-net45+win8\ServiceStack.Text.*" ..\..\Stripe\lib\pcl

COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib\signed

COPY ..\src\ServiceStack.Text.SL5\bin\SL5\ServiceStack.Text.* ..\..\ServiceStack\lib\sl5

COPY "..\src\ServiceStack.Text\bin\%BUILD%\portable45-net45+win8\ServiceStack.Text.dll" ..\..\ServiceStack\lib\pcl

COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.1\*.* ..\..\ServiceStack\lib\netstandard1.1
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\*.* ..\..\ServiceStack\lib\netstandard1.3
