module OnlineConlang.Api.Term

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.DB.Context
open OnlineConlang.Import.Term

open FSharp.Data.Sql
open FSharp.Json
open Giraffe
open Microsoft.AspNetCore.Http

let postTermHandler lid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! term = hctx.BindJsonAsync<Term>()
            let! wordClass =
                query {
                    for cv in ctx.Conlang.ClassValue do
                    where (cv.Name = term.wordClass && cv.Language = lid)
                    select (cv)
                } |> Seq.executeQueryAsync
            let! partOfSpeech =
                query {
                    for p in ctx.Conlang.SpeechPart do
                    where (p.Name = term.speechPart && p.Language = lid)
                    select (p)
                } |> Seq.executeQueryAsync
            match toList wordClass, toList partOfSpeech with
            | [], _ -> return! (badRequest400 "class does not exist") next hctx
            | _, [] -> return! (badRequest400 "part of speech does not exist") next hctx
            | _, _  ->
                let row = ctx.Conlang.Term.Create()
                row.Word <- term.word
                row.Language <- lid
                row.Class <- Some term.wordClass
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
                row.Inflection <- Some <| Json.serialize inflection
                ctx.SubmitUpdatesAsync() |> ignore
                return! (Successful.OK "") next hctx
        }

let deleteTermHandler lid tid =
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
            let! wordClass =
                query {
                    for cv in ctx.Conlang.ClassValue do
                    where (cv.Name = term.wordClass && cv.Language = lid)
                    select (cv)
                } |> Seq.executeQueryAsync
            let! partOfSpeech =
                query {
                    for p in ctx.Conlang.SpeechPart do
                    where (p.Name = term.speechPart && p.Language = lid)
                    select (p)
                } |> Seq.executeQueryAsync
            match toList wordClass, toList partOfSpeech with
            | [], _ -> return! (badRequest400 "class does not exist") next hctx
            | _, [] -> return! (badRequest400 "part of speech does not exist") next hctx
            | _, _  ->
                query {
                    for t in ctx.Conlang.Term do
                    where (t.Id = tid && t.Language = lid)
                } |> Seq.iter (fun t ->
                    t.Word <- term.word
                    t.Language <- lid
                    t.Class <- Some term.wordClass
                    t.SpeechPart <- Some term.speechPart

                    if term.transcription.IsSome then t.Transcription <- term.transcription
                    if term.inflection.IsSome then t.Inflection <- map Json.serialize term.inflection
                )
                ctx.SubmitUpdatesAsync() |> ignore
                return! (Successful.OK "") next hctx
        }
