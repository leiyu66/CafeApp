module Queries
open ReadModel
open Domain
open States
open System

type TableQueries = {
  GetTables : unit -> Async<Table list>
  GetTableByTableNumber : int -> Async<Table option>
  GetTableByTabId : Guid -> Async<Table option>
}
type FoodQueries = {
  GetFoodsByMenuNumbers : int[] -> Async<Choice<Food list, int[]>>
  GetFoodByMenuNumber : int -> Async<Food option>
}
type DrinkQueries = {
  GetDrinksByMenuNumbers : int[] -> Async<Choice<Drink list, int[]>>
  GetDrinkByMenuNumber : int -> Async<Drink option>
}
type ToDoQueries = {
  GetChefToDos : unit -> Async<ChefToDo list>
  GetWaiterToDos : unit -> Async<WaiterToDo list>
  GetCashierToDos : unit -> Async<Payment list>
}
type Queries = {
  Table : TableQueries
  ToDo : ToDoQueries
  Food : FoodQueries
  Drink : DrinkQueries
}
