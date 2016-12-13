module CommandApi
open System.Text
open CommandHandler
open OpenTab
open PlaceOrder
open Queries
open Chessie.ErrorHandling

// ValidationQueries -> EventStore -> string
//     -> Async<Result<(State*Event),ErrorResponse>>
let handleCommandRequest validationQueries eventStore
  = function
  | OpenTabRequest tab ->
      validationQueries.Table.GetTableByTableNumber
      |> openTabCommander
      |> handleCommand eventStore tab
  | PlaceOrderRequest placeOrder ->
    placeOrderCommander validationQueries
    |> handleCommand eventStore placeOrder
  | _ -> err "Invalid command" |> fail |> async.Return
