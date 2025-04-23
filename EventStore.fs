module EventStore

type EventStore<'Event> = {
    get: unit -> 'Event list
    append: 'Event list -> unit
}

type Message<'Event> =
    | Append of 'Event list
    | Get of AsyncReplyChannel<'Event list>

let initialize () : EventStore<'Event> =
    let mailbox = MailboxProcessor.Start(fun inbox ->
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
        mailbox.Post (Append events)

    let get () =
        mailbox.PostAndReply Get

    { get = get; append = append }