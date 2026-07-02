# Support Ticket System AI Rules

## Project

Customer Support Ticket Management System

## Technology Stack

- .NET 9
- ASP.NET Core Web API
- SQL Server
- Entity Framework Core
- ASP.NET Identity
- JWT
- Refresh Token
- Onion Architecture
- Generic Repository
- Unit Of Work
- AutoMapper
- FluentValidation
- Swagger

---

# Architecture

Core
    Domain
    Application

Infrastructure
    Persistence
    Infrastructure

Presentation
    API

---

# Coding Rules

- Follow SOLID principles.
- Use Clean Architecture.
- Use Onion Architecture.
- Use async/await everywhere.
- Controllers must not contain business logic.
- Services contain business logic.
- Repositories only access the database.
- Use Fluent API instead of Data Annotations.
- Use Dependency Injection.
- Keep code production-ready.

---

# Roles

Customer

SupportAgent

Admin

---

# Entities

ApplicationUser

SupportTicket

TicketReply

RefreshToken

---

# Ticket Status

Open

InProgress

Resolved

Closed

---

# Relationships

Customer -> Many Tickets

SupportAgent -> Many Tickets

SupportTicket -> Many Replies

ApplicationUser -> Many Replies

ApplicationUser -> Many RefreshTokens

---

# Authentication

Identity

JWT

Refresh Token

Role Based Authorization

---

# Important

Never generate the whole project at once.

Implement only the feature requested.

After each feature:

- Review code.
- Fix compile issues.
- Stop and wait.