module OnlineConlang.Api.Class

open OnlineConlang.Prelude

open OnlineConlang.DB.Context

open FSharp.Data.Sql
open Giraffe
open Microsoft.AspNetCore.Http

let postClassHandler lid c =
    let row = ctx.Conlang.ClassName.Create()
    row.Language <- lid
    row.Name <- c
    ctx.SubmitUpdates()
    Successful.OK ""

let putClassHandler lid oldC newC =
    query {
        for c in ctx.Conlang.ClassName do
        where (c.Name = oldC && c.Language = lid)
    } |> Seq.iter (fun c -> c.Name <- newC)
    ctx.SubmitUpdates()
    Successful.OK ""

let deleteClassHandler lid cName =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for c in ctx.Conlang.ClassName do
                where (c.Language = lid && c.Name = cName)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }

let postClassValueHandler lid c cv =
    let className = query {
        for cn in ctx.Conlang.ClassName do
        where (cn.Name = c && cn.Language = lid)
        select (cn.Name)
    }
    match Seq.toList className with
    | [] -> badRequest400 "class name does not exist"
    | cn::_ ->
        let row = ctx.Conlang.ClassValue.Create()
        row.Language <- lid
        row.Class <- cn
        row.Name <- cv
        ctx.SubmitUpdates()
        Successful.OK ""

let putClassValueHandler lid c oldCv newCv =
    let className = query {
        for cn in ctx.Conlang.ClassName do
        where (cn.Name = c && cn.Language = lid)
        select (cn.Name)
    }
    match Seq.toList className with
    | [] -> badRequest400 "class name does not exist"
    | cn::_ ->
        query {
            for cv in ctx.Conlang.ClassValue do
            where (cv.Name = oldCv && cv.Language = lid && cv.Class = cn)
        } |> Seq.iter (fun cv -> cv.Name <- newCv)
        ctx.SubmitUpdates()
        Successful.OK ""

let deleteClassValueHandler lid c cValue =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for cv in ctx.Conlang.ClassValue do
                where (cv.Language = lid && cv.Name = cValue && cv.Class = c)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }
