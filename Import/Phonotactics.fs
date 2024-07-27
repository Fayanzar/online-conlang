module OnlineConlang.Import.Phonotactics

open FSharpPlus

open SharedModels

open OnlineConlang.DB.Context
open OnlineConlang.Import.Phonology
open OnlineConlang.Import.Transformations

open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Serialization

let jsonOptions =
    JsonFSharpOptions.Default()
        .WithUnionExternalTag()
        .WithUnionNamedFields()
        .ToJsonSerializerOptions()

let transcriptionTransformations = new Dictionary<int, Transformation list>()

let updateTranscriptionTransformations lid =
    let transformations = query {
                                for r in ctx.MarraidhConlang.TranscriptionRule do
                                where (r.Language = lid)
                            } |> Seq.map (fun r -> JsonSerializer.Deserialize(r.Rule, jsonOptions)) |> toList
    transcriptionTransformations[lid] <- transformations

let mutable syllable : string = ""

type PhonemeClasses =
    | Node of char * Phoneme Set * PhonemeClasses list
    with
    static member Root = Node
                            ( 'P'
                            , (map ConsonantPhoneme IPA.Consonants) ++ (map VowelPhoneme IPA.Vowels)
                            , empty
                            )
    static member AddChild pc n (k, v) =
        match pc with
        | Node (s, p, l) when Node (s, p, l) = n ->
            let child = Node (k, p </Set.intersect/> v, empty)
            Node (s, p, child :: l)
        | Node (s, p, l) -> Node (s, p, map (fun t -> PhonemeClasses.AddChild t n (k, v)) l)
    static member AddChildByKey pc s (k, v) =
        match pc with
        | Node (s', p, l) when s' = s ->
            let child = Node (k, p </Set.intersect/> v, empty)
            Node (s', p, child :: l)
        | Node (s', p, l) -> Node (s', p, map (fun t -> PhonemeClasses.AddChildByKey t s (k, v)) l)
    static member Filter pc predicate =
        match pc with
        | Node (k, v, l) ->
            let filteredNodes =
                choose (fun (Node (k', v', l')) ->
                            let filteredSet = filter predicate v'
                            if filteredSet.IsEmpty
                                then None
                                else Some <| Node (k', filteredSet, l')
                       ) l
            Node ( k
                 , filter predicate v
                 , map (flip PhonemeClasses.Filter predicate) filteredNodes
                 )
    static member GetParentByKey pc s =
        match pc with
        | Node (k, v, l) ->
            match l |> tryFind (fun (t : PhonemeClasses) -> t.getKey = s) with
            | Some _ -> Some (Node (k, v, l))
            | None   -> choose (fun t -> PhonemeClasses.GetParentByKey t s) l |> tryHead
    static member ReplacePhonemesByKey pc s phonemes =
        match pc with
        | Node (k, v, l) ->
            if k = s then Node (k, phonemes, l)
                     else Node (k, v, map (fun p -> PhonemeClasses.ReplacePhonemesByKey p s phonemes) l)
    static member ReplaceKey pc s newS =
        match pc with
        | Node (k, v, l) ->
            if k = s then Node (newS, v, l)
                     else Node (k, v, map (fun p -> PhonemeClasses.ReplaceKey p s newS) l)
    static member DeleteByKeys pc keys =
        match pc with
        | Node (k, v, l) ->
            let children = filter (fun (Node (k', _, _)) -> not <| List.contains k' keys) l
            Node (k, v, map (fun p -> PhonemeClasses.DeleteByKeys p keys) children)

    member this.getParent pc = PhonemeClasses.GetParentByKey pc (this.getKey)
    member this.findNode s =
        match this with
        | Node (s', p, l) when s = s' -> Some <| Node (s', p, l)
        | Node (_, _, l) -> choose (fun (t : PhonemeClasses) -> t.findNode s) l |> tryHead
    member this.replaceSubtree s st =
        match this with
        | Node (s', _, _) when s = s' -> st
        | Node (s, p, l) -> Node (s, p, map (fun (t : PhonemeClasses) -> t.replaceSubtree s st) l)
    member this.getChildren =
        match this with
        | Node (_, _, l) -> l
    member this.getKeyVal =
        match this with
        | Node (k, v, _) -> (k, v)
    member this.getKey = fst this.getKeyVal
    member this.getVal = snd this.getKeyVal
    member this.findLowestChild predicate =
        match this with
        | Node (k, v, l) when predicate v ->
            let olowestChild = choose (fun (t : PhonemeClasses) -> t.findLowestChild predicate) l |> tryHead
            match olowestChild with
            | None -> Some <| Node (k, v, l)
            | Some n -> Some n
        | _ -> None
    override this.ToString() =
        match this with
        | Node (s, p, l) -> $""

let phonemeClasses = new Dictionary<int, PhonemeClasses>()

let rec private buildNodes pc pList phonemesAndClasses =
    match phonemesAndClasses with
    | [] -> pc
    | _ ->
        let children = phonemesAndClasses |> filter (fun (k, _) -> List.contains (snd k) pList)
        let notChildren = phonemesAndClasses |> filter (fun (k, _) -> not <| List.contains (snd k) pList)
        let deserializePhonemes (phonemes : string list) : Phoneme Set =
            phonemes |> List.map (fun p -> JsonSerializer.Deserialize(p, jsonOptions)) |> Set
        let newPc = fold (fun x ((k, p), v) ->
            PhonemeClasses.AddChildByKey x p (k, deserializePhonemes v)) pc children
        let newParentList = map (fst >> fst) children
        buildNodes newPc newParentList notChildren

let updatePhonemeClasses lid =
    let phonemesAndClasses =
        query {
            for p in ctx.MarraidhConlang.Phoneme do
            join pcp in ctx.MarraidhConlang.PhonemeClassPhoneme on (p.Phoneme = pcp.Phoneme)
            join pc in ctx.MarraidhConlang.PhonemeClass on (pcp.Class = pc.Id)
            where (pc.Language = lid)
            select ((pc.Key, pc.Parent), p.Phoneme)
        } |> Seq.groupBy fst |> Seq.map (fun ((cl, p), v) -> ((cl[0], p[0]), map snd v |> toList)) |> toList
    let rootNode = PhonemeClasses.Root
    let nodes = buildNodes rootNode ['P'] phonemesAndClasses
    phonemeClasses[lid] <- nodes

let basicPhonemeClasses =
    let root = PhonemeClasses.Root
    let consonants = PhonemeClasses.AddChildByKey
                        root
                        'P'
                        ('C', map ConsonantPhoneme IPA.Consonants)
    let consonantsAndVowels = PhonemeClasses.AddChildByKey
                                consonants
                                'P'
                                ('V', map VowelPhoneme IPA.Vowels)
    consonantsAndVowels

let private getPhonemeSymbols phonemeSet =
    phonemeSet |> map (fun (p : Phoneme) -> p.IPASymbol)

let rec private classifyTranscriptionChars' l s acc (phonemeTree : PhonemeClasses) =
    match toList s with
    | [] ->
        let onode = phonemeTree.findLowestChild
                        (fun ps -> Set.contains acc <| getPhonemeSymbols ps)
        option (fun (Node (k, _, _)) -> string k) "" onode
    | x :: xs ->
        let filteredTree = PhonemeClasses.Filter
                                phonemeTree
                                (fun p -> p.IPASymbol.StartsWith (acc + string x))
        match filteredTree with
        | Node (_, v, []) when v.IsEmpty ->
            let onode = phonemeTree.findLowestChild
                            (fun ps -> Set.contains acc <| getPhonemeSymbols ps)
            match onode with
            | None -> classifyTranscriptionChars'
                        l
                        s
                        ""
                        phonemeClasses[l]
            | Some (Node (k, _, _)) -> string k + classifyTranscriptionChars'
                                            l
                                            s
                                            ""
                                            phonemeClasses[l]
        | _ -> classifyTranscriptionChars' l (string xs) (acc + string x) filteredTree

let classifyTranscriptionChars l s =
    classifyTranscriptionChars' l s "" phonemeClasses[l]

let rec private getSubLists l =
    [ yield l
      for x in toList l do
        let rest = toList l |> List.except [x]
        yield string rest
        yield! getSubLists (string rest)
    ]

let rec private cartesian (p1 : string list) p2 =
    match p1, p2 with
    | _, [] -> []
    | [], _ -> []
    | x::xs, _ -> (map (fun y -> x + y) p2) @ (cartesian xs p2)

let rec getAllClasses l c =
    c :: match PhonemeClasses.GetParentByKey phonemeClasses[l] c with
            | None -> []
            | Some (Node (k, _, _)) -> k :: getAllClasses l k

let rec variateClasses l s =
    s |> toList |> map (getAllClasses l >> map string) |> fold cartesian [""]

let syllableVariations l (syllableBase : string) =
    let splitSyllable = syllableBase.Split 'V' |> toList
    let part1 = head splitSyllable |> getSubLists |> distinct |> map (fun s -> s + "V") |> List.collect (variateClasses l)
    let part2 = fold (+) "" splitSyllable.Tail |> getSubLists |> distinct |> List.collect (variateClasses l)
    cartesian part1 part2 |> sortBy length |> List.rev

let rec private syllabify' l (s : string) syllable (vars : string list) acc =
    match s, vars with
    | "", _    -> Some acc
    | _, x::xs ->
        if s.StartsWith x then syllabify' l (s.Substring <| length x) syllable (syllableVariations l syllable) (x::acc)
                          else syllabify' l s syllable xs acc
    | _, []    -> None

let syllabify l s syllable = syllabify' l s syllable (syllableVariations l syllable) []

let rec private insertDots s syllableLengths k =
    match toList s, syllableLengths with
    | x::xs, y::ys ->
        if y = k then insertDots (string (x :: '.' :: xs)) ys k
                     else insertDots s syllableLengths (k + 1)
    | _, _ -> s

let rec private moveDots (t : char list) inj (word : char list) (k : int) acc =
    match inj with
    | ((x1, x2), (y1, y2))::xs ->
        let sub1 = t[x1 + k .. x2 + k]
        let sub2 = word[y1 .. y2]
        let dotCount = filter ((=) '.') sub1 |> length
        match dotCount with
        | 0 -> moveDots t xs word k (acc @ sub2)
        | n -> moveDots t xs word (k + n) (acc @ sub2 @ List.replicate n '.')
    | _ -> acc

let syllabifyTranscription l t syllable =
    let tClassified = classifyTranscriptionChars l t
    match syllabify l tClassified syllable with
    | None -> t
    | Some tSyllabified ->
        let syllableLengths = map length tSyllabified
        insertDots t syllableLengths 1

let rec private contractChain chain =
    match chain with
    | Reverse (transcription, chainList) ->
        match chainList with
        | [] -> transcription
        | (transformation, word)::xs ->
            let newWord = moveDots (toList transcription)
                                   (Map.toList transformation.RevInjection)
                                   (toList word)
                                   0
                                   [] |> string
            contractChain <| Reverse (newWord, xs)
    | _ -> ""

let SyllabifyWord l word transformations syllable =
    let tChain = mkTChain word transformations
    match tChain with
    | Forward (chain, t) ->
        let sTranscription = syllabifyTranscription l t syllable
        let newChain = rev <| map (fun (x, y) -> (y, x)) chain
        let revTChain = Reverse (sTranscription, newChain)
        contractChain revTChain
    | _ -> word

let transcribeWord word transformations =
    let tChain = mkTChain word transformations
    match tChain with
    | Forward (_, t) -> t
    | _ -> word
