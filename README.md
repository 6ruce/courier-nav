# Courier navigation app

![Demo](https://github.com/6ruce/6ruce.github.io/blob/master/images/about/courier-screen.PNG?raw=true)

[Live demo](https://6ruce.github.io)

## Controls

 * Left mouse button  - place destination
 * Right mouse button - place obstacle

## Map file format 

 ```
 ......................................
 ###################################...
 #.......#...#.....#.........#.#..X#.#.
 #.#####.#...#.###.#.###.###.#.#...#.#.
 #.#...#.#.#.#...#.#.#...#.....#.#.#.#.
 #.#...#.###.###.#.###.#######.#.#.#.#.
 #.#.#.#.........#...#.......#...#.#.#.
 #.#.#.#############.#######.#######.#.
 #.#.#...#.........#.......#...#.....#.
 #.###...#.#######.#######.#...#.#####.
 #.....#.#.....#.#.......#.#.#.#.....#.
 #.###.#.#####.#.#######.#.###.#####.#.
 #...#.#.....#.#.......#.#.........#.#.
 ###########.#.#...###.#.#########.#.#.
 #...........#.#.#...#.#.......#.#.#.#.
 #..?#########.#.###.#.#######.#.#.#.#.
 #.#.#.........#.#...#.....#.#.#...#.#.
 #.#.#.#########.#.#######.#.#.#.###.#.
 #.#.#...#...#...#...#.....#.#.#.#...#.
 ###.###.#...#.#####.#.#####.#.###.###.
 #.......#.#.#.....#.#...#...#...#...#.
 #...#####.#.#####.#.###.#...###.###.#.
 #.#.......#.......#...#...#...#.....#.
 #.###################################.
 ......................................
 @.....................................
 ```

 `@`        - courier position
 `#`        - building
 `x` or `X` - destination position
 `?`        - obstacle

## Configuration options

```
   { Delay            = 100 // ms              -- one frame delay
   ; DefaultWaitTime  = 100 // steps/updates   -- live time (number of frames) of an obstacle
   ; CourierImage     = "images/courier.jpg"   -- courier 'skin'
   ; ConfusedImage    = "images/confused.png"  -- courier 'skin' for confused state (there are unreachable destinations)
   ; BuildingImage    = "images/building.png"  -- building 'skin'
   ; ObstacleImage    = "images/obstacle.png"  -- obstacle 'skin'
   ; DestinationImage = "images/burger.png"    -- destination 'skin'
   ; PathColor        = "yellow"               -- color for path highlighting
   ; MapFile          = "maze.txt"             -- path to the map file
```

## Main points

* We have a sort of Traveling Salesman Problem for 'find the shortest path between all destinations' requirement. Fastest but least precise way is to use NN (Nearest Neighbor) heuristic, which was used in the app
* 'Freezes' during building/obstacle placement are the result of NN search for all nodes respectively, to have shortest path visualized at once. If the whole path visualization is not needed then we can have running time not dependent on the number of destination but only on map sizes
* Using Fable/React and Html is not the most performant way for showing interface (if our goal to provide fastest response to the end user), but most demo friendly :)
* HTML rendering part could be optimized by using `canvas` instead of separate block elements, if our goal would be to display large maps in a browser

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 2.1 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)

## Building and running the app

* Install JS dependencies: `npm install`
* Start Webpack dev server: `npx webpack-dev-server` or `npm start`
* After the first compilation is finished, in your browser open: http://localhost:8080/
* To build the app in release mode, run: `npm run build`, `bundled.js` file with all needed scripts will be placed in `public/scripts` directory
