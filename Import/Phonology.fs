module OnlineConlang.Import.Phonology

open FSharpPlus

open OnlineConlang.Foundation

open OnlineConlang.Constants.Diacritics
open OnlineConlang.Constants.Consonants
open OnlineConlang.Constants.Suprasegmentals
open OnlineConlang.Constants.Tones
open OnlineConlang.Constants.Vowels

type Phoneme =
    | ConsonantPhoneme of Consonant
    | VowelPhoneme of Vowel
    member this.IPASymbol = (this :> IIPARepresentable).IPASymbol
    interface IIPARepresentable with
        member this.IPASymbol =
            match this with
            | ConsonantPhoneme c -> c.IPASymbol
            | VowelPhoneme v -> v.IPASymbol

[<AbstractClass; Sealed>]
type IPA =
    static member Consonants =
        Set.map Pulmonic pulmonicConsonants </Set.union/>
        Set.map Ejective ejectiveConsonants </Set.union/>
        Set.map Click clickConsonants </Set.union/>
        Set.map Implosive implosiveConsonants
    static member Vowels = vowels
    static member Diacritics = diacritics
    static member Tones = tones
    static member Suprasegmentals = suprasegmentals

    static member private GetChao' tone =
        match tone with
            | Tone (_, Chao, _) -> Some tone
            | Tone (pitch, _, _) ->
                Set.filter
                    (fun t -> match t with
                                | Tone (p, Chao, _) when p = pitch -> true
                                | _                                -> false)
                    IPA.Tones |> tryHead
    static member ToneToNumber tone =
        match IPA.GetChao' tone with
            | Some (Tone (_, _, s)) ->
                choose
                    (fun c -> match c with
                                | '˥' -> Some '5'
                                | '˦' -> Some '4'
                                | '˧' -> Some '3'
                                | '˨' -> Some '2'
                                | '˩' -> Some '1'
                                | _   -> None)
                    s |> string
            | _ -> ""
