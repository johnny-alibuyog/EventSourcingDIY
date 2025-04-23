module Projection

type Projection<'State, 'Event> = {
    init: 'State
    update: 'State -> 'Event -> 'State
}

let project projection events =
    events
    |> List.fold projection.update projection.init