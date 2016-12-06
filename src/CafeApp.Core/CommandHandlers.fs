module CommandHandlers
open Chessie.ErrorHandling
open Errors
open States
open Events
open System
open Domain
open Commands

let handleOpenTab tab = function
| ClosedTab _ -> [TabOpened tab] |> ok
| _ -> TabAlreadyOpened |> fail

let handlePlaceOrder order = function
| OpenedTab _ ->
  if List.isEmpty order.Foods && List.isEmpty order.Drinks then
    fail CanNotPlaceEmptyOrder
  else
    [OrderPlaced order] |> ok
| ClosedTab _ -> fail CanNotOrderWithClosedTab
| _ -> fail OrderAlreadyPlaced

let (|NonOrderedDrink|_|) order drink =
  match List.contains drink order.Drinks with
  | false -> Some drink
  | true -> None

let (|ServeDrinkCompletesOrder|_|) order drink =
  match isServingDrinkCompletesOrder order drink with
  | true -> Some drink
  | false -> None

let handleServeDrink drink tabId = function
| PlacedOrder order ->
  let event = DrinkServed (drink,tabId)
  match drink with
  | NonOrderedDrink order _ ->
    CanNotServeNonOrderedDrink drink |> fail
  | ServeDrinkCompletesOrder order _ ->
    let payment = {Tab = order.Tab; Amount = orderAmount order}
    event :: [OrderServed (order, payment)] |> ok
  | _ -> [event] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ ->  CanNotServeForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotServeWithClosedTab |> fail
| _ -> failwith "TODO"

let execute state command =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeDrink (drink, tabId) -> handleServeDrink drink tabId state
  | _ -> failwith "ToDo"

let evolve state command =
  match execute state command with
  | Ok (events,_) ->
    let newState = List.fold apply state events
    (newState, events) |> ok
  | Bad err -> Bad err
