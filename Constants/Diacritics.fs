module OnlineConlang.Constants.Diacritics

type Location =
    | Above
    | Below
    | Beside

type Phonation =
    | Voiceless
    | Voiced
    | Aspirated
    | MoreRounded
    | LessRounded
    | Advanced
    | Retracted
    | Centralized
    | MidCentralized
    | Syllabic
    | NonSyllabic
    | Rhoticity
    | Breathy
    | Creaky
    | Linguolabial
    | Labialized
    | Palatalized
    | Velarized
    | Pharyngealized
    | VelarizedOrPharyngealized
    | Raised
    | Lowered
    | AdvancedTongueRoot
    | RetractedTongueRoot
    | Dental
    | Apical
    | Laminal
    | Nasalized
    | NasalRelease
    | LateralRelease
    | NoAudibleRelease
    | SchwaRelease
    | ThetaRelease
    | ChiRelease
    | Ejective
    | Affricate

type Diacritic = Diacritic of Phonation * Location * string

let diacritics = Set.ofList [
    Diacritic (Voiceless, Below, "̥")
    Diacritic (Voiceless, Above, "̊")

    Diacritic (Voiced, Below, "̬")

    Diacritic (Aspirated, Beside, "ʰ")

    Diacritic (MoreRounded, Below, "̹")
    Diacritic (MoreRounded, Above, "͗")
    Diacritic (MoreRounded, Beside, "˒")

    Diacritic (LessRounded, Below, "̜")
    Diacritic (LessRounded, Above, "͑")
    Diacritic (LessRounded, Beside, "˓")

    Diacritic (Advanced, Below, "̟")
    Diacritic (Advanced, Beside, "˖")

    Diacritic (Retracted, Below, "̠")
    Diacritic (Retracted, Beside, "˗")

    Diacritic (Centralized, Above, "̈")

    Diacritic (MidCentralized, Above, "̽")

    Diacritic (Syllabic, Below, "̩")
    Diacritic (Syllabic, Above, "̍")

    Diacritic (NonSyllabic, Below, "̯")
    Diacritic (NonSyllabic, Above, "̑")

    Diacritic (Rhoticity, Beside, "˞")

    Diacritic (Breathy, Below, "̤")

    Diacritic (Creaky, Below, "̰")

    Diacritic (Linguolabial, Below, "̼")

    Diacritic (Labialized, Beside, "ʷ")

    Diacritic (Palatalized, Beside, "ʲ")

    Diacritic (Velarized, Beside, "ˠ")

    Diacritic (Pharyngealized, Beside, "ˤ")

    Diacritic (VelarizedOrPharyngealized, Beside, "̴")

    Diacritic (Raised, Below, "̝")
    Diacritic (Raised, Beside, "˔")

    Diacritic (Lowered, Below, "̞")
    Diacritic (Lowered, Beside, "˕")

    Diacritic (AdvancedTongueRoot, Below, "̘")
    Diacritic (AdvancedTongueRoot, Beside, "꭪")

    Diacritic (RetractedTongueRoot, Below, "̙")
    Diacritic (RetractedTongueRoot, Beside, "꭫")

    Diacritic (Dental, Below, "̪")
    Diacritic (Dental, Above, "͆")

    Diacritic (Apical, Below, "̺")

    Diacritic (Laminal, Below, "̻")

    Diacritic (Nasalized, Above, "̃")

    Diacritic (NasalRelease, Beside, "ⁿ")

    Diacritic (LateralRelease, Beside, "ˡ")

    Diacritic (NoAudibleRelease, Above, "̚")

    Diacritic (SchwaRelease, Beside, "ᵊ")

    Diacritic (ThetaRelease, Beside, "ᶿ")

    Diacritic (ChiRelease, Beside, "ˣ")

    Diacritic (Ejective, Beside, "ʼ")

    Diacritic (Affricate, Above, "͡")
    Diacritic (Affricate, Below, "͜")
]
