module Printer
open Domain

let printEvents events =
    events
    |> List.length
    |> printfn "History (Length: %i)"

    events
    |> List.iteri (printfn "%d: %A")

let printSoldFlavours flavours state =
    flavours
    |> List.iter (fun flavour ->
        let quantity =
            state
            |> getQuantity flavour

        printfn "Sold %A: %i" flavour quantity
    )
