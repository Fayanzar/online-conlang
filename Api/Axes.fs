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

let getAxisRulesHandler sp classes avid : Map<int, Rule> Async =
    async {
        let rules =
            query {
                for r in ctx.Conlang.Rule do
                join cr in ctx.Conlang.ClassesRule on (r.Id = cr.Rule)
                where (r.Axis = avid && cr.Class |=| classes && r.SpeechPart = sp)
                select (r.Id, r.Rule)
            } |> Seq.map (fun (k, r) -> (k, JsonSerializer.Deserialize(r, jsonOptions))) |> Map.ofSeq
        return rules
    }

let postAxisRuleHandler sp classes avid (rule : Rule) =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let row = ctx.Conlang.Rule.Create()
        row.Axis <- avid
        row.SpeechPart <- sp
        row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
        ctx.SubmitUpdates()
        let rid = ctx.Conlang.Rule |> Seq.last

        for cl in classes do
            let clRow = ctx.Conlang.ClassesRule.Create()
            clRow.Class <- cl
            clRow.Rule <- rid.Id
            ctx.SubmitUpdates()

        let lid = query {
            for a in ctx.Conlang.AxisName do
            join av in ctx.Conlang.AxisValue on (a.Id = av.Axis)
            where (av.Id = avid)
            select a.Language
        }
        lid |> Seq.tryHead |> map updateInflectTransformations |> ignore

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

let getInflectionsStructureHandler lid =
    async {
        let inflections =
            query {
                for i in ctx.Conlang.Inflection do
                join ic in ctx.Conlang.InflectionClass on (i.Id = ic.Inflection)
                join ia in ctx.Conlang.InflectionAxes on (i.Id = ia.Inflection)
                join sp in ctx.Conlang.SpeechPart on (i.SpeechPart = sp.Name)
                join a in ctx.Conlang.AxisName on (ia.Axis = a.Id)
                where (a.Language = lid && sp.Language = lid)
                select (i.Id, i.SpeechPart, ic.Class, ia.Axis)
            }
        let groupedInflections = inflections |> Seq.groupBy fst4 |> map (
            fun (k, v) -> (k, { inflectionSpeechPart = head <| map snd4 v
                              ; inflectionClasses = map thd4 v |> Set |> Set.toList
                              ; inflectionAxes = map fth4 v |> Seq.toList
                              })
        )
        return groupedInflections |> Map.ofSeq
    }

let putInflectionHandler iid inflection =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        do!
            query {
                for ic in ctx.Conlang.InflectionClass do
                where (ic.Inflection = iid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        do!
            query {
                for ia in ctx.Conlang.InflectionAxes do
                where (ia.Inflection = iid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore

        for c in inflection.inflectionClasses do
            let classRow = ctx.Conlang.InflectionClass.Create()
            classRow.Class <- c
            classRow.Inflection <- iid
            ctx.SubmitUpdates()

        for a in inflection.inflectionAxes do
            let aRow = ctx.Conlang.InflectionAxes.Create()
            aRow.Axis <- a
            aRow.Inflection <- iid
            ctx.SubmitUpdates()

        query {
            for i in ctx.Conlang.Inflection do
            where (i.Id = iid)
        } |> Seq.iter (fun i -> i.SpeechPart <- inflection.inflectionSpeechPart)
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

let postInflectionHandler inflection =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let iRow = ctx.Conlang.Inflection.Create()
        iRow.SpeechPart <- inflection.inflectionSpeechPart
        ctx.SubmitUpdates()
        let iId = ctx.Conlang.Inflection |> Seq.last

        for a in inflection.inflectionAxes do
            let aRow = ctx.Conlang.InflectionAxes.Create()
            aRow.Axis <- a
            aRow.Inflection <- iId.Id
            ctx.SubmitUpdates()

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
                join ia in ctx.Conlang.InflectionAxes on (i.Id = ia.Inflection)
                join a in ctx.Conlang.AxisName on (ia.Axis = a.Id)
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

let getOverrideRulesHandler sp classes lid =
    async {
        let rules =
            query {
                    for ro in ctx.Conlang.RuleOverride do
                    join aro in ctx.Conlang.AxesRuleOverride on (ro.Id = aro.RuleOverride)
                    join av in ctx.Conlang.AxisValue on (aro.AxisValue = av.Id)
                    join an in ctx.Conlang.AxisName on (av.Axis = an.Id)
                    join cro in ctx.Conlang.ClassesRuleOverride on (ro.Id = cro.RuleOverride)
                    where (an.Language = lid && ro.SpeechPart = sp && cro.Class |=| classes)
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

let postOverrideRuleHandler sp classes rule =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let row = ctx.Conlang.RuleOverride.Create()
        row.Rule <- JsonSerializer.Serialize(rule.overrideRule, jsonOptions)
        row.SpeechPart <- sp
        ctx.SubmitUpdates()

        let roId = ctx.Conlang.RuleOverride |> Seq.last

        for a in rule.overrideAxes do
            let aroRow = ctx.Conlang.AxesRuleOverride.Create()
            aroRow.RuleOverride <- roId.Id
            aroRow.AxisValue <- a
            ctx.SubmitUpdates()

        for cl in classes do
            let clRow = ctx.Conlang.ClassesRuleOverride.Create()
            clRow.Class <- cl
            clRow.RuleOverride <- roId.Id
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
            join av in !!ctx.Conlang.AxisValue on (an.Id = av.Axis)
            where (an.Language = lid)
            select ((an.Id, an.Name), (av.Id, av.Name))
        }
        let groupedAxes = Seq.groupBy fst axes
        let resp = groupedAxes |> Seq.map (fun (k, v) ->
            let aId = fst k
            let aName = snd k
            let aValues = v |> Seq.toList |> map snd |> List.filter (fun (avid, _) -> avid <> 0)
            { id = aId; name = aName; values = aValues }
        )
        return resp
    }

let getInflectionsHandler lid =
    async {
        let filteredInflections = inflectTransformations[lid]
        let resp = filteredInflections |> Seq.map (fun (sp, cl, a) ->
                { language = lid; speechPart = sp; classes = cl; axes = a}
            )
        return resp
    }
