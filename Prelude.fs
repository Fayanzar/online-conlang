module OnlineConlang.Prelude

open Giraffe
open System.Text.Json.Serialization

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

let fst4 (x, _, _, _) = x
let snd4 (_, y, _, _) = y
let thd4 (_, _, z, _) = z
let fth4 (_, _, _, w) = w

let fst5 (x, _, _, _, _) = x
let snd5 (_, y, _, _, _) = y
let thd5 (_, _, z, _, _) = z
let fth5 (_, _, _, u, _) = u
let ffh5 (_, _, _, _, v) = v

let badRequest400 msg =
    RequestErrors.BAD_REQUEST { errorCode = 400; message = msg }

let internalServerError msg =
    ServerErrors.INTERNAL_ERROR { errorCode = 500; message = msg }

let rec cartesian lstlst =
    match lstlst with
    | h::[] ->
        List.fold (fun acc elem -> [elem]::acc) [] h
    | h::t ->
        List.fold (fun cacc celem ->
            (List.fold (fun acc elem -> (elem::celem)::acc) [] h) @ cacc
            ) [] (cartesian t)
    | _ -> []

let jsonOptions =
    JsonFSharpOptions.Default()
        .WithUnionExternalTag()
        .WithUnionNamedFields()
        .ToJsonSerializerOptions()

type Either<'a, 'b> = Choice<'a, 'b>
let Left x : Either<'a, 'b> = Choice1Of2 x
let Right x : Either<'a, 'b> = Choice2Of2 x
let (|Left|Right|) = function Choice1Of2 x -> Left x | Choice2Of2 x -> Right x
