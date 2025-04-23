module Infrastructure =
    type EventStore<'Event> = {
        get: unit -> 'Event list
        append: 'Event list -> unit
    }

    module EventStore =
        type Message<'Event> =
            | Append of 'Event list
            | Get of AsyncReplyChannel<'Event list>

        let initialize () : EventStore<'Event> =
            
            // Create a mailbox processor to handle the events
            let agent = MailboxProcessor.Start(fun inbox ->            
                let rec loop history = async {
                    let! message = inbox.Receive()

                    match message with
                    
                    | Append events -> 
                        return! loop (history @ events)
                    
                    | Get channel ->
                        channel.Reply history
                        return! loop history
                }

                loop []
            )

            let append events =
                agent.Post (Append events)

            let get () =
                agent.PostAndReply Get

            { get = get; append = append }


module Domain =

    type Flavour =
        | Strawberry
        | Vanilla

    type Event =
        | Flavour_sold of flavour: Flavour
        | Flavour_restocked of flavour: Flavour * quantity: int
        | Flavour_went_out_of_stock of flavour: Flavour
        | Flavour_was_not_in_stock of flavour: Flavour


module Helpler =
    let private printUnorderList list =
        list
        |> List.iteri (fun i e -> printfn "%d: %A" i e)

    let printEvents events =
        events
        |> List.length
        |> printfn "History (Length: %i)"

        events
        |> printUnorderList


open Infrastructure
open Domain
open Helpler

let store = EventStore.initialize()

store.append [ Flavour_restocked (Vanilla, 3) ]
store.append [ Flavour_sold Vanilla ]
store.append [ Flavour_sold Vanilla ]
store.append [ Flavour_sold Vanilla; Flavour_went_out_of_stock Vanilla ]
store.get() |> printEvents