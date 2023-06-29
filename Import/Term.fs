module OnlineConlang.Import.Term

open FSharpPlus
open OnlineConlang.Prelude

open OnlineConlang.Import.Morphology
open OnlineConlang.Import.Phonotactics

[<CLIMutable>]
type Term =
    {
        word          : string
        transcription : string Option
        inflection    : ((int list) * string) list Option
        speechPart    : PartOfSpeech
        wordClass     : Class
    }
    with
    member this.syllabifiedTranscription = map syllabifyTranscription this.transcription <*> (Some syllable)
    member this.syllabifiedWord lid = SyllabifyWord this.word transcriptionTransformations[lid] syllable
    member this.mkInflections lid =
        let axes = inflectTransformations[(lid, this.speechPart, this.wordClass)]
        let allNames = map (fun a -> a.inflections.Keys |> toList) axes.axes |> cartesian
        map (fun names -> (names, inflect this.word axes names)) allNames
    member this.mkTranscription lid = transcribeWord this.word transcriptionTransformations[lid]
