# Silk.Data.SQL.ORM

`Silk.Data.SQL.ORM` is a .NET library for working with database entities and querying datasets.

It is intended to fill a gap between bigger, more complicated ORMs like EntityFramework/NHibernate and smaller libraries like Dapper - providing a rich query API surface that supports generating obvious SQL queries without the pains of change tracking APIs or lazy-loading from expired data contexts.

## Installing

`Silk.Data.SQL.ORM` is available as a NuGet package: https://www.nuget.org/packages/Silk.Data.SQL.ORM

You can install it from the NuGet package manager in Visual Studio or from command line with dotnet core:

~~~~
dotnet add package Silk.Data.SQL.ORM
~~~~

To run SQL queries against a database you'll also need a DataProvider library: for simplicity you can install [Silk.Data.SQL.ProvidersBundle](https://github.com/SilkStack/Silk.Data.SQL.ProvidersBundle)  which includes providers for [SQLite3](https://github.com/SilkStack/Silk.Data.SQL.SQLite3), [SqlServer](https://github.com/SilkStack/Silk.Data.SQL.SqlServer), [Postgresql](https://github.com/SilkStack/Silk.Data.SQL.Postgresql) and [MySQL/MariaDB](https://github.com/SilkStack/Silk.Data.SQL.MySQL).

## Features

- Type-safe query generation
- Entity and View type mappings
- Full async support
- Minimized round-trips to database servers
- Extensible query functions API
- Embed non-entity types into your schema
- Reference registered entity types with `JOIN` and support querying against referenced entities

## Platform Requirements

`Silk.Data.SQL.ORM` is built for netstandard2.0.

## License

`Silk.Data.SQL.ORM` is licensed under the MIT license.

## Usage

- Create a schema of entity types
- Create a database provider
- Create tables
- Basic entity queries
- Advanced querying
- Custom database functions

### Create a schema of entity types

To query a database first you have to tell `Silk.Data.SQL.ORM` the shape of your database. This is done by building a `Schema` instance (your application can have multiple schemas if needed) using the `SchemaBuilder` API to define your entity types:

~~~
var schema = new SchemaBuilder()
  .Define<UserAccount>(entityType => {
    entityType.Index("idx_loginNameEmailAddr", entity => entity.LoginName, entity => entity.EmailAddress)
  })
  .Build();
~~~

### Create a database provider

Creating a database provider isn't actually part of `Silk.Data.SQL.ORM` but is specific to which database you'd like to use. [Silk.Data.SQL.ProvidersBundle](https://github.com/SilkStack/Silk.Data.SQL.ProvidersBundle) provides 4 major providers:

~~~
var dataProvider =
  //  SQLite3
  new SQLite3DataProvider(new Uri("database.db", UriKind.Relative));
  //  SqlServer
  new MSSqlDataProvider(hostname, database, username, password);
  //  Postgresql
  new PostgresqlDataProvider(hostname, database, username, password);
  //  MySQL/MariaDB
  new MySQLDataProvider(hostname, database, username, password);
~~~

### Create tables

Now that you have a `Schema` and a `IDataProvider` we can start by creating tables. This is done using the `IEntityTable<T>` interface, the standard implementation of which is `EntityTable<T>`:

~~~
var table = new EntityTable<T>(schema, dataProvider);
await table.Create().ExecuteAsync();
~~~

### Basic entity queries

Now we have a `Schema`, an `IDataProvider` and we've created tables in the database provider: we're ready to start working with some entities!

The `ISqlEntityStore<T>` interface is what we'll work with for most entity operations. It allows us to work with single entities, query for entity sets and build custom queries as needed.

For now let's insert and query a simple entity set:

~~~
var store = new SqlEntityStore<T>(schema, dataProvider);

await new[]
{
  store
    .Insert(new UserAccount { LoginName = "DevJohnC", EmailAddress = "devjohnc@github.com", CreatedAtUtc = DateTime.UtcNow }),
    
  store
    .Select()
    .OrderByDescending(account => account.CreatedAtUtc)
    .Limit(1)
    .Defer(out var queryResult)
}.ExecuteAsync();

Console.WriteLine($"The most recently created user is: {queryResult.Result[0].LoginName}");
~~~

This will perform an `INSERT` query for your entity, map any generated primary key (currently any integer/guid type `Id` property) onto your entity instance and perform a `SELECT` for the most recently created user in a single round-trip to the database server.

### Advanced querying

But wait, there's more! `Silk.Data.SQL.ORM` also boasts some more powerful features:

**Transactions**

Execute your queries in a transaction using the `ExecuteAsTransaction`/`ExecuteAsTransactionAsync` extension methods or by creating an instance of `Transaction` directly and using it to execute queries:

~~~
var transaction = new Transaction();
await transaction.ExecuteAsync(...);
transaction.Commit();
~~~

**View Expressions**

The `SELECT` APIs on `ISqlEntityStore<T>` support providing an expression to be projected:

~~~
var allIDs = store.Select(entity => entity.Id).Execute();
~~~

This API currently supports specifying just a single property or database function call. In the future it will support a full anonymous type expression.

**View Type Projections**

As well as working with full entity types you can `SELECT`, `INSERT` and `DELETE` with view types too. View types use mappings generated by [Silk.Data.Modelling](https://github.com/SilkStack/Silk.Data.Modelling):

~~~
public class UserAccount
{
  //  a server-generated, auto-increment primary key
  public int Id { get; private set; }
  public string DisplayName { get; set; }
}

public class BlogPost
{
  public int Id { get; private set; }
  public UserAccount Author { get; set; }
}

public class BlogPostAuthorView
{
  //  mapped and queried from BlogPost.Author.DisplayName
  public string AuthorDisplayName { get; set; }
}

var blogPostAuthorsResult = blogPostStore
  .Select<BlogPostAuthorView>()
  .Execute();
~~~

**Type Coercion**

When providing projections `Silk.Data.SQL.ORM` can attempt to transform your data from the datatype it's stored as to the datatype declared on your view.

Type coercions supported:

- Conversion between numeric types
- Conversion to `string` using `ToString()`
- Conversion from `string` using any resolved `TryParse` method
- Conversion using a declared explicit operator

**Database Functions**

Any API in `Silk.Data.SQL.ORM` that accepts an `Expression<>` type can include method calls that will be converted in SQL query expressions. By default the methods declared on `DatabaseFunctions` and `Enum.HasFlag` are supported.

~~~
var count = store.Select(
    entity => DatabaseFunctions.Count(entity)
  )
  .AndWhere(
    entity => entity.Status.HasFlag(Status.IsPublished)
  )
  .Single()
  .Execute();
~~~

_Attempting to invoke CLR methods within your `Expression` will always be translated to SQL expressions, no runtime method invocations are supported in expressions currently._

### Custom database functions

You can provide your own database functions to help with writing complex or repetative SQL expressions for you.

- Write a static method signature on a helper class
- Implement `IMethodCallConverter`
- When building a schema call `Schema.AddMethodConverter` with a `MethodInfo` instance for your method and the `IMethodCallConverter` implementation

Examples of `IMethodCallConverter` can be found here: https://github.com/SilkStack/Silk.Data.SQL.ORM/tree/master/Silk.Data.SQL.ORM/Expressions.