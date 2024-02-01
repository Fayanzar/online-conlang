module OnlineConlang.Import.JsonWebToken

open FSharpPlus

open OnlineConlang.Prelude

open System
open System.Text
open System.Security.Cryptography
open System.Collections.Generic
open System.Text.Json

let base64UrlEncode bytes = Convert.ToBase64String(bytes)
let base64UrlDecode s = Convert.FromBase64String(s) |> Encoding.UTF8.GetString
type IJwtAuthority =
    inherit IDisposable
    abstract member IssueToken : header : Dictionary<string, string> -> payload : Dictionary<string, 'T> -> string
    abstract member VerifyToken : string -> bool
    abstract member DecodeToken : string -> Dictionary<string, 'T> Option
let newJwtAuthority (initAlg: byte array -> HMAC) key =
    let alg = initAlg(key)
    let encode : string -> string = Encoding.UTF8.GetBytes >> base64UrlEncode
    let issue (header : Dictionary<string, string>) (payload : Dictionary<string, 'T>) =
        let headerEncoded = JsonSerializer.Serialize(header, jsonOptions)
        let payloadEncoded = JsonSerializer.Serialize(payload, jsonOptions)
        let parts =
            [headerEncoded; payloadEncoded]
            |> List.map encode
            |> String.concat "."
        let signature = parts |> Encoding.UTF8.GetBytes |> alg.ComputeHash |> base64UrlEncode
        [parts; signature] |> String.concat "."
    let verify (token: string) =
        let secondDot = token.LastIndexOf(".")
        let parts = token.Substring(0, secondDot)
        let signature = token.Substring(secondDot + 1)
        (parts |> Encoding.UTF8.GetBytes |> alg.ComputeHash |> base64UrlEncode) = signature
    let decode (token : string) =
        if verify token then
            let payload = token.Split(".")[1] |> base64UrlDecode
            let decodedPayload = JsonSerializer.Deserialize(payload, jsonOptions)
            Some decodedPayload
        else None

    {
        new IJwtAuthority with
            member this.IssueToken header payload = issue header payload
            member this.VerifyToken token = verify token
            member this.DecodeToken token = decode token
            member this.Dispose() = alg.Dispose()
    }

let defJWTHeader =
    let header = new Dictionary<string, string>()
    header.Add("alg", "HS256")
    header.Add("typ", "JWT")
    header
