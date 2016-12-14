module JsonFormatter
open Newtonsoft.Json.Linq
open Domain
open States
open CommandHandlers
open Suave
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
let (.=) key (value : obj) = new JProperty(key, value)
let jobj jProperties =
  let jObject = new JObject()
  jProperties |> List.iter jObject.Add
  jObject
let jArray jObjects =
  let jArray = new JArray()
  jObjects |> List.iter jArray.Add
  jArray

let tabIdJObj tabId =
  jobj [
    "tabId" .= tabId
  ]

let tabJObj tab =
  jobj [
    "id" .= tab.Id
    "tableNumber" .= tab.TableNumber
  ]
let itemJObj item =
  jobj [
    "menuNumber" .= item.MenuNumber
    "name" .= item.Name
  ]
let foodJObj (Food item) = itemJObj item
let drinkJObj (Drink item) = itemJObj item
let foodJArray foods =
  foods |> List.map foodJObj |> jArray
let drinkJArray drinks =
  drinks |> List.map drinkJObj |> jArray

let orderJObj (order : Order) =
  jobj [
    "tabId" .= order.Tab.Id
    "tableNumber" .= order.Tab.TableNumber
    "foods" .= foodJArray order.Foods
    "drinks" .= drinkJArray order.Drinks
  ]

let orderInProgressJObj ipo =
  jobj [
    "tabId" .=  ipo.PlacedOrder.Tab.Id
    "tableNumber" .= ipo.PlacedOrder.Tab.TableNumber
    "preparedFoods" .= foodJArray ipo.PreparedFoods
    "servedFoods" .= foodJArray ipo.ServedFoods
    "servedDrinks" .= drinkJArray ipo.ServedDrinks]

let stateJObj = function
| ClosedTab tabId ->
  let state = "state" .= "ClosedTab"
  match tabId with
  | Some id ->
    jobj [ state; "data" .= tabIdJObj id ]
  | None -> jobj [state]
| OpenedTab tab ->
  jobj [
    "state" .= "OpenedTab"
    "data" .= tabJObj tab
  ]
| PlacedOrder order ->
  jobj [
    "state" .= "PlacedOrder"
    "data" .= orderJObj order
  ]
| OrderInProgress ipo ->
  jobj [
    "state" .= "OrderInProgress"
    "data" .= orderInProgressJObj ipo
  ]
| ServedOrder order ->
  jobj [
    "state" .= "ServedOrder"
    "data" .= orderJObj order
  ]

let JSON webpart jsonString (context : HttpContext) = async {
  let wp =
    webpart jsonString >=>
      Writers.setMimeType
        "application/json; charset=utf-8"
  return! wp context
}
let toStateJson state =
  state |> stateJObj |> string |> JSON OK

let toErrorJson err =
  jobj [ "error" .= err.Message ]
  |> string |> JSON BAD_REQUEST
