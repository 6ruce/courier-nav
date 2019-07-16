module Core

open Domain

type Model =
    { Disposition   : Disposition
    ; Viewport      : (float * float)
    ; Paused        : bool
    }

type Msg =
    | NoOp
    | Update
    | PlaceDestination of int
    | PlaceObstacle    of int
    | PlaceMapData     of (Building list * Destination list * Obstacle list * Courier * MapSize)
    | Viewport         of (float * float)

type Config =
    { Delay             : int
    ; DefaultWaitTime   : int
    ; CourierImage      : string
    ; ConfusedImage     : string
    ; BuildingImage     : string
    ; ObstacleImage     : string
    ; DestinationImage  : string
    ; PathColor         : string
    ; MapFile           : string
    }
