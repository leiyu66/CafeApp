module Program

open Suave
open Suave.Web
open Suave.Successful
open Suave.RequestErrors
open Suave.Operators
open Suave.Filters
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

[<EntryPoint>]
let main argv =
  eventsStream.Publish.Add(projectEvents)
  let app =
    let eventStore = inMemoryEventStore ()
    choose [
      commandApi eventStore
      queriesApi inMemoryQueries eventStore
    ]
  let cfg = 
    {defaultConfig with
      bindings = [HttpBinding.mkSimple HTTP "0.0.0.0" 8083]}
  startWebServer cfg app
  0