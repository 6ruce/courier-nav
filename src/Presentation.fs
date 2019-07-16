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

  let cellColor disposition index =
    let pos = denormalize index
    if List.contains pos disposition.Path
    then config.PathColor
    else System.String.Empty
  //TODO: Place these properties separately, there are a lot of redundant checkings
  let cellImage disposition index =
    let pos = denormalize index
    let (Courier courierPos) = disposition.Courier
    if pos = courierPos then
      sprintf "url(%s)" <| if disposition.Unreachable then config.ConfusedImage else config.CourierImage
    else
    if Set.contains pos disposition.Destinations           then sprintf "url(%s)" config.DestinationImage else
    if Set.contains pos disposition.Buildings              then sprintf "url(%s)" config.BuildingImage    else
    if Set.exists (fst >> ((=) pos)) disposition.Obstacles then sprintf "url(%s)" config.ObstacleImage    else
      System.String.Empty
  //TODO: Show debugging info on the right side
  //TODO: Redisign interface: add side panel to the right
  div [ Style
           [ Width                  containerSize
           ; Height                 containerSize
           ; Display                DisplayOptions.Grid
           ; GridTemplateColumns    (sprintf "repeat(%i, %fpx)" mapSize (containerSize / float mapSize))
           ]
      ] <| List.map
               (fun i ->
                      div [ Style
                              [ BorderTop       "0.5px dashed gray"
                              ; BorderLeft      "0.5px dashed gray"
                              ; BorderBottom    (if i > mapSize * mapSize - mapSize then "0.5px dashed gray" else "none")
                              ; BorderRight     (if i % mapSize = 0 then "0.5px dashed gray" else "none")
                              ; BackgroundColor    (cellColor disposition i)
                              ; BackgroundImage    (cellImage disposition i)
                              ; BackgroundSize     "cover"
                              ]
                            Id (i.ToString())
                            OnClick (fun _ -> dispatch <| PlaceDestination i)
                            OnContextMenu (fun e ->
                                //Fable doesn't have preventDefault build-in, so this is not really 'pure' way of doing thigs
                                e.preventDefault ()
                                dispatch <| PlaceObstacle i)
                      ] [])
               ([1 .. mapSize * mapSize])
