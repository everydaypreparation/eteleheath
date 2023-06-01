FROM mcr.microsoft.com/dotnet/aspnet:3.1

RUN apt-get -y update
RUN apt-get install -y postgresql-client
RUN psql --version  

WORKDIR /app

COPY ./build/output/EMRO.Web.Host/. .


RUN apt-get --allow-releaseinfo-change update \ 
    && apt-get install -y --no-install-recommends \
        zlib1g \
        fontconfig \
        libfreetype6 \
        libx11-6 \
        libxext6 \
        libxrender1 \
		libgdiplus \
    && curl -o /usr/lib/libwkhtmltox.so \
        --location \
        https://github.com/rdvojmoc/DinkToPdf/raw/v1.0.8/v0.12.4/64%20bit/libwkhtmltox.so

#
RUN cd /usr/lib
RUN ln -s libgdiplus.so gdiplus.dll

ENTRYPOINT ["dotnet", "EMRO.Web.Host.dll"]
# CMD env "PATH=$PATH:/home/USER/.dotnet/tools" dotnet ef database update
