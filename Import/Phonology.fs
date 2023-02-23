module OnlineConlang.Import.Phonology

open OnlineConlang.Constants.Diacritics
open OnlineConlang.Constants.Consonants
open OnlineConlang.Constants.Vowels
open OnlineConlang.Prelude

[<AbstractClass; Sealed>]
type IPA =
    static member Consonants =
        Set.map Pulmonic pulmonicConsonants </Set.union/>
        Set.map Ejective ejectiveConsonants </Set.union/>
        Set.map Click clickConsonants </Set.union/>
        Set.map Implosive implosiveConsonants
    static member Vowels = vowels
    static member Diacritics = diacritics
