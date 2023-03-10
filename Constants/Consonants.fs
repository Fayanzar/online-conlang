module OnlineConlang.Constants.Consonants

open OnlineConlang.Foundation

type MannerArticulation = Nasal
                        | Plosive
                        | SibilantAffricate
                        | NonSibilantAffricate
                        | SibilantFricative
                        | NonSibilantFricative
                        | Approximant
                        | Tap
                        | Trill
                        | LateralAffricate
                        | LateralFricative
                        | LateralApproximant
                        | LateralTap

type Voicedness = Unvoiced | Voiced

type PlaceArticulation = Bilabial
                       | Labiodental
                       | Linguolabial
                       | Dental
                       | Alveolar
                       | Postalveolar
                       | Retroflex
                       | Palatal
                       | Velar
                       | Uvular
                       | Pharyngeal
                       | Glottal

type MannerArticulationClick = TenuisNonLateral
                             | VoicedNonLateral
                             | NasalNonLateral
                             | TenuisLateral
                             | VoicedLateral
                             | NasalLateral

type PulmonicConsonant =
    | BasicConsonant of PlaceArticulation * MannerArticulation * Voicedness * string
    | CoArtConsonant of PlaceArticulation * PlaceArticulation * MannerArticulation * Voicedness * string
    member this.IPASymbol = (this :> IIPARepresentable).IPASymbol
    interface IIPARepresentable with
        member this.IPASymbol =
            match this with
            | BasicConsonant (_, _, _, s) -> s
            | CoArtConsonant (_, _, _, _, s) -> s

type EjectiveConsonant =
    | EjectiveConsonant of PlaceArticulation * MannerArticulation * string
    member this.IPASymbol = (this :> IIPARepresentable).IPASymbol
    interface IIPARepresentable with
        member this.IPASymbol =
            match this with
            | EjectiveConsonant (_, _, s) -> s

type ClickConsonant =
    | ClickConsonant of PlaceArticulation * PlaceArticulation * MannerArticulationClick * string
    member this.IPASymbol = (this :> IIPARepresentable).IPASymbol
    interface IIPARepresentable with
        member this.IPASymbol =
            match this with
            | ClickConsonant (_, _, _, s) -> s

type ImplosiveConsonant =
    | ImplosiveConsonant of PlaceArticulation * Voicedness * string
    member this.IPASymbol = (this :> IIPARepresentable).IPASymbol
    interface IIPARepresentable with
        member this.IPASymbol =
            match this with
            | ImplosiveConsonant (_, _, s) -> s

type Consonant =
    | Pulmonic of PulmonicConsonant
    | Ejective of EjectiveConsonant
    | Click of ClickConsonant
    | Implosive of ImplosiveConsonant
    member this.IPASymbol = (this :> IIPARepresentable).IPASymbol
    interface IIPARepresentable with
        member this.IPASymbol =
            match this with
            | Pulmonic  c -> c.IPASymbol
            | Ejective  c -> c.IPASymbol
            | Click     c -> c.IPASymbol
            | Implosive c -> c.IPASymbol

let pulmonicConsonants = Set.ofList [
    BasicConsonant (Bilabial, Nasal, Unvoiced, "m??")
    BasicConsonant (Bilabial, Nasal, Voiced, "m")
    BasicConsonant (Labiodental, Nasal, Voiced, "??")
    BasicConsonant (Linguolabial, Nasal, Voiced, "n??")
    BasicConsonant (Alveolar, Nasal, Unvoiced, "n??")
    BasicConsonant (Alveolar, Nasal, Voiced, "n")
    BasicConsonant (Retroflex, Nasal, Unvoiced, "????")
    BasicConsonant (Retroflex, Nasal, Voiced, "??")
    BasicConsonant (Palatal, Nasal, Unvoiced, "????")
    BasicConsonant (Palatal, Nasal, Voiced, "??")
    BasicConsonant (Velar, Nasal, Unvoiced, "????")
    BasicConsonant (Velar, Nasal, Voiced, "??")
    BasicConsonant (Uvular, Nasal, Voiced, "??")

    BasicConsonant (Bilabial, Plosive, Unvoiced, "p")
    BasicConsonant (Bilabial, Plosive, Voiced, "b")
    BasicConsonant (Labiodental, Plosive, Unvoiced, "p??")
    BasicConsonant (Labiodental, Plosive, Voiced, "b??")
    BasicConsonant (Linguolabial, Plosive, Unvoiced, "t??")
    BasicConsonant (Linguolabial, Plosive, Voiced, "d??")
    BasicConsonant (Alveolar, Plosive, Unvoiced, "t")
    BasicConsonant (Alveolar, Plosive, Voiced, "d")
    BasicConsonant (Retroflex, Plosive, Unvoiced, "??")
    BasicConsonant (Retroflex, Plosive, Voiced, "??")
    BasicConsonant (Palatal, Plosive, Unvoiced, "c")
    BasicConsonant (Palatal, Plosive, Voiced, "??")
    BasicConsonant (Velar, Plosive, Unvoiced, "k")
    BasicConsonant (Velar, Plosive, Voiced, "??")
    BasicConsonant (Uvular, Plosive, Unvoiced, "q")
    BasicConsonant (Uvular, Plosive, Voiced, "??")
    BasicConsonant (Pharyngeal, Plosive, Unvoiced, "??")
    BasicConsonant (Glottal, Plosive, Unvoiced, "??")

    BasicConsonant (Alveolar, SibilantAffricate, Unvoiced, "t??s")
    BasicConsonant (Alveolar, SibilantAffricate, Voiced, "d??z")
    BasicConsonant (Postalveolar, SibilantAffricate, Unvoiced, "t??????")
    BasicConsonant (Postalveolar, SibilantAffricate, Voiced, "d??????")
    BasicConsonant (Retroflex, SibilantAffricate, Unvoiced, "t????")
    BasicConsonant (Retroflex, SibilantAffricate, Voiced, "d????")
    BasicConsonant (Palatal, SibilantAffricate, Unvoiced, "t????")
    BasicConsonant (Palatal, SibilantAffricate, Voiced, "d????")

    BasicConsonant (Bilabial, NonSibilantAffricate, Unvoiced, "p????")
    BasicConsonant (Bilabial, NonSibilantAffricate, Voiced, "b????")
    BasicConsonant (Labiodental, NonSibilantAffricate, Unvoiced, "p????f")
    BasicConsonant (Labiodental, NonSibilantAffricate, Voiced, "b????v")
    BasicConsonant (Dental, NonSibilantAffricate, Unvoiced, "t??????")
    BasicConsonant (Dental, NonSibilantAffricate, Voiced, "d??????")
    BasicConsonant (Alveolar, NonSibilantAffricate, Unvoiced, "t????????")
    BasicConsonant (Alveolar, NonSibilantAffricate, Voiced, "d??????")
    BasicConsonant (Postalveolar, NonSibilantAffricate, Unvoiced, "t????????????")
    BasicConsonant (Postalveolar, NonSibilantAffricate, Voiced, "d??????????")
    BasicConsonant (Palatal, NonSibilantAffricate, Unvoiced, "c????")
    BasicConsonant (Palatal, NonSibilantAffricate, Voiced, "??????")
    BasicConsonant (Velar, NonSibilantAffricate, Unvoiced, "k??x")
    BasicConsonant (Velar, NonSibilantAffricate, Voiced, "??????")
    BasicConsonant (Uvular, NonSibilantAffricate, Unvoiced, "q????")
    BasicConsonant (Uvular, NonSibilantAffricate, Voiced, "??????")
    BasicConsonant (Pharyngeal, NonSibilantAffricate, Unvoiced, "??????")
    BasicConsonant (Pharyngeal, NonSibilantAffricate, Voiced, "??????")
    BasicConsonant (Glottal, NonSibilantAffricate, Unvoiced, "????h")

    BasicConsonant (Alveolar, SibilantFricative, Unvoiced, "s")
    BasicConsonant (Alveolar, SibilantFricative, Voiced, "z")
    BasicConsonant (Postalveolar, SibilantFricative, Unvoiced, "??")
    BasicConsonant (Postalveolar, SibilantFricative, Voiced, "??")
    BasicConsonant (Retroflex, SibilantFricative, Unvoiced, "??")
    BasicConsonant (Retroflex, SibilantFricative, Voiced, "??")
    BasicConsonant (Palatal, SibilantFricative, Unvoiced, "??")
    BasicConsonant (Palatal, SibilantFricative, Voiced, "??")

    BasicConsonant (Bilabial, NonSibilantFricative, Unvoiced, "??")
    BasicConsonant (Bilabial, NonSibilantFricative, Voiced, "??")
    BasicConsonant (Labiodental, NonSibilantFricative, Unvoiced, "f")
    BasicConsonant (Labiodental, NonSibilantFricative, Voiced, "v")
    BasicConsonant (Linguolabial, NonSibilantFricative, Unvoiced, "????")
    BasicConsonant (Linguolabial, NonSibilantFricative, Voiced, "????")
    BasicConsonant (Dental, NonSibilantFricative, Unvoiced, "??")
    BasicConsonant (Dental, NonSibilantFricative, Voiced, "??")
    BasicConsonant (Alveolar, NonSibilantFricative, Unvoiced, "????")
    BasicConsonant (Alveolar, NonSibilantFricative, Voiced, "????")
    BasicConsonant (Postalveolar, NonSibilantFricative, Unvoiced, "????????")
    BasicConsonant (Postalveolar, NonSibilantFricative, Voiced, "??????")
    BasicConsonant (Retroflex, NonSibilantFricative, Unvoiced, "??????")
    BasicConsonant (Retroflex, NonSibilantFricative, Voiced, "????")
    BasicConsonant (Palatal, NonSibilantFricative, Unvoiced, "??")
    BasicConsonant (Palatal, NonSibilantFricative, Voiced, "??")
    BasicConsonant (Velar, NonSibilantFricative, Unvoiced, "x")
    BasicConsonant (Velar, NonSibilantFricative, Voiced, "??")
    BasicConsonant (Uvular, NonSibilantFricative, Unvoiced, "??")
    BasicConsonant (Uvular, NonSibilantFricative, Voiced, "??")
    BasicConsonant (Pharyngeal, NonSibilantFricative, Unvoiced, "??")
    BasicConsonant (Pharyngeal, NonSibilantFricative, Voiced, "??")
    BasicConsonant (Glottal, NonSibilantFricative, Unvoiced, "h")
    BasicConsonant (Glottal, NonSibilantFricative, Voiced, "??")

    BasicConsonant (Labiodental, Approximant, Voiced, "??")
    BasicConsonant (Alveolar, Approximant, Voiced, "??")
    BasicConsonant (Retroflex, Approximant, Voiced, "??")
    BasicConsonant (Palatal, Approximant, Voiced, "j")
    BasicConsonant (Velar, Approximant, Voiced, "??")
    BasicConsonant (Glottal, Approximant, Voiced, "????")

    BasicConsonant (Bilabial, Tap, Voiced, "?????")
    BasicConsonant (Labiodental, Tap, Voiced, "???")
    BasicConsonant (Linguolabial, Tap, Voiced, "????")
    BasicConsonant (Alveolar, Tap, Unvoiced, "????")
    BasicConsonant (Alveolar, Tap, Voiced, "??")
    BasicConsonant (Retroflex, Tap, Unvoiced, "????")
    BasicConsonant (Retroflex, Tap, Voiced, "??")
    BasicConsonant (Velar, Tap, Voiced, "????")
    BasicConsonant (Uvular, Tap, Voiced, "????")
    BasicConsonant (Pharyngeal, Tap, Voiced, "????")

    BasicConsonant (Bilabial, Trill, Unvoiced, "????")
    BasicConsonant (Bilabial, Trill, Voiced, "??")
    BasicConsonant (Alveolar, Trill, Unvoiced, "r??")
    BasicConsonant (Alveolar, Trill, Voiced, "r")
    BasicConsonant (Retroflex, Trill, Unvoiced, "????r??")
    BasicConsonant (Retroflex, Trill, Voiced, "??r")
    BasicConsonant (Uvular, Trill, Unvoiced, "????")
    BasicConsonant (Uvular, Trill, Voiced, "??")
    BasicConsonant (Pharyngeal, Trill, Unvoiced, "??")
    BasicConsonant (Pharyngeal, Trill, Voiced, "??")

    BasicConsonant (Alveolar, LateralAffricate, Unvoiced, "t????")
    BasicConsonant (Alveolar, LateralAffricate, Voiced, "d????")
    BasicConsonant (Retroflex, LateralAffricate, Unvoiced, "t?????")
    BasicConsonant (Retroflex, LateralAffricate, Voiced, "d??????")
    BasicConsonant (Palatal, LateralAffricate, Unvoiced, "c????????")
    BasicConsonant (Palatal, LateralAffricate, Voiced, "????????")
    BasicConsonant (Velar, LateralAffricate, Unvoiced, "k??????")
    BasicConsonant (Velar, LateralAffricate, Voiced, "????????")

    BasicConsonant (Alveolar, LateralFricative, Unvoiced, "??")
    BasicConsonant (Alveolar, LateralFricative, Voiced, "??")
    BasicConsonant (Retroflex, LateralFricative, Unvoiced, "???")
    BasicConsonant (Retroflex, LateralFricative, Voiced, "????")
    BasicConsonant (Palatal, LateralFricative, Unvoiced, "??????")
    BasicConsonant (Palatal, LateralFricative, Voiced, "????")
    BasicConsonant (Velar, LateralFricative, Unvoiced, "????")
    BasicConsonant (Velar, LateralFricative, Voiced, "????")

    BasicConsonant (Alveolar, LateralApproximant, Voiced, "l")
    BasicConsonant (Retroflex, LateralApproximant, Voiced, "??")
    BasicConsonant (Palatal, LateralApproximant, Voiced, "??")
    BasicConsonant (Velar, LateralApproximant, Voiced, "??")
    BasicConsonant (Uvular, LateralApproximant, Voiced, "????")

    BasicConsonant (Alveolar, LateralTap, Unvoiced, "????")
    BasicConsonant (Alveolar, LateralTap, Voiced, "??")
    BasicConsonant (Retroflex, LateralTap, Unvoiced, "??????")
    BasicConsonant (Retroflex, LateralTap, Voiced, "????")
    BasicConsonant (Palatal, LateralTap, Voiced, "????")
    BasicConsonant (Velar, LateralTap, Voiced, "????")

    CoArtConsonant (Bilabial, Alveolar, Nasal, Voiced, "n??m")
    CoArtConsonant (Bilabial, Velar, Nasal, Voiced, "????m")
    CoArtConsonant (Bilabial, Alveolar, Plosive, Unvoiced, "t??p")
    CoArtConsonant (Bilabial, Alveolar, Plosive, Voiced, "d??b")
    CoArtConsonant (Bilabial, Velar, Plosive, Unvoiced, "k??p")
    CoArtConsonant (Bilabial, Velar, Plosive, Voiced, "g??b")
    CoArtConsonant (Uvular, Pharyngeal, Plosive, Unvoiced, "q????")

    CoArtConsonant (Bilabial, Palatal, Approximant, Unvoiced, "????")
    CoArtConsonant (Bilabial, Palatal, Approximant, Voiced, "??")
    CoArtConsonant (Bilabial, Velar, Approximant, Unvoiced, "??")
    CoArtConsonant (Bilabial, Velar, Approximant, Voiced, "w")

    CoArtConsonant (Labiodental, Velar, NonSibilantFricative, Unvoiced, "??")
    CoArtConsonant (Alveolar, Velar, LateralApproximant, Voiced, "??")
    ]

let ejectiveConsonants = Set.ofList [
    EjectiveConsonant (Bilabial, Plosive, "p??")
    EjectiveConsonant (Alveolar, Plosive, "t??")
    EjectiveConsonant (Retroflex, Plosive, "????")
    EjectiveConsonant (Palatal, Plosive, "c??")
    EjectiveConsonant (Velar, Plosive, "k??")
    EjectiveConsonant (Uvular, Plosive, "q??")
    EjectiveConsonant (Pharyngeal, Plosive, "????")

    EjectiveConsonant (Alveolar, SibilantAffricate, "t??s??")
    EjectiveConsonant (Postalveolar, SibilantAffricate, "t????????")
    EjectiveConsonant (Retroflex, SibilantAffricate, "t??????")

    EjectiveConsonant (Dental, NonSibilantAffricate, "t????????")
    EjectiveConsonant (Velar, NonSibilantAffricate, "k??x??")
    EjectiveConsonant (Uvular, NonSibilantAffricate, "q??????")

    EjectiveConsonant (Alveolar, SibilantFricative, "s??")
    EjectiveConsonant (Postalveolar, SibilantFricative, "????")
    EjectiveConsonant (Retroflex, SibilantFricative, "????")
    EjectiveConsonant (Palatal, SibilantFricative, "????")

    EjectiveConsonant (Bilabial, NonSibilantFricative, "????")
    EjectiveConsonant (Labiodental, NonSibilantFricative, "f??")
    EjectiveConsonant (Dental, NonSibilantFricative, "????")
    EjectiveConsonant (Velar, NonSibilantFricative, "x??")
    EjectiveConsonant (Uvular, NonSibilantFricative, "????")

    EjectiveConsonant (Alveolar, LateralAffricate, "t??????")
    EjectiveConsonant (Palatal, LateralAffricate, "c??????????")
    EjectiveConsonant (Velar, LateralAffricate, "k????????")

    EjectiveConsonant (Alveolar, LateralFricative, "????")
    ]

let clickConsonants = Set.ofList [
    ClickConsonant (Velar, Bilabial, TenuisNonLateral, "k????")
    ClickConsonant (Velar, Dental, TenuisNonLateral, "k????")
    ClickConsonant (Velar, Alveolar, TenuisNonLateral, "k????")
    ClickConsonant (Velar, Retroflex, TenuisNonLateral, "k?????")
    ClickConsonant (Velar, Palatal, TenuisNonLateral, "k????")

    ClickConsonant (Uvular, Bilabial, TenuisNonLateral, "q????")
    ClickConsonant (Uvular, Dental, TenuisNonLateral, "q????")
    ClickConsonant (Uvular, Alveolar, TenuisNonLateral, "q????")
    ClickConsonant (Uvular, Retroflex, TenuisNonLateral, "q?????")
    ClickConsonant (Uvular, Palatal, TenuisNonLateral, "q????")

    ClickConsonant (Velar, Bilabial, VoicedNonLateral, "??????")
    ClickConsonant (Velar, Dental, VoicedNonLateral, "??????")
    ClickConsonant (Velar, Alveolar, VoicedNonLateral, "??????")
    ClickConsonant (Velar, Retroflex, VoicedNonLateral, "???????")
    ClickConsonant (Velar, Palatal, VoicedNonLateral, "??????")

    ClickConsonant (Uvular, Bilabial, VoicedNonLateral, "??????")
    ClickConsonant (Uvular, Dental, VoicedNonLateral, "??????")
    ClickConsonant (Uvular, Alveolar, VoicedNonLateral, "??????")
    ClickConsonant (Uvular, Retroflex, VoicedNonLateral, "???????")
    ClickConsonant (Uvular, Palatal, VoicedNonLateral, "??????")

    ClickConsonant (Velar, Bilabial, NasalNonLateral, "??????")
    ClickConsonant (Velar, Dental, NasalNonLateral, "??????")
    ClickConsonant (Velar, Alveolar, NasalNonLateral, "??????")
    ClickConsonant (Velar, Retroflex, NasalNonLateral, "???????")
    ClickConsonant (Velar, Palatal, NasalNonLateral, "??????")
    ClickConsonant (Velar, Velar, NasalNonLateral, "??")

    ClickConsonant (Uvular, Bilabial, NasalNonLateral, "??????")
    ClickConsonant (Uvular, Dental, NasalNonLateral, "??????")
    ClickConsonant (Uvular, Alveolar, NasalNonLateral, "??????")
    ClickConsonant (Uvular, Retroflex, NasalNonLateral, "???????")
    ClickConsonant (Uvular, Palatal, NasalNonLateral, "??????")

    ClickConsonant (Velar, Alveolar, TenuisLateral, "k????")

    ClickConsonant (Uvular, Alveolar, TenuisLateral, "q????")

    ClickConsonant (Velar, Alveolar, VoicedLateral, "??????")

    ClickConsonant (Uvular, Alveolar, VoicedLateral, "??????")

    ClickConsonant (Velar, Alveolar, NasalLateral, "??????")

    ClickConsonant (Uvular, Alveolar, NasalLateral, "??????")
    ]

let implosiveConsonants = Set.ofList [
    ImplosiveConsonant (Bilabial, Unvoiced, "????")
    ImplosiveConsonant (Bilabial, Voiced, "??")

    ImplosiveConsonant (Alveolar, Unvoiced, "????")
    ImplosiveConsonant (Alveolar, Voiced, "??")

    ImplosiveConsonant (Retroflex, Unvoiced, "?????")
    ImplosiveConsonant (Retroflex, Voiced, "???")

    ImplosiveConsonant (Palatal, Unvoiced, "????")
    ImplosiveConsonant (Palatal, Voiced, "??")

    ImplosiveConsonant (Velar, Unvoiced, "????")
    ImplosiveConsonant (Velar, Voiced, "??")

    ImplosiveConsonant (Uvular, Unvoiced, "????")
    ImplosiveConsonant (Uvular, Voiced, "??")
    ]
