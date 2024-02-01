module OnlineConlang.Api.User

open OnlineConlang.DB.Context

open OnlineConlang.Import.User

open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging

let registerUserHandler (logger : ILogger) loginInfo : Async<unit> =
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

let loginUserHandler (logger : ILogger) loginInfo : Async<SecurityToken> =
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
            return (SecurityToken "")
        | Some hashedPassword ->
            match hasher.VerifyHashedPassword(username, hashedPassword, password) with
            | PasswordVerificationResult.Failed ->
                failwith "wrong password"
                return (SecurityToken "")
            | _ ->
                return (SecurityToken "")
    }
