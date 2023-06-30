module OnlineConlang.Api.Term

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.DB.Context
open OnlineConlang.Import.Term
open OnlineConlang.Import.Morphology

open FSharp.Data.Sql
open Giraffe
open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Text.Json.Serialization
open System.Transactions

let jsonOptions =
    JsonFSharpOptions.Default()
        .WithUnionExternalTag()
        .WithUnionNamedFields()
        .ToJsonSerializerOptions()

let postTermHandler lid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! term = hctx.BindJsonAsync<Term>()
            use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
            let! wordClasses =
                query {
                    for cv in ctx.Conlang.ClassValue do
                    where (cv.Language = lid)
                    select cv.Name
                } |> Seq.executeQueryAsync
            let! partOfSpeech =
                query {
                    for p in ctx.Conlang.SpeechPart do
                    where (p.Name = term.speechPart && p.Language = lid)
                    select p
                } |> Seq.executeQueryAsync
            let newWordClasses = Set.intersect (Set wordClasses) term.wordClasses
            if newWordClasses <> term.wordClasses then
                transaction.Complete()
                return! (badRequest400 "one of classes does not exist") next hctx
            else
                match toList partOfSpeech with
                | [] ->
                    transaction.Complete()
                    return! (badRequest400 "part of speech does not exist") next hctx
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
                    return! (Successful.OK "") next hctx
        }

let deleteTermHandler (lid, tid) =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for t in ctx.Conlang.Term do
                where (t.Id = tid && t.Language = lid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }

let putTermHandler lid tid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! term = hctx.BindJsonAsync<Term>()
            let! wordClasses =
                query {
                    for cv in ctx.Conlang.ClassValue do
                    where (Set.contains cv.Name term.wordClasses && cv.Language = lid)
                    select (cv)
                } |> Seq.executeQueryAsync
            let! partOfSpeech =
                query {
                    for p in ctx.Conlang.SpeechPart do
                    where (p.Name = term.speechPart && p.Language = lid)
                    select (p)
                } |> Seq.executeQueryAsync
            match toList wordClasses, toList partOfSpeech with
            | [], _ -> return! (badRequest400 "class does not exist") next hctx
            | _, [] -> return! (badRequest400 "part of speech does not exist") next hctx
            | _, _  ->
                query {
                    for t in ctx.Conlang.Term do
                    where (t.Id = tid && t.Language = lid)
                } |> Seq.iter (fun t ->
                    t.Word <- term.word
                    t.Language <- lid
                    t.SpeechPart <- Some term.speechPart

                    if term.transcription.IsSome then t.Transcription <- term.transcription
                    if term.inflection.IsSome then t.Inflection <- term.inflection |> map (fun i ->
                                                        JsonSerializer.Serialize(i, jsonOptions)
                                                    )
                )
                ctx.SubmitUpdatesAsync() |> ignore

                query {
                    for tc in ctx.Conlang.TermClass do
                    where (tc.Term = tid)
                } |> Seq.``delete all items from single table`` |> ignore

                wordClasses |> Seq.iter (
                    fun c ->
                        let rowClass = ctx.Conlang.TermClass.Create()
                        rowClass.Class <- c.Name
                        rowClass.Term <- tid
                        ctx.SubmitUpdatesAsync() |> ignore
                )
                return! (Successful.OK "") next hctx
        }

let postRebuildInflectionsHandler tid =
    let termWithClasses = query {
                    for t in ctx.Conlang.Term do
                    join tc in ctx.Conlang.TermClass on (t.Id = tc.Term)
                    where (t.Id = tid)
                    select (t, tc.Class)
    }
    let term = termWithClasses |> Seq.head |> fst
    let classes = termWithClasses |> Seq.toList |> map snd |> Set
    query {
        for t in ctx.Conlang.Term do
        where (t.Id = tid)
    } |> Seq.iter (fun t ->
        match term.SpeechPart with
        | Some speechPart ->
            let axes = inflectTransformations[(term.Language, speechPart, classes)]
            let allNames = map (fun a -> a.inflections.Keys |> toList) axes.axes |> cartesian
            let inflection = map (fun names -> (names, inflect term.Word axes names)) allNames
            t.Inflection <- Some <| JsonSerializer.Serialize(inflection, jsonOptions)
        | _ -> ()
    )
    ctx.SubmitUpdates()
    Successful.OK ""
