module OnlineConlang.Import.Phonotactics

open FSharpPlus

open OnlineConlang.Import.Phonology
open OnlineConlang.Import.Transformations

let mutable transcriptionTransformations : (Transformation list) = []

let mutable syllable : string = ""

type PhonemeClasses =
    | Node of string * Phoneme Set * PhonemeClasses list
    with
    static member Root = Node
                            ( "P"
                            , (map ConsonantPhoneme IPA.Consonants) ++ (map VowelPhoneme IPA.Vowels)
                            , empty
                            )
    static member addChild pc n (k, v) =
        match pc with
        | Node (s, p, l) when Node (s, p, l) = n ->
            let newKey = if s = k then k + "'"
                                  else k
            let child = Node (newKey, p </Set.intersect/> v, empty)
            Node (s, p, child :: l)
        | Node (s, p, l) -> Node (s, p, map (fun t -> PhonemeClasses.addChild t n (k, v)) l)
    static member addChildByKey pc s (k, v) =
        match pc with
        | Node (s', p, l) when s' = s ->
            let newKey = if s' = k then k + "'"
                                   else k
            let child = Node (newKey, p </Set.intersect/> v, empty)
            Node (s', p, child :: l)
        | Node (s', p, l) -> Node (s', p, map (fun t -> PhonemeClasses.addChildByKey t s (k, v)) l)
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

    member this.findNode s =
        match this with
        | Node (s', p, l) when s = s' -> Some <| Node (s', p, l)
        | Node (_, _, l) -> choose (fun (t : PhonemeClasses) -> t.findNode s) l |> tryHead
    member this.replaceSubtree s st =
        match this with
        | Node (s', _, _) when s = s' -> st
        | Node (s, p, l) -> Node (s, p, map (fun (t : PhonemeClasses) -> t.replaceSubtree s st) l)
    member private this.getParent' pc p' =
        match pc with
        | x when x = this -> Some p'
        | Node (s, p, l) -> choose (fun t -> this.getParent' t (Node (s, p, l))) l |> tryHead
    member this.getParent pc = this.getParent' pc (Node ("", Set.empty, empty))
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

let mutable phonemeClasses =
    let root = PhonemeClasses.Root
    let consonants = PhonemeClasses.addChildByKey
                        root
                        "P"
                        ("C", map ConsonantPhoneme IPA.Consonants)
    let consonantsAndVowels = PhonemeClasses.addChildByKey
                                consonants
                                "P"
                                ("V", map VowelPhoneme IPA.Vowels)
    consonantsAndVowels

let private getPhonemeSymbols phonemeSet =
    phonemeSet |> map (fun (p : Phoneme) -> p.IPASymbol)

let rec private classifyTranscriptionChars' s acc (phonemeTree : PhonemeClasses) =
    match toList s with
    | [] ->
        let onode = phonemeTree.findLowestChild
                        (fun ps -> Set.contains acc <| getPhonemeSymbols ps)
        option (fun (Node (k, _, _)) -> k) "" onode
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
                        s
                        ""
                        phonemeClasses
            | Some (Node (k, _, _)) -> k + classifyTranscriptionChars'
                                            s
                                            ""
                                            phonemeClasses
        | _ -> classifyTranscriptionChars' (string xs) (acc + string x) filteredTree

let classifyTranscriptionChars s =
    classifyTranscriptionChars' s "" phonemeClasses

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


let syllableVariations (syllableBase : string) =
    let splitSyllable = syllableBase.Split 'V' |> toList
    let part1 = head splitSyllable |> getSubLists |> map (fun s -> s + "V")
    let part2 = fold (+) "" splitSyllable.Tail |> getSubLists
    cartesian part1 part2 |> sortBy length |> List.rev

let rec private syllabify' (s : string) syllable (vars : string list) acc =
    match s, vars with
    | "", _    -> Some acc
    | _, x::xs ->
        if s.StartsWith x then syllabify' (s.Substring <| length x) syllable (syllableVariations syllable) (x::acc)
                          else syllabify' s syllable xs acc
    | _, []    -> None

let syllabify s syllable = syllabify' s syllable (syllableVariations syllable) []

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

let syllabifyTranscription t syllable =
    let tClassified = classifyTranscriptionChars t
    match syllabify tClassified syllable with
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

let SyllabifyWord word transformations syllable =
    let tChain = mkTChain word transformations
    match tChain with
    | Forward (chain, t) ->
        let sTranscription = syllabifyTranscription t syllable
        let newChain = rev <| map (fun (x, y) -> (y, x)) chain
        let revTChain = Reverse (sTranscription, newChain)
        contractChain revTChain
    | _ -> word

let transcribeWord word transformations =
    let tChain = mkTChain word transformations
    match tChain with
    | Forward (_, t) -> t
    | _ -> word
