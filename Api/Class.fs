module OnlineConlang.Api.Class

open FSharpPlus

open OnlineConlang.DB.Context
open OnlineConlang.Import.User

open FSharp.Data.Sql
open Microsoft.Extensions.Logging

let postClassHandler (logger : ILogger) stoken lid c =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            let row = ctx.MarraidhConlang.ClassName.Create()
            row.Language <- lid
            row.Name <- c
            try
                ctx.SubmitUpdates()
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                failwith e.Message
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let putClassHandler (logger : ILogger) stoken lid oldC newC =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            query {
                for c in ctx.MarraidhConlang.ClassName do
                where (c.Name = oldC && c.Language = lid)
            } |> Seq.iter (fun c -> c.Name <- newC)
            try
                ctx.SubmitUpdates()
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                failwith e.Message
        else
            failwith $"user {ouser} does not own the language {lid}"
    }
let deleteClassHandler (logger : ILogger) stoken lid cName =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            do!
                query {
                    for c in ctx.MarraidhConlang.ClassName do
                    where (c.Language = lid && c.Name = cName)
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let postClassValueHandler (logger : ILogger) stoken lid cn cv =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            let row = ctx.MarraidhConlang.ClassValue.Create()
            row.Language <- lid
            row.Class <- cn
            row.Name <- cv
            try
                ctx.SubmitUpdates()
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                failwith e.Message
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let putClassValueHandler (logger : ILogger) stoken lid c oldCv newCv =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            let className = query {
                for cn in ctx.MarraidhConlang.ClassName do
                where (cn.Name = c && cn.Language = lid)
                select (cn.Name)
            }
            match Seq.toList className with
            | [] -> failwith "class name does not exist"
            | cn::_ ->
                query {
                    for cv in ctx.MarraidhConlang.ClassValue do
                    where (cv.Name = oldCv && cv.Language = lid && cv.Class = cn)
                } |> Seq.iter (fun cv -> cv.Name <- newCv)
                try
                    ctx.SubmitUpdates()
                with
                | e ->
                    ctx.ClearUpdates() |> ignore
                    failwith e.Message
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let deleteClassValueHandler (logger : ILogger) stoken lid c cValue =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            do!
                query {
                    for cv in ctx.MarraidhConlang.ClassValue do
                    where (cv.Language = lid && cv.Name = cValue && cv.Class = c)
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let getClassesHandler (logger : ILogger) lid =
    async {
        let! classes =
            query {
                for cn in ctx.MarraidhConlang.ClassName do
                join cv in !!ctx.MarraidhConlang.ClassValue on ((cn.Name, cn.Language) = (cv.Class, cv.Language))
                where (cn.Language = lid)
                select (cn.Name, cv.Name)
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        let classMap = classes
                    |> Seq.groupBy fst
                    |> map (fun (k, v) -> (k, map snd v |> filter ((<>) "")))
        return Map classMap
    }
