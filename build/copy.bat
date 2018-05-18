
REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack\lib\net45
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard2.0\ServiceStack.Text.* ..\..\ServiceStack\lib\netstandard2.0
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib\net45
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard2.0\ServiceStack.Text.* ..\..\ServiceStack.Redis\lib\netstandard2.0
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib\net45
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard2.0\ServiceStack.Text.* ..\..\ServiceStack.OrmLite\lib\netstandard2.0
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Aws\lib\net45
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard2.0\ServiceStack.Text.* ..\..\ServiceStack.Aws\lib\netstandard2.0
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\ServiceStack.Admin\lib\net45
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard2.0\ServiceStack.Text.* ..\..\ServiceStack.Admin\lib\netstandard2.0
COPY ..\src\ServiceStack.Text\bin\%BUILD%\net45\ServiceStack.Text.* ..\..\Stripe\lib\net45
COPY ..\src\ServiceStack.Text\bin\%BUILD%\netstandard2.0\ServiceStack.Text.* ..\..\Stripe\lib\netstandard2.0

