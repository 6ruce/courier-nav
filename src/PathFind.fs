module PathFind

open Common

open System.Collections.Generic

let private reconstruct (path : Dictionary<(int * int), (int * int)>) (destination : Position) : Path =
    let rec reconstruct' acc dest =
        let found, next = path.TryGetValue dest
        if found then reconstruct' (next :: acc) next else acc
    reconstruct' [destination] destination

let private aStar
    (mapSize : int)
    (buildings : Set<Position>)
    (obstacles : (Position * int) list)
    (sourceX: int, sourceY: int)
    (destX: int, destY: int)
    : Path option =
    let pointInsideMap (x, y) = x >= 0 && y >= 0 && x < mapSize && y < mapSize
    let estimate       (x, y) = x - destX + y - destY
    let notBuilding           = not << flip Set.contains buildings
    let wage ((neighbourX, neighbourY), g)        =
        match List.tryFind (fun ((x, y), _) -> neighbourX = x && neighbourY = y) obstacles with
        | Some (_, delay) ->
            max (delay - g) 0 + 1
        | None            -> 1
    //Mutable algorythm but it's a local mutation so it's a kind of ok
    let openSet   = HashSet<(int * int * int)>()
    let visited   = HashSet<(int * int)>()
    let gs        = Dictionary<(int * int), int>()
    let path      = Dictionary<(int * int), (int * int)>()
    let mutable result = None
    openSet .Add (sourceX, sourceY, 0)   |> ignore
    gs      .Add ((sourceX, sourceY), 0) |> ignore
    //TODO: Fix termination step
    while openSet.Count <> 0 && Option.isNone result do
        //TODO: User priority queue(heap) for popping smallest F element
        let (currentX, currentY, f) = Seq.minBy thr openSet
        if (currentX = destX && currentY = destY) then
            result <- Some <| reconstruct path (destX, destY)
        else
            openSet .Remove (currentX, currentY, f) |> ignore
            visited .Add    ((currentX, currentY))  |> ignore

        for neighbour in
            [(currentX - 1, currentY);
             (currentX + 1, currentY);
             (currentX, currentY - 1);
             (currentX, currentY + 1)]
                |> List.where (fun neighbour -> pointInsideMap neighbour && notBuilding neighbour) do
            if visited.Contains neighbour then ()
            else
                let (neighbourX, neighbourY) = neighbour
                let currentG = gs.[(currentX, currentY)]
                let g =  currentG + wage (neighbour, currentG)
                let f = g + estimate neighbour
                match Seq.tryFind (fun (x, y, _) -> x = neighbourX && y = neighbourY) openSet with
                | Some found -> 
                    if gs.[neighbour] > g then
                        gs   .[neighbour] <- g
                        path .[neighbour] <- (currentX, currentY)
                        openSet .Remove found                       |> ignore
                        openSet .Add    (neighbourX, neighbourY, f) |> ignore
                    else ()
                | None ->
                    openSet .Add (neighbourX, neighbourY, f) |> ignore
                    gs      .Add (neighbour, g)
                    path    .Add (neighbour, (currentX, currentY))
    result

//TODO: Generalize to A*
let private dijstra
    (mapSize : int)
    (buildings : Set<Position>)
    (obstacles : (Position * int) list)
    (sourceX: int, sourceY: int)
    (destinations : Position list)
    : (Position * Path option) list =

    let pointInsideMap (x, y) = x >= 0 && y >= 0 && x < mapSize && y < mapSize
    //TODO: Try prepare the whole map inside array for faster access
    let notBuilding           = not << flip Set.contains buildings
    let wage ((neighbourX, neighbourY), g)        =
        match List.tryFind (fun ((x, y), _) -> neighbourX = x && neighbourY = y) obstacles with
        | Some (_, delay) ->
            max (delay - g) 0 + 1
        | None            -> 1
    //Mutable algorythm but it's a local mutation so it's a kind of ok
    //TODO: Try to find priority queue for F# to use here
    let openSet   = HashSet<(int * int * int)>()
    let visited   = HashSet<(int * int)>()
    let gs        = Dictionary<(int * int), int>()
    let path      = Dictionary<(int * int), (int * int)>()
    let result    = List<(int * int) * Option<Path>>()
    let dests     = List<(int * int)>(destinations)

    openSet .Add (sourceX, sourceY, 0)      |> ignore
    gs      .Add ((sourceX, sourceY), 0) |> ignore

    while openSet.Count <> 0 && dests.Count <> 0 do
        let (currentX, currentY, currentG) = Seq.minBy thr openSet
        if Seq.contains (currentX, currentY) dests
        then
            //All this trickery is needed only to have correct JS transpilation
            dests  .RemoveAt (dests.FindIndex(fun (x, y) -> currentX = x && currentY = y)) |> ignore
            result .Add      ((currentX, currentY), Some <| reconstruct path (currentX, currentY))
        else ()

        openSet .Remove (currentX, currentY, currentG) |> ignore
        visited .Add    (currentX, currentY) |> ignore

        for neighbour in
            [(currentX - 1, currentY);
             (currentX + 1, currentY);
             (currentX, currentY - 1);
             (currentX, currentY + 1)]
                |> List.where (fun neighbour -> pointInsideMap neighbour && notBuilding neighbour) do
            if visited.Contains neighbour then ()
            else
                let (neighbourX, neighbourY) = neighbour
                let currentG = gs.[(currentX, currentY)]
                let g = currentG + wage (neighbour, currentG)
                match Seq.tryFind (fun (x, y, _) -> x = neighbourX && y = neighbourY) openSet with
                | Some found ->
                    //TODO: try to replace dictionaries with arrays
                    if gs.[neighbour] > g then
                        gs   .[neighbour] <- g
                        path .[neighbour] <- (currentX, currentY)
                        openSet .Remove found                       |> ignore
                        openSet .Add    (neighbourX, neighbourY, g) |> ignore
                    else ()
                | None ->
                    openSet .Add (neighbourX, neighbourY, g) |> ignore
                    gs      .Add (neighbour, g)
                    path    .Add (neighbour, (currentX, currentY))

    result .AddRange (Seq.map (flip (<.) None) dests)
    List.ofSeq result

(*| Tries to find the shortest route to visit all destinations *)
let findRoute (mapSize : int) (buildings : Set<Position>) (obstacles : Set<Position * int>) (source : Position) (destinations : Set<Position>) : Path option =
    let obstacleList = Set.toList obstacles
    let rec segment current places =
        match places with
        | []          -> []
        | place :: [] ->
            (Option.defaultValue [] << Option.map List.tail) <| aStar mapSize buildings obstacleList current place
        | _           ->
            let reachable, unreachable =
                List.partition (Option.isSome << snd) <| dijstra mapSize buildings obstacleList current places
            match List.choose reflectSnd reachable with
            | []    -> []
            | paths -> //List.tail <| snd (List.minBy (List.length << snd) paths)
                let shortest = List.minBy (List.length << snd) paths
                let visited  = fst shortest :: List.map fst unreachable
                List.append (List.tail <| snd shortest) (segment (fst shortest) <| List.except visited places)
    match segment source (Set.toList destinations) with
    | []    -> None
    | route -> Some route

