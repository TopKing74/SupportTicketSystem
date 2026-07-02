==============================================================
 Support Ticket Management System
==============================================================

Project Name : Support Ticket Management System
Architecture : 7-Layer Onion Architecture
               (Domain, Application, Persistence, Infrastructure,
                API + cross-cutting services)
Controllers  : Three main controller groups -> Admin, Auth, Tickets

--------------------------------------------------------------
 1. SOLUTION STRUCTURE
--------------------------------------------------------------
Core/
  - SupportTicketSystem.Domain          : Entities, Enums (no dependencies)
  - SupportTicketSystem.Application     : DTOs, Interfaces, Services, Mapping
Infrastructure/
  - SupportTicketSystem.Persistence     : DbContext, Fluent API configs, Repositories, UnitOfWork, Migrations
  - SupportTicketSystem.Infrastructure  : Cross-cutting services (JwtTokenGenerator, CurrentUserService)
Presentation/
  - SupportTicketSystem.API             : Web API host, Controllers, DI, Middleware

--------------------------------------------------------------
 2. PREREQUISITES
--------------------------------------------------------------
- .NET 9 SDK
- SQL Server (LocalDB is sufficient for development)
- EF Core CLI tools (optional, for manual migrations):
    dotnet tool install --global dotnet-ef --version 9.0.0

--------------------------------------------------------------
 3. SETUP & RUN INSTRUCTIONS
--------------------------------------------------------------
A) Configure the database connection string
   Open: Presentation/SupportTicketSystem.API/appsettings.json
   Update "ConnectionStrings:DefaultConnection" to point to your
   SQL Server / LocalDB instance.

B) Apply migrations & create the database
   From the solution root, run ONE of the following:

   Option 1 (apply directly via EF CLI):
       dotnet ef database update `
         --project Infrastructure/SupportTicketSystem.Persistence `
         --startup-project Presentation/SupportTicketSystem.API

   Option 2 (let the app apply pending migrations on startup):
       A) Add to Program.cs (before app.Run()):
              await app.Services.CreateScope()
                  .ServiceProvider.GetRequiredService<ApplicationDbContext>()
                  .Database.MigrateAsync();
       B) Then just run the API (step C).

C) Start the Web API
       dotnet run --project Presentation/SupportTicketSystem.API

   The API will launch on the following CURRENT ACTIVE PORTS
   (defined in Presentation/SupportTicketSystem.API/Properties/launchSettings.json):
       HTTPS:  https://localhost:7055
       HTTP:   http://localhost:5055

D) Explore the API
   - Swagger UI (Development environment):
       https://localhost:7055/swagger/index.html
   - Click "Authorize", enter: Bearer <your-jwt-token>
   - Obtain a token via the Auth/login or Auth/register endpoint,
     or use the seeded Admin credentials below.

--------------------------------------------------------------
  3.1 DEFAULT SEEDED ADMIN CREDENTIALS
--------------------------------------------------------------
On first run (migrations applied), the system seeds a default Admin
user (see Program.cs seeding logic). Use these to log in immediately:

   Email    : admin@support.com
   Password : AdminPassword123!
   Role     : Admin

--------------------------------------------------------------
 4. AUTHENTICATION FLOW
--------------------------------------------------------------
- POST /api/auth/register   -> creates user, assigns "Customer" role,
                               returns JWT + RefreshToken.
- POST /api/auth/login      -> validates credentials, returns
                               JWT + RefreshToken.
- POST /api/auth/refresh-token -> rotates refresh token, returns new pair.
- POST /api/auth/logout     -> revokes refresh tokens.

JWT is validated via JwtBearer with Issuer, Audience, and
SymmetricSecurityKey from configuration ("Jwt:Key").

Roles used by the system: Admin, Agent, Customer.

--------------------------------------------------------------
 5. MAIN API ENDPOINTS
--------------------------------------------------------------

[ AUTH ]   (public)
  POST   /api/auth/register           -> Register a new customer
  POST   /api/auth/login              -> Login & receive tokens
  POST   /api/auth/refresh-token      -> Rotate refresh token
  POST   /api/auth/logout             -> Revoke refresh tokens

[ TICKETS ]   (requires authentication)
  POST   /api/tickets                  -> Create a support ticket
  GET    /api/tickets/my-tickets       -> Tickets created by / assigned to user
  GET    /api/tickets/assigned          -> Tickets assigned to the agent
  GET    /api/tickets/search            -> Search/filter tickets by:
                                          Title, Customer, Status,
                                          Date (FromDate / ToDate),
                                          AssignedAgent
  GET    /api/tickets/{id}             -> Get ticket details by ID
  PUT    /api/tickets/{id}/status      -> Update status (state-machine validated)
  POST   /api/tickets/{id}/reply       -> Add a reply to a ticket
  GET    /api/tickets/{id}/replies     -> List replies for a ticket

  Search query parameters (all optional, combinable):
     ?title=<partial title>
     &customer=<partial customer name OR customer id>
     &status=<Open|InProgress|Resolved|Closed>
     &fromDate=<ISO-8601>  &toDate=<ISO-8601>   (CreatedAt range)
     &assignedAgent=<partial agent name OR agent id>
     Example: /api/tickets/search?status=Open&customer=john&fromDate=2025-01-01

  Ticket Status state machine:
    Open       -> InProgress | Closed
    InProgress -> Resolved   | Closed
    Resolved   -> Closed
    Closed     -> (terminal)

[ ADMIN ]   (requires Admin role)
  PUT    /api/admin/tickets/{id}/assign-agent -> Assign an agent to a ticket
  GET    /api/admin/dashboard                  -> Status counters (Open/InProgress/Resolved/Closed/Total)

--------------------------------------------------------------
 6. NOTES
--------------------------------------------------------------
- Default JWT signing key is for local development ONLY.
  Replace "Jwt:Key" with a strong secret in production.
- Refresh tokens are persisted in the RefreshTokens table and
  rotated on every refresh.
- All data access goes through the Generic Repository + Unit of Work
  pattern (no direct DbContext usage in services/controllers).
==============================================================
