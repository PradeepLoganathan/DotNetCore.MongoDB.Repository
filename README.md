# DotNetCore.MongoDB.Repository

Generic repository implementation for MongoDB on .NET Core Applications. The original code is from: https://github.com/esendir/MongoRepository

In addition to the original package, this package introduces TrackedEntity and TrackedDocumentRepository types. TrackedEntity is a generic type that has parameters (TrackedProperty) which holds their own history. To have a better understanding please see the tests.

## Installation

DotNetCore.MongoDB.Repository can be installed using the NuGet command line or the NuGet Package Manager


**Install using the command line:**
```bash
Install-Package DotNetCore.MongoDB.Repository -Pre
```

### Definition

#### Model
You don't need to create a model, but if you are doing so you need to extend Entity
```csharp
	//if you are able to define your model
	public class User : Entity
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}
```

#### Repository
There are multiple base constructors, read summaries of others
```csharp
	public class UserRepository : Repository<User>
	{
		public UserRepository(string connectionString) : base(connectionString) {}

		//custom method
		public User FindbyUsername(string username)
		{
			return First(i => i.Username == username);
		}
		
		//custom method2
		public void UpdatePassword(User item, string newPassword)
		{
			repo.Update(item, i => i.Password, newPassword);
		}
	}
```

*If you want to create a repository for already defined non-entity model*
```csharp
	public class UserRepository : Repository<Entity<User>>
	{
		public UserRepository(string connectionString) : base(connectionString) {}

		//custom method
		public User FindbyUsername(string username)
		{
			return First(i => i.Content.Username == username);
		}
	}
```

### Usage

Each method has multiple overloads, read method summary for additional parameters

```csharp
	UserRepository repo = new UserRepository("mongodb://localhost/sample")

	//Get
	User user = repo.Get("58a18d16bc1e253bb80a67c9");

	//Insert
	User item = new User(){
		Username = "username",
		Password = "password"
	};
	repo.Insert(item);

	//Update
	//single property
	repo.Update(item, i => i.Username, "newUsername");

	//multiple property
	//Updater has many methods like Inc, Push, CurrentDate, etc.
	var update1 = Updater.Set(i => i.Username, "oldUsername");
	var update2 = Updater.Set(i => i.Password, "newPassword");
	repo.Update(item, update1, update2);

	//all entity
	item.Username = "someUsername";
	repo.Replace(item);

	//Delete
	repo.Delete(item);

	//Queries - all queries has filter, order and paging features
	var first = repo.First();
	var last = repo.Last();
	var search = repo.Find(i => i.Username == "username");
	var allItems = repo.FindAll();

	//Utils
	var count = repo.Count();
	var any = repo.Any(i => i.Username.Contains("user"));
```
