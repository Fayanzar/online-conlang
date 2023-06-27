module OnlineConlang.Api.Language

open OnlineConlang.DB.Context

open FSharp.Data.Sql
open Giraffe
open Microsoft.AspNetCore.Http

let postLanguageHandler language =
    let row = ctx.Conlang.Language.Create()
    row.Name <- language
    ctx.SubmitUpdatesAsync() |> Async.AwaitTask |> ignore
    Successful.OK ""

let deleteLanguageHandler language =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for l in ctx.Conlang.Language do
                where (l.Name = language)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }

let updateLanguageHandler lid newName =
    query {
        for l in ctx.Conlang.Language do
        where (l.Id = lid)
    } |> Seq.iter (fun l -> l.Name <- newName)
    ctx.SubmitUpdates()
    Successful.OK ""
