module EventStore

type EventProducer<'Event> = 
    'Event list -> 'Event list

type Projection<'State, 'Event> = {
    init: 'State
    update: 'State -> 'Event -> 'State
}

type EventStore<'Event> = {
    get: unit -> 'Event list
    append: 'Event list -> unit
    evolve: EventProducer<'Event> -> unit
}

type Message<'Event> =
    | Append of 'Event list
    | Get of AsyncReplyChannel<'Event list>
    | Evolve of EventProducer<'Event>

let project projection events =
    events
    |> List.fold projection.update projection.init

let initialize () : EventStore<'Event> =
    let mailbox: MailboxProcessor<Message<'Event>> = MailboxProcessor.Start(fun inbox ->
        let rec loop allEvents = async {
            let! message = inbox.Receive()

            match message with

            | Append newEvents ->
                return! loop (allEvents @ newEvents)

            | Get channel ->
                channel.Reply allEvents
                return! loop allEvents

            | Evolve producer ->
                let newEvents = producer allEvents
                return! loop (allEvents @ newEvents)
        }

        loop []
    )

    let append events =
        mailbox.Post (Append events)

    let get () =
        mailbox.PostAndReply Get

    let evelove producer =
        mailbox.Post (Evolve producer)

    { get = get; append = append; evolve = evelove }