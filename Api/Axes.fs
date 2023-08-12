module OnlineConlang.Api.Axes

open FSharpPlus

open SharedModels

open OnlineConlang.DB.Context
open OnlineConlang.Import.Morphology

open FSharp.Data.Sql
open System.Text.Json
open System.Transactions

let postAxisNameHandler lid an =
    async {
        let row = ctx.Conlang.AxisName.Create()
        row.Language <- lid
        row.Name <- an
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let putAxisNameHandler aid an =
    async {
        query {
            for a in ctx.Conlang.AxisName do
            where (a.Id = aid)
        } |> Seq.iter (fun a -> a.Name <- an)
    }

let deleteAxisNameHandler anid =
    async {
        query {
            for an in ctx.Conlang.AxisName do
            where (an.Id = anid)
        } |> Seq.``delete all items from single table`` |> Async.AwaitTask |> ignore
    }

let postAxisValueHandler aid av =
    async {
        let row = ctx.Conlang.AxisValue.Create()
        row.Axis <- aid
        row.Name <- av
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let putAxisValueHandler avid av =
    async {
        query {
            for a in ctx.Conlang.AxisValue do
            where (a.Id = avid)
        } |> Seq.iter (fun a -> a.Name <- av)
    }

let deleteAxisValueHandler avid =
    async {
        query {
            for a in ctx.Conlang.AxisValue do
            where (a.Id = avid)
        } |> Seq.``delete all items from single table`` |> Async.AwaitTask |> ignore
    }

let postAxisRuleHandler avid (rule : Rule) =
    async {
        let row = ctx.Conlang.Rule.Create()
        row.Axis <- avid
        row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let putAxisRuleHandler rid (rule : Rule) =
    async {
        query {
            for r in ctx.Conlang.Rule do
            where (r.Id = rid)
        } |> Seq.iter (fun r -> r.Rule <- JsonSerializer.Serialize(rule, jsonOptions))
    }

let deleteAxisRuleHandler rid =
    async {
        query {
            for r in ctx.Conlang.Rule do
            where (r.Id = rid)
        } |> Seq.``delete all items from single table`` |> Async.AwaitTask |> ignore
    }

let postInflectionHandler inflection =
    async {
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
    }

let deleteInflectionHandler iid =
    async {
        query {
            for i in ctx.Conlang.Inflection do
            where (i.Id = iid)
        } |> Seq.``delete all items from single table`` |> Async.AwaitTask |> ignore
    }

let postOverrideRuleHandler rule =
    async {
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
    }

let putOverrideRuleHandler rid rule =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        query {
            for oa in ctx.Conlang.AxesRuleOverride do
            where (oa.RuleOverride = rid)
        } |> Seq.``delete all items from single table`` |> Async.AwaitTask |> ignore
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
    }

let deleteOverrideRuleHandler rid =
    async {
        query {
            for r in ctx.Conlang.AxesRuleOverride do
            where (r.Id = rid)
        } |> Seq.``delete all items from single table`` |> Async.AwaitTask |> ignore
    }

let getAxesHandler lid =
    async {
        let axes = query {
            for an in ctx.Conlang.AxisName do
            join av in ctx.Conlang.AxisValue on (an.Id = av.Axis)
            where (an.Language = lid)
            select ((an.Id, an.Name), (av.Id, av.Name))
        }
        let groupedAxes = Seq.groupBy fst axes
        let resp = groupedAxes |> Seq.map (fun (k, v) ->
            let aId = fst k
            let aName = snd k
            let aValues = v |> Seq.toList |> map snd
            { id = aId; name = aName; values = aValues }
        )
        return resp
    }

let getInflectionsHandler lid =
    async {
        let filteredInflections = inflectTransformations |> Seq.filter
                                    (fun (KeyValue((l, _, _), _)) ->
                                        l = lid
                                    )
        let resp = filteredInflections |> Seq.map (fun (KeyValue((l, sp, cl), a)) ->
                { language = l; speechPart = sp; classes = cl; axes = a}
            )
        return resp
    }
