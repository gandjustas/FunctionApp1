# FunctionApp1

For local development create [user secret](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) in Visual Studio or using command line:
```Batchfile
dotnet user-secrets set "ConnecionStrings:Dynamics" "AuthType=OAuth;Username=<USERNAME>;Password=<PASSWORD>;Url=<ORG_URL>;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97"
```

To deploy to Azure:  
- Create Functions App using Azure portal
- Create "Dynamics" application settings with conection srting to Dynamics 365 same as user secret
- Use publishing wizard in Visual Studio to create publishing profile and deploy application 

