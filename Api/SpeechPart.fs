module OnlineConlang.Api.SpeechPart

open FSharpPlus

open OnlineConlang.DB.Context
open OnlineConlang.Import.User

open FSharp.Data.Sql
open Microsoft.Extensions.Logging
open MySql.Data.MySqlClient

let postSpeechPartHandler (logger : ILogger) stoken lid sp =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            let row = ctx.Conlang.SpeechPart.Create()
            row.Language <- lid
            row.Name <- sp
            try
                ctx.SubmitUpdates()
            with
            | e ->
                ctx.ClearUpdates() |> ignore
                failwith e.Message
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let putSpeechPartHandler (logger : ILogger) stoken lid oldSp newSp =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            use con = new MySqlConnection(connectionString)
            con.Open ()
            // SQLProvider cannot handle updating a column used in the WHERE clause
            let q = $"UPDATE speech_part SET speech_part.name = '{newSp}'
                        WHERE speech_part.name = '{oldSp}' AND speech_part.language = {lid}"
            let cmd = new MySqlCommand(q, con)
            cmd.ExecuteNonQuery () |> ignore
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let deleteSpeechPartHandler (logger : ILogger) stoken lid spName =
    async {
        let ouser = getUser logger stoken
        if userHasLanguage ouser lid then
            do!
                query {
                    for sp in ctx.Conlang.SpeechPart do
                    where (sp.Language = lid && sp.Name = spName)
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore
        else
            failwith $"user {ouser} does not own the language {lid}"
    }

let getSpeechPartsHandler (logger : ILogger) lid =
    async {
        let! parts =
            query {
                for sp in ctx.Conlang.SpeechPart do
                where (sp.Language = lid)
                select sp.Name
            } |> Seq.executeQueryAsync |> Async.AwaitTask
        return parts
    }
