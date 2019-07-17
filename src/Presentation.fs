module Presentation

open Core
open Common
open Domain

open Fable.React
open Fable.React.Props

let view (config : Config) (model : Model) dispatch =
  let (w, h)        = model.Viewport
  let containerSize = min w h - 16.0 //<body> padding
  let disposition   = Domain.unpack model.Disposition
  let mapSize       = disposition.MapSize
  let denormalize   = denormalize mapSize
  let (Courier courierPos) = disposition.Courier

  let cellColor pos =
    if List.contains pos disposition.Path
    then config.PathColor
    else System.String.Empty
  let cellImage pos =
    if pos = courierPos then
      sprintf "url(%s)" <| if disposition.Unreachable then config.ConfusedImage else config.CourierImage
    else
    if Set.contains pos              disposition.Destinations then sprintf "url(%s)" config.DestinationImage else
    if Set.contains pos              disposition.Buildings    then sprintf "url(%s)" config.BuildingImage    else
    if Set.exists (fst >> ((=) pos)) disposition.Obstacles    then sprintf "url(%s)" config.ObstacleImage    else
      System.String.Empty
  let border = "0.5px dashed gray"
  let cell i =
      let (x, y) = denormalize i
      div [ Style
              [ BorderTop       border
              ; BorderLeft      border
              ; BorderBottom    (if y = 0           then border else "none")
              ; BorderRight     (if x = mapSize - 1 then border else "none")
              ; BackgroundColor    (cellColor (x, y))
              ; BackgroundImage    (cellImage (x, y))
              ; BackgroundSize     "cover"
              ]
            OnClick (fun _ -> dispatch <| PlaceDestination i)
            OnContextMenu (fun e ->
                //Fable doesn't have preventDefault build-in, so this is not really 'pure' way of doing thigs
                e.preventDefault ()
                dispatch <| PlaceObstacle i)
      ] []

  let grid = List.map cell ([1 .. mapSize * mapSize])
  div [ Style
           [ Width                  containerSize
           ; Height                 containerSize
           ; Display                DisplayOptions.Grid
           ; GridTemplateColumns    (sprintf "repeat(%i, %fpx)" mapSize (containerSize / float mapSize))
           ]
      ] grid
