module OnlineConlang.Import.Term

open OnlineConlang.Import.Phonotactics
open OnlineConlang.Import.Morphology

type Term =
    {
        word         : string
        partOfSpeech : PartOfSpeech
    }
    with
    member this.transcription = transcribeWord this.word transcriptionTransformations
    member this.syllabifiedTranscription = syllabifyTranscription this.transcription syllable
    member this.syllabifiedWord = SyllabifyWord this.word transcriptionTransformations syllable
