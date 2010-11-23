
REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.dll C:\src\ServiceStack\release\latest\ServiceStack.Text\
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.dll C:\src\ServiceStack\release\lib
COPY ..\src\ServiceStack.Text\bin\%BUILD%\*.pdb C:\src\ServiceStack\release\lib
