module OnlineConlang.Api.Axes

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.DB.Context
open OnlineConlang.Import.Morphology

open FSharp.Data.Sql
open System.Text.Json
open System.Transactions
open Giraffe
open Microsoft.AspNetCore.Http

let postAxisNameHandler (lid, an) =
    let row = ctx.Conlang.AxisName.Create()
    row.Language <- lid
    row.Name <- an
    try
        ctx.SubmitUpdates()
        Successful.OK ""
    with
    | e ->
        ctx.ClearUpdates() |> ignore
        internalServerError e.Message

let putAxisNameHandler (aid, an) =
    query {
        for a in ctx.Conlang.AxisName do
        where (a.Id = aid)
    } |> Seq.iter (fun a -> a.Name <- an)
    Successful.OK ""

let deleteAxisNameHandler anid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for an in ctx.Conlang.AxisName do
                where (an.Id = anid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! Successful.OK "" next hctx
        }

let postAxisValueHandler (aid, av) =
    let row = ctx.Conlang.AxisValue.Create()
    row.Axis <- aid
    row.Name <- av
    try
        ctx.SubmitUpdates()
        Successful.OK ""
    with
    | e ->
        ctx.ClearUpdates() |> ignore
        internalServerError e.Message

let putAxisValueHandler (avid, av) =
    query {
        for a in ctx.Conlang.AxisValue do
        where (a.Id = avid)
    } |> Seq.iter (fun a -> a.Name <- av)
    Successful.OK ""

let deleteAxisValueHandler avid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for a in ctx.Conlang.AxisValue do
                where (a.Id = avid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! Successful.OK "" next hctx
        }

let postAxisRuleHandler avid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<Rule>()
            let row = ctx.Conlang.Rule.Create()
            row.Axis <- avid
            row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
            try
                ctx.SubmitUpdates()
                return! Successful.OK "" next hctx
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                return! (internalServerError e.Message) next hctx
        }

let putAxisRuleHandler rid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<Rule>()
            query {
                for r in ctx.Conlang.Rule do
                where (r.Id = rid)
            } |> Seq.iter (fun r -> r.Rule <- JsonSerializer.Serialize(rule, jsonOptions))
            return! Successful.OK "" next hctx
        }

let deleteAxisRuleHandler rid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for r in ctx.Conlang.Rule do
                where (r.Id = rid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! Successful.OK "" next hctx
        }

type Inflection =
    {
        inflectionClasses    : Class list
        inflectionSpeechPart : PartOfSpeech
        inflectionAxes       : int list
    }

type OverrideRule =
    {
        overrideRule : Rule
        overrideAxes : int list
    }

let postInflectionHandler =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! inflection = hctx.BindJsonAsync<Inflection>()
            use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
            for a in inflection.inflectionAxes do
                let row = ctx.Conlang.Inflection.Create()
                row.Axis <- a
                row.SpeechPart <- inflection.inflectionSpeechPart
                ctx.SubmitUpdates()

                let iId = ctx.Conlang.Inflection |> Seq.last

                for c in inflection.inflectionClasses do
                    let classRow = ctx.Conlang.InflectionClass.Create()
                    classRow.Class <- c
                    classRow.Inflection <- iId.Id
                    ctx.SubmitUpdates()

            transaction.Complete()
            return! Successful.OK "" next hctx
        }

let deleteInflectionHandler iid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for i in ctx.Conlang.Inflection do
                where (i.Id = iid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! Successful.OK "" next hctx
        }

let postOverrideRuleHandler =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<OverrideRule>()
            use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
            let row = ctx.Conlang.RuleOverride.Create()
            row.Rule <- JsonSerializer.Serialize(rule.overrideRule, jsonOptions)
            ctx.SubmitUpdates()

            let roId = ctx.Conlang.RuleOverride |> Seq.last

            for a in rule.overrideAxes do
                let aroRow = ctx.Conlang.AxesRuleOverride.Create()
                aroRow.RuleOverride <- roId.Id
                aroRow.AxisValue <- a
                ctx.SubmitUpdates()

            transaction.Complete()
            return! Successful.OK "" next hctx
        }

let putOverrideRuleHandler rid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<OverrideRule>()
            use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
            query {
                for oa in ctx.Conlang.AxesRuleOverride do
                where (oa.RuleOverride = rid)
            } |> Seq.``delete all items from single table`` |> ignore
            query {
                for r in ctx.Conlang.RuleOverride do
                where (r.Id = rid)
            } |> Seq.iter (fun r -> r.Rule <- JsonSerializer.Serialize(rule.overrideRule, jsonOptions))

            for a in rule.overrideAxes do
                let aroRow = ctx.Conlang.AxesRuleOverride.Create()
                aroRow.RuleOverride <- rid
                aroRow.AxisValue <- a
                ctx.SubmitUpdates()

            transaction.Complete()
            return! Successful.OK "" next hctx
        }

let deleteOverrideRuleHandler rid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for r in ctx.Conlang.AxesRuleOverride do
                where (r.Id = rid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! Successful.OK "" next hctx
        }

type AxisForAPI =
    {
        id     : int
        name   : string
        values : (int * string) list
    }

type InflectTForAPI =
    {
        language   : int
        speechPart : PartOfSpeech
        classes    : Class Set
        axes       : Axes
    }

let getAxesHandler lid =
    let axes = query {
        for an in ctx.Conlang.AxisName do
        join av in ctx.Conlang.AxisValue on (an.Id = av.Axis)
        where (an.Language = lid)
        select ((an.Id, an.Name), (av.Id, av.Name))
    }
    let groupedAxes = Seq.groupBy fst axes
    let resp = groupedAxes |> Seq.toList |> map (fun (k, v) ->
        let aId = fst k
        let aName = snd k
        let aValues = v |> Seq.toList |> map snd
        { id = aId; name = aName; values = aValues }
    )
    json resp

let getInflectionsHandler lid =
    let filteredInflections = inflectTransformations |> Seq.filter
                                (fun (KeyValue((l, _, _), _)) ->
                                    l = lid
                                )
    let resp = filteredInflections |> Seq.map (fun (KeyValue((l, sp, cl), a)) ->
            { language = l; speechPart = sp; classes = cl; axes = a}
        )
    json (toList resp)
