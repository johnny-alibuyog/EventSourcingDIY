open EventStore
open Projection
open Domain
open Printer

let store = EventStore.initialize()

store.append [ Flavour_restocked (Vanilla, 3) ]
store.append [ Flavour_sold (Vanilla, 2) ]
store.append [ Flavour_sold (Vanilla, 3) ]
store.append [ Flavour_sold (Strawberry, 4) ]
store.append [ Flavour_sold (Vanilla, 4); Flavour_went_out_of_stock Vanilla ]

let events = store.get()

let sold = events |> project soldFlavours

printEvents events
printSoldFlavours sold