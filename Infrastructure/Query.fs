namespace Infrastructure

module Query =

    type Result =
        | Handled of obj
        | NotHandled
        | QueryError of string

    type Handler<'Query> = {
        handle: 'Query -> Async<Result>
    }

    module Handler =
        let rec private choice handlers query =
            async {
                match handlers with
                | [] ->
                    return NotHandled

                | handler :: rest ->
                    let! result = handler.handle query

                    match result with
                    | NotHandled ->
                        return! choice rest query

                    | Handled response ->
                        return Handled response

                    | QueryError response ->
                         return QueryError response
            }

        let initialize handlers: Handler<_> = {
            handle = choice handlers
        }