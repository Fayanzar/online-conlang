module OnlineConlang.Api.Axes

open FSharpPlus

open SharedModels

open OnlineConlang.Prelude

open OnlineConlang.DB.Context
open OnlineConlang.Import.Morphology

open FSharp.Data.Sql
open Microsoft.Extensions.Logging
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
        try
            ctx.SubmitUpdates()
            let lid = query {
                    for a in ctx.Conlang.AxisName do
                    where (a.Id = aid)
                    select a.Language
            }
            lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let deleteAxisNameHandler aid =
    async {
        let! lid =
            query {
                for a in ctx.Conlang.AxisName do
                where (a.Id = aid)
                select a.Language
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        do!
            query {
                for an in ctx.Conlang.AxisName do
                where (an.Id = aid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
    }

let postAxisValueHandler aid av =
    async {
        let row = ctx.Conlang.AxisValue.Create()
        row.Axis <- aid
        row.Name <- av
        try
            ctx.SubmitUpdates()
            let lid = query {
                for a in ctx.Conlang.AxisName do
                where (a.Id = aid)
                select a.Language
            }
            lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
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
        try
            ctx.SubmitUpdates()
            let lid = query {
                for av in ctx.Conlang.AxisValue do
                join a in ctx.Conlang.AxisName on (av.Axis = a.Id)
                where (av.Id = avid)
                select a.Language
            }
            lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let deleteAxisValueHandler avid =
    async {
        let! lid =
            query {
                for av in ctx.Conlang.AxisValue do
                join a in ctx.Conlang.AxisName on (av.Axis = a.Id)
                where (av.Id = avid)
                select a.Language
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        do!
            query {
                for a in ctx.Conlang.AxisValue do
                where (a.Id = avid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
    }

let getAxisRulesHandler avid : Map<int, Rule> Async =
    async {
        let rules =
            query {
                for r in ctx.Conlang.Rule do
                where (r.Axis = avid)
                select (r.Id, r.Rule)
            } |> Seq.map (fun (k, r) -> (k, JsonSerializer.Deserialize(r, jsonOptions))) |> Map.ofSeq
        return rules
    }

let postAxisRuleHandler avid (rule : Rule) =
    async {
        printfn $"rule: {rule}"
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let row = ctx.Conlang.Rule.Create()
        row.Axis <- avid
        row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
        try
            ctx.SubmitUpdates()
            let lid = query {
                for a in ctx.Conlang.AxisName do
                join av in ctx.Conlang.AxisValue on (a.Id = av.Axis)
                where (av.Id = avid)
                select a.Language
            }
            lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
        transaction.Complete()
    }

let putAxisRuleHandler (_ : ILogger) rid (rule : Rule) =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        query {
            for r in ctx.Conlang.Rule do
            where (r.Id = rid)
        } |> Seq.iter (fun r -> r.Rule <- JsonSerializer.Serialize(rule, jsonOptions))
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
        let lid = query {
            for a in ctx.Conlang.AxisName do
            join av in ctx.Conlang.AxisValue on (a.Id = av.Axis)
            join r in ctx.Conlang.Rule on (av.Id = r.Axis)
            where (r.Id = rid)
            select a.Language
        }
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
        transaction.Complete()
    }

let deleteAxisRuleHandler rid =
    async {
        let! lid =
            query {
                for a in ctx.Conlang.AxisName do
                join av in ctx.Conlang.AxisValue on (a.Id = av.Axis)
                join r in ctx.Conlang.Rule on (av.Id = r.Axis)
                where (r.Id = rid)
                select a.Language
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        do!
            query {
                for r in ctx.Conlang.Rule do
                where (r.Id = rid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
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

        match inflection.inflectionAxes with
        | [] -> ()
        | aid::_ ->
            let lid = query {
                for a in ctx.Conlang.AxisName do
                where (a.Id = aid)
                select a.Language
            }
            lid |> Seq.tryHead |> map updateInflectTransformations |> ignore

        transaction.Complete()
    }

let deleteInflectionHandler iid =
    async {
        let! lid =
            query {
                for i in ctx.Conlang.Inflection do
                join a in ctx.Conlang.AxisName on (i.Axis = a.Id)
                select a.Language
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        do!
            query {
                for i in ctx.Conlang.Inflection do
                where (i.Id = iid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
    }

let getOverrideRulesHandler lid =
    async {
        let rules =
            query {
                    for ro in ctx.Conlang.RuleOverride do
                    join aro in ctx.Conlang.AxesRuleOverride on (ro.Id = aro.RuleOverride)
                    join av in ctx.Conlang.AxisValue on (aro.AxisValue = av.Id)
                    join an in ctx.Conlang.AxisName on (av.Axis = an.Id)
                    where (an.Language = lid)
                    select (ro.Id, ro.Rule, av.Id)
            } |> Seq.toList
        let groupedRules = rules |> List.groupBy (fun (rid, r, _) -> (rid, r))
                                 |> map (fun ((rid, r), l) ->
            let oRule =
                { overrideRule = JsonSerializer.Deserialize(r, jsonOptions);
                overrideAxes = map thd3 l
                }
            (rid, oRule))
        return groupedRules |> Map.ofList
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

        let avid = List.head rule.overrideAxes
        let lid = query {
            for a in ctx.Conlang.AxisName do
            join av in ctx.Conlang.AxisValue on (a.Id = av.Axis)
            where (av.Id = avid)
            select a.Language
        }
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
        transaction.Complete()
    }

let putOverrideRuleHandler (logger : ILogger) rid rule =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        do!
            query {
                for oa in ctx.Conlang.AxesRuleOverride do
                where (oa.RuleOverride = rid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        query {
            for r in ctx.Conlang.RuleOverride do
            where (r.Id = rid)
        } |> Seq.iter (fun r -> r.Rule <- JsonSerializer.Serialize(rule.overrideRule, jsonOptions))
        ctx.SubmitUpdates()

        for a in rule.overrideAxes do
            let aroRow = ctx.Conlang.AxesRuleOverride.Create()
            aroRow.RuleOverride <- rid
            aroRow.AxisValue <- a
            ctx.SubmitUpdates()

        match List.tryHead rule.overrideAxes with
        | None -> ()
        | Some avid ->
            let lid = query {
                for a in ctx.Conlang.AxisName do
                join av in ctx.Conlang.AxisValue on (a.Id = av.Axis)
                where (av.Id = avid)
                select a.Language
            }
            lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
        transaction.Complete()
    }

let deleteOverrideRuleHandler (logger : ILogger) rid =
    async {
        let! lid =
            query {
                for ro in ctx.Conlang.RuleOverride do
                join aro in ctx.Conlang.AxesRuleOverride on (ro.Id = aro.RuleOverride)
                join av in ctx.Conlang.AxisValue on (aro.AxisValue = av.Id)
                join a in ctx.Conlang.AxisName on (av.Axis = a.Id)
                select a.Language
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        do!
            query {
                for r in ctx.Conlang.RuleOverride do
                where (r.Id = rid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore
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
