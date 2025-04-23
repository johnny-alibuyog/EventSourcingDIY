
module Domain
open Projection

type Flavour =
    | Strawberry
    | Vanilla

type Event =
    | Flavour_sold of flavour: Flavour * quantity: int
    | Flavour_restocked of flavour: Flavour * quantity: int
    | Flavour_went_out_of_stock of flavour: Flavour
    | Flavour_was_not_in_stock of flavour: Flavour

let soldFlavours = {
    init = Map.empty
    update = (fun state event ->
        match event with
        | Flavour_sold (flavour, quantity) ->
            let portion =
                state
                |> Map.tryFind flavour
                |> Option.defaultValue 0                    
                |> (+) quantity

            state
            |> Map.add flavour portion


        | _ ->
            state
    )
}