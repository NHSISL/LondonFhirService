# The Standard — Exposers — Rules

## Thin Controller

**ts-exposers-001** [ERROR] Controllers must be thin — all business logic must be delegated to a single injected service.
**ts-exposers-002** [ERROR] Controllers must map each service exception type to the correct HTTP status code.

## HTTP Verbs and Status Codes

**ts-exposers-003** [ERROR] POST endpoints must return 201 Created with the created resource in the body.
**ts-exposers-004** [ERROR] GET endpoints must return 200 OK.
**ts-exposers-005** [ERROR] PUT endpoints must return 200 OK with the updated resource.
**ts-exposers-006** [ERROR] DELETE endpoints must return 200 OK with the deleted resource.

## Attributes and Base Class

**ts-exposers-007** [ERROR] Controllers must be decorated with `[ApiController]` and `[Route("api/[controller]")]`.
**ts-exposers-008** [ERROR] Controllers must inherit from `RESTFulController` or `ControllerBase`.
