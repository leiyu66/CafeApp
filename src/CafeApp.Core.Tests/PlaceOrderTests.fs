module PlaceOrderTests

open NUnit.Framework
open CafeAppTestsDSL
open Domain
open System
open States
open Commands
open Events
open Errors

let tab = {Id = Guid.NewGuid(); TableNumber = 1}
let coke = Drink {
            MenuNumber = 1
            Name = "Coke"
            Price = 1.5m}
let order = {Tab = tab;Foods = [];Drinks = []}

[<Test>]
let ``Can place only drinks order`` () =
  let order = {order with Drinks = [coke]}
  Given (OpenedTab tab)
  |> When (PlaceOrder order)
  |> ThenStateShouldBe (PlacedOrder order)
  |> WithEvents [OrderPlaced order]

[<Test>]
let ``Can not place empty order`` () =
  Given (OpenedTab tab)
  |> When (PlaceOrder order)
  |> ShouldFailWith CanNotPlaceEmptyOrder

[<Test>]
let ``Can not place order with a closed tab`` () =
  let order = {order with Drinks = [coke]}
  Given (ClosedTab None)
  |> When (PlaceOrder order)
  |> ShouldFailWith CanNotOrderWithClosedTab

[<Test>]
let ``Can not place order multiple times`` () =
  let order = {order with Drinks = [coke]}
  Given (PlacedOrder order)
  |> When (PlaceOrder order)
  |> ShouldFailWith OrderAlreadyPlaced
