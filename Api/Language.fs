module OnlineConlang.Api.Language

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.DB.Context

open FSharp.Data.Sql
open Giraffe
open Microsoft.AspNetCore.Http

let postLanguageHandler language =
    let row = ctx.Conlang.Language.Create()
    row.Name <- language
    ctx.SubmitUpdates()
    Successful.OK ""

let deleteLanguageHandler lid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for l in ctx.Conlang.Language do
                where (l.Id = lid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }

let putLanguageHandler lid newName =
    query {
        for l in ctx.Conlang.Language do
        where (l.Id = lid)
    } |> Seq.iter (fun l -> l.Name <- newName)
    try
        ctx.SubmitUpdates()
        Successful.OK ""
    with
        | e -> internalServerError e.Message

type Languages =
    {
        id   : int
        name : string
    }

let getLanguagesHandler =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! langs =
                query {
                    for l in ctx.Conlang.Language do
                    select (l.Id, l.Name)
                } |> Seq.executeQueryAsync
            let langsMap = langs |> map (
                fun (id, name) -> { id = id; name = name }
            )
            return! (json langsMap) next hctx
        }
