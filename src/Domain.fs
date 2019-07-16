module Domain

open Common

type Courier           = Courier of Position
type Building          = Position
type Obstacle          = Position
type Destination       = Position
type Obstacles         = Set<(Obstacle * int)>
type Buildings         = Set<Building>
type Destinations      = Set<Position>
type MapSize           = int

type Configuration =
    { ObstacleWaitTime : int
    ;
    }

type DispositionR =
    { Courier       : Courier
    ; Buildings     : Buildings
    ; Obstacles     : Obstacles
    ; Destinations  : Destinations
    ; MapSize       : int
    ; Path          : Position list
    ; Unreachable   : bool
    }

type Disposition = private MkD of DispositionR

(*/ Upacking underlying type for reading *)
let unpack ((MkD disposition) : Disposition) : DispositionR = disposition



let private isPlacementAlowed (disposition : DispositionR) (courier : Position) (target : Position) =
    not <| (Set.contains target <|
            Set.unionMany
                [ (Set.add courier disposition.Buildings)
                ; (Set.add courier disposition.Destinations)
                ; (Set.map (fun (pos, _) -> pos) disposition.Obstacles)
                ])

let private dummyDisposition (config : Configuration) : Disposition =
    MkD
        { Courier       = Courier (25, 25)
        ; Buildings     =
            Set.ofList
                [
                    (24, 26); (25, 26); (26, 26);
                    (24, 24); (25, 24); (26, 24)
                ]
        ; Obstacles     = Set.ofList <| List.map (flip (<.) config.ObstacleWaitTime) [(24, 25); (26, 25); ]
        ; Destinations  = Set.ofList []
        ; MapSize       = 50
        ; Path          = []
        ; Unreachable   = false
        }

let private createDisposition
    (mapSize      : int)
    (courier      : Courier)
    (buidings     : Buildings)
    (obstacles    : Obstacles)
    (destinations : Destinations)
    : Result<Disposition, string> =
        // Each parameter validation can be placed here
        (Ok << MkD)
            { Courier       = courier
            ; Buildings     = buidings
            ; Obstacles     = obstacles
            ; Destinations  = destinations
            ; MapSize       = mapSize
            ; Path          = []
            ; Unreachable   = false
            }

let private calculatePath (disposition : DispositionR) : Path option =
    let (Courier courier) = disposition.Courier
    PathFind.findRoute disposition.MapSize disposition.Buildings disposition.Obstacles courier disposition.Destinations

let private recalculatePath (disposition : DispositionR) : DispositionR =
    match calculatePath disposition with
    | Some path -> { disposition with Path = path }
    | None      -> { disposition with Unreachable = true }

let rec nextStep (disposition : DispositionR) : DispositionR =
    match disposition.Path with
    | [] when disposition.Unreachable || Set.isEmpty disposition.Destinations
                      -> disposition
    | []              -> nextStep <| recalculatePath disposition
    | next :: _ when Set.exists (fst >> ((=) next)) disposition.Obstacles
                      -> disposition
    | next :: rest    ->
        { disposition with
            Path         = rest;
            Courier      = Courier next
            Destinations = Set.remove next disposition.Destinations }

let dispositionFrame ((MkD disposition) : Disposition) : Disposition =
    let updateObstacles model =
        { model with
            Obstacles = (Set.map (fun (pos, step) -> (pos, step - 1)) << Set.filter ((flip (>) 0) << snd)) model.Obstacles }
    (MkD << updateObstacles) <| nextStep disposition

let private tryPlaceDestination ((MkD disposition) : Disposition) (destination : Destination) : Disposition option =
    let (Courier courier) = disposition.Courier
    match isPlacementAlowed disposition courier destination with
    | true   ->
        (Some << MkD << recalculatePath)
            { disposition with
                Destinations = Set.add destination disposition.Destinations;
                Unreachable  = false;
                Path         = List.empty }
    | false  -> None

let private tryPlaceObstacle (config : Configuration) ((MkD disposition) : Disposition) (obstacle : Obstacle) : Disposition option =
    let (Courier courier) = disposition.Courier
    match isPlacementAlowed disposition courier obstacle with
    | true  ->
        let updated = { disposition with Obstacles = Set.add (obstacle, config.ObstacleWaitTime) disposition.Obstacles }
        let interrupting = List.contains obstacle disposition.Path
        (Some << MkD) (if interrupting then recalculatePath updated else updated)
    | false -> None

type Instance (config : Configuration) =
    (*/ Sample disposition with dummy data *)
    member x.dummyDisposition    = dummyDisposition config

    (*/ In order to avoid invalid disposition it should be created preserving domain rules *)
    member x.createDisposition   = createDisposition

    (*/ Tries to place destination location and recalculates disposition (new path or unreachable state) if needed *)
    member x.tryPlaceDestination = tryPlaceDestination

    (*/ Tries to place obstacle location and recalculates disposition (new path or unreachable state) if needed *)
    member x.tryPlaceObstacle    = tryPlaceObstacle config

    (*/ Simulates 1 frame forward using provided disposition *)
    member x.dispositionFrame    = dispositionFrame
