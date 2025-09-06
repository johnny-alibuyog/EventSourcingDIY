module Printer
open Domain.IceCream
open EventStore

let printEvents shop events =
    printfn "==========================================="
    printfn "Events for shop: %s" (Aggregate.value shop)
    events
    |> List.length
    |> printfn "History (Length: %i)"

    events
    |> List.iteri (printfn "%d: %A")

let printFlavourState projectionDescription shop flavours state =
    printfn "==========================================="
    printfn "%s %s flavours" projectionDescription (Aggregate.value shop)

    flavours
    |> List.iter (fun flavour ->
        let quantity =
            state
            |> Shop.getQuantity flavour

        printfn "%s %A: %i" projectionDescription flavour quantity
    )
