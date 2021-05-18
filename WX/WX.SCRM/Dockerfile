#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM registry.cn-shanghai.aliyuncs.com/youlo_wx/wx_scrm:aspnet AS base
WORKDIR /app
run ls
EXPOSE 80
EXPOSE 443

FROM registry.cn-shanghai.aliyuncs.com/youlo_wx/wx_scrm:sdk AS build
WORKDIR /src
run ls
ADD ["WX.SCRM/WX.SCRM.csproj", "WX.SCRM/"]
COPY ["WX.AdvancedTools/WX.AdvancedTools.csproj", "WX.AdvancedTools/"]
COPY ["WX.DataCache/WX.DataCache.csproj", "WX.DataCache/"]
COPY ["Common/WX.Common.csproj", "Common/"]
COPY ["WX.Comcon.Caching/WX.Comcon.Caching.csproj", "WX.Comcon.Caching/"]
COPY ["Wx.DB/WX.DB.csproj", "Wx.DB/"]
RUN dotnet --version
RUN dotnet restore "WX.SCRM/WX.SCRM.csproj"
run ls
COPY . .
run ls
WORKDIR "/src/WX.SCRM"
RUN dotnet build "WX.SCRM.csproj"  -o /home/build/WX.SCRM

FROM build AS publish
RUN dotnet publish "WX.SCRM.csproj"  -o /home/publish/WX.SCRM

FROM base AS final
WORKDIR /app
COPY --from=publish /home/publish/WX.SCRM .
ENTRYPOINT ["dotnet", "WX.SCRM.dll"]