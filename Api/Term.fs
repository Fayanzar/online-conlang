module OnlineConlang.Api.Term

open FSharpPlus
open OnlineConlang.Prelude

open SharedModels

open OnlineConlang.DB.Context
open OnlineConlang.Import.Term
open OnlineConlang.Import.Morphology

open FSharp.Data.Sql
open System.Text.Json
open System.Transactions
open Microsoft.Extensions.Logging

let postTermHandler lid termApi =
    async {
        let term = parseTerm termApi
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let! wordClasses =
            query {
                for cv in ctx.Conlang.ClassValue do
                where (cv.Language = lid)
                select cv.Name
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        let! partOfSpeech =
            query {
                for p in ctx.Conlang.SpeechPart do
                where (p.Name = term.speechPart && p.Language = lid)
                select p
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        let newWordClasses = Set.intersect (Set wordClasses) term.wordClasses
        if newWordClasses <> term.wordClasses then
            transaction.Complete()
            failwith "one of classes does not exist"
        else
            match toList partOfSpeech with
            | [] ->
                transaction.Complete()
                failwith "part of speech does not exist"
            | _  ->
                let row = ctx.Conlang.Term.Create()
                row.Word <- term.word
                row.Language <- lid
                row.SpeechPart <- Some term.speechPart

                let transcription =
                    match term.transcription with
                    | None   -> term.mkTranscription lid
                    | Some t -> t

                let inflection =
                    match term.inflection with
                    | None   -> term.mkInflections lid
                    | Some i -> i

                row.Transcription <- Some transcription
                row.Inflection <- Some <| JsonSerializer.Serialize(inflection, jsonOptions)

                ctx.SubmitUpdates()

                let tid = ctx.Conlang.Term |> Seq.last

                for c in newWordClasses do
                    let rowClass = ctx.Conlang.TermClass.Create()
                    rowClass.Class <- c
                    rowClass.Term <- tid.Id
                    ctx.SubmitUpdates()

                transaction.Complete()
    }

let deleteTermHandler lid tid =
    async {
        do!
            query {
                for t in ctx.Conlang.Term do
                where (t.Id = tid && t.Language = lid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
    }

let putTermHandler lid tid termApi =
    async {
        let term = parseTerm termApi
        use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
        let wordClasses =
            query {
                for cv in ctx.Conlang.ClassValue do
                where (cv.Name |=| term.wordClasses && cv.Language = lid)
                select (cv)
            } |> Seq.toList
        let partOfSpeech =
            query {
                for p in ctx.Conlang.SpeechPart do
                where (p.Name = term.speechPart && p.Language = lid)
                select (p)
            } |> Seq.toList
        match wordClasses, partOfSpeech with
        | [], _ ->
            transaction.Complete()
            failwith "class does not exist"
        | _, [] ->
            transaction.Complete()
            failwith "part of speech does not exist"
        | _, _  ->
            query {
                for t in ctx.Conlang.Term do
                where (t.Id = tid && t.Language = lid)
            } |> Seq.iter (fun t ->
                t.Word <- term.word
                t.Language <- lid
                t.SpeechPart <- Some term.speechPart
                t.Transcription <- term.mkTranscription lid |> Some
                t.Inflection <- Some <| JsonSerializer.Serialize(term.mkInflections lid, jsonOptions)
            )
            ctx.SubmitUpdates()

            do!
                query {
                    for tc in ctx.Conlang.TermClass do
                    where (tc.Term = tid)
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore

            wordClasses |> iter (
                fun c ->
                    let rowClass = ctx.Conlang.TermClass.Create()
                    rowClass.Class <- c.Name
                    rowClass.Term <- tid
                    ctx.SubmitUpdates()
            )

            transaction.Complete()
    }

let getTermsHandler lid =
    async {
        let terms =
            query {
                for t in ctx.Conlang.Term do
                join tc in ctx.Conlang.TermClass on (t.Id = tc.Term)
                where (t.Language = lid)
                select (t, tc.Class)
            }
        let termsResponse =
            terms |> Seq.groupBy fst |> Seq.map (fun (t, tc) ->
                { TermForAPI.word = t.Word
                  TermForAPI.id = t.Id
                  inflection =
                    match t.Inflection with
                        | Some inflection -> JsonSerializer.Deserialize(inflection, jsonOptions)
                        | None -> None
                  speechPart = option id "" t.SpeechPart
                  wordClasses = Seq.map snd tc |> Set
                  transcription = t.Transcription
                }
            )
        return termsResponse
    }

let postRebuildInflectionsHandler (logger : ILogger) tid =
    async {
        let termWithClasses =
            query {
                for t in ctx.Conlang.Term do
                join tc in ctx.Conlang.TermClass on (t.Id = tc.Term)
                where (t.Id = tid)
                select (t, tc.Class)
            } |> Seq.toList
        let term = termWithClasses |> List.head |> fst
        let classes = termWithClasses |> map snd |> Set
        query {
            for t in ctx.Conlang.Term do
            where (t.Id = tid)
        } |> Seq.iter (fun t ->
            match term.SpeechPart with
            | Some speechPart ->
                let inflections = inflectTransformations[term.Language] |> filter (fun (sp, classes', _) ->
                    sp = speechPart && classes' = classes
                )
                let allAxes = inflections |> map thd3
                let inflection = allAxes |> map (fun axes ->
                    logger.LogInformation(axes.ToString())
                    let allNames = map (fun a -> a.inflections.Keys |> toList) axes.axes |> cartesian
                    ( axes.axes |> List.map (fun a -> a.name)
                    , map (fun names -> (names, inflect term.Word axes names)) allNames)
                )
                t.Inflection <- Some <| JsonSerializer.Serialize(inflection, jsonOptions)
            | _ -> ()
        )
        ctx.SubmitUpdates()
    }
