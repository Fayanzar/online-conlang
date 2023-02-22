module OnlineConlang.Storage.File

open System.IO

let writeTextFile path content =
    use file = File.AppendText path
    List.map (file.WriteLine : string -> unit) content |> ignore


let readTextFile path =
    try
        File.ReadAllLines path |> Array.toList
    with
        | :? IOException -> []
