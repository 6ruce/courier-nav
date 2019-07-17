module CourierNav

open Common
open Core
open Domain

open Browser
open Elmish
open Elmish.React

let config =
     { Delay            = 100 // ms
     ; DefaultWaitTime  = 100 // steps/updates
     ; CourierImage     = "images/courier.jpg"
     ; ConfusedImage    = "images/confused.png"
     ; BuildingImage    = "images/building.png"
     ; ObstacleImage    = "images/obstacle.png"
     ; DestinationImage = "images/burger.png"
     ; PathColor        = "yellow"
     ; MapFile          = "maze.txt"
     }

let domain = Domain.Instance({ ObstacleWaitTime = config.DefaultWaitTime })

//TODO: Also react to viewport change
let initViewport () : Cmd<Msg> =
    let document = Dom.document
    Cmd.OfFunc.either
        (fun _ ->
            //Not the safest way to do this but should be good enough in our case
            (document.getElementsByTagName "body").[0]
                |> (fun el -> el.getBoundingClientRect ())
                |> (fun style ->  (style.width, style.height)))
        ()
        Viewport
        (fun err ->
            Dom.console.error err
            NoOp)


let private init(_ : Unit) : Model * Cmd<Msg> =
    { Disposition   = domain.dummyDisposition
    ; Viewport      = (0.0, 0.0)
    ; Paused        = false
    }, Cmd.batch [initViewport (); Resources.loadMap config.MapFile;]


let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    let { Disposition = disposition } = model
    let mapSize = ((Domain.unpack disposition) : DispositionR).MapSize
    let denormalize = denormalize mapSize
    match msg with
    | NoOp            -> model, Cmd.none
    | Viewport sizes  -> { model with Viewport = sizes }, Cmd.none
    | PlaceDestination i ->
        let target = denormalize i
        match domain.tryPlaceDestination disposition target with
        | Some newDisposition -> { model with Disposition = newDisposition }, Cmd.none
        | None                -> model, Cmd.none
    | PlaceObstacle i ->
        let target = denormalize i
        match domain.tryPlaceObstacle disposition target with
        | Some newDisposition -> { model with Disposition = newDisposition }, Cmd.none
        | None                -> model, Cmd.none
    | PlaceMapData (destinations, buildings, obstacles, courier, size) ->
        let obstacles = Set.ofList <| List.map (flip (<.) config.DefaultWaitTime) obstacles; 
        match domain.createDisposition size courier (Set.ofList buildings) obstacles (Set.ofList destinations) with
        | Ok newDisposition -> { model with Disposition = newDisposition }, Cmd.none
        | Error err         ->
            Dom.console.error err
            model, Cmd.none
    | Update ->
        (if model.Paused then model else { model with Disposition = domain.dispositionFrame model.Disposition }), Cmd.none


let timer initial =
    let sub dispatch =
        Dom.window.setInterval ((fun _ -> dispatch Update), config.Delay, Array.empty) |> ignore
    Cmd.ofSub sub

// App
Program.mkProgram init update (Presentation.view config)
    |> Program.withReactBatched "elmish-app"
    |> Program.withSubscription timer
    |> Program.run
