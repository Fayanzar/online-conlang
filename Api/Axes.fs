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

let postAxisNameHandler (logger : ILogger) lid an =
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

let putAxisNameHandler (logger : ILogger) aid an =
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

let deleteAxisNameHandler (logger : ILogger) aid =
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

let postAxisValueHandler (logger : ILogger) aid av =
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

let putAxisValueHandler (logger : ILogger) avid av =
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

let deleteAxisValueHandler (logger : ILogger) avid =
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

let getAxisRulesHandler (logger : ILogger) i avid : Map<int, Rule> Async =
    async {
        let rules =
            query {
                for r in ctx.Conlang.Rule do
                where (r.Axis = avid && r.Inflection = i)
                select (r.Id, r.Rule)
            } |> Seq.map (fun (k, r) -> (k, JsonSerializer.Deserialize(r, jsonOptions))) |> Map.ofSeq
        return rules
    }

let postAxisRulesHandler (logger : ILogger) i avid (rules : Rule list) =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        for rule in rules do
            let row = ctx.Conlang.Rule.Create()
            row.Axis <- avid
            row.Inflection <- i
            row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
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

let postAxisRuleHandler (logger : ILogger) i avid (rule : Rule) =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        do!
            query {
                for r in ctx.Conlang.Rule do
                where (r.Axis = avid && r.Inflection = i)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        let row = ctx.Conlang.Rule.Create()
        row.Axis <- avid
        row.Inflection <- i
        row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
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

let deleteAxisRuleHandler (logger : ILogger) rid =
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

let getInflectionsStructureHandler (logger : ILogger) lid =
    async {
        let inflections =
            query {
                for i in ctx.Conlang.Inflection do
                join ic in ctx.Conlang.InflectionClass on (i.Id = ic.Inflection)
                join ia in ctx.Conlang.InflectionAxes on (i.Id = ia.Inflection)
                join sp in ctx.Conlang.SpeechPart on (i.SpeechPart = sp.Name)
                join a in ctx.Conlang.AxisName on (ia.Axis = a.Id)
                where (a.Language = lid && sp.Language = lid)
                select (i.Id, i.Name, i.SpeechPart, ic.Class, ia.Axis)
            }
        let groupedInflections = inflections |> Seq.groupBy fst5 |> map (
            fun (k, v) -> (k, { inflectionName = head <| map snd5 v
                              ; inflectionSpeechPart = head <| map thd5 v
                              ; inflectionClasses = map fth5 v |> Set |> Set.toList
                              ; inflectionAxes = map ffh5 v |> Seq.toList |> List.distinct
                              })
        )
        return groupedInflections |> Map.ofSeq
    }

let rewriteInflectionRules (logger : ILogger) iid inflection =
    async {
        logger.LogInformation("Getting speech part")

        logger.LogInformation("Getting rules to delete")
        let rulesToDelete =
            query {
                for ia in ctx.Conlang.InflectionAxes do
                join a in ctx.Conlang.AxisName on (ia.Axis = a.Id)
                join av in ctx.Conlang.AxisValue on (a.Id = av.Axis)
                join r in ctx.Conlang.Rule on (av.Id = r.Axis)
                where (ia.Inflection = iid
                        && ia.Axis |<>| inflection.inflectionAxes
                        && r.Inflection = iid)
                select (r.Id)
            } |> Seq.toList |> List.distinct
        logger.LogInformation($"Deleting rules %A{rulesToDelete}")
        do!
            query {
                for r in ctx.Conlang.Rule do
                where (r.Id |=| rulesToDelete)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore

        logger.LogInformation("Getting axes")
        let axes =
            query {
                for ia in ctx.Conlang.InflectionAxes do
                where (ia.Inflection = iid)
                select (ia.Axis)
            } |> Seq.toList |> List.distinct

        logger.LogInformation($"Getting override rules for axes %A{axes}")
        let overrideRules =
            query {
                for aro in ctx.Conlang.AxesRuleOverride do
                join av in ctx.Conlang.AxisValue on (aro.AxisValue = av.Id)
                join a in ctx.Conlang.AxisName on (av.Axis = a.Id)
                join ro in ctx.Conlang.RuleOverride on (aro.RuleOverride = ro.Id)
                where (a.Id |=| axes && ro.Inflection = iid)
                select (ro.Id)
            } |> Seq.toList |> List.distinct

        if Set axes <> Set inflection.inflectionAxes then
            logger.LogInformation("Deleting override rules")
            do!
                query {
                    for ro in ctx.Conlang.RuleOverride do
                    where (ro.Id |=| overrideRules)
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore
    }

let putInflectionHandler (logger : ILogger) iid inflection =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)

        do! rewriteInflectionRules logger iid inflection

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
        } |> Seq.iter (fun i ->
            i.SpeechPart <- inflection.inflectionSpeechPart
            i.Name <- inflection.inflectionName
        )
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

let postInflectionHandler (logger : ILogger) inflection =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let iRow = ctx.Conlang.Inflection.Create()
        iRow.SpeechPart <- inflection.inflectionSpeechPart
        iRow.Name <- inflection.inflectionName
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

let deleteInflectionHandler (logger : ILogger) iid =
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

let getOverrideRulesHandler (logger : ILogger) i lid =
    async {
        let rules =
            query {
                    for ro in ctx.Conlang.RuleOverride do
                    join aro in ctx.Conlang.AxesRuleOverride on (ro.Id = aro.RuleOverride)
                    join av in ctx.Conlang.AxisValue on (aro.AxisValue = av.Id)
                    join an in ctx.Conlang.AxisName on (av.Axis = an.Id)
                    where (an.Language = lid && ro.Inflection = i)
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

let postOverrideRulesHandler (logger : ILogger) i rules =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        do!
            query {
                for ro in ctx.Conlang.RuleOverride do
                where (ro.Inflection = i)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        for rule in rules do
            let row = ctx.Conlang.RuleOverride.Create()
            row.Rule <- JsonSerializer.Serialize(rule.overrideRule, jsonOptions)
            row.Inflection <- i
            ctx.SubmitUpdates()

            let roId = ctx.Conlang.RuleOverride |> Seq.last

            for a in rule.overrideAxes do
                let aroRow = ctx.Conlang.AxesRuleOverride.Create()
                aroRow.RuleOverride <- roId.Id
                aroRow.AxisValue <- a
                ctx.SubmitUpdates()

        match rules with
        | [] -> ()
        | rule::_ ->
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

let postOverrideRuleHandler (logger : ILogger) i rule =
    async {
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let row = ctx.Conlang.RuleOverride.Create()
        row.Rule <- JsonSerializer.Serialize(rule.overrideRule, jsonOptions)
        row.Inflection <- i
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

let getAxesHandler (logger : ILogger) lid =
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

let getInflectionsHandler (logger : ILogger) lid =
    async {
        let filteredInflections =
            inflectTransformations
            |> toList
            |> filter (fun (KeyValue((l, _), _)) -> l = lid)
            |> map (fun i -> (i.Key, i.Value))
        let resp = filteredInflections |> Seq.map (fun ((_, iid), (n, sp, cl, a)) ->
                { id = iid; language = lid; speechPart = sp; classes = cl; axes = a; name = n}
            )
        return resp
    }
