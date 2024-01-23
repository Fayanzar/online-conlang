module OnlineConlang.Api.Class

open FSharpPlus

open OnlineConlang.DB.Context

open FSharp.Data.Sql

let postClassHandler lid c =
    async {
        let row = ctx.Conlang.ClassName.Create()
        row.Language <- lid
        row.Name <- c
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let putClassHandler lid oldC newC =
    async {
        query {
            for c in ctx.Conlang.ClassName do
            where (c.Name = oldC && c.Language = lid)
        } |> Seq.iter (fun c -> c.Name <- newC)
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }
let deleteClassHandler lid cName =
    async {
        do!
            query {
                for c in ctx.Conlang.ClassName do
                where (c.Language = lid && c.Name = cName)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
    }

let postClassValueHandler lid cn cv =
    async {
        let row = ctx.Conlang.ClassValue.Create()
        row.Language <- lid
        row.Class <- cn
        row.Name <- cv
        try
            ctx.SubmitUpdates()
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let putClassValueHandler lid c oldCv newCv =
    async {
        let className = query {
            for cn in ctx.Conlang.ClassName do
            where (cn.Name = c && cn.Language = lid)
            select (cn.Name)
        }
        match Seq.toList className with
        | [] -> failwith "class name does not exist"
        | cn::_ ->
            query {
                for cv in ctx.Conlang.ClassValue do
                where (cv.Name = oldCv && cv.Language = lid && cv.Class = cn)
            } |> Seq.iter (fun cv -> cv.Name <- newCv)
            try
                ctx.SubmitUpdates()
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                failwith e.Message
    }

let deleteClassValueHandler lid c cValue =
    async {
        do!
            query {
                for cv in ctx.Conlang.ClassValue do
                where (cv.Language = lid && cv.Name = cValue && cv.Class = c)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
    }

let getClassesHandler lid =
    async {
        let! classes =
            query {
                for cv in ctx.Conlang.ClassValue do
                join cn in ctx.Conlang.ClassName on ((cv.Class, cv.Language) = (cn.Name, cn.Language))
                where (cn.Language = lid)
                select (cn.Name, cv.Name)
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        let classMap = classes |> Seq.groupBy fst |> map (fun (k, v) -> (k, map snd v))
        return Map classMap
    }
