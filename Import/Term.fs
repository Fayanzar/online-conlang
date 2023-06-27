module OnlineConlang.Import.Term

open OnlineConlang.Import.Phonotactics
open OnlineConlang.Import.Morphology

[<CLIMutable>]
type Term =
    {
        word          : string
        transcription : string
        speechPart    : PartOfSpeech
        wordClass     : Class
    }
    with
    member this.syllabifiedTranscription = syllabifyTranscription this.transcription syllable
    member this.syllabifiedWord = SyllabifyWord this.word transcriptionTransformations syllable
