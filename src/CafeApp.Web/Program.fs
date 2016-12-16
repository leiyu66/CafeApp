module Program

open Suave
open Suave.Web
open Suave.Successful
open Suave.RequestErrors
open Suave.Operators
open Suave.Filters
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open CommandApi
open QueriesApi
open InMemory
open System.Text
open Chessie.ErrorHandling
open Projections
open Events
open JsonFormatter

let eventsStream = new Control.Event<Event list>()
let commandApiHandler eventStore (context : HttpContext) = async {
  let payload =
    Encoding.UTF8.GetString context.request.rawForm
  let! response =
    handleCommandRequest
      inMemoryQueries eventStore payload
  match response with
  | Ok ((state,events), _) ->
    eventsStream.Trigger(events)
    do! eventStore.SaveEvents state events
    return! toStateJson state context
  | Bad (err) ->
    return! toErrorJson err.Head context
}
let commandApi eventStore =
  path "/command"
    >=> POST
    >=> commandApiHandler eventStore

let project event =
  projectReadModel inMemoryActions event
  |> Async.RunSynchronously |> ignore
let projectEvents = List.iter project

let socketHandler (ws : WebSocket) cx = socket {
  while true do
    let! events =
      Control.Async.AwaitEvent(eventsStream.Publish)
      |> Suave.Sockets.SocketOp.ofAsync
    for event in events do
      let eventData =
        event |> eventJObj |> string |> Encoding.UTF8.GetBytes
      do! ws.send Text eventData true
}

[<EntryPoint>]
let main argv =
  eventsStream.Publish.Add(projectEvents)
  let app =
    let eventStore = inMemoryEventStore ()
    choose [
      path "/websocket" >=> handShake socketHandler
      commandApi eventStore
      queriesApi inMemoryQueries eventStore
    ]
  let cfg =
    {defaultConfig with
      bindings = [HttpBinding.mkSimple HTTP "0.0.0.0" 8083]}
  startWebServer cfg app
  0
