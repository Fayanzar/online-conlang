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
                          // [(inflectionName, (inflectionAxes, [(axesValues, word)])]
        inflection        : (string Option * ((string list) * ((int list) * string) list)) list Option
        transcription     : string Option
    }
    with
    member this.syllabifiedTranscription lid = map (syllabifyTranscription lid) this.transcription <*> (Some syllable)
    member this.syllabifiedWord lid =
        if List.contains lid (toList transcriptionTransformations.Keys) then
            SyllabifyWord lid this.word transcriptionTransformations[lid] syllable
        else this.word
    member this.mkInflections lid =
        if ((toList inflectTransformations.Keys) |> filter (fun (l, _) -> l = lid)) <> [] then
            let inflections =
                inflectTransformations
                |> toList
                |> filter (fun a ->
                    let (l, _) = a.Key
                    let (_, sp, classes, _) = a.Value
                    sp = this.speechPart
                    && classes = this.wordClasses
                    && l = lid
                ) |> map (fun i -> i.Value)
            let axesWithNames = inflections |> map (fun (name, _, _, axes) -> (name, axes))
            axesWithNames |> map (fun (name, axes) ->
                let allNames = map (fun a -> a.inflections.Keys |> toList) axes.axes |> cartesian
                ( name,
                ( axes.axes |> map (fun a -> a.name)
                , map (fun names -> (names, inflect this.word axes names)) allNames))
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
