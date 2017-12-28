module Calc

let add x y = x + y

let square x = x * x

let cube x = x * square x

let factorial n =
    [1 .. n]
    |> Seq.fold (*) 1