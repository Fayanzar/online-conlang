module OnlineConlang.Api.Phonemes

open FSharpPlus
open OnlineConlang.Prelude

open SharedModels

open OnlineConlang.Import.Phonology
open OnlineConlang.Import.Phonotactics
open OnlineConlang.DB.Context

open FSharp.Data.Sql

open System.Text.Json

let postPhonemeClassHandler lid (cl : char) (p : char option) =
    async {
        let parent = option id 'P' p
        let classes = query {
            for p in ctx.Conlang.PhonemeClass do
            where (p.Key = string cl && p.Language = lid)
        }
        if not <| Seq.isEmpty classes then failwith ^<| "class " + string cl + " already exists"

        let row = ctx.Conlang.PhonemeClass.Create()
        row.Key <- string cl
        row.Language <- lid
        row.Parent <- string parent
        try
            ctx.SubmitUpdates()
            phonemeClasses[lid] <- PhonemeClasses.AddChildByKey phonemeClasses[lid] parent (cl, Set.empty)
        with
            | e ->
                ctx.ClearUpdates() |> ignore
                failwith e.Message
    }

let putPhonemeClassHandler lid (cl : char) (newCl : char) =
    async {
        let classes = query {
            for p in ctx.Conlang.PhonemeClass do
            where (p.Key = string newCl && p.Key <> string cl && p.Language = lid)
            select (p.Key)
        }
        if not <| Seq.isEmpty classes then failwith ^<| "class " + string newCl + " already exists"

        query {
            for p in ctx.Conlang.PhonemeClass do
            where (p.Key = string cl && p.Language = lid)
        } |> Seq.iter (fun p -> p.Key <- string newCl)
        try
            ctx.SubmitUpdates()
            phonemeClasses[lid] <- PhonemeClasses.ReplaceKey phonemeClasses[lid] cl newCl
        with
        | e ->
            ctx.ClearUpdates() |> ignore
            failwith e.Message
    }

let rec getClassesForDeletion lid (parents : string list) =
    match parents with
    | [] -> []
    | _ ->
        let classes = query {
            for p in ctx.Conlang.PhonemeClass do
            where (List.contains p.Parent parents && p.Language = lid)
            select (p.Key)
        }
        parents @ getClassesForDeletion lid (Seq.toList classes)

let deletePhonemeClassHandler lid (cl : char) =
    async {
        let classes = getClassesForDeletion lid [string cl]
        do!
            query {
                for p in ctx.Conlang.PhonemeClass do
                where (List.contains p.Key classes)
            } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                            |> map ignore
        phonemeClasses[lid] <- PhonemeClasses.DeleteByKeys phonemeClasses[lid] <| map (fun s -> head s) classes
    }

let rec private updateChildPhonemes cl pc (phonemes : Phoneme Set) =
    match pc with
    | Node (k, v, l) ->
        let phonemesSerialized = map (fun p -> JsonSerializer.Serialize(p, jsonOptions)) phonemes
        query {
            for p in ctx.Conlang.PhonemeClassPhoneme do
            where (p.Class = cl && (not <| Set.contains p.Phoneme phonemesSerialized))
        } |> Seq.``delete all items from single table`` |> Async.AwaitTask |> Async.RunSynchronously |> ignore
        Node (k, v </Set.intersect/> phonemes, map (fun p -> updateChildPhonemes cl p phonemes) l)

let putPhonemesPhonemeClass lid (cl : char) (phonemes : Phoneme Set) =
    async {
        let classes = query {
                            for p in ctx.Conlang.PhonemeClass do
                            where (p.Key = string cl && p.Language = lid)
                            select p.Id
                        } |> Seq.toList
        match classes with
        | [] -> failwith "no such class"
        | [classId] ->
            let oparent = PhonemeClasses.GetParentByKey phonemeClasses[lid] cl
            let allowedPhonemes =
                match oparent with
                | None -> phonemes
                | Some (Node (_, v, _)) -> phonemes </Set.intersect/> v
            do!
                query {
                    for p in ctx.Conlang.PhonemeClassPhoneme do
                    where (p.Class = classId)
                    select p.Phoneme
                } |> Seq.``delete all items from single table`` |> Async.AwaitTask
                                                                |> map ignore
            for p in allowedPhonemes do
                let row = ctx.Conlang.PhonemeClassPhoneme.Create()
                row.Class <- classId
                row.Phoneme <- JsonSerializer.Serialize(p, jsonOptions)
            try
                ctx.SubmitUpdates()
                phonemeClasses[lid] <- PhonemeClasses.ReplacePhonemesByKey phonemeClasses[lid] cl allowedPhonemes
                let onode = phonemeClasses[lid].findNode cl
                match onode with
                | None -> ()
                | Some node ->
                    let updatedSubtree = updateChildPhonemes classId node allowedPhonemes
                    phonemeClasses[lid] <- phonemeClasses[lid].replaceSubtree cl updatedSubtree
            with
                | e ->
                    ctx.ClearUpdates() |> ignore
                    failwith e.Message
        | _ -> failwith "more than one class"
    }
