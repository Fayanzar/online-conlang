module OnlineConlang.Import.User

open SharedModels

open OnlineConlang.Prelude
open OnlineConlang.Foundation

open OnlineConlang.DB.Context

open MailKit.Net.Smtp
open MimeKit
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open System.Collections.Generic
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Globalization
open System.Text
open System.Text.RegularExpressions
open System

type IJWTHandler =
    abstract member Issue: issuer: string ->
                           audience: string ->
                           claims: Claim list ->
                           notBefore: Nullable<DateTime> ->
                           expires: Nullable<DateTime> ->
                           string
    abstract member Read: token: string -> Claim list
    abstract member Validate: token: string -> Either<exn, ClaimsPrincipal>

let userLanguages = new Dictionary<int, list<int>>()

let updateUserLanguages uid langs =
    if userLanguages.ContainsKey uid then
        userLanguages.Item uid <- langs
    else
        userLanguages.Add(uid, langs)

let updateUsersLanguages =
    let ulangs =
        query {
            for ul in ctx.MarraidhConlang.UserLanguages do
            select (ul.User, ul.Language)
        } |> Seq.groupBy fst
          |> Seq.map (fun (k, v) -> (k, Seq.map snd v |> Seq.toList))
          |> Seq.toList
    userLanguages.Clear()
    for (user, languages) in ulangs do
        userLanguages.Add(user, languages)

let jwtHandler (secret : string) =
    let key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    let tokenHandler = new JwtSecurityTokenHandler()
    let issue issuer audience claims notBefore expires =
        let jwt = new JwtSecurityToken
                        ( issuer
                        , audience
                        , claims
                        , notBefore
                        , expires
                        , new SigningCredentials(key, SecurityAlgorithms.HmacSha256))
        tokenHandler.WriteToken(jwt)
    let read token =
        let decodedToken = tokenHandler.ReadJwtToken token
        decodedToken.Claims |> Seq.toList
    let validate (token : string) =
        let validatedToken = ref null
        let validationParams = new TokenValidationParameters ()
        validationParams.IssuerSigningKey <- key
        validationParams.ValidateLifetime <- false
        validationParams.ValidateIssuer <- false
        validationParams.ValidAudience <- config.jwt.audience.ToString()
        try
            Right <| tokenHandler.ValidateToken(token, validationParams, validatedToken)
        with
        | e -> Left e


    {
        new IJWTHandler with
            member this.Issue issuer audience claims notBefore expires
                            = issue issuer audience claims notBefore expires
            member this.Read token = read token
            member this.Validate token = validate token
    }

let getUser (logger : ILogger) (stoken : SharedModels.SecurityToken) =
    let token = match stoken with SecurityToken t -> t
    let tokenHandler = jwtHandler config.jwt.secret
    match tokenHandler.Validate token with
    | Left e ->
        logger.LogWarning e.Message
        None
    | Right claims ->
        let oname = claims.Claims
                    |> Seq.tryFind (fun c -> c.Type = "name")
                    |> Option.map (fun c -> c.Value)
        match oname with
        | None -> None
        | Some name ->
            let ouid =
                query {
                    for u in ctx.MarraidhConlang.User do
                    where (u.Login = name)
                    select u.Id
                } |> Seq.tryHead
            match ouid with
            | None -> None
            | Some uid -> Some (uid, name)

let userHasLanguage ouser lang =
    match ouser with
    | None -> false
    | Some (uid, _) ->
        if userLanguages.ContainsKey uid then
            let langs = userLanguages.Item uid
            List.contains lang langs
        else false

let sendEmail email subject body =
    let message = new MimeMessage ()
    message.From.Add <| new MailboxAddress (config.mail.fromName, config.mail.from)
    message.To.Add <| new MailboxAddress ("", email)
    message.Subject <- subject
    let messageBody = new TextPart "html"
    messageBody.Text <- body
    message.Body <- messageBody
    use client = new SmtpClient ()
    client.Connect (config.mail.smtpClient, config.mail.smtpPort, true)
    client.Authenticate (config.mail.user, config.mail.password)
    client.Send message |> ignore
    client.Disconnect true

let sendVerificationEmail username =
    let subject = "Verify your email"
    let oemailAndKey =
        query {
            for u in ctx.MarraidhConlang.User do
            where (u.Login = username)
            select (u.Email, u.VerificationKey)
        } |> Seq.tryHead
    match oemailAndKey with
    | None -> failwith "no such user"
    | Some (email, Some key) ->
        let escapedKey = Uri.EscapeDataString key
        let link = Uri(config.baseExternalUrl, $"/verify?username={username}&key={escapedKey}")
        let body =
            $"<h2>Hello, {username}!</h2>
            <p>Thanks for registering at Marràidh Conlanging.
            To be able to create and edit languages,
            please click the verification link below.</p>
            <p>{link}</p>
            "
        sendEmail email subject body
    | _ -> failwith "verification key not found"

let validateEmail email =
    if String.IsNullOrWhiteSpace(email) then false
    else
        try
            let domainMapper (m : Match) =
                let idn = new IdnMapping()
                let domainName = idn.GetAscii m.Groups[2].Value
                m.Groups[1].Value + domainName

            let normalizedEmail = Regex.Replace(email, @"(@)(.+)$", domainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200))
            Regex.IsMatch(normalizedEmail,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))
        with
        | _ -> false
