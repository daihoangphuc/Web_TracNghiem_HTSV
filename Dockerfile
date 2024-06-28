# Step 1: Use ASP.NET 6.0 image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Step 2: Set up HTTP for Kestrel
ENV ASPNETCORE_URLS=http://+:80

# Step 3: Install certificates if needed (commented out for HTTP-only deployment)
# COPY Certificates/certificate.crt /app
# COPY Certificates/private.key /app
# COPY Certificates/your_certificate.pfx /app

# Step 4: Create a new stage to execute dotnet dev-certs commands if needed
# FROM mcr.microsoft.com/dotnet/sdk:6.0 AS certs
# WORKDIR /app

# Declare ARG to pass variables from the build command
# ARG PFX_PASSWORD

# Use the ARG variable with the dotnet dev-certs command
# RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p $PFX_PASSWORD
# RUN openssl pkcs12 -in /https/aspnetapp.pfx -out /https/aspnetapp.pem -nodes -password pass:$PFX_PASSWORD

# Step 5: Install the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

# Declare ARG variables to pass from CI/CD pipeline secrets
ARG DB_PASSWORD
ARG SMTP_PASSWORD
# ARG PFX_PASSWORD

# Step 6: Set environment variables at runtime
ENV DB_PASSWORD=$DB_PASSWORD
ENV SMTP_PASSWORD=$SMTP_PASSWORD
# ENV PFX_PASSWORD=$PFX_PASSWORD

# Replace the ${secrets.} string in appsettings.json with the value of the environment variable
RUN sed -i "s|\${secrets.DB_PASSWORD}|$DB_PASSWORD|g" appsettings.json
RUN sed -i "s|\${secrets.SMTP_PASSWORD}|$SMTP_PASSWORD|g" appsettings.json

RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Step 7: Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Step 8: Build the final application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Web_TracNghiem_HTSV.dll"]
