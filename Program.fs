open System
open Utilities
open Infrastructure.EventStore
open Domain.IceCream

type State =
    | Start
    | StoreSelected of Aggregate
    | FlavourSelected of Aggregate * Flavour
    | QuantitySpecified of Aggregate * Flavour * int
    | PurchaseConfirmed of Aggregate * Flavour * int
    | Exit

[<EntryPoint>]
let main _ =
    let shopOptions = [
        "Shop 1", Some (Shop.createIdentity "shop-1")
        "Shop 2", Some (Shop.createIdentity "shop-2")
        "Exit", None
    ]

    let flavourOptions = [
        "Vanilla", Some Vanilla
        "Strawberry", Some Strawberry
        "Chocolate", Some Chocolate
        "Back", None
    ]

    let quantityOptions = [
        "1", Some 1
        "2", Some 2
        "3", Some 3
        "4", Some 4
        "5", Some 5
        "Back", None
    ]

    let confirmationOptions = [
        "Yes", true
        "No", false
    ]

    let createStore () =
        let store = init()

        let updateStore1 = store.evolve (Shop.createIdentity "shop-1")
        let updateStore2 = store.evolve (Shop.createIdentity "shop-2")

        updateStore1 (Shop.restockFlavour (Vanilla, 3))
        updateStore1 (Shop.restockFlavour (Strawberry, 1))
        updateStore1 (Shop.sellFlavour (Vanilla, 2))
        updateStore1 (Shop.sellFlavour (Strawberry, 2))

        updateStore2 (Shop.restockFlavour (Vanilla, 2))
        updateStore2 (Shop.sellFlavour (Vanilla, 1))

        store

    let store = createStore ()

    let mutable state = Start

    while state <> Exit do
        state <-
            match state with
            | Start ->
                Console.Clear()

                let selectedShop = 
                    UI.menu "Select a shop:" shopOptions

                match selectedShop with
                | Some shop -> StoreSelected shop
                | None -> Start

            | StoreSelected shop ->
                let selectedFlavour = 
                    UI.menu $"Selected store: {Aggregate.value shop}\nSelect a flavour to sell:" flavourOptions

                match selectedFlavour with
                | Some flavour -> FlavourSelected (shop, flavour)
                | None -> Start

            | FlavourSelected (shop, flavour) ->
                let selectedQuantity = 
                    UI.menu $"Selected store: {Aggregate.value shop}\nSelected flavour: {flavour}\nSelect quantity to sell:" quantityOptions

                match selectedQuantity with
                | Some quantity -> QuantitySpecified (shop, flavour, quantity)
                | None -> StoreSelected shop

            | QuantitySpecified (shop, flavour, quantity) ->
                let confirmPurchase = 
                    UI.menu $"Selected store: {Aggregate.value shop}\nSelected flavour: {flavour}\nSelected quantity: {quantity}\nConfirm sale?" confirmationOptions
                
                match confirmPurchase with
                | true -> PurchaseConfirmed (shop, flavour, quantity)
                | false -> FlavourSelected (shop, flavour)

            | PurchaseConfirmed (shop, flavour, quantity) ->
                store.evolve shop (Shop.sellFlavour (flavour, quantity))
                Console.Clear ()
                Console.WriteLine $"Sale of {quantity} {flavour} from {Aggregate.value shop} completed."
                Console.ReadLine |> ignore
                Start

            | Exit -> 
                Console.Clear ()
                Console.WriteLine $"Exiting..."
                Console.ReadLine |> ignore
                Exit
    0

// open Domain.IceCream
// open Domain.IceCreamTest
// open Infrastructure
// open Utilities.Printer


// runTests () |> ignore

// let store = EventStore.init()
// let shop1 = Shop.createIdentity "shop-1"
// let shop2 = Shop.createIdentity "shop-2"

// store.evolve shop1 (Shop.restockFlavour (Vanilla, 3))
// store.evolve shop1 (Shop.restockFlavour (Strawberry, 1))
// store.evolve shop1 (Shop.sellFlavour (Vanilla, 2))
// store.evolve shop1 (Shop.sellFlavour (Strawberry, 2))

// store.evolve shop2 (Shop.restockFlavour (Vanilla, 2))
// store.evolve shop2 (Shop.sellFlavour (Vanilla, 1))

// let printFlavours shop flavours = 
//     let events = 
//         store.getAggregateEvents shop

//     let soldFlavours = 
//         events
//         |> EventStore.project Shop.soldFlavours

//     let inStockFlavours = 
//         events
//         |> EventStore.project Shop.flavoursInStock

//     printEvents shop events
//     printFlavourState "Sold" shop flavours soldFlavours
//     printFlavourState "In-Stock" shop flavours inStockFlavours

// printFlavours shop1 [ Vanilla; Strawberry; ]
// printFlavours shop2 [ Vanilla; Strawberry; ]