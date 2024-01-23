module OnlineConlang.Api.Language

open FSharpPlus

open SharedModels

open OnlineConlang.DB.Context

open FSharp.Data.Sql

let postLanguageHandler language =
    async {
        let row = ctx.Conlang.Language.Create()
        row.Name <- language
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let deleteLanguageHandler lid =
    async {
        do!
            query {
                for l in ctx.Conlang.Language do
                where (l.Id = lid)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
    }

let putLanguageHandler lid newName =
    async {
        query {
            for l in ctx.Conlang.Language do
            where (l.Id = lid)
        } |> Seq.iter (fun l -> l.Name <- newName)
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let getLanguagesHandler =
    async {
        let! langs =
            query {
                for l in ctx.Conlang.Language do
                select (l.Id, l.Name)
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        let langsMap = langs |> Seq.toList |> map (
            fun (id, name) -> { id = id; name = name }
        )
        return langsMap
    }
