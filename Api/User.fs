module OnlineConlang.Api.User

open SharedModels

open OnlineConlang.Foundation

open OnlineConlang.DB.Context

open OnlineConlang.Import.User

open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open System
open System.Security.Claims;

let postRegisterUserHandler (logger : ILogger) loginInfo : Async<unit> =
    async {
        let username = loginInfo.username
        let password = loginInfo.password
        let hasher = new PasswordHasher<string> ()
        let hashedPassword = hasher.HashPassword(username, password)

        let userExists =
            query {
                for u in ctx.Conlang.User do
                where (u.Login = username)
                select u.Id
            } |> Seq.tryHead

        if Option.isSome userExists then
            failwith "user with such username already exists, try another username"
        else
            let row = ctx.Conlang.User.Create()
            row.Login <- username
            row.Password <- hashedPassword
            ctx.SubmitUpdates()
    }

let postLoginUserHandler (logger : ILogger) loginInfo : Async<SecurityToken> =
    async {
        let username = loginInfo.username
        let password = loginInfo.password
        let hasher = new PasswordHasher<string> ()
        let ohashedPassword =
            query {
                for u in ctx.Conlang.User do
                where (u.Login = username)
                select u.Password
            } |> Seq.tryHead
        match ohashedPassword with
        | None ->
            failwith "user does not exist"
            return SecurityToken ""
        | Some hashedPassword ->
            match hasher.VerifyHashedPassword(username, hashedPassword, password) with
            | PasswordVerificationResult.Failed ->
                failwith "wrong password"
                return SecurityToken ""
            | _ ->
                let languages =
                    query {
                        for u in ctx.Conlang.User do
                        join ul in ctx.Conlang.UserLanguages on (u.Id = ul.User)
                        select ul.Language
                    } |> Seq.toList
                let issuer = config.jwt.issuer.ToString()
                let audience = config.jwt.audience.ToString()
                let tokenHandler = jwtHandler config.jwt.secret
                let claims = [ new Claim("name", username)
                             ; new Claim("languages", languages.ToString())
                             ]
                let token = tokenHandler.Issue
                                issuer
                                audience
                                claims
                                (Nullable())
                                (Nullable())
                return SecurityToken token
    }
