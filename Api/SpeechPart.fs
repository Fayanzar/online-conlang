module OnlineConlang.Api.SpeechPart

open FSharpPlus

open OnlineConlang.DB.Context

open FSharp.Data.Sql

let postSpeechPartHandler lid sp =
    async {
        let row = ctx.Conlang.SpeechPart.Create()
        row.Language <- lid
        row.Name <- sp
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let putSpeechPartHandler lid oldSp newSp =
    async {
        query {
            for sp in ctx.Conlang.SpeechPart do
            where (sp.Name = oldSp && sp.Language = lid)
        } |> Seq.iter (fun sp -> sp.Name <- newSp)
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let deleteSpeechPartHandler lid spName =
    async {
        do!
            query {
                for sp in ctx.Conlang.SpeechPart do
                where (sp.Language = lid && sp.Name = spName)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
    }

let getSpeechPartsHandler lid =
    async {
        let! parts =
            query {
                for sp in ctx.Conlang.SpeechPart do
                where (sp.Language = lid)
                select sp.Name
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        return parts
    }
