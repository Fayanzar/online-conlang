module OnlineConlang.Api.SpeechPart

open OnlineConlang.Prelude

open OnlineConlang.DB.Context

open FSharp.Data.Sql
open Giraffe
open Microsoft.AspNetCore.Http

let postSpeechPartHandler (lid, sp) =
    let row = ctx.Conlang.SpeechPart.Create()
    row.Language <- lid
    row.Name <- sp
    try
        ctx.SubmitUpdates()
        Successful.OK ""
    with
    | e ->
        ctx.ClearUpdates() |> ignore
        internalServerError e.Message

let putSpeechPartHandler lid oldSp newSp =
    query {
        for sp in ctx.Conlang.SpeechPart do
        where (sp.Name = oldSp && sp.Language = lid)
    } |> Seq.iter (fun sp -> sp.Name <- newSp)
    try
        ctx.SubmitUpdates()
        Successful.OK ""
    with
    | e ->
        ctx.ClearUpdates() |> ignore
        internalServerError e.Message

let deleteSpeechPartHandler (lid, spName) =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for sp in ctx.Conlang.SpeechPart do
                where (sp.Language = lid && sp.Name = spName)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }

let getSpeechPartsHandler lid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! parts =
                query {
                    for sp in ctx.Conlang.SpeechPart do
                    where (sp.Language = lid)
                    select sp.Name
                } |> Seq.executeQueryAsync
            return! (json parts) next hctx
        }
