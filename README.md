# ODAL

### Introduction

Oracle Data Access Layer is a convention-based, native .NET library for accessing data in an Oracle database.

The library is developed mainly for ASP.NET Core as it is well suited for the HTTP request executing and the unit of work concept. However, with slight modifications, you can make it work with .NET Framework without any issues.


### How it works

The library works by presenting required tables as models which are C# classes that is used in coordenation with a repository to execute CRUD operations. The same goes for procedures. All the interesting stuff is executed in BaseRepository.cs and BaseProcedure.cs
