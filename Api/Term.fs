module OnlineConlang.Api.Term

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.DB.Context
open OnlineConlang.Import.Term

open FSharp.Data.Sql
open Giraffe
open Microsoft.AspNetCore.Http

let postTermHandler lid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! term = hctx.BindJsonAsync<Term>()
            let! wordClass =
                query {
                    for c in ctx.Conlang.ClassName do
                    where (c.Name = term.wordClass)
                    select (c)
                } |> Seq.executeQueryAsync
            let! partOfSpeech =
                query {
                    for p in ctx.Conlang.SpeechPart do
                    where (p.Name = term.speechPart)
                    select (p)
                } |> Seq.executeQueryAsync
            match toList wordClass, toList partOfSpeech with
            | [], _ -> return! (badRequest400 "class does not exist") next hctx
            | _, [] -> return! (badRequest400 "part of speech does not exist") next hctx
            | _, _  ->
                let row = ctx.Conlang.Term.Create()
                row.Word <- term.word
                row.Language <- lid
                row.Class <- Some term.wordClass
                row.SpeechPart <- Some term.speechPart
                ctx.SubmitUpdatesAsync() |> ignore
                return! (Successful.OK "") next hctx
        }

let deleteTermHandler lid tid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            query {
                for t in ctx.Conlang.Term do
                where (t.Id = tid && t.Language = lid)
            } |> Seq.``delete all items from single table`` |> ignore
            return! (Successful.OK "") next hctx
        }

let putTermHandler lid tid =
    fun (next : HttpFunc) (hctx : HttpContext) ->
        task {
            let! term = hctx.BindJsonAsync<Term>()
            let! wordClass =
                query {
                    for c in ctx.Conlang.ClassName do
                    where (c.Name = term.wordClass)
                    select (c)
                } |> Seq.executeQueryAsync
            let! partOfSpeech =
                query {
                    for p in ctx.Conlang.SpeechPart do
                    where (p.Name = term.speechPart)
                    select (p)
                } |> Seq.executeQueryAsync
            match toList wordClass, toList partOfSpeech with
            | [], _ -> return! (badRequest400 "class does not exist") next hctx
            | _, [] -> return! (badRequest400 "part of speech does not exist") next hctx
            | _, _  ->
                query {
                    for t in ctx.Conlang.Term do
                    where (t.Id = tid && t.Language = lid)
                } |> Seq.iter (fun t ->
                    t.Word <- term.word
                    t.Language <- lid
                    t.Class <- Some term.wordClass
                    t.SpeechPart <- Some term.speechPart
                )
                ctx.SubmitUpdatesAsync() |> ignore
                return! (Successful.OK "") next hctx
        }
