module OnlineConlang.Api.SpeechPart

open OnlineConlang.DB.Context

open FSharp.Data.Sql
open Giraffe
open Microsoft.AspNetCore.Http

let postSpeechPartHandler lid sp =
    let row = ctx.Conlang.SpeechPart.Create()
    row.Language <- lid
    row.Name <- sp
    ctx.SubmitUpdates()
    Successful.OK ""

let putSpeechPartHandler lid oldSp newSp =
    query {
        for sp in ctx.Conlang.SpeechPart do
        where (sp.Name = oldSp && sp.Language = lid)
    } |> Seq.iter (fun sp -> sp.Name <- newSp)
    ctx.SubmitUpdates()
    Successful.OK ""

let deleteSpeechPartHandler lid spName =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for sp in ctx.Conlang.SpeechPart do
                where (sp.Language = lid && sp.Name = spName)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }
