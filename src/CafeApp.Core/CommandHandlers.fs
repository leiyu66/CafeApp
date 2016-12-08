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

let (|AlreadyServedDrink|_|) ipo drink =
  match List.contains drink ipo.ServedDrinks with
  | true -> Some drink
  | false -> None

let (|ServeDrinkCompletesIPOrder|_|) ipo drink =
  match isServingDrinkCompletesIPOrder ipo drink with
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
| OrderInProgress ipo ->
  let order = ipo.PlacedOrder
  let drinkServed = DrinkServed (drink, order.Tab.Id)
  match drink with
  | NonOrderedDrink order _ ->
    CanNotServeNonOrderedDrink drink |> fail
  | AlreadyServedDrink ipo _ ->
    CanNotServeAlreadyServedDrink drink |> fail
  | ServeDrinkCompletesIPOrder ipo _ ->
    drinkServed ::
      [OrderServed (ipo.PlacedOrder,  payment ipo.PlacedOrder)]
    |> ok
  | _ -> [drinkServed] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ ->  CanNotServeForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotServeWithClosedTab |> fail
| _ -> failwith "TODO"

let (|NonOrderedFood|_|) order food =
  match List.contains food order.Foods with
  | false -> Some food
  | true -> None

let (|AlreadyPreparedFood|_|) ipo food =
  match List.contains food ipo.PreparedFoods with
  | true -> Some food
  | false -> None

let handlePrepareFood food tabId = function
| PlacedOrder order ->
  match food with
  | NonOrderedFood order _ ->
    CanNotPrepareNonOrderedFood food |> fail
  | _ -> [FoodPrepared (food, tabId)] |> ok
| OrderInProgress ipo ->
  let order = ipo.PlacedOrder
  match food with
  | NonOrderedFood order _ ->
    CanNotPrepareNonOrderedFood food |> fail
  | AlreadyPreparedFood ipo _ ->
      CanNotPrepareAlreadyPreparedFood food |> fail
  | _ -> [FoodPrepared (food, tabId)] |> ok
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ ->  CanNotPrepareForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotPrepareWithClosedTab |> fail
| _ -> failwith "TODO"

let (|UnPreparedFood|_|) ipo food =
  match List.contains food ipo.PreparedFoods with
  | false -> Some food
  | true -> None
let (|AlreadyServedFood|_|) ipo food =
  match List.contains food ipo.ServedFoods with
  | true -> Some food
  | false -> None

let (|ServeFoodCompletesIPOrder|_|) ipo food =
  match isServingFoodCompletesIPOrder ipo food with
  | true -> Some food
  | false -> None

let handleServeFood food tabId = function
| OrderInProgress ipo ->
  let order = ipo.PlacedOrder
  let foodServed = FoodServed (food, tabId)
  match food with
  | NonOrderedFood order _ ->
    CanNotServeNonOrderedFood food |> fail
  | AlreadyServedFood ipo _ ->
    CanNotServeAlreadyServedFood food |> fail
  | UnPreparedFood ipo _ ->
    CanNotServeNonPreparedFood food |> fail
  | ServeFoodCompletesIPOrder ipo _ ->
    foodServed ::
      [OrderServed (ipo.PlacedOrder, payment ipo.PlacedOrder)]
    |> ok
  | _ -> [foodServed] |> ok
| PlacedOrder _ -> CanNotServeNonPreparedFood food |> fail
| ServedOrder _ -> OrderAlreadyServed |> fail
| OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
| ClosedTab _ -> CanNotServeWithClosedTab |> fail
| _ -> failwith "Todo"

let handleCloseTab payment = function
| ServedOrder order ->
  let orderAmount = orderAmount order
  if payment.Amount = orderAmount then
    [TabClosed payment] |> ok
  else
    InvalidPayment (orderAmount, payment.Amount) |> fail
| _ -> CanNotPayForNonServedOrder |> fail

let execute state command =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeDrink (drink, tabId) -> handleServeDrink drink tabId state
  | PrepareFood (food, tabId) -> handlePrepareFood food tabId state
  | ServeFood (food, tabId) -> handleServeFood food tabId state
  | CloseTab payment -> handleCloseTab payment state
  | _ -> failwith "ToDo"

let evolve state command =
  match execute state command with
  | Ok (events,_) ->
    let newState = List.fold apply state events
    (newState, events) |> ok
  | Bad err -> Bad err
