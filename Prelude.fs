module OnlineConlang.Prelude

open Giraffe

type ErrorResponse =
    {
        errorCode : int
        message   : string
    }
// Haskell `$`
let inline (^<|) f a = f a

let fst3 (x, _, _) = x
let snd3 (_, y, _) = y
let thd3 (_, _, z) = z

let badRequest400 msg =
    RequestErrors.BAD_REQUEST { errorCode = 400; message = msg}

let rec cartesian lstlst =
    match lstlst with
    | h::[] ->
        List.fold (fun acc elem -> [elem]::acc) [] h
    | h::t ->
        List.fold (fun cacc celem ->
            (List.fold (fun acc elem -> (elem::celem)::acc) [] h) @ cacc
            ) [] (cartesian t)
    | _ -> []
