module OnlineConlang.Api.Transcription

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.DB.Context
open OnlineConlang.Import.Phonotactics
open OnlineConlang.Import.Transformations

open FSharp.Data.Sql
open System.Text.Json
open Giraffe
open Microsoft.AspNetCore.Http

let postTranscriptionHandler lid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<Transformation>()
            let row = ctx.Conlang.TranscriptionRule.Create()
            row.Language <- lid
            row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
            try
                ctx.SubmitUpdatesAsync() |> ignore
                updateTranscriptionTransformations lid
                return! (Successful.OK "") next hctx
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                return! internalServerError e.Message next hctx
        }

let putTranscriptionHandler lid tid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<Transformation>()
            query {
                for r in ctx.Conlang.TranscriptionRule do
                where (r.Id = tid && r.Language = lid)
            } |> Seq.iter (fun r ->
                r.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
            )
            try
                ctx.SubmitUpdatesAsync() |> ignore
                updateTranscriptionTransformations lid
                return! Successful.OK "" next hctx
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                return! internalServerError e.Message next hctx
        }

let deleteTranscriptionHandler lid tid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for r in ctx.Conlang.TranscriptionRule do
                where (r.Id = tid && r.Language = lid)
            } |> Seq.``delete all items from single table`` |> ignore

            updateTranscriptionTransformations lid
            return! Successful.OK "" next hctx
        }

let getTranscriptionsHandler lid =
    let rules : Transformation list =
        query {
            for t in ctx.Conlang.TranscriptionRule do
            where (t.Language = lid)
            select t.Rule
        } |> Seq.toList |> map (fun r -> JsonSerializer.Deserialize(r, jsonOptions))
    json rules
