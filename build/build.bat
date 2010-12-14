
REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\..\ServiceStack\release\latest\ServiceStack.Text\
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\..\ServiceStack\release\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.* ..\..\ServiceStack\lib
