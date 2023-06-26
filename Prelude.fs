module OnlineConlang.Prelude

// Haskell `$`
let inline (^<|) f a = f a

let fst3 (x, _, _) = x
let snd3 (_, y, _) = y
let thd3 (_, _, z) = z
