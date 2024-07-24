module OnlineConlang.Api.User

open SharedModels

open OnlineConlang.Foundation

open OnlineConlang.DB.Context

open OnlineConlang.Import.User

open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open System
open System.Security.Claims;
open System.Security.Cryptography

let mkHashedKey username email =
    let hasher = new PasswordHasher<string> ()
    let rng = RandomNumberGenerator.Create()
    let mutable bytes : byte array = Array.zeroCreate 32
    rng.GetBytes(bytes, 0, 32)
    let byteString = System.Text.Encoding.ASCII.GetString bytes
    hasher.HashPassword(username, email + byteString)

let postRegisterUserHandler (logger : ILogger) loginInfo email : Async<unit> =
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
        else if validateEmail email then
            let row = ctx.Conlang.User.Create()
            row.Login <- username
            row.Password <- hashedPassword
            row.Email <- email
            row.VerificationKey <- Some <| mkHashedKey username email
            ctx.SubmitUpdates()

            sendVerificationEmail username
        else
            failwith $"{email} is not a valid email"
    }

let postLoginUserHandler (logger : ILogger) loginInfo : Async<SecurityToken> =
    async {
        let username = loginInfo.username
        let password = loginInfo.password
        let hasher = new PasswordHasher<string> ()
        let ohashedPasswordAndVerified =
            query {
                for u in ctx.Conlang.User do
                where (u.Login = username)
                select (u.Password, u.Verified)
            } |> Seq.tryHead
        match ohashedPasswordAndVerified with
        | None ->
            failwith "user does not exist"
            return SecurityToken ""
        | Some (hashedPassword, isVerified) ->
            match hasher.VerifyHashedPassword(username, hashedPassword, password) with
            | PasswordVerificationResult.Failed ->
                failwith "wrong password"
                return SecurityToken ""
            | _ ->
                if toBool isVerified then
                    let languages =
                        query {
                            for u in ctx.Conlang.User do
                            join ul in ctx.Conlang.UserLanguages on (u.Id = ul.User)
                            where (u.Login = username)
                            select ul.Language
                        } |> Seq.toList
                    let issuer = config.jwt.issuer.ToString()
                    let audience = config.jwt.audience.ToString()
                    let tokenHandler = jwtHandler config.jwt.secret
                    let claims = [ new Claim("name", username)
                                 ; new Claim("languages", languages.ToString())
                                 ; new Claim("verified", "true")
                                 ]
                    let token = tokenHandler.Issue
                                    issuer
                                    audience
                                    claims
                                    (Nullable())
                                    (Nullable())
                    return SecurityToken token
                else
                    let issuer = config.jwt.issuer.ToString()
                    let audience = config.jwt.audience.ToString()
                    let tokenHandler = jwtHandler config.jwt.secret
                    let claims = [ new Claim("name", username)
                                 ; new Claim("verified", "false")
                                 ]
                    let token = tokenHandler.Issue
                                    issuer
                                    audience
                                    claims
                                    (Nullable())
                                    (Nullable())
                    return SecurityToken token
    }

let postLogoutUserHandler (logger : ILogger) stoken =
    async {
        match getUser logger stoken with
        | None ->
            failwith "user does not exist"
            return SecurityToken ""
        | Some _ ->
            return SecurityToken ""

    }

let postVerifyUserHandler (logger : ILogger) username verificationKey =
    async {
        let ouser =
            query {
                for u in ctx.Conlang.User do
                where (u.Login = username && u.VerificationKey = Some verificationKey)
                select u
            } |> Seq.tryHead
        match ouser with
        | None ->
            failwith "username or verification key incorrect"
            return SecurityToken ""
        | Some user ->
            if toBool user.Verified then
                failwith "user is already verified"
                return SecurityToken ""
            else
                user.Verified <- fromBool true
                user.VerificationKey <- None
                ctx.SubmitUpdates()

                let languages =
                    query {
                        for u in ctx.Conlang.User do
                        join ul in ctx.Conlang.UserLanguages on (u.Id = ul.User)
                        where (user.Id = u.Id)
                        select ul.Language
                    } |> Seq.toList
                let issuer = config.jwt.issuer.ToString()
                let audience = config.jwt.audience.ToString()
                let tokenHandler = jwtHandler config.jwt.secret
                let claims = [ new Claim("name", username)
                                ; new Claim("languages", languages.ToString())
                                ; new Claim("verified", "true")
                                ]
                let token = tokenHandler.Issue
                                issuer
                                audience
                                claims
                                (Nullable())
                                (Nullable())
                return SecurityToken token
    }

let postSendVerificationEmailHandler (logger : ILogger) stoken =
    async {
        let ouser' = getUser logger stoken
        match ouser' with
        | None -> failwith "user not logged in"
        | Some (_, username) ->
            let ouser =
                query {
                    for u in ctx.Conlang.User do
                    where (u.Login = username)
                    select u
                } |> Seq.tryHead
            match ouser with
            | None -> failwith "user does not exist"
            | Some user ->
                if toBool user.Verified then failwith "user is already verified"
                else
                    let newHashedKey = mkHashedKey username user.Email
                    user.VerificationKey <- Some newHashedKey
                    ctx.SubmitUpdates()

                    sendVerificationEmail username
    }

let getUserHandler (logger : ILogger) stoken =
    async {
        match getUser logger stoken with
        | None -> return (None, false)
        | Some (uid, username) ->
            let isVerified =
                query {
                    for u in ctx.Conlang.User do
                    where (u.Id = uid)
                    select u.Verified
                    exactlyOneOrDefault
                }
            if toBool isVerified then return (Some (uid, username), true)
                                 else return (Some (uid, username), false)
    }