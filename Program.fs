module OnlineConlang.App

open SharedModels

open OnlineConlang.Api.Class
open OnlineConlang.Api.Language
open OnlineConlang.Api.SpeechPart
open OnlineConlang.Api.Term
open OnlineConlang.Api.Transcription
open OnlineConlang.Api.Axes
open OnlineConlang.Api.Phonemes
open OnlineConlang.Api.User

open OnlineConlang.DB.Context
open OnlineConlang.Import.Morphology
open OnlineConlang.Import.Phonotactics
open OnlineConlang.Import.Phonology
open OnlineConlang.Import.User

open System
open System.IO
open System.Text.Json
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Fable.Remoting.Server
open Fable.Remoting.Giraffe

// ---------------------------------
// Models
// ---------------------------------

type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open Giraffe.ViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title []  [ encodedText "OnlineConlang" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () =
        h1 [] [ encodedText "OnlineConlang" ]

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

type SecurityToken = SecurityToken of string

// possible errors when logging in
type LoginError =
    | UserDoesNotExist
    | PasswordIncorrect
    | AccountBanned

// a request with a token
type SecureRequest<'T> = { token : SecurityToken; content : 'T }

// possible authentication/authorization errors
type AuthenticationError =
   | UserTokenExpired
   | TokenInvalid
   | UserDoesNotHaveAccess

let server (logger : ILogger) : IServer = {
    postLogin = postLoginUserHandler logger
    postLogout = postLogoutUserHandler logger
    postRegister = postRegisterUserHandler logger
    postVerifyUser = postVerifyUserHandler logger
    postSendVerificationEmail = postSendVerificationEmailHandler logger

    getUser = getUserHandler logger

    getLanguages = getLanguagesHandler logger
    deleteLanguage = deleteLanguageHandler logger
    postLanguage = postLanguageHandler logger
    putLanguage = putLanguageHandler logger

    getClasses = getClassesHandler logger
    postClass = postClassHandler logger
    putClass = putClassHandler logger
    deleteClass = deleteClassHandler logger

    postClassValue = postClassValueHandler logger
    putClassValue = putClassValueHandler logger
    deleteClassValue = deleteClassValueHandler logger

    getSpeechParts = getSpeechPartsHandler logger
    postSpeechPart = postSpeechPartHandler logger
    putSpeechPart = putSpeechPartHandler logger
    deleteSpeechPart = deleteSpeechPartHandler logger

    getTranscriptions = getTranscriptionsHandler logger
    postTranscription = postTranscriptionHandler logger
    putTranscription = putTranscriptionHandler logger
    deleteTranscription = deleteTranscriptionHandler logger

    getTerms = getTermsHandler logger
    postTerm = postTermHandler logger
    putTerm = putTermHandler logger
    deleteTerm = deleteTermHandler logger

    rebuildInflections = postRebuildInflectionsHandler logger

    getAxes = getAxesHandler logger
    postAxisName = postAxisNameHandler logger
    putAxisName = putAxisNameHandler logger
    deleteAxisName = deleteAxisNameHandler logger

    postAxisValue = postAxisValueHandler logger
    putAxisValue = putAxisValueHandler logger
    deleteAxisValue = deleteAxisValueHandler logger

    getAxisRules = getAxisRulesHandler logger
    postAxisRules = postAxisRulesHandler logger
    putAxisRule = putAxisRuleHandler logger
    deleteAxisRule = deleteAxisRuleHandler logger

    getOverrideRules = getOverrideRulesHandler logger
    postOverrideRules = postOverrideRulesHandler logger
    putOverrideRule = putOverrideRuleHandler logger
    deleteOverrideRule = deleteOverrideRuleHandler logger

    getInflections = getInflectionsHandler logger
    getInflectionsStructure = getInflectionsStructureHandler logger
    postInflection = postInflectionHandler logger
    putInflection = putInflectionHandler logger
    deleteInflection = deleteInflectionHandler logger

    postPhonemeClass = postPhonemeClassHandler logger
    putPhonemeClass = putPhonemeClassHandler logger
    deletePhonemeClass = deletePhonemeClassHandler logger
}

let serverAPI (ctx: HttpContext) : IServer =
    let logger = ctx.GetLogger("CLG")
    server logger

let fableErrorHandler (ex: Exception) (routeInfo: RouteInfo<HttpContext>) =
    printfn "Error at %s on method %s" routeInfo.path routeInfo.methodName
    let customError = { errorMsg = ex.Message }
    Propagate customError

let webApp : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.withErrorHandler fableErrorHandler
    |> Remoting.fromContext serverAPI
    |> Remoting.buildHttpHandler

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "moz-extension://812691db-0ba9-4cc4-918a-9b2169390784",
            "http://localhost:5173",
            "http://localhost:5000",
            "https://localhost:5001")
       .AllowAnyMethod()
       .AllowAnyHeader()
       |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(jsonOptions)) |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    let phonemes = query {
        for p in ctx.MarraidhConlang.Phoneme do
        select p
    }
    if Seq.isEmpty phonemes then
        for p in IPA.Consonants do
            let row = ctx.MarraidhConlang.Phoneme.Create()
            row.Phoneme <- JsonSerializer.Serialize(p, jsonOptions)
        for p in IPA.Vowels do
            let row = ctx.MarraidhConlang.Phoneme.Create()
            row.Phoneme <- JsonSerializer.Serialize(p, jsonOptions)
        ctx.SubmitUpdates()
    let languages = query {
        for l in ctx.MarraidhConlang.Language do
        select l.Id
    }
    for lid in (Seq.toList languages) do
        updateInflectTransformations lid
        updatePhonemeClasses lid

    updateUsersLanguages

    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .UseUrls("https://localhost:5001")
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0
