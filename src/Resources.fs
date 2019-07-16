module Resources

open Core
open Domain

open Browser
open Elmish

let private produceMap (map : string) : Msg =
    let lines = map.Split [| '\n' |] |> Array.filter (fun line -> line.Length <> 0)
    let first = Array.head lines
    let mapSize = first.Length - 1
    //TODO: Transform all the if's into monads
    //TODO: Allso add error for line length/line count mistmatch
    if  Array.forall (fun (line : string) -> line.Length = first.Length) lines
    then
        let mapData =
            Array.mapi
                (fun i line ->
                    let y = mapSize - i - 1
                    Seq.fold
                        (fun (destinations, buildings, couriers, obstacles, x) char ->
                            match char with
                            | '#'       -> (destinations           , (x, y) :: buildings  , couriers           , obstacles           , x + 1)
                            | '@'       -> (destinations           , buildings            , (x, y) :: couriers , obstacles           , x + 1)
                            | 'x' | 'X' -> ((x, y) :: destinations , buildings            , couriers           , obstacles           , x + 1)
                            | '?'       -> (destinations           , buildings            , couriers           , (x, y) :: obstacles , x + 1)
                            | _         -> (destinations           , buildings            , couriers           , obstacles           , x + 1))
                        ([], [], [], [], 0)
                        line) lines
        let (destinations, buildings, couriers, obstacles) =
            Array.fold
                //TODO: Some weird looking remapping, maybe tune this
                (fun (accDest, accBuildings, accCouriers, accObstacles) (destinations, buildings, couriers, obstacles, _) ->
                    (List.append accDest destinations, List.append accBuildings buildings, List.append accCouriers couriers, List.append accObstacles obstacles))
                ([], [], [], [])
                mapData
        if List.length couriers <> 1
        then
            Dom.console.error ("There is no courier on the map or it's more than one", couriers)
            NoOp
        else
            PlaceMapData (destinations, buildings, obstacles, Courier <| List.head couriers, mapSize)
    else
        Dom.console.error "Not all lines are the same length"
        NoOp

let loadMap (mapFile : string) : Cmd<Msg> =
    Cmd.OfPromise.either
        (fun _ -> Fetch.fetch mapFile [] |> Promise.bind (fun res -> res.text ()))
        ()
        produceMap
        (fun err ->
            Dom.console.error err
            NoOp)
