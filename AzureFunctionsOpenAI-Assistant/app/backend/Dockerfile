# Use the official Microsoft .NET 5.0 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Add support for .NET 8 (required for Azure Functions build process)
COPY --from=mcr.microsoft.com/dotnet/sdk:8.0 /usr/share/dotnet/shared /usr/share/dotnet/shared

# Environment variable for getting test version
ENV testVersion=99.99.99-test



# Copy the .csproj file and restore the NuGet packages
COPY . /root


# Set the working directory
#WORKDIR /AzureFunctionsOpenAI-RAG-Assistant/app/backend

#COPY /AzureFunctionsOpenAI-RAG-Assistant/app/backend/ /root/app/backend
#RUN cd /root/AzureFunctionsOpenAI-RAG-Assistant/app/backend
# Build the extension 
RUN   mkdir -p /home/site/wwwroot 
RUN   mkdir ~/NuGetPackagesLocal 

RUN ls /root/app/backend/local-packages/

RUN  dotnet build /root/src/WebJobs.Extensions.OpenAI/WebJobs.Extensions.OpenAI.csproj -p:WebJobsVersion=$testVersion -p:Version=$testVersion 
  RUN  cp "/root/src/WebJobs.Extensions.OpenAI/bin/Debug/Microsoft.Azure.WebJobs.Extensions.OpenAI.${testVersion}.nupkg" ~/NuGetPackagesLocal 
  #RUN cp  "/root/app/backend/local-packages/Microsoft.Azure.Functions.Extensions.Dapr.99.99.99.nupkg" ~/NuGetPackagesLocal 
  #RUN cp  "/root/app/backend/local-packages/Microsoft.Azure.Functions.Extensions.Dapr.Core.99.99.99.nupkg" ~/NuGetPackagesLocal 
  RUN cp -r "/root/app/backend/local-packages/" ~/NuGetPackagesLocal
  RUN ls ~/NuGetPackagesLocal
  RUN ls /root/app/backend/local-packages/
  
  RUN  dotnet nuget add source ~/NuGetPackagesLocal --configfile  ~/.nuget/NuGet/NuGet.Config 
  WORKDIR /root/app/backend
  RUN  dotnet build -c Release -p:WebJobsVersion=$testVersion -p:Version=$testVersion --configfile ~/.nuget/NuGet/NuGet.Config 
  RUN dotnet publish --no-restore -c Release -o /home/site/wwwroot -p:WebJobsVersion=$testVersion -p:Version=$testVersion

  

  #RUN  dotnet nuget add source ~/NuGetPackagesLocal --configfile  ~/.nuget/NuGet/NuGet.Config 
  #RUN  dotnet build -c Release -p:WebJobsVersion="99.99.99" -p:Version="99.99.99" --configfile ~/.nuget/NuGet/NuGet.Config 
  #RUN dotnet publish --no-restore -c Release -o /home/site/wwwroot -p:WebJobsVersion="99.99.99" -p:Version="99.99.99"

# The final image is based on the Azure Functions 4.0 runtime image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

# This is the standard setup for Azure Functions running in Docker containers
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
COPY --from=build-env ["/home/site/wwwroot", "/home/site/wwwroot"]






