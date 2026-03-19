# SKILL: scaffold-netcore-microservice

## Purpose
Generate a new .NET Core microservice with proper structure, including API controllers, domain models, repositories, and tests.

## Use When
- Creating a new microservice (User, Post, Notification, etc.)
- Setting up service boilerplate
- Ensuring consistent service structure

## Required Inputs
- Service name (e.g., User, Post, Notification)
- Primary domain entities
- Key business operations

## Expected Output
- Multi-project .NET solution
- API project with controllers
- Domain and application layers
- Infrastructure with repository pattern
- Unit and integration test projects
- Docker file configured

## Execution Approach

### Step 1: Create Solution and Projects
```bash
# Create solution
dotnet new globaljson --sdk-version 6.0.0 --roll-forward latestFeature
dotnet new sln -n GoingMy.{ServiceName}Service

# Create projects
dotnet new classlib -n GoingMy.{ServiceName}.Domain
dotnet new classlib -n GoingMy.{ServiceName}.Application
dotnet new classlib -n GoingMy.{ServiceName}.Infrastructure
dotnet new webapi -n GoingMy.{ServiceName}.API
dotnet new xunit -n GoingMy.{ServiceName}.Tests
dotnet new xunit -n GoingMy.{ServiceName}.IntegrationTests

# Add projects to solution
dotnet sln GoingMy.{ServiceName}Service.sln add src/**/*.csproj tests/**/*.csproj
```

### Step 2: Establish Project Structure
```
GoingMy.{ServiceName}Service/
├── src/
│   ├── GoingMy.{ServiceName}.API/
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   ├── GoingMy.{ServiceName}.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Interfaces/
│   │   └── Exceptions/
│   ├── GoingMy.{ServiceName}.Application/
│   │   ├── Services/
│   │   ├── DTOs/
│   │   └── Validators/
│   └── GoingMy.{ServiceName}.Infrastructure/
│       ├── Data/
│       │   ├── DbContext.cs
│       │   ├── Repositories/
│       │   └── Migrations/
│       └── ExternalServices/
├── tests/
│   ├── GoingMy.{ServiceName}.Tests/
│   └── GoingMy.{ServiceName}.IntegrationTests/
└── docker-compose.yml
```

### Step 3: Configure Dependencies
Typical NuGet packages:
- `Microsoft.EntityFrameworkCore.PostgreSQL`
- `Serilog`
- `FluentValidation`
- `MediatR`
- `Moq`, `xunit` for testing

### Step 4: Set Up Core Files

**Program.cs Template:**
```csharp
builder.Services.AddControllers();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<I{ServiceName}Service, {ServiceName}Service>();
builder.Services.AddDbContext<{ServiceName}DbContext>();
builder.Logging.AddSerilog();

var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Run();
```

**Entity Template:**
```csharp
public class {Entity} : BaseEntity {
    public string Name { get; set; }
    // Add domain properties
    
    public static {Entity} Create(string name) {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name required");
        return new {Entity} { Name = name };
    }
}
```

**Controller Template:**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class {Entity}Controller : ControllerBase {
    private readonly I{ServiceName}Service _service;
    
    public {Entity}Controller(I{ServiceName}Service service) {
        _service = service;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) {
        var result = await _service.GetById{Entity}Async(id);
        return Ok(result);
    }
}
```

### Step 5: Configure Database Context
```csharp
public class {ServiceName}DbContext : DbContext {
    public DbSet<{Entity}> {Entities} { get; set; }
    
    using (var client = new HttpClient()) {
        var response = await client.GetAsync("http://health-check/db");
    }
}
```

### Step 6: Add Tests
```csharp
public class {ServiceName}ServiceTests {
    [Fact]
    public async Task Create{Entity}_WithValidInput_ReturnsSuccess() {
        // Arrange
        var service = new {ServiceName}Service();
        
        // Act
        var result = await service.Create(new CreateRequest());
        
        // Assert
        Assert.NotNull(result);
    }
}
```

## Quality Criteria
- Solution builds without errors
- All dependencies properly configured
- Database context properly set up
- Controllers follow REST conventions
- Base test infrastructure in place
- Dockerfile configured correctly

## Verification Checklist
- [ ] Solution builds: `dotnet build`
- [ ] Tests run: `dotnet test`
- [ ] Database migrations generated
- [ ] Swagger/OpenAPI configured
- [ ] Logging configured
- [ ] Entity relationships defined
- [ ] Service interfaces defined

## Edge Cases
- Handling circular dependencies between services
- Database migration strategy for multiple services
- Service-to-service authentication

## References
- Microsoft .NET Architecture Guide
- Clean Architecture by Robert C. Martin

## Changelog
- v1.0: Basic microservice scaffolding template
