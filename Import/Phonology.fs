module OnlineConlang.Import.Phonology

open OnlineConlang.Constants.Consonants
open OnlineConlang.Prelude

type Consonant = Pulmonic of PulmonicConsonant
               | Ejective of EjectiveConsonant
               | Click of ClickConsonant
               | Implosive of ImplosiveConsonant

[<AbstractClass; Sealed>]
type IPA =
    static member Consonants =
        Set.map Pulmonic pulmonicConsonants </Set.union/>
        Set.map Ejective ejectiveConsonants </Set.union/>
        Set.map Click clickConsonants </Set.union/>
        Set.map Implosive implosiveConsonants
