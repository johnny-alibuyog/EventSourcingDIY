open EventStore
open Domain
open DomainTest
open Printer


runTests () |> ignore

let store = EventStore.initialize()

// store.append [ Flavour_restocked (Vanilla, 3) ]
// store.append [ Flavour_sold (Vanilla, 2) ]
// store.append [ Flavour_sold (Vanilla, 3) ]
// store.append [ Flavour_sold (Strawberry, 4) ]
// store.append [ Flavour_sold (Vanilla, 4); Flavour_went_out_of_stock Vanilla ]

store.evolve (restockFlavour (Vanilla, 3))
store.evolve (restockFlavour (Strawberry, 1))
store.evolve (sellFlavour (Vanilla, 2))
store.evolve (sellFlavour (Strawberry, 2))

let events = store.get()
let sold = events |> project soldFlavours

printEvents events
printSoldFlavours [Vanilla; Strawberry] sold