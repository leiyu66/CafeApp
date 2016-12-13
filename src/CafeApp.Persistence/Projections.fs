module Projections
open Events
open Domain
open System

type TableActions = {
  OpenTab : Tab -> Async<unit>
  ReceivedOrder : Guid -> Async<unit>
  CloseTab : Tab -> Async<unit>
}

type CashierActions = {
  AddTabAmount : Guid -> decimal -> Async<unit>
  Remove : Guid -> Async<unit>
}

type ChefActions = {
  AddFoodsToPrepare : Guid -> Food list -> Async<unit>
  RemoveFood : Guid -> Food -> Async<unit>
  Remove : Guid -> Async<unit>
}

type WaiterActions = {
  AddDrinksToServe : Guid -> Drink list -> Async<unit>
  MarkDrinkServed : Guid -> Drink -> Async<unit>
  AddFoodToServe : Guid -> Food -> Async<unit>
  MarkFoodServed : Guid -> Food -> Async<unit>
  Remove : Guid -> Async<unit>
}

type ProjectionActions = {
  Table : TableActions
  Waiter : WaiterActions
  Chef : ChefActions
  Cashier : CashierActions
}

let projectReadModel actions = function
| TabOpened tab ->
  [actions.Table.OpenTab tab] |> Async.Parallel
| OrderPlaced order ->  
  let tabId = order.Tab.Id
  [    
    actions.Table.ReceivedOrder tabId
    actions.Chef.AddFoodsToPrepare tabId order.Foods
    actions.Waiter.AddDrinksToServe tabId order.Drinks
  ] |> Async.Parallel
| DrinkServed (item, tabId) ->
  [actions.Waiter.MarkDrinkServed tabId item]
  |> Async.Parallel
| FoodPrepared (food, tabId) ->
  [
    actions.Chef.RemoveFood tabId food
    actions.Waiter.AddFoodToServe tabId food
  ] |> Async.Parallel
| FoodServed (food, tabId) ->
  [actions.Waiter.MarkFoodServed tabId food]
  |> Async.Parallel 
| OrderServed (order, payment) ->
  let tabId = order.Tab.Id
  [
    actions.Waiter.Remove tabId
    actions.Chef.Remove tabId
    actions.Cashier.AddTabAmount tabId payment.Amount
  ] |> Async.Parallel
| TabClosed payment ->
  let tabId = payment.Tab.Id
  [
    actions.Cashier.Remove tabId  
    actions.Table.CloseTab payment.Tab
  ] |> Async.Parallel
| _ -> failwith "Todo"
