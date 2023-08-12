module OnlineConlang.Import.Morphology

open FSharpPlus
open OnlineConlang.Prelude

open SharedModels

open OnlineConlang.DB.Context
open OnlineConlang.Import.Transformations

open FSharp.Data.Sql
open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Serialization

let jsonOptions =
    JsonFSharpOptions.Default()
        .WithUnionExternalTag()
        .WithUnionNamedFields()
        .ToJsonSerializerOptions()

let infixSeparator = '·'

let performFusion w1 w2 t =
    let word = snd <| transformWord t (w1 + " " + w2)
    String.collect (fun c -> if c = ' ' then "" else string c) word

let addSuffix word suffix fusion = performFusion word suffix fusion

let addPrefix word prefix fusion = performFusion prefix word fusion

let addInfix (word : string) infix fusion1 fusion2 pos =
    let mutable splitWord = word.Split infixSeparator
    splitWord[pos] <- addSuffix splitWord[pos] infix fusion1
    splitWord[pos] <- addSuffix splitWord[pos] splitWord[pos + 1] fusion2
    splitWord[pos + 1] <- ""
    fold (+) "" <| toList splitWord

let addAffix word affix =
    match affix with
    | Prefix (p, f) -> addPrefix word p f
    | Suffix (s, f) -> addSuffix word s f
    | Infix (i, f1, f2, pos) -> addInfix word i f1 f2 pos

let inflectTransformations = new Dictionary<int * PartOfSpeech * (Class Set), Axes>()

let buildAxis aid =
    let axisName = query {
                        for an in ctx.Conlang.AxisName do
                        where (an.Id = aid)
                        select (an.Name)
                    } |> Seq.head
    let axisValues = query {
                            for av in ctx.Conlang.AxisValue do
                            where (av.Axis = aid)
                            select (av.Id)
                        } |> Seq.toList
    let axisRules = axisValues |> map (
        fun avId ->
            let rules = query {
                            for r in ctx.Conlang.Rule do
                            where (r.Axis = avId)
                            select (r.Rule)
                        } |> Seq.toList
            (avId, rules |> map (fun r -> JsonSerializer.Deserialize(r, jsonOptions)))
    )
    ({ name = axisName; inflections = Map axisRules }, axisValues)

let buildAxes (aidList : int list) =
    let (axisList, axisIds) = (map (fst << buildAxis) aidList, map (snd << buildAxis) aidList)
    let flattenedIds = List.concat axisIds
    let cartIds = cartesian axisIds
    let overrides = query {
                        for aro in ctx.Conlang.AxesRuleOverride do
                        where (aro.AxisValue |=| flattenedIds)
                        select (aro.AxisValue, aro.RuleOverride)
                    } |> Seq.groupBy (snd) |> Seq.toList |> map (fun (oId, p) -> (oId, p |> map fst |> toList))
    let filteredIds = overrides |> filter (fun o -> List.contains (snd o) cartIds)
    let overrideRules = filteredIds |> map (
        fun (rId, avIds) ->
            let rules = query {
                            for ro in ctx.Conlang.RuleOverride do
                            where (ro.Id = rId)
                            select ro.Rule
                        } |> Seq.toList |> map (fun r -> JsonSerializer.Deserialize(r, jsonOptions))
            (avIds, rules)
    )
    { axes = axisList; overrides = Map overrideRules }

let updateInflectTransformations lid =
    inflectTransformations.Clear()
    let inflections = query {
        for i in ctx.Conlang.Inflection do
        join sp in ctx.Conlang.SpeechPart on (i.SpeechPart = sp.Name)
        join ic in ctx.Conlang.InflectionClass on (i.Id = ic.Inflection)
        where (sp.Language = lid)
        select (i.Id, i.SpeechPart, ic.Class, i.Axis)
    }
    let groupedByClass = inflections |> Seq.groupBy fst4 |> map (
        fun (_, v) -> ( head <| map snd4 v
                      , map thd4 v |> Set
                      , head <| map fth4 v
                      )
    )
    let groupedInflections = groupedByClass |> Seq.groupBy (fun i -> (fst3 i, snd3 i))
    groupedInflections |> toList |> iter (
        fun (k, v) ->
            let axes = v |> map thd3 |> toList |> buildAxes
            inflectTransformations.Add((lid, fst k, snd k), axes)
    )

let rec private inflect' word rules =
    match rules with
    | [] -> word
    | x::xs ->
        match x with
        | AffixRule affix -> inflect' (addAffix word affix) xs
        | TRule t -> inflect' (transformWordNoChain t word) xs

let inflect word (axes : Axes) names = inflect' word (axes.ruleSetByNames names)
