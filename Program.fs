module OnlineConlang.App

open SharedModels

open OnlineConlang.Api.Class
open OnlineConlang.Api.Language
open OnlineConlang.Api.SpeechPart
open OnlineConlang.Api.Term
open OnlineConlang.Api.Transcription
open OnlineConlang.Api.Axes
open OnlineConlang.Api.Phonemes

open OnlineConlang.DB.Context
open OnlineConlang.Import.Morphology
open OnlineConlang.Import.Phonotactics
open OnlineConlang.Import.Phonology

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

let server (logger : ILogger) : IServer = {
    getLanguages = getLanguagesHandler
    deleteLanguage = deleteLanguageHandler
    postLanguage = postLanguageHandler
    putLanguage = putLanguageHandler

    getClasses = getClassesHandler
    postClass = postClassHandler
    putClass = putClassHandler
    deleteClass = deleteClassHandler

    postClassValue = postClassValueHandler
    putClassValue = putClassValueHandler
    deleteClassValue = deleteClassValueHandler

    getSpeechParts = getSpeechPartsHandler
    postSpeechPart = postSpeechPartHandler
    putSpeechPart = putSpeechPartHandler
    deleteSpeechPart = deleteSpeechPartHandler

    getTranscriptions = getTranscriptionsHandler
    postTranscription = postTranscriptionHandler
    putTranscription = putTranscriptionHandler
    deleteTranscription = deleteTranscriptionHandler

    getTerms = getTermsHandler
    postTerm = postTermHandler
    putTerm = putTermHandler
    deleteTerm = deleteTermHandler

    rebuildInflections = postRebuildInflectionsHandler

    getAxes = getAxesHandler
    postAxisName = postAxisNameHandler
    putAxisName = putAxisNameHandler
    deleteAxisName = deleteAxisNameHandler

    postAxisValue = postAxisValueHandler
    putAxisValue = putAxisValueHandler
    deleteAxisValue = deleteAxisValueHandler

    getAxisRules = getAxisRulesHandler
    postAxisRule = postAxisRuleHandler
    putAxisRule = putAxisRuleHandler logger
    deleteAxisRule = deleteAxisRuleHandler

    getOverrideRules = getOverrideRulesHandler
    postOverrideRule = postOverrideRuleHandler
    putOverrideRule = putOverrideRuleHandler
    deleteOverrideRule = deleteOverrideRuleHandler

    getInflections = getInflectionsHandler
    postInflection = postInflectionHandler
    deleteInflection = deleteInflectionHandler

    postPhonemeClass = postPhonemeClassHandler
    putPhonemeClass = putPhonemeClassHandler
    deletePhonemeClass = deletePhonemeClassHandler
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
        for p in ctx.Conlang.Phoneme do
        select p
    }
    if Seq.isEmpty phonemes then
        for p in IPA.Consonants do
            let row = ctx.Conlang.Phoneme.Create()
            row.Phoneme <- JsonSerializer.Serialize(p, jsonOptions)
        for p in IPA.Vowels do
            let row = ctx.Conlang.Phoneme.Create()
            row.Phoneme <- JsonSerializer.Serialize(p, jsonOptions)
        ctx.SubmitUpdates()
    let languages = query {
        for l in ctx.Conlang.Language do
        select l.Id
    }
    for lid in (Seq.toList languages) do
        updateInflectTransformations lid
        updatePhonemeClasses lid

    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0
