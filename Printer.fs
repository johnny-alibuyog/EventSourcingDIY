module Printer

let printEvents events =
    events
    |> List.length
    |> printfn "History (Length: %i)"

    events
    |> List.iteri (printfn "%d: %A")

let printSoldFlavours state =
    state
    |> Map.iter (printfn "Sold %A: %i")