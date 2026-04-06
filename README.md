# LateralGroup CMS Sync Service

This is a .NET service that ingests CMS webhook events, stores versioned
content locally, and exposes that data through a secured REST API.

I intentionally kept the solution straightforward. The goal here was to
make the event flow easy to follow, keep the behavior predictable, and
cover the core business rules without over-engineering it.

------------------------------------------------------------------------

## What this does

The service receives CMS events via a webhook and builds a local
representation of content over time.

Supported event types:

-   **publish**
    -   Creates or updates content
    -   Marks it as visible
-   **unpublish**
    -   Keeps the content in storage
    -   Marks it as disabled by the CMS
-   **delete**
    -   Removes the content entirely (hard delete)

### Key behaviors handled

-   Only **published content** is visible to consumers\
-   **Unpublish does not delete** content\
-   **Delete does remove** content\
-   **Stale/out-of-order events are ignored**\
-   **Version history is retained**\
-   **Admins can locally disable content** without affecting CMS state

------------------------------------------------------------------------

## Architecture (high level)

I went with a simple layered approach:

-   **API**
    -   Controllers, auth, and request/response mapping
-   **Application**
    -   Use-case logic (event processing, querying, admin actions)
-   **Domain**
    -   Entities + core state transitions
-   **Infrastructure**
    -   EF Core + persistence

I also split **read/write DbContexts** since the prompt called that out
and it makes query optimization clearer.

This is not full CQRS --- just a clean separation of concerns.

------------------------------------------------------------------------

## Tech stack

-   .NET 9
-   ASP.NET Core
-   EF Core
-   SQLite
-   xUnit

SQLite was chosen to keep setup simple and portable for a take-home.

------------------------------------------------------------------------

## Authentication

Basic Authentication is used.

There are three access boundaries:

-   **CMS webhook user**
    -   Can only call `/cms/events`
-   **Consumer user**
    -   Can read content
-   **Admin user**
    -   Can read all content + perform admin actions

This is enforced via policies (not route structure).

Credentials are currently in `appsettings.json` for simplicity.

------------------------------------------------------------------------

## API Endpoints

### CMS Webhook

    POST /cms/events

Accepts batched events.

Example:

``` json
[
  {
    "type": "publish",
    "id": "X",
    "payload": { "title": "Hello" },
    "version": 1,
    "timestamp": "2026-01-01T00:00:00Z"
  }
]
```

------------------------------------------------------------------------

### Content API

    GET /api/content-items
    GET /api/content-items/{id}

#### Consumer visibility rules

Content must be: - published - not disabled by CMS - not disabled by
admin

#### Admin behavior

Admins can see all persisted content (except deleted).

------------------------------------------------------------------------

### Admin actions

    POST /api/content-items/{id}/disable
    POST /api/content-items/{id}/enable

This sets a **local override flag** (`IsDisabledByAdmin`).

Important: - This does NOT affect the CMS - This does NOT change
versions - It only affects API visibility

------------------------------------------------------------------------

## Running locally

### Prerequisites

-   .NET SDK

### Run the API

``` bash
dotnet run --project LateralGroup.API
```

------------------------------------------------------------------------

## Database

SQLite is used for persistence.

-   DB is created/migrated on startup
-   Stored locally for easy setup

------------------------------------------------------------------------

## API docs

Available in development:

    /docs

------------------------------------------------------------------------

## Running tests

Run all tests:

``` bash
dotnet test
```

------------------------------------------------------------------------

## Notes on interpretation

The prompt mentions **Add / Update / Delete**, but the actual payloads
use:

-   `publish`
-   `unpublish`
-   `delete`

I interpreted this as:

-   `publish` = materialized version (new or updated)
-   `unpublish` = disable but keep data
-   `delete` = remove completely

This seemed to best align the rules with the sample payloads.

------------------------------------------------------------------------

## What's covered in tests

-   Webhook authentication
-   Consumer vs admin authorization
-   Publish / unpublish / delete behavior
-   Stale event handling
-   Validation failures
-   Query filtering rules
-   Admin enable/disable behavior
-   Application service logic

------------------------------------------------------------------------

## What I would improve next

If this were going further:

-   Move auth secrets out of config
-   Expand structured logging (especially around ingestion)
-   Add pagination/filtering to content endpoints
-   Add more detailed audit tracking
-   Clean up minor test warnings

------------------------------------------------------------------------

## Final note

I focused on keeping the solution **clear and maintainable** over adding
extra layers or patterns that weren't necessary for the scope (e.g.,
full CQRS, MediatR, etc.).
