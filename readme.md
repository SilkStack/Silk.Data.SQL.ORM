# Silk.Data.SQL.ORM

**This document outlines the intended state of the API by the time vNext targets are reached - it's too long for a readme but serves also as a living design document.**

## Summary

`Silk.Data.SQL.ORM` is a data orientated API for working with entities and building SQL statements that execute against *almost* any SQL server (MySQL, Postgresql, SQLite3 and Microsoft SQL Server). The goal is to build a simple ORM that sits in the middleground between big, complicated ORMs like EntityFramework or nHibernate and the small, feature limited "ORMs" like Dapper or massive.

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

	//  first, create a data domain to work with
	var sqlProvider = new SQLite3Provider("friends.db");
	var domainDefinition = new DataDomainDefinition();
	domainDefinition.AddEntityType<UserAccount>();
	var dataDomain = domainDefinition.Build();

	//  second, create the schema - this will CREATE all tables, indexes and relationship constraints
	await dataDomain.CreateSchema().ExecuteAsync(sqlProvider);

	//  third, now query the entities in the database
	IDatabase<T> database = new EntityDatabase<UserAccount>(dataDomain);
	var (joey, monica) = await database
		.Insert(
			new UserAccount { Username = "joey.tribiani", EmailAddress = "joey@friends.com" },
			new UserAccount { Username = "monica.geller", EmailAddress = "monica@friends.com" }
		)
		.SelectSingle(
			userAccount => userAccount.Username == "joey.tribiani"
		)
		.SelectSingle(
			userAccount => userAccount.Username == "monica.geller"
		)
		.AsBatch()
		.ExecuteAsync(sqlProvider);

### DataDomain

The first thing you might notice is the `DataDomain`. This object contains all the collected knowledge about your schema and entities; each of the data components in your project can register entities in the domain as it's being built so the domain can orchestrate the loading of related entities without components having to be aware of each other.

A `DataDomain` is built from a `DataDomainDefinition` to which you register your entity types and provide opinions on how you want your schema to be designed.

### IDatabase(T)

`IDatabase<T>` is the easiest, but not the only, way to work with entities. It provides a fluent API for working with a single type of entity **but should not be considered a repository in your application's design!**

The standard implementation of `IDatabase<T>` is `EntityDatabase<T>` though you might want to implement your own and register that in your IoC container.

Of course, the example queries are a little inefficient - we could query Joey and Monica in a single statement - but it shows how `Silk.Data.SQL.ORM` is designed to allow you to batch SQL statements together - that is, perform all the above SQL in a single round-trip to the SQL server.

### Executing queries

Queries remain in-memory expression trees until `Execute` or `ExecuteAsync` are invoked. Each requires an `IDataProvider` instance to query against - above we use an SQLite3 database provider.

Different providers have slightly different SQL syntaxes so no SQL is actually written until `Execute` or `ExecuteAsync` is invoked.

### Batching

Batching writes all your SQL statements into a single SQL statement, reducing the network latency cost of going to your SQL server in many round trips.

Just add an `AsBatch()` call to your query to create a batched query.

**Beware: any data created on the SQL server, like auto incremented integer IDs, aren't available in a batched query so followup queries can fail if a generated field value is required.**

### Transactions

Just add an `AsTransaction()` call to your query to create a transaction. Batched queries can also be executed as a transaction!

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

	var userDatabase = new EntityDatabase<UserAccount>(dataDomain);  // use full entity type
	var userDob = (await userDatabase
		.SelectSingle<UserDobView>(userAccount => userAccount.Id == userId)
		.ExecuteAsync(sqlProvider))
		.ProfileDateOfBirth;

Here the property `ProfileDateOfBirth` is flattened from `UserAccount`.**`Profile`** and `UserProfile`.**`DateOfBirth`**.

Using projection will also permit you to INSERT, UPDATE or DELETE incomplete entities so long as the primary key field(s) are present on the projected type.

#### Other projections

It's possible to project without mapping from a type, any `Silk.Data.Modelling` `Model` can be used to create a projection for use in queries - however you will need to map your query result to something.

### SQL functions

Unlike EntityFramework, `Silk.Data.SQL.ORM` has no support for translating native C# methods into SQL. This is because it's desired that it be *very* explicit which SQL functions are supported and writing out a list of supported and not-supported extension and LINQ methods would be a hard API surface to program against.

To solve this issue `Silk.Data.SQL.ORM` provides the static `SQLFunctions` class that can build SQL function calls in your query expressions for you:

	var username = "JoEy.TrIBianI";
	var query = userDatabase.Select(userAccount => SQLFunctions.IsIn(
		SQLFunctions.ToLower(userAccount.Username),
		SQLFunctions.ToLower("MONicA.GeLLeR"), SQLFunctions.ToLower(username)
		));

As you can see these functions work on variables, literals, entity properties and even the results of other SQL function calls.

## DataDomain in-depth

### Conventions

When a `DataDomain` is built (`DataDomainDefinition`.`Build`) entity types are modeled into a schema, complete with primary keys, foreign key constraints, data types - the works.

A collection of conventions are used to control how the schema is designed - you can override any schema design behavior by providing your own conventions to `DataDomainDefinition`.`Build`.

A separate set of conventions are used to create projections and can also be provided to `DataDomainDefinition`.`Build`.

### Schema opinions

`DataDomainDefinition`.`AddEntityType<T>` returns an `EntityTypeDefinition` instance which can be used to configure how you want your entity type stored. These opinions can be used to add to or override what a convention might decide to do to store your entity type.

	var entityTypeDefinition = dataDomainDefinition.AddEntityType<UserAccount>();
	//  set a primary key
	entityTypeDefinition.For(userAccount => userAccount.AccountId).IsPrimaryKey();
	//  add a custom index
	entityTypeDefinition.For(userAccount => userAccount.Username).Index();
	//  require email address to be unique
	entityTypeDefinition.For(userAccount => userAccount.EmailAddress).Index(requireUnique: true);
	//  set a maximum length for the user name
	entityTypeDefinition.For(userAccount => userAccount.Username).Length(100);

### Examining the schema

`DataDomain`.`Schema` is an instance of `DomainSchema` that exposes the details of the database schema that has been designed for the SQL server.

You can get the schema designed for a specific entity type using `DomainSchema`.`GetEntitySchema<T>`.

## Components

Components provided to make building your own ORM features a bit easier:

* Query builders - SELECT, INSERT, UPDATE, DELETE query builders write `ORMQuery` expression trees
* LINQ expression converter - converts LINQ expressions for WHERE, HAVING etc. clauses into `QueryExpression` instances
* QueryCollection types - the underlying API used by `EntityDatabase<T>` to contain and execute a collection of queries