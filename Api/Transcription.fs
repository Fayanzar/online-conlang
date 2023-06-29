module OnlineConlang.Api.transcription

open OnlineConlang.DB.Context
open OnlineConlang.Import.Phonotactics
open OnlineConlang.Import.Transformations

open FSharp.Data.Sql
open FSharp.Json
open Giraffe
open Microsoft.AspNetCore.Http

let postTranscriptionHandler lid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<Transformation>()
            let row = ctx.Conlang.TranscriptionRule.Create()
            row.Language <- lid
            row.Rule <- Json.serialize rule
            ctx.SubmitUpdatesAsync() |> ignore

            updateTranscriptionTransformations lid
            return! (Successful.OK "") next hctx
        }

let putTranscriptionHandler lid tid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! rule = hctx.BindJsonAsync<Transformation>()
            query {
                for r in ctx.Conlang.TranscriptionRule do
                where (r.Id = tid && r.Language = lid)
            } |> Seq.iter (fun r ->
                r.Rule <- Json.serialize rule
            )
            ctx.SubmitUpdatesAsync() |> ignore

            updateTranscriptionTransformations lid
            return! (Successful.OK "") next hctx
        }

let deleteTranscriptionHandler lid tid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for r in ctx.Conlang.TranscriptionRule do
                where (r.Id = tid && r.Language = lid)
            } |> Seq.``delete all items from single table`` |> ignore

            updateTranscriptionTransformations lid
            return! (Successful.OK "") next hctx
        }
