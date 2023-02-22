module OnlineConlang.Api.Transcription

open FSharpPlus

open SharedModels

open OnlineConlang.DB.Context
open OnlineConlang.Import.Phonotactics
open OnlineConlang.Import.User

open FSharp.Data.Sql
open System.Text.Json
open Microsoft.Extensions.Logging

let postTranscriptionHandler (logger : ILogger) stoken lid (rule : Transformation) =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            let row = ctx.MarraidhConlang.TranscriptionRule.Create()
            row.Language <- lid
            row.Rule <- JsonSerializer.Serialize(rule, jsonOptions)
            try
                ctx.SubmitUpdates()
                updateTranscriptionTransformations lid
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                failwith e.Message
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let putTranscriptionHandler (logger : ILogger) stoken tid (rule : Transformation) =
    async {
        let ouser = getUser logger stoken
        let lid =
            query {
                for t in ctx.MarraidhConlang.TranscriptionRule do
                where (t.Id = tid)
                select t.Language
            } |> Seq.head
        if userHasLanguage ouser lid then
            query {
                for r in ctx.MarraidhConlang.TranscriptionRule do
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
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let deleteTranscriptionHandler (logger : ILogger) stoken tid =
    async {
        let ouser = getUser logger stoken
        let lid =
            query {
                for t in ctx.MarraidhConlang.TranscriptionRule do
                where (t.Id = tid)
                select t.Language
            } |> Seq.head
        if userHasLanguage ouser lid then
            do!
                query {
                    for r in ctx.MarraidhConlang.TranscriptionRule do
                    where (r.Id = tid && r.Language = lid)
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore
            updateTranscriptionTransformations lid
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let getTranscriptionsHandler (logger : ILogger) lid =
    async {
        let rules : Transformation list =
            query {
                for t in ctx.MarraidhConlang.TranscriptionRule do
                where (t.Language = lid)
                select t.Rule
            } |> Seq.toList |> map (fun r -> JsonSerializer.Deserialize(r, jsonOptions))
        return rules
    }
