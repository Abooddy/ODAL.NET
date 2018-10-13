# ODAL

### Introduction

Oracle Data Access Layer is a convention-based, native .NET library for accessing data in an Oracle database.

The library is developed mainly for ASP.NET Core as it is well suited for the HTTP request executing and the unit of work concept. However, with slight modifications, you can make it work with .NET Framework without any issues.



### How it works

The library works by presenting required tables as models which are C# classes that is used in coordination with a repository to execute CRUD operations. The same goes for procedures. All the interesting stuff is executed in BaseRepository.cs and BaseProcedure.cs.



### Conventions

1. Anything related to the database should be underscore conventioned, e.g. **T_USERS** as users table and **USER_ROLE** as a column in the users table. 

2. Each table intended to interact with the library should have an **ID** column of type NUMBER and a sequence named **S_TABLE_NAME**, eg, **S_USERS** to work with **T_USERS**.

3. Anything related to the .NET part should be initial case conventioned, eg, **T_USERS** in the database will be **TUsers** in .NET and **USER_ROLE** column will be **UserRole** in .NET.

4. Each procedure intended to interact with the library should have three output parameters, **O_DATA_OUT**, **O_ERROR_CODE** and **O_ERROR_DESC**. The first parameter is for any data you wish to return from the procedure as JSON. The second parameter is the error code of the procedure indicating the result of executing the procedure. And the last parameter is for any string you wish to return as a reason for the result.



### But Abdullah, those conventions are easy to forget!

Don't worry, in the PLSQL directory there is a code generator to write to you all the code that you need. Just give it the table name and hit execute in a PLSQL test window and all the code will be generated for you in the DBMS_OUTPUT! Same goes for procedures.

How to use the code generator:

1. Compile the package ODAL_CODE_GENERATOR.
2. Change the CONST_OWNER constant in the package specification to your schema user.
3. To generate the code required for tables execute the REPOSITORY_MAIN_CG procedure and give it the table name as a parameter.

4. To generate the code required for procedures execute the PROCEDURE_MAIN_CG and give it the full procedure name (preceded with package name) as a parameter.

5. Create the model in the Models directory and the repository class and interface in their corresponding directories. Same goes for procedures. You can have as many input parameters as you want.

After executing the corresponding code generator you should have for example:

Model:

```C#
using System;

namespace ODAL.Models
{
    public class TUsers : IModel
    {
        public int? Id { get; set; }
        public string InstId { get; set; }
        public int? RegisteredBy { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string IsBlocked { get; set; }
        public int? FailedLoginCounter { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? BlockDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserRole { get; set; }
    }
}
```

Fields:

```C#
namespace ODAL.Fields
{
    public class TUsersFields
    {
        public const string Id = "ID";
        public const string InstId = "INST_ID";
        public const string RegisteredBy = "REGISTERED_BY";
        public const string Username = "USERNAME";
        public const string Password = "PASSWORD";
        public const string Salt = "SALT";
        public const string IsBlocked = "IS_BLOCKED";
        public const string FailedLoginCounter = "FAILED_LOGIN_COUNTER";
        public const string InsertDate = "INSERT_DATE";
        public const string BlockDate = "BLOCK_DATE";
        public const string LastUpdate = "LAST_UPDATE";
        public const string LastLoginDate = "LAST_LOGIN_DATE";
        public const string FirstName = "FIRST_NAME";
        public const string LastName = "LAST_NAME";
        public const string UserRole = "USER_ROLE";
    }
}
```

Repository Class:

```C#
using ODAL.Helpers;
using ODAL.Infrastructure;
using ODAL.Models;
using Microsoft.Extensions.Options;

namespace ODAL.Repositories
{
    public class TUsersRepository : BaseRepository, ITUsersRepository
    {
        public TUsersRepository(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {

        }

        protected override string GetTableName()
        {
            return DatabaseHelper.ToDbNamingConvention(typeof(TUsers).Name);
        }

        protected override IModel CreateModel()
        {
            return new TUsers();
        }
    }
}
```

Repository Interface:

```C#
namespace ODAL.Repositories
{
    public interface ITUsersRepository : IBaseRepository
    {

    }
}
```

If you were generating code for a procedure then you will have the procedure class:

```C#
using System.Collections.Generic;
using System.Data;
using ODAL.Helpers;
using ODAL.Infrastructure;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace ODAL.Procedures
{
    public class LoginProcedure : BaseProcedure, ILoginProcedure
    {
        public string PUsername { get; set; }
        public string PPassword { get; set; }
        public string PInst { get; set; }

        private readonly DatabaseConfigHelper configuration;

        public LoginProcedure(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {
            this.configuration = configuration.Value;
        }

        protected override string GetProcedureName()
        {
            return "PKG_ACCOUNT.LOGIN";
        }

        protected override IEnumerable<OracleParameter> GetProcedureParameters()
        {
            List<OracleParameter> parameterCollection = new List<OracleParameter>();

            foreach (var parameter in this.GetType().GetProperties())
            {
                parameterCollection.Add(new OracleParameter(
                 DatabaseHelper.ToDbNamingConvention(parameter.Name),
                 DatabaseHelper.ToOraclDbType(parameter.PropertyType),
                 DatabaseHelper.GetParameterSize(parameter.PropertyType, configuration),
                 parameter.GetValue(this),
                 ParameterDirection.Input));
            }

            // ADD ADDITIONAL CUSTOM PARAMETERS HERE! STILL NOT RECOMMENDED.

            return parameterCollection;
        }
    }
}
```

Procedure Interface:

```C#
namespace ODAL.Procedures
{
    public interface ILoginProcedure : IBaseProcedure
    {
        string PUsername { get; set; }
        string PPassword { get; set; }
        string PInst { get; set; }
    }
}
```

6. If you're using the library in an ASP.NET Core application then you should register the repositories and procedures as services in StartUp.cs file along with all needed objects and instantiate them via constructor injection.

```C#
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IDatabaseContextFactory, DatabaseContextFactory>();
services.AddScoped<IQueryContext, QueryContext>();        
services.AddScoped<ITUsersRepository, TUsersRepository>();
services.AddScoped<ILoginProcedure, LoginProcedure>();
```


## Examples

To execute CRUD operations, you will deal with the query context object to get data from the database and the repository to execute the get operations plus the DML operations. Please note that all required objects in the examples were obtained using constructor injection.

#### Get Data

Getting a user by his ID:

```C#
                queryContext.OffsetClause = null;
                queryContext.OrderByClause = null;

                queryContext.Columns = new List<string>()
                {
                    TUsersFields.Username,
                    TUsersFields.InstId,
                    TUsersFields.FirstName,
                    TUsersFields.LastName,
                    TUsersFields.UserRole
                };

                queryContext.WhereClause = new WhereClause()
                {
                    Conditions = new List<Condition>()
                    {
                        new Condition(TUsersFields.Id, userId)
                    }
                };

                var data = (await usersRepository.GetAsync(queryContext, cancellationToken)).First() as TUsers;
```

Getting all users and ordering them:

```C#
                queryContext.WhereClause = null;
                queryContext.OffsetClause = null;

                queryContext.Columns = new List<string>()
                {
                    TUsersFields.Id,
                    TUsersFields.InstId,
                    TUsersFields.Username,
                    TUsersFields.IsBlocked,
                    TUsersFields.InsertDate,
                    TUsersFields.FirstName,
                    TUsersFields.LastName,
                    TUsersFields.UserRole
                };

                queryContext.OrderByClause = new OrderByClause()
                {
                    ColumnName = TUsersFields.InsertDate,
                    Operator = OrderByOperator.DESC
                };

                var data = await usersRepository.GetAsync(queryContext, cancellationToken);
```

You can use the offset clause by specifying the page index and page size to the OffestClause object after instantiating it.

#### Inserting Data

```C#
                    OracleTransaction transaction = unitOfWork.BeginTransaction();
                    
                    TUsers newUser = new TUsers()
                    {
                        Id = null,                    // Will be generated automatically by the library.
                        InstId = Institution,
                        RegisteredBy = CuId,
                        Username = Username,
                        Password = hashedPassword,
                        Salt = salt,
                        IsBlocked = "N",
                        FailedLoginCounter = 0,
                        InsertDate = DateTime.Now,
                        BlockDate = null,
                        LastUpdate = DateTime.Now,
                        LastLoginDate = null,
                        FirstName = FirstName,
                        LastName = LastName,
                        UserRole = Role
                    };

                    var insertionResult = await usersRepository.InsertAsync(newUser, transaction, cancellationToken);

                    if (insertionResult)
                    {
                        transaction.Commit();
                    }

                    else
                    {
                        transaction.Rollback();
                    }
```

#### Updating Data

```C#
                OracleTransaction transaction = unitOfWork.BeginTransaction();
                
                queryContext.Columns = null;
                queryContext.OffsetClause = null;
                queryContext.OrderByClause = null;
                queryContext.WhereClause = new WhereClause()
                {
                    Conditions = new List<Condition>()
                    {
                        new Condition(TUsersFields.Id, UserId),
                        new Condition(TUsersFields.Username, Username),
                    }
                };

                var user = (await usersRepository.GetAsync(queryContext, cancellationToken)).First() as TUsers;

                user.InstId = NewInstitution;
                user.FirstName = NewFirstName;
                user.LastName = NewLastName;
                user.UserRole = NewRole;
                user.LastUpdate = DateTime.Now;

                var updateResult = await usersRepository.UpdateAsync(user, transaction, cancellationToken);

                if (updateResult)
                {
                    transaction.Commit();
                }

                else
                {
                    transaction.Rollback();
                }
```

#### Deleting Data

```C#
                OracleTransaction transaction = unitOfWork.BeginTransaction();
                
                var deletionResult = await usersRepository.DeleteAsync(UserId, transaction, cancellationToken);

                if (deletionResult)
                {
                    transaction.Commit();
                }

                else
                {
                    transaction.Rollback();
                }
```

#### Executing Procedures

```C#
                loginProcedure.PUsername = username;
                loginProcedure.PPassword = hashedPassword;
                loginProcedure.PInst = institution;

                var procedureResult = await loginProcedure.Execute(cancellationToken);
```
