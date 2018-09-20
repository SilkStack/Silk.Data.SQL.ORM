# Silk.Data.SQL.ORM

## Goals

* Strongly typed SQL queries
* Extensibility
* Obvious query generation - no guessing at what queries are being executed, or when (no lazy-loading)
* Minimize round-trips to the database server
* Run against any database provider
* Capability for individual, property expression queries (`SET field = expression/value` instead of a full entity update)
* Middle-ground between EF/nHibernate and micro-ORMs Dapper/massive


## Summary

`Silk.Data.SQL.ORM` is a library intended for working with data in any SQL database. The API design is different from most other ORMs, most notably that developers create queries and execute them separately - one liners are more complicated than you might be used to _but_ offer more flexaility and capability.

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

### Basic Use

First, we need to define a `Schema`. This holds all the information about our entity types.

	var schemaBuilder = new SchemaBuilder();
	schemaBuilder.DefineEntityType<UserAccount>();
	var schema = schemaBuilder.Build();

Next we're going to need a data provider. A data provider is our connection to the database we want to talk to and will be responsible for executing all of our queries.

    var sqlProvider = new SQLite3Provider("friends.db");
    
Now we have defined our entity types and a connection to the database we can execute SQL queries.

	var joey = default(UserAccount);
	var monica = default(UserAccount);
	await sqlProvider.ExecuteAsync(
	  schema
    	  .CreateSelect<UserAccount>(query => query.AndWhere(user => user.FullName == "Monica Geller"))
    	  .WithFirstResult(r => monica = r);
	  schema
    	  .CreateSelect<UserAccount>(query => query.AndWhere(user => user.FullName == "Joey Tribiani"))
    	  .WithFirstResult(r => joey = r);
	);
	
These queries are built using the extension APIs hanging off `Schema`. We can write our own extension methods that express intent and hide complexity.

### Batching

`Execute` and `ExecuteAsync` on `IDataProvider` are responsible for executing most of your SQL queries. They accept any number of `Query` objects to run a collection of queries in a single round-trip to the database.

The `CreateX` APIs hanging off `Schema` produce `Query` instances that contain all the required information for executing a query and processing the results.

**It's recommended that developers produce their own `Query` APIs and implementations using the QueryBuilder APIs available.**

### Query Builders

For the major query types (SELECT, UPDATE, DELETE, INSERT) there are corresponding query builder types. These offer a strongly-typed API for crafting SQL queries, sub-queries and mapping results.

Let's say we want to log a user into a system. We could use the `CreateSelect` API to first query the user's password hash, validate the user's entered password and then query again for the user fields we want. Or, we could reduce all that to a single query and request the password hash and the fields we want in a single query:

    var queryBuilder = new SelectBuilder<UserAccount>(schema);
    queryBuilder.AndWhere(account => account.EmailAddress == userId || account.Username == userId);
    var passwordHashReader = queryBuilder.Project(account => account.PasswordHash);
    var viewMapper = queryBuilder.Project<TView>();
    
    using (var queryResult = await dataProvider.ExecuteReaderAsync(queryBuilder.BuildQuery()))
    {
      while (await queryResult.ReadAsync())
      {
        var passwordHash = passwordHashReader.Read(queryResult);
        var viewOfUserAccount = viewMapper.Map(queryResult);
      }
    }

### Expression Support

A lot of the methods on the query builder APIs accept expressions as parameters. This allows developers to work with their entity types in queries and supports some extra features like sub-queries and translating method calls:

    var subQueryBuilder = new SelectBuilder<UserAccount>(schema);
    subQueryBuilder.Project(account => account.Id);
    subQueryBuilder.AndWhere(account => account.IsActive);
    
    var selectBuilder = new SelectBuilder<UserAccount>(schema);
    selectBuilder.Project<TView>();
    selectBuilder.AndWhere(account => DatabaseFunctions.IsIn(account.Id, subQueryBuilder));
    selectBuilder.OrderBy(account => account.DisplayName);
    
In this (admittedly convoluted) example we create a sub-query that selects the Id of all active users and passes that to an C# function that is translated to an IN SQL expression.

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

	var dateOfBirth = default(DateTime);
	await provider.ExecuteAsync(
	  schema.CreateSelect<UserAccount, UserDobView>()
    	  .WithFirstResult(r => dateOfBirth = r.ProfileDateOfBirth)
    );

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

Developers can customize storage of entity properties/fields. The `DefineEntity<T>` API accepts a callback method to customize how a type is modelled in the database:

    schemaBuilder.DefineEntity<UserAccount>(entityBuilder => {
      entityBuilder.For(type => type.Id).ColumnName = "AccountId";
    });
    
### Relationships and embedded objects

When your entity type declares a property that is a complex type, that is, a nullable type that isn't `string` (ie. a class), `Silk.Data.SQL.ORM` will do one of two things:

* If the declared property type is defined as an entity type in the schema a relationship is created and accessing members of the property type will be expressed with a JOIN clause in your queries.
* If the declared property type is not defined as an entity type in the schema all properties of the property type will be embedded into the same table as the parent entity type.

`Silk.Data.SQL.ORM` also supports many-to-many relationships but these are declared on the `Schema` rather than modelled on your entity types.

    schemaBuilder.DefineRelationship<UserAccount, Role>("UserRoles");
 
 There are similar extension methods and query builders for theses relationships; the result types of queries are tuples of TLeft and TRight.