module Chef
open Domain
open Projections
open ReadModel
open System.Collections.Generic
open System
open Table

let private chefToDos = new Dictionary<Guid, ChefToDo>()
let private addFoodsToPrepare tabId foods =
  match getTableByTabId tabId with
  | Some table ->
    let tab = {Id = tabId; TableNumber = table.Number}
    let todo : ChefToDo = {Tab = tab; Foods = foods}
    chefToDos.Add(tabId, todo)
  | None -> ()
  async.Return ()
let private removeFood tabId food =
  let todo = chefToDos.[tabId]
  let chefToDo =
    { todo with Foods =
                  List.filter (fun d -> d <> food) todo.Foods}
  chefToDos.[tabId] <- chefToDo
  async.Return ()
let private remove tabId =
  chefToDos.Remove(tabId) |> ignore
  async.Return ()
let chefActions = {
  AddFoodsToPrepare = addFoodsToPrepare
  RemoveFood = removeFood
  Remove = remove
}
let getChefToDos () =
  chefToDos.Values
  |> Seq.toList
  |> async.Return

