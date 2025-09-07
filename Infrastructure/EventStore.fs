namespace Infrastructure

module EventStore =

    type EventProducer<'Event> = 
        'Event list -> 'Event list

    type Projection<'State, 'Event> = {
        init: 'State
        update: 'State -> 'Event -> 'State
    }

    type Aggregate = Aggregate of string

    module Aggregate =
        let value (Aggregate v) = v

    type EventStore<'Event> = {
        get: unit -> Map<Aggregate, 'Event list>
        getAggregateEvents: Aggregate -> 'Event list
        append: Aggregate -> 'Event list -> unit
        evolve: Aggregate -> EventProducer<'Event> -> unit
    }

    type Message<'Event> =
        | Append of Aggregate * 'Event list
        | Get of AsyncReplyChannel<Map<Aggregate, 'Event list>>
        | GetAggregateEvents of Aggregate * AsyncReplyChannel<'Event list>
        | Evolve of Aggregate * EventProducer<'Event>

    let project projection events =
        events
        |> List.fold projection.update projection.init

    let getAggregateEvents aggregate allEvents =
        allEvents
        |> Map.tryFind aggregate
        |> Option.defaultValue []

    let init () : EventStore<'Event> =
        let mailbox: MailboxProcessor<Message<'Event>> = MailboxProcessor.Start(fun inbox ->
            let rec loop allEvents = async {
                let! message = inbox.Receive()

                match message with

                | Append (aggregate, newEvents) ->
                    let aggregateEvents = 
                        allEvents
                        |> getAggregateEvents aggregate

                    let newAllEvents = 
                        allEvents 
                        |> Map.add aggregate (aggregateEvents @ newEvents)

                    return! loop newAllEvents

                | Get channel ->
                    channel.Reply allEvents

                    return! loop allEvents

                | GetAggregateEvents (aggregate, channel) ->
                    allEvents
                    |> getAggregateEvents aggregate
                    |> channel.Reply

                    return! loop allEvents

                | Evolve (aggregate, producer) ->
                    let aggregateEvents = 
                        allEvents
                        |> getAggregateEvents aggregate

                    let newEvents = producer aggregateEvents

                    let newAllEvents = 
                        allEvents 
                        |> Map.add aggregate (aggregateEvents @ newEvents)

                    return! loop newAllEvents
            }

            loop Map.empty
        )

        let append aggregate events =
            mailbox.Post (Append (aggregate, events))

        let get () =
            mailbox.PostAndReply Get

        let getAggregateEvents aggregate =
            mailbox.PostAndReply (fun channel -> GetAggregateEvents (aggregate, channel))

        let evelove aggregate producer =
            mailbox.Post (Evolve (aggregate, producer))

        { get = get; getAggregateEvents = getAggregateEvents; append = append; evolve = evelove }