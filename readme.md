# Silk.Data.SQL.ORM

## Goals

* Strongly typed SQL queries
* Obvious query generation - no quessing at what queries are being executed, or when (no lazy-loading)
* Minimize round-trips to the database server
* Run against any database provider
* Capability to run (queries like UPDATE a SET b = b + 1? expressions?)
* Middle-ground between EF/nHibernate and micro-ORMs Dapper/massive


## Summary

`Silk.Data.SQL.ORM` is an ORM.

### What it does

* Avoids hand-written SQL statements
* Inserts, updates, deletes and queries entities
* SELECTs just the fields you request
* Performs automatic type flattening and JOINing for known entity types
* Transforms LINQ expressions into query expressions (ie. supports `Where(q => q.IsAwesome)`)

### What it doesn't do

* No lazy-loading - projection of needs is preferred
* No state change/modification monitoring
* No production of proxy/entity types
* Doesn't implement `IQueryable`
* Doesn't hide that you're working with an SQL database

## Usage

To get a good idea of how `Silk.Data.SQL.ORM` is intended to be used here's a basic example:

	//  first, create a schema to work with
	//  the schema will contain information about all our entity types
	var sqlProvider = new SQLite3Provider("friends.db");
	var schemaBuilder = new SchemaBuilder();
	schemaBuilder.DefineEntityType<UserAccount>();
	var schema = schemaBuilder.Build();

	//  second, query the database
	UserAccount joey;
	UserAccount monica;
	await sqlProvider.ExecuteAsync(
	  schema
    	  .CreateSelect<UserAccount>(query => query.AndWhere(user => user.FullName == "Monica Geller"))
    	  .WithFirstResult(r => monica = r);
	  schema
    	  .CreateSelect<UserAccount>(query => query.AnsWhere(user => user.FullName == "Joey Tribiani"))
    	  .WithFirstResult(r => joey = r);
	);

### Schema

The schema contains all the information about the entity types you can query against in the database. Most SQL APIs are extension methods on `Schema`.

### Executing queries

The APIs that hang off `Schema` only generate queries, they don't execute them. To execute queries you need a data provider: Sqlite3, MySQL/MariaDB, Postgresql and Sql Server are all supported currently and it's easy to write your own provider for another server type yourself.

See `Silk.Data.SQL.Base` for how to produce your own data provider.

### Batching

One of the main design goals of `Silk.Data.SQL.ORM` is to minimize round-trips to the database server. `Execute` and `ExecuteAsync` APIs hang off the `IDataProvider` interface and will execute any number of SQL queries you provide in a single trip to the SQL server.

### Projection

Projection is a powerful tool that allows you to retrieve just the data you want from your data domain.

Using projection you can request a series of fields you're interested in, a flattened representation of a graph, or projected related entities.

Consider:

	public class UserAccount
	{
		public Guid Id { get; private set; }
		public string Username { get; set; }
		public string EmailAddress { get; set; }
		public UserProfile Profile { get; set; }
	}

	public class UserProfile
	{
		public string Bio { get; set; }
		public Gender Gender { get; set; }
		public DateTime DateOfBirth { get; set; }
	}

What if we just need to get a users date of birth - we really don't want to query *everything* just to know a user's date of birth!

Create a projection view and query:

	public class UserDobView
	{
		public DateTime ProfileDateOfBirth { get; set; }
	}

	TODO: Code for query here

Here the property `ProfileDateOfBirth` is flattened from `UserAccount`.**`Profile`** and `UserProfile`.**`DateOfBirth`**.

Using projection will also permit you to INSERT, UPDATE or DELETE incomplete entities so long as the primary key field(s) are present on the projected type.

### Expression queries

`Silk.Data.SQL.ORM` provides API methods for executing queries against entity instances. For example:

    public class MyEntity
    {
        public int Id { get; private set; }
        public int Count { get; set; }
    }
    
Let's say we want to update the `Count` property of the `MyEntity` instance with an `Id` value of `1`.

    await provider.ExecuteAsync(
      schema.CreateUpdate<MyEntity>(query => {
        query.Set(entity => entity.Count, entity => entity.Count + 1);
        query.AndWhere(entity => entity.Id == 1);
      });
    );

### SQL functions

`Silk.Data.SQL.ORM` contains support for converting a C# method call expression into SQL in your queries - you can add your own converters to the `SchemaBuilder` when constructing you `Schema`.

By default the `DatabaseFunctions` static class methods are supported:

  * IsIn
  * Like
  * Count
  * Random
  * Alias

Example:

    var idArray = new[] { 1, 2, 4, 5 };
    schema.CreateSelect<MyEntity>(
      query => query.AndWhere(
        entity => DatabaseFunctions.IsIn(entity.Id, idArray)
      )
    );

### SchemaBuilder customization