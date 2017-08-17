REM SET BUILD=Debug
SET BUILD=Release

COPY "..\..\ServiceStack\src\ServiceStack.Interfaces\bin\%BUILD%\portable40-net45+sl5+win8+wp8+wpa81\ServiceStack.Interfaces.*" pcl

COPY ..\..\ServiceStack\src\ServiceStack.Client\bin\%BUILD%\net45\ServiceStack.Client.* net45
COPY ..\..\ServiceStack\src\ServiceStack.Client\bin\%BUILD%\netstandard1.1\ServiceStack.Client.* netstandard1.1
COPY ..\..\ServiceStack\src\ServiceStack.Client\bin\%BUILD%\netstandard1.6\ServiceStack.Client.* netstandard1.6

COPY ..\..\ServiceStack\src\ServiceStack.Common\bin\%BUILD%\net45\ServiceStack.Common.* net45
COPY ..\..\ServiceStack\src\ServiceStack.Common\bin\%BUILD%\netstandard1.3\ServiceStack.Common.* netstandard1.3

COPY ..\..\ServiceStack\src\ServiceStack\bin\%BUILD%\net45\ServiceStack.dll net45
COPY ..\..\ServiceStack\src\ServiceStack\bin\%BUILD%\net45\ServiceStack.xml net45
COPY ..\..\ServiceStack\src\ServiceStack\bin\%BUILD%\netstandard1.6\ServiceStack.dll netstandard1.6
COPY ..\..\ServiceStack\src\ServiceStack\bin\%BUILD%\netstandard1.6\ServiceStack.xml netstandard1.6

