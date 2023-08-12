module OnlineConlang.App

open SharedModels

open OnlineConlang.Api.Class
open OnlineConlang.Api.Language
open OnlineConlang.Api.SpeechPart
open OnlineConlang.Api.Term
open OnlineConlang.Api.Transcription
open OnlineConlang.Api.Axes

open OnlineConlang.DB.Context
open OnlineConlang.Import.Morphology

open System
open System.IO
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

let serverAPI : IServer = {
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

    postAxisRule = postAxisRuleHandler
    putAxisRule = putAxisRuleHandler
    deleteAxisRule = deleteAxisRuleHandler

    postOverrideRule = postOverrideRuleHandler
    putOverrideRule = putOverrideRuleHandler
    deleteOverrideRule = deleteOverrideRuleHandler

    getInflections = getInflectionsHandler
    postInflection = postInflectionHandler
    deleteInflection = deleteInflectionHandler
}

let webApp : HttpHandler =
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.fromValue serverAPI
    |> Remoting.buildHttpHandler

// let webApp =
//     choose [
//         GET >=>
//             choose [
//                 routef "api/%i/transcriptions" getTranscriptionsHandler
//                 routef "api/%i/speechparts" getSpeechPartsHandler
//                 routef "api/%i/classes" getClassesHandler
//                 route "api/languages" >=> getLanguagesHandler
//                 routef "api/%i/axes" getAxesHandler
//             ]
//         POST >=>
//             choose [
//                 routef "api/language/%s" postLanguageHandler
//                 routef "api/%i/term" postTermHandler
//                 routef "api/%i/classname/%s" postClassHandler
//                 routef "api/%i/%s/classvalue/%s" postClassValueHandler
//                 routef "api/%i/speechpart/%s" postSpeechPartHandler
//                 routef "api/%i/axisname/%s" postAxisNameHandler
//                 routef "api/%i/axisvalue/%s" postAxisValueHandler
//                 routef "api/%i/axisrule" postAxisRuleHandler
//                 route "api/inflection" >=> postInflectionHandler
//                 routef "api/%i/rebuildinflection" postRebuildInflectionsHandler
//                 route "api/overriderule" >=> postOverrideRuleHandler
//             ]
//         DELETE >=>
//             choose [
//                 routef "api/language/%i" deleteLanguageHandler
//                 routef "api/%i/term/%i" deleteTermHandler
//                 routef "api/%i/classname/%s" deleteClassHandler
//                 routef "api/%i/%s/classvalue/%s" deleteClassValueHandler
//                 routef "api/%i/speechpart/%s" deleteSpeechPartHandler
//             ]
//         setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

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
    let languages = query {
        for l in ctx.Conlang.Language do
        select l.Id
    }
    for lid in (Seq.toList languages) do
        updateInflectTransformations lid

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
