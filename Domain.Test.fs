module DomainTest

open Domain
open Expecto
open Expecto.Expect

let Given events =
    events

let When eventProducer events =
    eventProducer events

let Then expectedEvents actualEvents =
    equal expectedEvents actualEvents "Expected events do not match actual events"

let tests =
    testList "sellFlavour" [

        testCase "Flavour_sold" <| fun _ ->
            Given [ Flavour_restocked (Vanilla, 3) ]
            |> When (sellFlavour (Vanilla, 2))
            |> Then [ Flavour_sold (Vanilla, 2) ]
        
        testCase "Flavour_went_out_of_stock" <| fun _ ->
            Given [ Flavour_restocked (Vanilla, 3) ]
            |> When (sellFlavour (Vanilla, 3))
            |> Then [ 
                Flavour_sold (Vanilla, 3)
                Flavour_went_out_of_stock Vanilla 
            ]
        
        testCase "Flavour_was_not_in_stock" <| fun _ ->
            Given [ ]
            |> When (sellFlavour (Strawberry, 1))
            |> Then [ Flavour_was_not_in_stock Strawberry ]
    ]

let runTests () =
    tests
    |> runTestsWithCLIArgs [] [||]

