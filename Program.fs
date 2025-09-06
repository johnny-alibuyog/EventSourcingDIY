open Domain.IceCream
open Domain.IceCreamTest
open Printer


runTests () |> ignore

let store = EventStore.init()
let shop1 = Shop.createIdentity "shop-1"
let shop2 = Shop.createIdentity "shop-2"

store.evolve shop1 (Shop.restockFlavour (Vanilla, 3))
store.evolve shop1 (Shop.restockFlavour (Strawberry, 1))
store.evolve shop1 (Shop.sellFlavour (Vanilla, 2))
store.evolve shop1 (Shop.sellFlavour (Strawberry, 2))

store.evolve shop2 (Shop.restockFlavour (Vanilla, 2))
store.evolve shop2 (Shop.sellFlavour (Vanilla, 1))

let printFlavours shop flavours = 
    let events = 
        store.getAggregateEvents shop

    let soldFlavours = 
        events
        |> EventStore.project Shop.soldFlavours

    let inStockFlavours = 
        events
        |> EventStore.project Shop.flavoursInStock

    printEvents shop events
    printFlavourState "Sold" shop flavours soldFlavours
    printFlavourState "In-Stock" shop flavours inStockFlavours

printFlavours shop1 [ Vanilla; Strawberry; ]
printFlavours shop2 [ Vanilla; Strawberry; ]