module OnlineConlang.Import.Term

open FSharpPlus
open OnlineConlang.Prelude

open SharedModels

open OnlineConlang.Import.Morphology
open OnlineConlang.Import.Phonotactics

[<CLIMutable>]
type Term =
    {
        word              : string
        speechPart        : PartOfSpeech
        wordClasses       : Class Set
        inflection        : ((int list) * string) list Option
        transcription     : string Option
    }
    with
    member this.syllabifiedTranscription = map syllabifyTranscription this.transcription <*> (Some syllable)
    member this.syllabifiedWord lid =
        if List.contains lid (toList transcriptionTransformations.Keys) then
            SyllabifyWord this.word transcriptionTransformations[lid] syllable
        else this.word
    member this.mkInflections lid =
        if List.contains (lid, this.speechPart, this.wordClasses) (toList inflectTransformations.Keys) then
            let axes = inflectTransformations[(lid, this.speechPart, this.wordClasses)]
            let allNames = map (fun a -> a.inflections.Keys |> toList) axes.axes |> cartesian
            map (fun names -> (names, inflect this.word axes names)) allNames
        else
            []
    member this.mkTranscription lid =
        if List.contains lid (toList transcriptionTransformations.Keys) then
            transcribeWord this.word transcriptionTransformations[lid]
        else
            this.word

let parseTerm (termApi : TermForAPI) =
    { word = termApi.word
      speechPart = termApi.speechPart
      wordClasses = termApi.wordClasses
      inflection = termApi.inflection
      transcription = termApi.transcription
    }
