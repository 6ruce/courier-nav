module Common

//TODO: Find better place for these types
type Position          = (int * int)
type Path              = Position list

// Little `hack` for infix functions
type System.Int32 with
    member maybeZero.ifZeroThen otherwise    = if maybeZero <> 0 then maybeZero else otherwise
    member maybeZero.ifNotZeroThen otherwise = if maybeZero <> 0 then otherwise else maybeZero

//TODO: Little bit funky, there should be some better way to calculate this
let denormalize (mapSize : int) (i : int) =
(* X *) ((i % mapSize) .ifZeroThen mapSize) - 1,
(* Y *) mapSize - (i / mapSize - 1 + (i % mapSize) .ifNotZeroThen 1)  - 1

let squareArray size def = Array.init size (fun _ -> Array.create size def)

let range start stop gen =
    if start > stop
    then [for i in start .. - 1 .. stop -> gen i]
    else [for i in start .. stop -> gen i]

let flip f a b = f b a

let constant c _ = c

// F# tuple operator is not a function for some reason, so we'll make one :)
let inline (<.) a b = (a, b)

let thr (_, _, c) = c

let reflectSnd (a, bopt) =
    match bopt with
    | Some b -> Some (a, b)
    | None   -> None
