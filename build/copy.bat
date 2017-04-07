
REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Contrib\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Aws\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Admin\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\Stripe\lib

COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack.Aws\lib\signed
COPY ..\src\ServiceStack.Text\bin\Signed\net45\ServiceStack.Text.* ..\..\ServiceStack.Admin\lib\signed

COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.1\*.* ..\..\ServiceStack\lib\netstandard1.1
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\*.* ..\..\ServiceStack\lib\netstandard1.3
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.1\*.* ..\..\ServiceStack.Redis\lib\netstandard1.1
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\*.* ..\..\ServiceStack.Redis\lib\netstandard1.3
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.1\*.* ..\..\ServiceStack.OrmLite\lib\netstandard1.1
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\*.* ..\..\ServiceStack.OrmLite\lib\netstandard1.3
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.1\*.* ..\..\ServiceStack.Aws\lib\netstandard1.1
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\*.* ..\..\ServiceStack.Aws\lib\netstandard1.3
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.1\*.* ..\..\Admin\lib\netstandard1.1
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\*.* ..\..\Admin\lib\netstandard1.3
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.1\*.* ..\..\Stripe\lib\netstandard1.1
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard1.3\*.* ..\..\Stripe\lib\netstandard1.3

COPY "..\src\ServiceStack.Text\bin\%BUILD%\portable45-net45+win8\ServiceStack.Text.dll" ..\..\ServiceStack\lib\pcl
COPY "..\src\ServiceStack.Text\bin\%BUILD%\portable45-net45+win8\ServiceStack.Text.*" ..\..\Stripe\lib\pcl

COPY ..\src\ServiceStack.Text.SL5\bin\SL5\ServiceStack.Text.* ..\..\ServiceStack\lib\sl5
