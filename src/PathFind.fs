module PathFind

open Common

open System.Collections.Generic

let private reconstruct (path : Dictionary<(int * int), (int * int)>) (destination : Position) : Path =
    let rec reconstruct' acc dest =
        let found, next = path.TryGetValue dest
        if found then reconstruct' (next :: acc) next else acc
    reconstruct' [destination] destination

let private dijstra
    (mapSize : int)
    (buildings : int[][])
    (obstacles : int[][])
    (sourceX: int, sourceY: int)
    (destinationsMap : int[][])
    (destinations : Position list)
    : (Position * Path option) list =

    let pointInsideMap (x, y) = x >= 0 && y >= 0 && x < mapSize && y < mapSize
    let notBuilding (x, y)          = buildings.[x].[y] = -1
    let wage ((neighbourX, neighbourY), g)        =
        match obstacles.[neighbourX].[neighbourY] with
        | -1    -> 1
        | delay -> max (delay - g) 0 + 1
    //Mutable algorythm but it's a local mutation so it's a kind of ok
    let openSet   = HashSet<(int * int) * int>()
    let visited   = squareArray mapSize -1
    let gs        = squareArray mapSize -1
    let path      = Dictionary<(int * int), (int * int)>()
    let result    = List<(int * int) * Option<Path>>()
    let dests     = List<(int * int)>(destinations)

    openSet .Add ((sourceX, sourceY), 0) |> ignore
    gs      .[sourceX].[sourceY] <- 0

    while openSet.Count <> 0 && dests.Count <> 0 do
        let ((currentX, currentY), currentG) = Seq.minBy snd openSet
        if destinationsMap.[currentX].[currentY] <> -1 
        then
            //All this trickery is needed only to have correct JS transpilation
            dests  .RemoveAt (dests.FindIndex(fun (x, y) -> currentX = x && currentY = y)) |> ignore
            result .Add      ((currentX, currentY), Some <| reconstruct path (currentX, currentY))
        else ()

        openSet .Remove ((currentX, currentY), currentG) |> ignore
        visited .[currentX].[currentY] <- 1

        for neighbour in
            [(currentX - 1, currentY);
             (currentX + 1, currentY);
             (currentX, currentY - 1);
             (currentX, currentY + 1)]
                |> List.where (fun neighbour -> pointInsideMap neighbour && notBuilding neighbour) do
            let (neighbourX, neighbourY) = neighbour
            if visited.[neighbourX].[neighbourY] <> -1 then ()
            else
                let currentG = gs.[currentX].[currentY]
                let g = currentG + wage (neighbour, currentG)
                match Seq.tryFind (fst >> ((=) neighbour)) openSet with
                | Some found ->
                    let neighbourG = gs.[neighbourX].[neighbourY]
                    if neighbourG > g then
                        gs   .[neighbourX].[neighbourY] <- g
                        path .[neighbour] <- (currentX, currentY)
                        openSet .Remove found                       |> ignore
                        openSet .Add    (neighbour, g) |> ignore
                    else ()
                | None ->
                    openSet .Add (neighbour, g) |> ignore
                    path    .Add (neighbour, (currentX, currentY))
                    gs      .[neighbourX].[neighbourY] <- g

    result .AddRange (Seq.map (flip (<.) None) dests)
    List.ofSeq result

(*| Tries to find the shortest route to visit all destinations *)
let findRoute
    (mapSize      : int)
    (buildings    : Set<Position>)
    (obstacles    : Set<Position * int>)
    (source       : Position)
    (destinations : Set<Position>)
    : Path option =
    let buildingsArr, obstaclesArr, destinationsArr =
        squareArray mapSize -1, squareArray mapSize -1, squareArray mapSize -1
    Set.iter (fun (x, y)      -> buildingsArr.[x].[y]    <- 1) buildings
    Set.iter (fun ((x, y), d) -> obstaclesArr.[x].[y]    <- d) obstacles
    Set.iter (fun (x, y)      -> destinationsArr.[x].[y] <- 1) destinations
    let rec segment current places =
        match places with
        | []          -> []
        | _           ->
            let reachable, unreachable =
                List.partition (Option.isSome << snd) <| dijstra mapSize buildingsArr obstaclesArr current destinationsArr places
            match List.choose reflectSnd reachable with
            | []    -> []
            | paths ->
                let shortest = List.minBy (List.length << snd) paths
                let visited  = fst shortest :: List.map fst unreachable
                let (x, y)   = fst shortest
                // *** Dangerous mutation ***
                destinationsArr.[x].[y] <- -1
                List.append (List.tail <| snd shortest) (segment (fst shortest) <| List.except visited places)
    match segment source (Set.toList destinations) with
    | []    -> None
    | route -> Some route

