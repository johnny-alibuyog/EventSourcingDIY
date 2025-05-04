
module Domain

open EventStore

type Flavour =
    | Strawberry
    | Vanilla

type Event =
    | Flavour_sold of flavour: Flavour * quantity: int
    | Flavour_restocked of flavour: Flavour * quantity: int
    | Flavour_went_out_of_stock of flavour: Flavour
    | Flavour_was_not_in_stock of flavour: Flavour

let getQuantity flavour state =
    state
    |> Map.tryFind flavour
    |> Option.defaultValue 0

let soldFlavours = {
    init = Map.empty
    update = (fun state event ->
        match event with
        | Flavour_sold (flavour, quantity) ->
            let portion =
                state
                |> getQuantity flavour
                |> (+) quantity

            state
            |> Map.add flavour portion

        | _ ->
            state
    )
}

let flavoursInStock = {
    init = Map.empty
    update = (fun state event ->
        match event with
        | Flavour_restocked (flavour, quantity) ->
            let portion =
                state
                |> getQuantity flavour
                |> (+) quantity

            state
            |> Map.add flavour portion

        | Flavour_sold (flavour, quantity) ->
            let portion =
                state
                |> getQuantity flavour
                |> (-) quantity

            state
            |> Map.add flavour portion


        | _ ->
            state
    )
}

let restockFlavour (flavour, quantity) events=
    [Flavour_restocked (flavour, quantity)]

let sellFlavour (flavour,  quantity) events=

    // get stock for a specific flavour
    let stock =
        events 
        |> project flavoursInStock
        |> getQuantity flavour


    // sell based on available stock
    match stock with
    | 0 ->
        [Flavour_was_not_in_stock flavour]
    | _ when stock < quantity ->
        [Flavour_sold (flavour, stock); Flavour_went_out_of_stock flavour]
    | _ ->
        [Flavour_sold (flavour, quantity)]

