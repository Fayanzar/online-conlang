module OnlineConlang.Api.Transcription

open FSharpPlus

open SharedModels

open OnlineConlang.DB.Context
open OnlineConlang.Import.Phonotactics

open FSharp.Data.Sql
open System.Text.Json
open Microsoft.Extensions.Logging

let postTranscriptionHandler (logger : ILogger) lid (rule : Transformation) =
    async {
        let row = ctx.Conlang.TranscriptionRule.Create()
        row.Language <- lid
        row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
        try
            ctx.SubmitUpdates()
            updateTranscriptionTransformations lid
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let putTranscriptionHandler (logger : ILogger) lid tid (rule : Transformation) =
    async {
        query {
            for r in ctx.Conlang.TranscriptionRule do
            where (r.Id = tid && r.Language = lid)
        } |> Seq.iter (fun r ->
            r.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
        )
        try
            ctx.SubmitUpdates()
            updateTranscriptionTransformations lid
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let deleteTranscriptionHandler (logger : ILogger) lid tid =
    async {
        do!
            query {
                for r in ctx.Conlang.TranscriptionRule do
                where (r.Id = tid && r.Language = lid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        updateTranscriptionTransformations lid
    }

let getTranscriptionsHandler (logger : ILogger) lid =
    async {
        let rules : Transformation list =
            query {
                for t in ctx.Conlang.TranscriptionRule do
                where (t.Language = lid)
                select t.Rule
            } |> Seq.toList |> map (fun r -> JsonSerializer.Deserialize(r, jsonOptions))
        return rules
    }
