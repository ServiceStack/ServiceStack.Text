SET NUGET=..\src\.nuget\nuget
%NUGET% push ServiceStack.Text.4.0.15.nupkg
%NUGET% push ServiceStack.Text.4.0.15.symbols.nupkg

%NUGET% push ServiceStack.Text.Pcl.4.0.15.nupkg
%NUGET% push ServiceStack.Text.Pcl.4.0.15.symbols.nupkg

%NUGET% push ServiceStack.Text.Signed.4.0.15.nupkg
%NUGET% push ServiceStack.Text.Signed.4.0.15.symbols.nupkg
