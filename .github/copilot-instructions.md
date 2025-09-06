# Copilot instructions for EventSourcingDIY

Small, idiomatic F# event-sourcing sample with an in-memory, aggregate-aware event store and Expecto tests that run on app start.

## Big picture
- Event store (`EventStore.fs`):
  - `Aggregate = string`
  - `EventProducer<'Event> = 'Event list -> 'Event list` (pure decision from history)
  - `Projection<'State,'Event>` and `project` to fold events into read models
  - `EventStore<'Event>` via `MailboxProcessor` exposes:
    - `get(): Map<Aggregate,'Event list>`; `getAggregateEvents agg`
    - `append agg events`; `evolve agg producer`
- Domain (`Domain/IceCream.fs`, module `Shop`):
  - Events: `Flavour_restocked | Flavour_sold | Flavour_went_out_of_stock | Flavour_was_not_in_stock` (snake_case)
  - `createIdentity name : Aggregate` (string echo)
  - Projections: `soldFlavours`, `flavoursInStock` -> `Map<Flavour,int>`
  - Decisions (pure producers): `restockFlavour`, `sellFlavour` (read stock via `flavoursInStock`)
- App/tests: `Domain/IceCreamTest.fs` (Expecto) + `Program.fs` (runs tests, then multi-aggregate demo); `Printer.fs` formats output.

## Build, run, test
- Paket used (pinned in `.config/dotnet-tools.json`, storage: none). Target framework: net9.0.
- PowerShell commands:
  - dotnet build
  - dotnet run  # runs tests first, then the console demo
  - dotnet tool restore; dotnet paket restore  # if Paket restore is needed

## Testing pattern (Expecto)
- Tests live in `Domain/IceCreamTest.fs`; `Program.fs` calls `runTests()`.
- Mini BDD DSL in tests:
  - `Given events |> When (Shop.sellFlavour (...)) |> Then [ expected events ]`
- Add new test cases to the `tests` list; keep producers deterministic and side-effect free.

## Domain rules (sell/restock)
- `restockFlavour (f,q)` -> `[Flavour_restocked (f,q)]`.
- `sellFlavour (f,q)` using `flavoursInStock` projection:
  - stock = 0 -> `[Flavour_was_not_in_stock f]`
  - stock <= q -> `[Flavour_sold (f,stock); Flavour_went_out_of_stock f]`
  - else -> `[Flavour_sold (f,q)]`

## Using the event store (per-aggregate)
- `let store = EventStore.init()`; `let shop = Shop.createIdentity "shop-1"`
- Apply decisions: `store.evolve shop (Shop.restockFlavour (Vanilla, 3))`
- Query read model:
  - `let events = store.getAggregateEvents shop`
  - `let sold = events |> EventStore.project Shop.soldFlavours`

Key files: `EventStore.fs`, `Domain/IceCream.fs`, `Domain/IceCreamTest.fs`, `Program.fs`, `Printer.fs`, `paket.*`.
