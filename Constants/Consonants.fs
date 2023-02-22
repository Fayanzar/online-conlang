module OnlineConlang.Constants.Consonants

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

type EjectiveConsonant = EjectiveConsonant of PlaceArticulation * MannerArticulation * string

type ClickConsonant = ClickConsonant of PlaceArticulation * PlaceArticulation * MannerArticulationClick * string

type ImplosiveConsonant = ImplosiveConsonant of PlaceArticulation * Voicedness * string

let pulmonicConsonants = Set.ofList [
    BasicConsonant (Bilabial, Nasal, Unvoiced, "m̥")
    BasicConsonant (Bilabial, Nasal, Voiced, "m")
    BasicConsonant (Labiodental, Nasal, Voiced, "ɱ")
    BasicConsonant (Linguolabial, Nasal, Voiced, "n̼")
    BasicConsonant (Alveolar, Nasal, Unvoiced, "n̥")
    BasicConsonant (Alveolar, Nasal, Voiced, "n")
    BasicConsonant (Retroflex, Nasal, Unvoiced, "ɳ̊")
    BasicConsonant (Retroflex, Nasal, Voiced, "ɳ")
    BasicConsonant (Palatal, Nasal, Unvoiced, "ɲ̊")
    BasicConsonant (Palatal, Nasal, Voiced, "ɲ")
    BasicConsonant (Velar, Nasal, Unvoiced, "ŋ̊")
    BasicConsonant (Velar, Nasal, Voiced, "ŋ")
    BasicConsonant (Uvular, Nasal, Voiced, "ɴ")

    BasicConsonant (Bilabial, Plosive, Unvoiced, "p")
    BasicConsonant (Bilabial, Plosive, Voiced, "b")
    BasicConsonant (Labiodental, Plosive, Unvoiced, "p̪")
    BasicConsonant (Labiodental, Plosive, Voiced, "b̪")
    BasicConsonant (Linguolabial, Plosive, Unvoiced, "t̼")
    BasicConsonant (Linguolabial, Plosive, Voiced, "d̼")
    BasicConsonant (Alveolar, Plosive, Unvoiced, "t")
    BasicConsonant (Alveolar, Plosive, Voiced, "d")
    BasicConsonant (Retroflex, Plosive, Unvoiced, "ʈ")
    BasicConsonant (Retroflex, Plosive, Voiced, "ɖ")
    BasicConsonant (Palatal, Plosive, Unvoiced, "c")
    BasicConsonant (Palatal, Plosive, Voiced, "ɟ")
    BasicConsonant (Velar, Plosive, Unvoiced, "k")
    BasicConsonant (Velar, Plosive, Voiced, "ɡ")
    BasicConsonant (Uvular, Plosive, Unvoiced, "q")
    BasicConsonant (Uvular, Plosive, Voiced, "ɢ")
    BasicConsonant (Pharyngeal, Plosive, Unvoiced, "ʡ")
    BasicConsonant (Glottal, Plosive, Unvoiced, "ʔ")

    BasicConsonant (Alveolar, SibilantAffricate, Unvoiced, "t͡s")
    BasicConsonant (Alveolar, SibilantAffricate, Voiced, "d͡z")
    BasicConsonant (Postalveolar, SibilantAffricate, Unvoiced, "t̠͡ʃ")
    BasicConsonant (Postalveolar, SibilantAffricate, Voiced, "d̠͡ʒ")
    BasicConsonant (Retroflex, SibilantAffricate, Unvoiced, "t͡ʂ")
    BasicConsonant (Retroflex, SibilantAffricate, Voiced, "d͡ʐ")
    BasicConsonant (Palatal, SibilantAffricate, Unvoiced, "t͡ɕ")
    BasicConsonant (Palatal, SibilantAffricate, Voiced, "d͡ʑ")

    BasicConsonant (Bilabial, NonSibilantAffricate, Unvoiced, "p͡ɸ")
    BasicConsonant (Bilabial, NonSibilantAffricate, Voiced, "b͡β")
    BasicConsonant (Labiodental, NonSibilantAffricate, Unvoiced, "p̪͡f")
    BasicConsonant (Labiodental, NonSibilantAffricate, Voiced, "b̪͡v")
    BasicConsonant (Dental, NonSibilantAffricate, Unvoiced, "t̪͡θ")
    BasicConsonant (Dental, NonSibilantAffricate, Voiced, "d̪͡ð")
    BasicConsonant (Alveolar, NonSibilantAffricate, Unvoiced, "t͡ɹ̝̊")
    BasicConsonant (Alveolar, NonSibilantAffricate, Voiced, "d͡ɹ̝")
    BasicConsonant (Postalveolar, NonSibilantAffricate, Unvoiced, "t̠͡ɹ̠̊˔")
    BasicConsonant (Postalveolar, NonSibilantAffricate, Voiced, "d̠͡ɹ̠˔")
    BasicConsonant (Palatal, NonSibilantAffricate, Unvoiced, "c͡ç")
    BasicConsonant (Palatal, NonSibilantAffricate, Voiced, "ɟ͡ʝ")
    BasicConsonant (Velar, NonSibilantAffricate, Unvoiced, "k͡x")
    BasicConsonant (Velar, NonSibilantAffricate, Voiced, "ɡ͡ɣ")
    BasicConsonant (Uvular, NonSibilantAffricate, Unvoiced, "q͡χ")
    BasicConsonant (Uvular, NonSibilantAffricate, Voiced, "ɢ͡ʁ")
    BasicConsonant (Pharyngeal, NonSibilantAffricate, Unvoiced, "ʡ͡ʜ")
    BasicConsonant (Pharyngeal, NonSibilantAffricate, Voiced, "ʡ͡ʢ")
    BasicConsonant (Glottal, NonSibilantAffricate, Unvoiced, "ʔ͡h")

    BasicConsonant (Alveolar, SibilantFricative, Unvoiced, "s")
    BasicConsonant (Alveolar, SibilantFricative, Voiced, "z")
    BasicConsonant (Postalveolar, SibilantFricative, Unvoiced, "ʃ")
    BasicConsonant (Postalveolar, SibilantFricative, Voiced, "ʒ")
    BasicConsonant (Retroflex, SibilantFricative, Unvoiced, "ʂ")
    BasicConsonant (Retroflex, SibilantFricative, Voiced, "ʐ")
    BasicConsonant (Palatal, SibilantFricative, Unvoiced, "ɕ")
    BasicConsonant (Palatal, SibilantFricative, Voiced, "ʑ")

    BasicConsonant (Bilabial, NonSibilantFricative, Unvoiced, "ɸ")
    BasicConsonant (Bilabial, NonSibilantFricative, Voiced, "β")
    BasicConsonant (Labiodental, NonSibilantFricative, Unvoiced, "f")
    BasicConsonant (Labiodental, NonSibilantFricative, Voiced, "v")
    BasicConsonant (Linguolabial, NonSibilantFricative, Unvoiced, "θ̼")
    BasicConsonant (Linguolabial, NonSibilantFricative, Voiced, "ð̼")
    BasicConsonant (Dental, NonSibilantFricative, Unvoiced, "θ")
    BasicConsonant (Dental, NonSibilantFricative, Voiced, "ð")
    BasicConsonant (Alveolar, NonSibilantFricative, Unvoiced, "θ̠")
    BasicConsonant (Alveolar, NonSibilantFricative, Voiced, "ð̠")
    BasicConsonant (Postalveolar, NonSibilantFricative, Unvoiced, "ɹ̠̊˔")
    BasicConsonant (Postalveolar, NonSibilantFricative, Voiced, "ɹ̠˔")
    BasicConsonant (Retroflex, NonSibilantFricative, Unvoiced, "ɻ̊˔")
    BasicConsonant (Retroflex, NonSibilantFricative, Voiced, "ɻ˔")
    BasicConsonant (Palatal, NonSibilantFricative, Unvoiced, "ç")
    BasicConsonant (Palatal, NonSibilantFricative, Voiced, "ʝ")
    BasicConsonant (Velar, NonSibilantFricative, Unvoiced, "x")
    BasicConsonant (Velar, NonSibilantFricative, Voiced, "ɣ")
    BasicConsonant (Uvular, NonSibilantFricative, Unvoiced, "χ")
    BasicConsonant (Uvular, NonSibilantFricative, Voiced, "ʁ")
    BasicConsonant (Pharyngeal, NonSibilantFricative, Unvoiced, "ħ")
    BasicConsonant (Pharyngeal, NonSibilantFricative, Voiced, "ʕ")
    BasicConsonant (Glottal, NonSibilantFricative, Unvoiced, "h")
    BasicConsonant (Glottal, NonSibilantFricative, Voiced, "ɦ")

    BasicConsonant (Labiodental, Approximant, Voiced, "ʋ")
    BasicConsonant (Alveolar, Approximant, Voiced, "ɹ")
    BasicConsonant (Retroflex, Approximant, Voiced, "ɻ")
    BasicConsonant (Palatal, Approximant, Voiced, "j")
    BasicConsonant (Velar, Approximant, Voiced, "ɰ")
    BasicConsonant (Glottal, Approximant, Voiced, "ʔ̞")

    BasicConsonant (Bilabial, Tap, Voiced, "ⱱ̟")
    BasicConsonant (Labiodental, Tap, Voiced, "ⱱ")
    BasicConsonant (Linguolabial, Tap, Voiced, "ɾ̼")
    BasicConsonant (Alveolar, Tap, Unvoiced, "ɾ̥")
    BasicConsonant (Alveolar, Tap, Voiced, "ɾ")
    BasicConsonant (Retroflex, Tap, Unvoiced, "ɽ̊")
    BasicConsonant (Retroflex, Tap, Voiced, "ɽ")
    BasicConsonant (Velar, Tap, Voiced, "ɡ̆")
    BasicConsonant (Uvular, Tap, Voiced, "ɢ̆")
    BasicConsonant (Pharyngeal, Tap, Voiced, "ʡ̆")

    BasicConsonant (Bilabial, Trill, Unvoiced, "ʙ̥")
    BasicConsonant (Bilabial, Trill, Voiced, "ʙ")
    BasicConsonant (Alveolar, Trill, Unvoiced, "r̥")
    BasicConsonant (Alveolar, Trill, Voiced, "r")
    BasicConsonant (Retroflex, Trill, Unvoiced, "ɽ̊r̥")
    BasicConsonant (Retroflex, Trill, Voiced, "ɽr")
    BasicConsonant (Uvular, Trill, Unvoiced, "ʀ̥")
    BasicConsonant (Uvular, Trill, Voiced, "ʀ")
    BasicConsonant (Pharyngeal, Trill, Unvoiced, "ʜ")
    BasicConsonant (Pharyngeal, Trill, Voiced, "ʢ")

    BasicConsonant (Alveolar, LateralAffricate, Unvoiced, "t͡ɬ")
    BasicConsonant (Alveolar, LateralAffricate, Voiced, "d͡ɮ")
    BasicConsonant (Retroflex, LateralAffricate, Unvoiced, "t͡ꞎ")
    BasicConsonant (Retroflex, LateralAffricate, Voiced, "d͡ɭ˔")
    BasicConsonant (Palatal, LateralAffricate, Unvoiced, "c͡ʎ̝̊")
    BasicConsonant (Palatal, LateralAffricate, Voiced, "ɟ͡ʎ̝")
    BasicConsonant (Velar, LateralAffricate, Unvoiced, "k͡𝼄")
    BasicConsonant (Velar, LateralAffricate, Voiced, "ɡ͡ʟ̝")

    BasicConsonant (Alveolar, LateralFricative, Unvoiced, "ɬ")
    BasicConsonant (Alveolar, LateralFricative, Voiced, "ɮ")
    BasicConsonant (Retroflex, LateralFricative, Unvoiced, "ꞎ")
    BasicConsonant (Retroflex, LateralFricative, Voiced, "ɭ˔")
    BasicConsonant (Palatal, LateralFricative, Unvoiced, "ʎ̝̊")
    BasicConsonant (Palatal, LateralFricative, Voiced, "ʎ̝")
    BasicConsonant (Velar, LateralFricative, Unvoiced, "𝼄")
    BasicConsonant (Velar, LateralFricative, Voiced, "ʟ̝")

    BasicConsonant (Alveolar, LateralApproximant, Voiced, "l")
    BasicConsonant (Retroflex, LateralApproximant, Voiced, "ɭ")
    BasicConsonant (Palatal, LateralApproximant, Voiced, "ʎ")
    BasicConsonant (Velar, LateralApproximant, Voiced, "ʟ")
    BasicConsonant (Uvular, LateralApproximant, Voiced, "ʟ̠")

    BasicConsonant (Alveolar, LateralTap, Unvoiced, "ɺ̥")
    BasicConsonant (Alveolar, LateralTap, Voiced, "ɺ")
    BasicConsonant (Retroflex, LateralTap, Unvoiced, "ɭ̥̆")
    BasicConsonant (Retroflex, LateralTap, Voiced, "ɭ̆")
    BasicConsonant (Palatal, LateralTap, Voiced, "ʎ̆")
    BasicConsonant (Velar, LateralTap, Voiced, "ʟ̆")

    CoArtConsonant (Bilabial, Alveolar, Nasal, Voiced, "n͡m")
    CoArtConsonant (Bilabial, Velar, Nasal, Voiced, "ŋ͡m")
    CoArtConsonant (Bilabial, Alveolar, Plosive, Unvoiced, "t͡p")
    CoArtConsonant (Bilabial, Alveolar, Plosive, Voiced, "d͡b")
    CoArtConsonant (Bilabial, Velar, Plosive, Unvoiced, "k͡p")
    CoArtConsonant (Bilabial, Velar, Plosive, Voiced, "g͡b")
    CoArtConsonant (Uvular, Pharyngeal, Plosive, Unvoiced, "q͡ʡ")

    CoArtConsonant (Bilabial, Palatal, Approximant, Unvoiced, "ɥ̊")
    CoArtConsonant (Bilabial, Palatal, Approximant, Voiced, "ɥ")
    CoArtConsonant (Bilabial, Velar, Approximant, Unvoiced, "ʍ")
    CoArtConsonant (Bilabial, Velar, Approximant, Voiced, "w")

    CoArtConsonant (Labiodental, Velar, NonSibilantFricative, Unvoiced, "ɧ")
    CoArtConsonant (Alveolar, Velar, LateralApproximant, Voiced, "ɫ")
    ]

let ejectiveConsonants = Set.ofList [
    EjectiveConsonant (Bilabial, Plosive, "pʼ")
    EjectiveConsonant (Alveolar, Plosive, "tʼ")
    EjectiveConsonant (Retroflex, Plosive, "ʈʼ")
    EjectiveConsonant (Palatal, Plosive, "cʼ")
    EjectiveConsonant (Velar, Plosive, "kʼ")
    EjectiveConsonant (Uvular, Plosive, "qʼ")
    EjectiveConsonant (Pharyngeal, Plosive, "ʡʼ")

    EjectiveConsonant (Alveolar, SibilantAffricate, "t͡sʼ")
    EjectiveConsonant (Postalveolar, SibilantAffricate, "t̠͡ʃʼ")
    EjectiveConsonant (Retroflex, SibilantAffricate, "t͡ʂʼ")

    EjectiveConsonant (Dental, NonSibilantAffricate, "t̪͡θʼ")
    EjectiveConsonant (Velar, NonSibilantAffricate, "k͡xʼ")
    EjectiveConsonant (Uvular, NonSibilantAffricate, "q͡χʼ")

    EjectiveConsonant (Alveolar, SibilantFricative, "sʼ")
    EjectiveConsonant (Postalveolar, SibilantFricative, "ʃʼ")
    EjectiveConsonant (Retroflex, SibilantFricative, "ʂʼ")
    EjectiveConsonant (Palatal, SibilantFricative, "ɕʼ")

    EjectiveConsonant (Bilabial, NonSibilantFricative, "ɸʼ")
    EjectiveConsonant (Labiodental, NonSibilantFricative, "fʼ")
    EjectiveConsonant (Dental, NonSibilantFricative, "θʼ")
    EjectiveConsonant (Velar, NonSibilantFricative, "xʼ")
    EjectiveConsonant (Uvular, NonSibilantFricative, "χʼ")

    EjectiveConsonant (Alveolar, LateralAffricate, "t͡ɬʼ")
    EjectiveConsonant (Palatal, LateralAffricate, "c͡ʎ̝̊ʼ")
    EjectiveConsonant (Velar, LateralAffricate, "k͡𝼄ʼ")

    EjectiveConsonant (Alveolar, LateralFricative, "ɬʼ")
    ]

let clickConsonants = Set.ofList [
    ClickConsonant (Velar, Bilabial, TenuisNonLateral, "k͡ʘ")
    ClickConsonant (Velar, Dental, TenuisNonLateral, "k͡ǀ")
    ClickConsonant (Velar, Alveolar, TenuisNonLateral, "k͡ǃ")
    ClickConsonant (Velar, Retroflex, TenuisNonLateral, "k͡‼")
    ClickConsonant (Velar, Palatal, TenuisNonLateral, "k͡ǂ")

    ClickConsonant (Uvular, Bilabial, TenuisNonLateral, "q͡ʘ")
    ClickConsonant (Uvular, Dental, TenuisNonLateral, "q͡ǀ")
    ClickConsonant (Uvular, Alveolar, TenuisNonLateral, "q͡ǃ")
    ClickConsonant (Uvular, Retroflex, TenuisNonLateral, "q͡‼")
    ClickConsonant (Uvular, Palatal, TenuisNonLateral, "q͡ǂ")

    ClickConsonant (Velar, Bilabial, VoicedNonLateral, "ɡ͡ʘ")
    ClickConsonant (Velar, Dental, VoicedNonLateral, "ɡ͡ǀ")
    ClickConsonant (Velar, Alveolar, VoicedNonLateral, "ɡ͡ǃ")
    ClickConsonant (Velar, Retroflex, VoicedNonLateral, "ɡ͡‼")
    ClickConsonant (Velar, Palatal, VoicedNonLateral, "ɡ͡ǂ")

    ClickConsonant (Uvular, Bilabial, VoicedNonLateral, "ɢ͡ʘ")
    ClickConsonant (Uvular, Dental, VoicedNonLateral, "ɢ͡ǀ")
    ClickConsonant (Uvular, Alveolar, VoicedNonLateral, "ɢ͡ǃ")
    ClickConsonant (Uvular, Retroflex, VoicedNonLateral, "ɢ͡‼")
    ClickConsonant (Uvular, Palatal, VoicedNonLateral, "ɢ͡ǂ")

    ClickConsonant (Velar, Bilabial, NasalNonLateral, "ŋ͡ʘ")
    ClickConsonant (Velar, Dental, NasalNonLateral, "ŋ͡ǀ")
    ClickConsonant (Velar, Alveolar, NasalNonLateral, "ŋ͡ǃ")
    ClickConsonant (Velar, Retroflex, NasalNonLateral, "ŋ͡‼")
    ClickConsonant (Velar, Palatal, NasalNonLateral, "ŋ͡ǂ")
    ClickConsonant (Velar, Velar, NasalNonLateral, "ʞ")

    ClickConsonant (Uvular, Bilabial, NasalNonLateral, "ɴ͡ʘ")
    ClickConsonant (Uvular, Dental, NasalNonLateral, "ɴ͡ǀ")
    ClickConsonant (Uvular, Alveolar, NasalNonLateral, "ɴ͡ǃ")
    ClickConsonant (Uvular, Retroflex, NasalNonLateral, "ɴ͡‼")
    ClickConsonant (Uvular, Palatal, NasalNonLateral, "ɴ͡ǂ")

    ClickConsonant (Velar, Alveolar, TenuisLateral, "k͡ǁ")

    ClickConsonant (Uvular, Alveolar, TenuisLateral, "q͡ǁ")

    ClickConsonant (Velar, Alveolar, VoicedLateral, "ɡ͡ǁ")

    ClickConsonant (Uvular, Alveolar, VoicedLateral, "ɢ͡ǁ")

    ClickConsonant (Velar, Alveolar, NasalLateral, "ŋ͡ǁ")

    ClickConsonant (Uvular, Alveolar, NasalLateral, "ɴ͡ǁ")
    ]

let implosiveConsonants = Set.ofList [
    ImplosiveConsonant (Bilabial, Unvoiced, "ɓ̥")
    ImplosiveConsonant (Bilabial, Voiced, "ɓ")

    ImplosiveConsonant (Alveolar, Unvoiced, "ɗ̥")
    ImplosiveConsonant (Alveolar, Voiced, "ɗ")

    ImplosiveConsonant (Retroflex, Unvoiced, "ᶑ̊")
    ImplosiveConsonant (Retroflex, Voiced, "ᶑ")

    ImplosiveConsonant (Palatal, Unvoiced, "ʄ̊")
    ImplosiveConsonant (Palatal, Voiced, "ʄ")

    ImplosiveConsonant (Velar, Unvoiced, "ɠ̊")
    ImplosiveConsonant (Velar, Voiced, "ɠ")

    ImplosiveConsonant (Uvular, Unvoiced, "ʛ̥")
    ImplosiveConsonant (Uvular, Voiced, "ʛ")
    ]
