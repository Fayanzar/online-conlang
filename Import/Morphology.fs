module OnlineConlang.Import.Morphology

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.Import.Transformations

let infixSeparator = '·'

let performFusion w1 w2 t =
    snd <| transformWord t (w1 + " " + w2)

let addSuffix word suffix fusion = performFusion word suffix fusion

let addPrefix word prefix fusion = performFusion prefix word fusion

let addInfix (word : string) infix fusion1 fusion2 pos =
    let mutable splitWord = word.Split infixSeparator
    splitWord[pos] <- addSuffix splitWord[pos] infix fusion1
    splitWord[pos] <- addSuffix splitWord[pos] splitWord[pos + 1] fusion2
    splitWord[pos + 1] <- ""
    fold (+) "" <| toList splitWord

type Affix =
    | Prefix of string * Transformation
    | Suffix of string * Transformation
    | Infix of string * Transformation * Transformation * int

let addAffix word affix =
    match affix with
    | Prefix (p, f) -> addPrefix word p f
    | Suffix (s, f) -> addSuffix word s f
    | Infix (i, f1, f2, pos) -> addInfix word i f1 f2 pos

type Rule =
    | AffixRule of Affix
    | TRule of Transformation

type RuleSet = Rule list

type PartOfSpeech = string

type Class = string

type Axis =
    {
        name         : string
        partOfSpeech : PartOfSpeech
        inflections  : Map<string, RuleSet>
    }
    with
    member this.InflectionNames = this.inflections.Keys |> toList
    member this.InflectionRules = this.inflections.Values |> toList

type Axes =
    {
        axes      : Axis list
        overrides : Map<string list, RuleSet>
    }
    with
    member this.axesNumber = length this.axes
    member this.ruleSetByNames names =
        match Map.tryFind names this.overrides with
        | None ->
            let inflections = map (fun a -> a.inflections) this.axes
            let ruleSetList = List.map2 (fun name i -> Map.find name i) names inflections
            List.concat ruleSetList
        | Some ruleSet -> ruleSet

let rec private inflect' word rules =
    match rules with
    | [] -> word
    | x::xs ->
        match x with
        | AffixRule affix -> inflect' (addAffix word affix) xs
        | TRule t -> inflect' (transformWordNoChain t word) xs

let inflect word (axes : Axes) names = inflect' word (axes.ruleSetByNames names)
