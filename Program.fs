module OnlineConlang.App

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

open System.Text.Json.Serialization

let options =
    JsonFSharpOptions.Default()
        .WithUnionExternalTag()
        .WithUnionNamedFields()
        .ToJsonSerializerOptions()

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

let indexHandler (name : string) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let greetings = $"Hi, {name}!"
        let model     = { Text = greetings }
        let view      = Views.index model
        htmlView view next ctx

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler "world"
                routef "/hello/%s" indexHandler
                routef "/%i/transcriptions" getTranscriptionsHandler
                routef "/%i/speechparts" getSpeechPartsHandler
                routef "/%i/classes" getClassesHandler
                route "/languages" >=> getLanguagesHandler
                routef "/%i/axes" getAxesHandler
            ]
        POST >=>
            choose [
                routef "/language/%s" postLanguageHandler
                routef "/%i/term" postTermHandler
                routef "/%i/classname/%s" postClassHandler
                routef "/%i/%s/classvalue/%s" postClassValueHandler
                routef "/%i/speechpart/%s" postSpeechPartHandler
                routef "/%i/axisname/%s" postAxisNameHandler
                routef "/%i/axisvalue/%s" postAxisValueHandler
                routef "/%i/axisrule" postAxisRuleHandler
                route "/inflection" >=> postInflectionHandler
                routef "/%i/rebuildinflection" postRebuildInflectionsHandler
                route "/overriderule" >=> postOverrideRuleHandler
            ]
        DELETE >=>
            choose [
                routef "/language/%i" deleteLanguageHandler
                routef "/%i/term/%i" deleteTermHandler
                routef "/%i/classname/%s" deleteClassHandler
                routef "/%i/%s/classvalue/%s" deleteClassValueHandler
                routef "/%i/speechpart/%s" deleteSpeechPartHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]

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
    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(options)) |> ignore

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
