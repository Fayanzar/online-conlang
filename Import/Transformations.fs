module OnlineConlang.Import.Transformations

open FSharpPlus

open SharedModels

open System.Text.RegularExpressions

type TransformationWithInjection =
    {
        transformation : Transformation
        injection      : Map<int * int, int * int>
    }
    with
    member this.RevInjection =
        Map.fold (fun m key value -> Map.add value key m) Map.empty this.injection

type TChain =
    | Forward of (string * TransformationWithInjection) list * string
    | Reverse of string * (TransformationWithInjection * string) list
    with
    member this.Rev =
        match this with
        | Forward (l, s) -> Reverse (s, rev <| map (fun (x, y) -> (y, x)) l)
        | Reverse (s, l) -> Forward (rev <| map (fun (x, y) -> (y, x)) l, s)

let mutable transformations : Transformation list = []

let rec private mkInjection (word : string) (matches : Match list) (output : string) k l acc  =
    match toList word, matches with
    | _::xs, y::ys ->
        if y.Index <> k then mkInjection (string xs)
                                         matches
                                         output
                                         (k + 1)
                                         (l + 1)
                                         (acc @ [((k, k), (l, l))])
                        else mkInjection (string xs)
                                         ys output
                                         (k + y.Length)
                                         (l + length output)
                                         (acc @ [((k, k + y.Length - 1), (l, l + length output - 1))])
    | _, _         -> Map.ofList acc

let transformWord transformation word =
    let matches = Regex.Matches(word, transformation.input) |> Seq.cast |> toList
    let injection = mkInjection word matches transformation.output 0 0 []
    let tWithI = { transformation = transformation; injection = injection}
    ((word, tWithI), Regex.Replace(word, transformation.input, transformation.output))

let transformWordNoChain transformation word =
    Regex.Replace(word, transformation.input, transformation.output)

let rec private mkTChain' word transformations acc =
    match transformations with
    | x::xs ->
        let wordTransformed = transformWord x word
        let newWord = snd wordTransformed
        if newWord = word then
            mkTChain' word xs acc
        else
            if x.applyMultiple then
                mkTChain' newWord (xs @ [x]) (acc @ [fst wordTransformed])
            else
                mkTChain' newWord xs (acc @ [fst wordTransformed])
    | [] -> Forward (acc, word)

let mkTChain word transformations = mkTChain' word transformations []

// a -> b
let transformAToB a b = { input = a; output = b; applyMultiple = false }

// ba -> '(?<=b)a' -> bc
let transformAAfterBToC a b c =
    { input = $"(?<={b}){a}"; output = c; applyMultiple = false }

// ab -> 'a(?=b)' -> cb
let transformABeforeBToC a b c =
    { input = $"{a}(?={b})"; output = c; applyMultiple = false }

// bac -> '(?<=b)a(?=c)' -> bdc
let transformABetweenBAndCToD a b c d =
    { input = $"(?<={b}){a}(?={c})"; output = d; applyMultiple = false }
