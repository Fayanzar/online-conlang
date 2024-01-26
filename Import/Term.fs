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
        inflection        : ((string list) * ((int list) * string) list) list Option
        transcription     : string Option
    }
    with
    member this.syllabifiedTranscription lid = map (syllabifyTranscription lid) this.transcription <*> (Some syllable)
    member this.syllabifiedWord lid =
        if List.contains lid (toList transcriptionTransformations.Keys) then
            SyllabifyWord lid this.word transcriptionTransformations[lid] syllable
        else this.word
    member this.mkInflections lid =
        if List.contains lid (toList inflectTransformations.Keys) then
            let inflection = inflectTransformations[lid] |> filter (fun (sp, classes, _) ->
                sp = this.speechPart && classes = this.wordClasses
            )
            let allAxes = inflection |> map thd3
            allAxes |> map (fun axes ->
                let allNames = map (fun a -> a.inflections.Keys |> toList) axes.axes |> cartesian
                ( axes.axes |> map (fun a -> a.name)
                , map (fun names -> (names, inflect this.word axes names)) allNames)
            )
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
