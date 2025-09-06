module Domain.IceCreamTest

open Domain.IceCream
open Expecto
open Expecto.Expect

let Given events = events
let When eventProducer events = eventProducer events
let Then expected message actual = equal actual expected message


let sellFlavourTests =
    testList "sellFlavour" [

        testCase "Flavour_sold" (fun _ ->
            Given [ Flavour_restocked (Vanilla, 3) ]
            |> When (Shop.sellFlavour (Vanilla, 2))
            |> Then [ Flavour_sold (Vanilla, 2) ] "Expected events do not match actual events"
        )
        
        testCase "Flavour_went_out_of_stock" (fun _ ->
            Given [ Flavour_restocked (Vanilla, 3) ]
            |> When (Shop.sellFlavour (Vanilla, 3))
            |> Then [ 
                Flavour_sold (Vanilla, 3)
                Flavour_went_out_of_stock Vanilla
            ] "Expected events do not match actual events"
        )
        
        testCase "Flavour_was_not_in_stock" (fun _ ->
            Given [ ]
            |> When (Shop.sellFlavour (Strawberry, 1))
            |> Then [ Flavour_was_not_in_stock Strawberry ] "Expected events do not match actual events"
        )
    ]

let getQuantityTests =
    testList "getQuantity" [
        testCase "returns 0 for unknown flavour" (fun _ ->
            Given Map.empty
            |> When (Shop.getQuantity Vanilla)
            |> Then 0 "Expected quantity does not match actual quantity"
        )

        testCase "returns correct quantity for known flavour" (fun _ ->
            Given Map.ofList [ Vanilla, 5 ]
            |> When (Shop.getQuantity Vanilla)
            |> Then 5 "Expected quantity does not match actual quantity"
        )
    ]

let runTests () =
    [
        sellFlavourTests
        getQuantityTests
    ]
    |> List.iter (fun tests ->
        tests
        |> runTestsWithCLIArgs [] [||]
        |> ignore
    )
