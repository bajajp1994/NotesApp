﻿# NotesApp
A simple and efficient note-taking application designed for seamless note creation, retrieval, and management.
Built using modern frameworks and tools to ensure scalability and maintainability.

* **UI Access:** https://notesapi20241207161404.azurewebsites.net/

## Technologies Used
### DB and Framework
* **Framework:** ASP.NET Core 8

Chosen for its high performance, scalability, and compatibility with modern web standards.
* **Database:** SQL Server

Selected for its reliability and seamless integration with .NET applications.
* **Unit Testing**: XUnit

### 3rd Party Tools
* **Swagger (Swashbuckle):** To provide an interactive API documentation page for developers.
* **Postman:** For API testing and providing an easy-to-use collection for developers.


## How To Run the project locally
1) Download .NET SDK and verify `dotnet --version`
2) Install Visual Studio 2022 
3) Install [`SQL Server`](https://www.microsoft.com/en-us/sql-server) for DB with local credentials `Server=localhost;Database=NotesDb;User Id=sa;Password=notesapi;TrustServerCertificate=true;` in appSettings.json file under connectionSettings
1. DB: `NotesDB`
1. UserId: `sa`
1. Password: `notesapi`
4) To run the project, go inside the root folder of the project and run: `dotnet clean; dotnet build; dotnet run` and swagger will be automatically open
5) To run Unit tests: go to directory NotesAPI.Tests. Run `dotnet clean; dotnet build; dotnet test`


### Criterias

1) Implemented Throttling of 100 requests/min (appSettings.json)

```
"IpRateLimiting": { //Throttling Configuration
      "EnableEndpointRateLimiting": true,
      "StackBlockedRequests": false,
      "RealTime": true,
      "GeneralRules": [
        {
          "Endpoint": "*",
          "Period": "1m",
          "Limit": 100
        }
      ]
}
```
2) For Integration Test:

    Shared Postman API collections

3) Unit tests
   
* AuthControllerTests.cs
* NotesControllerTests.cs

5) For Authorization and Authentication

   `JWT token` has been implemented

6) Deployment

   Azure App Service   

