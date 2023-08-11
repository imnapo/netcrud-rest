# netcrud-rest
A framework for building REST APIs using .NET Core and Entity Framework Core.
The goal of this library is to offer features such as sorting, filtering and pagination. You just need to focus on defining the resources and implementing your custom business logic. This library has been designed around dependency injection, making extensibility incredibly easy.
it supports for the Controller, Service and Repository layers, applying the UnitOfWork principles

## Features
- **Filtering**: perform advanced filtering using the `filter` query string parameter.
- **Sorting**: order resources on one or multiple attributes using the `sort` query string parameter.
- **Pagination**: Leverage the benefits of paginated resources using `pageNumber`, `pageSize` and `paged` query string parameters
- **field selection**: Get only the data you need using `field` query string parameter.
- **Relationship inclusion**: load related resources using the `include` query string parameter.

## Installation
### .NET CLI
```
dotnet add package NetCrud.Rest
dotnet add package NetCrud.Rest.EntityFramework
```
### Package Manager
```
Install-Package NetCrud.Rest
Install-Package NetCrud.Rest.EntityFramework
```
## Define Entities
Define your domain models and inherit from EntityBase<TId> where `TId` is the type of the primary key.
```
public class User : EntityBase<int>
{
        
    public string Name { get; set; }

    public string Email { get; set; }

    public int Age { get; set; }

}
```
alternatively, you may inherit from `EntityBase` which is same as `EntityBase<int>`.
You may also create your own base entity like below and inherit from it.
```
public class CrudEntity<TId> : EntityBase<TId>
{
    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
}

public class User : CrudEntity<int>
{
    ...
}
```
### Entity Attributes
#### **`JsonIgnore` Attribute**: 
By default, all properties of your models are publicly available. In order to prevent it for a specific property, you may use `JsonIgnore` attribute of the `Newtonsoft.json` Package. 
```
public class User : EntityBase<int>
{     
    public string Name { get; set; }

    [JsonIgnore]
    public string Email { get; set; }

}
```
#### **`EagerLoad` Attribute**: 
In order to force a related resource to load always with your domain model, use `EagerLoad` attribute on a navigation property.
```
public class User : EntityBase<int>
{
    ...

    [EagerLoad]
    public Address Address { get; set; }
}
```

#### **`NotMapped` Attribute**: 
As you already know, The NotMapped attribute can be applied to properties of an entity class for which we do not want to create corresponding columns in the database.
```
public class User : EntityBase<int>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    [NotMapped]
    public string FullName { get { return FirstName + " " + LastName; } }
}
```

#### **`LoadRelation` Attribute**: 
The LoadRelation attribute can be applied to calculated properties of an entity class that need some related entities to be eager loaded. 

```
public class User : EntityBase<int>
{
    ...

    public ICollection<UserGame> UserGames { get; set; }

    [LoadRelation("UserGames")]
    public int? NumberOFGames { get { return UserGames?.Count; } }
}
```
Please be aware that:
- this property is **not** loaded always like a property with the `EagorLoad` attribute. It will be only populated when we specify the name of property in the `include` query string parameter.
- Properties with `LoadRelation` attribute are by default like Properties with `NotMapped` Attribute.

#### **`AtomicAny` Attribute**: 
There are some cases that we just want to know are domain models have any related entity of specific type. For example, in our sample we want to know if users have any games related to them. the first approach is to just include games in our results and count them, but sometimes it is not good practice as we may have lots of games related to users and all we need is ***users has related games or not***. In this case we may use the `AtomicAny(nameOfProperty)` attribute.
```
public class User : EntityBase<int>
{
   public ICollection<UserGame> UserGames { get; set; }

    [AtomicAny("UserGames")]
    public bool HasGames { get; set; } }
}
```

## Controller
In order to use all capabilities of this package you have to inherit your controllers from `CrudControllerBase<TEntity>` base class.
```
[Route("api/[controller]")]
public class UsersController : CrudControllerBase<User>
{
    public UsersController(IEntityService<User, int> service, IDataShaper<User> dataShaper) : base(service, dataShaper)
    {
    }
}
```
This controller will generate the necessary action methods to perform general CRUD operation for you. There are also other implementations of this base class like `CrudControllerBase<TEntity, TId, TParams>` that you may use for more advanced and customized scenarios.

## Service Layer
In order to implement the base controller, you need an implementation of `IEntityService<TEntity, TId>` interface. You may use pre-defined `EntityService<TEntity, TId>` implementation by registering the service and adding it to program.cs :
```
builder.Services.AddScoped<IEntityService<User, int>, EntityService<User, int>>();
```

for more advanced scenarios, you may create your own service layer like below
```
public class UserService : EntityService<User, int>
{
    public UserService(IRepository<User> repository, IUnitOfWork unitOfWork)
    : base(repository, unitOfWork)
    {
    }

    public override Task<object> Before(IList<User> entities, ServiceActionType actionType)
    {
        foreach (var entity in entities)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;

        }
        return base.Before(entities, actionType);
    }
}
```
```
builder.Services.AddScoped<IEntityService<User, int>, UserService>();
```

## Repository Layer
Our pre-defined `EntityService` class needs `IRepository<IEntity>` implementation of the repository layer. likewise service layer, we already have a predefined one `Repository<TEntity, TContext>` that you may use it by registering and adding it to program.cs :

```
builder.Services.AddScoped<IRepository<User>, Repository<User, CrubDbContext>>();
```

## Unit Of Work
As I mentioned earlier, We use UOW principle (`IUnitOfWork` interface) in this package. The easiest way to implement it is to just use `UnitOfWork<IDbContext>` class and register it in program.cs file.
```
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<CrubDbContext>>();
```

## Data Shaper Layer
The last required layer for using this pacakge is implementing the `IDataShaper<TEntity>` interface. It enables us to select (shape) the data by choosing the fields through the `field` query string parameter. Like all other layers there is a pre-defined implementation `DataShaper<T>`. You just need to add it to program.cs:
```
builder.Services.AddScoped<IDataShaper<User>, DataShaper<User>>()
```


