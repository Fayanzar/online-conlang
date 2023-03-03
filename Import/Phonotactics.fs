module OnlineConlang.Import.Phonotactics

open OnlineConlang.Import.Phonology
open OnlineConlang.Import.Transformations

open FSharpPlus

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

type TransformationChain = Transformation list * string

let private getPhonemeSymbols phonemeSet =
    phonemeSet |> map (fun (p : Phoneme) -> p.IPASymbol)

let rec private classifyTranscriptionChars' s acc (phonemeTree : PhonemeClasses) =
    match Seq.toList s with
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
