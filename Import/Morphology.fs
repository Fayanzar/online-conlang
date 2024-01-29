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

let performFusion (w1 : string) (w2 : string) t =
    let word = snd <| transformWord t (w1 + "$" + w2)
    String.collect (fun c -> if c = '$' then "" else string c) word

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

let inflectTransformations = new Dictionary<int * int, string Option * PartOfSpeech * (Class Set) * Axes>()

let buildAxis iid aid =
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
            let rules =
                query {
                    for r in ctx.Conlang.Rule do
                    where (r.Axis = avId && r.Inflection = iid)
                    select (r.Id, r.Rule)
                } |> Seq.toList |> List.distinctBy fst |> map snd
            (avId, rules |> map (fun r -> JsonSerializer.Deserialize(r, jsonOptions)))
    )
    ({ name = axisName; inflections = Map axisRules }, axisValues)

let buildAxes iid (aidList : int list) =
    let axisList, axisIds = (map (fst << buildAxis iid) aidList,
                             map (snd << buildAxis iid) aidList)
    let flattenedIds = List.concat axisIds
    let cartIds = cartesian axisIds |> List.map Set
    let overrides = query {
                        for aro in ctx.Conlang.AxesRuleOverride do
                        where (aro.AxisValue |=| flattenedIds)
                        select (aro.AxisValue, aro.RuleOverride)
                    } |> Seq.groupBy (snd) |> Seq.toList |> map (fun (oId, p) -> (oId, p |> map fst |> toList))
    let filteredIds = overrides |> filter (fun o -> List.contains (Set <| snd o) cartIds)
    let overrideRules = filteredIds |> map (
        fun (rId, avIds) ->
            let (orules : Rule list) =
                query {
                    for ro in ctx.Conlang.RuleOverride do
                    where (ro.Id = rId && ro.Inflection = iid)
                    select (ro.Id, ro.Rule)
                } |> Seq.toList |> List.distinctBy fst |> map snd
                  |> map (fun r -> JsonSerializer.Deserialize(r, jsonOptions))
            match orules with
            | [] -> None
            | rules -> Some (avIds, rules)
    )
    { axes = axisList; overrides = overrideRules |> List.choose id }

let updateInflectTransformations lid =
    inflectTransformations.Clear()
    let inflections = query {
        for i in ctx.Conlang.Inflection do
        join sp in ctx.Conlang.SpeechPart on (i.SpeechPart = sp.Name)
        join ic in ctx.Conlang.InflectionClass on (i.Id = ic.Inflection)
        join ia in ctx.Conlang.InflectionAxes on (i.Id = ia.Inflection)
        join a in ctx.Conlang.AxisName on (ia.Axis = a.Id)
        where (a.Language = lid && sp.Language = lid)
        select (i.Id, i.Name, i.SpeechPart, ic.Class, ia.Axis)
    }
    let groupedInflections = inflections |> Seq.groupBy fst5 |> map (
        fun (k, v) -> ( k
                      , head <| map snd5 v
                      , head <| map thd5 v
                      , map fth5 v |> Set
                      , map ffh5 v |> Seq.toList |> List.distinct
                      )
    )
    groupedInflections |> toList |> iter (fun (k, name, sp, classes, axes) ->
        inflectTransformations.Add((lid, k), (name, sp, classes, buildAxes k axes))
    )

let rec private inflect' word rules =
    match rules with
    | [] -> word
    | x::xs ->
        match x with
        | AffixRule affix -> inflect' (addAffix word affix) xs
        | TRule t -> inflect' (transformWordNoChain t word) xs

let inflect word (axes : Axes) names = inflect' word (axes.ruleSetByNames names)
