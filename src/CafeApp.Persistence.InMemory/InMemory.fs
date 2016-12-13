module InMemory
open Table
open Chef
open Waiter
open Cashier
open Projections
open Queries
open Items
open EventStore
open NEventStore

type InMemoryEventStore () =
  static member Instance =
                  Wireup.Init()
                    .UsingInMemoryPersistence()
                    .Build()

let inMemoryEventStore () =
  let eventStoreInstance = InMemoryEventStore.Instance
  {
    GetState = getState eventStoreInstance
    SaveEvents = saveEvents eventStoreInstance
  }

let toDoQueries = {
  GetChefToDos = getChefToDos
  GetCashierToDos = getCashierToDos
  GetWaiterToDos = getWaiterToDos
}

let inMemoryQueries = {
  Table = tableQueries
  ToDo = toDoQueries
  Food = foodQueries
  Drink = drinkQueries
}
let inMemoryActions = {
  Table = tableActions
  Chef = chefActions
  Waiter = waiterActions
  Cashier = cashierActions
}
