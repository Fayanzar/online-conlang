module OnlineConlang.Constants.Vowels

type Height =
    | Open = 0
    | NearOpen = 1
    | OpenMid = 2
    | Mid = 3
    | CloseMid = 4
    | NearClose = 5
    | Close = 6

type Backness =
    | Back = 0
    | NearBack = 1
    | Central = 2
    | NearFront = 3
    | Front = 4

type Roundedness = Rounded | Unrounded

type Vowel = Vowel of Height * Backness * Roundedness * string

let vowels = Set.ofList [
    Vowel (Height.Close, Backness.Front, Unrounded, "i")
    Vowel (Height.Close, Backness.Central, Unrounded, "ɨ")
    Vowel (Height.Close, Backness.Back, Unrounded, "ɯ")
    Vowel (Height.Close, Backness.Front, Rounded, "y")
    Vowel (Height.Close, Backness.Central, Rounded, "ʉ")
    Vowel (Height.Close, Backness.Back, Rounded, "u")

    Vowel (Height.NearClose, Backness.NearFront, Unrounded, "ɪ")
    Vowel (Height.NearClose, Backness.NearFront, Rounded, "ʏ")
    Vowel (Height.NearClose, Backness.NearBack, Rounded, "ʊ")

    Vowel (Height.CloseMid, Backness.Front, Unrounded, "e")
    Vowel (Height.CloseMid, Backness.Central, Unrounded, "ɘ")
    Vowel (Height.CloseMid, Backness.Back, Unrounded, "ɤ")
    Vowel (Height.CloseMid, Backness.Front, Rounded, "ø")
    Vowel (Height.CloseMid, Backness.Central, Rounded, "ɵ")
    Vowel (Height.CloseMid, Backness.Back, Rounded, "o")

    Vowel (Height.Mid, Backness.Front, Unrounded, "e̞")
    Vowel (Height.Mid, Backness.Central, Unrounded, "ə")
    Vowel (Height.Mid, Backness.Back, Unrounded, "ɤ̞")
    Vowel (Height.Mid, Backness.Front, Rounded, "ø̞")
    Vowel (Height.Mid, Backness.Back, Rounded, "o̞")

    Vowel (Height.OpenMid, Backness.Front, Unrounded, "ɛ")
    Vowel (Height.OpenMid, Backness.Central, Unrounded, "ɜ")
    Vowel (Height.OpenMid, Backness.Back, Unrounded, "ʌ")
    Vowel (Height.OpenMid, Backness.Front, Rounded, "œ")
    Vowel (Height.OpenMid, Backness.Central, Rounded, "ɞ")
    Vowel (Height.OpenMid, Backness.Back, Rounded, "ɔ")

    Vowel (Height.NearOpen, Backness.Front, Unrounded, "æ")
    Vowel (Height.NearOpen, Backness.Central, Unrounded, "ɐ")

    Vowel (Height.Open, Backness.Front, Unrounded, "a")
    Vowel (Height.Open, Backness.Central, Unrounded, "ä")
    Vowel (Height.Open, Backness.Back, Unrounded, "ɑ")
    Vowel (Height.Open, Backness.Front, Rounded, "ɶ")
    Vowel (Height.Open, Backness.Back, Rounded, "ɒ")
]
