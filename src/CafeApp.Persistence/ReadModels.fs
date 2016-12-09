module ReadModel
open Domain
type TableStatus =
| Open of System.Guid
| InService of System.Guid
| Closed
type Table = {
  Number : int
  Waiter : string
  Status : TableStatus
}
type ChefToDo = {
  Tab : Tab
  Foods : Food list
}
type WaiterToDo = {
  Tab : Tab
  Foods : Food list
  Drinks : Drink list
}
