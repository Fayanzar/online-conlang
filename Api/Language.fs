module OnlineConlang.Api.Language

open FSharpPlus

open SharedModels

open OnlineConlang.Foundation

open OnlineConlang.DB.Context
open OnlineConlang.Import.User

open FSharp.Data.Sql
open Microsoft.Extensions.Logging
open System.Security.Claims;
open System
open System.Transactions

let postLanguageHandler (logger : ILogger) stoken language : Async<SecurityToken> =
    async {
        let ouser = getUser logger stoken
        match ouser with
        | None ->
            failwith "no such user"
            return SecurityToken ""
        | Some (uid, name) ->
            use transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)
            let lRow = ctx.Conlang.Language.Create()
            lRow.Name <- language
            ctx.SubmitUpdates()

            let lastLanguage =
                query {
                    for l in ctx.Conlang.Language do
                    select l.Id
                } |> Seq.last

            let uRow = ctx.Conlang.UserLanguages.Create()
            uRow.User <- uid
            uRow.Language <- lastLanguage
            ctx.SubmitUpdates()

            let languages =
                query {
                    for u in ctx.Conlang.User do
                    join ul in ctx.Conlang.UserLanguages on (u.Id = ul.User)
                    select ul.Language
                } |> Seq.toList

            updateUserLanguages uid languages

            let tokenHandler = jwtHandler config.jwt.secret
            let issuer = config.jwt.issuer.ToString()
            let audience = config.jwt.audience.ToString()
            let claims = [ new Claim("name", name)
                         ; new Claim("languages", languages.ToString())
                         ]
            let token = tokenHandler.Issue
                            issuer
                            audience
                            claims
                            (Nullable())
                            (Nullable())
            transaction.Complete()
            return SecurityToken token
    }

let deleteLanguageHandler (logger : ILogger) stoken lid =
    async {
        let ouser = getUser logger stoken
        match ouser with
        | None ->
            failwith "no such user"
        | Some (uid, _) ->
            let langs = userLanguages.Item uid
            updateUserLanguages uid (List.except [lid] langs)
            do!
                query {
                    for l in ctx.Conlang.Language do
                    where (l.Id = lid)
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore
    }

let putLanguageHandler (logger : ILogger) stoken lid newName =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
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
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let getLanguagesHandler (logger : ILogger) =
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
