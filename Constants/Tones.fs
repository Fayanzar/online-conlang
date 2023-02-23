module OnlineConlang.Constants.Tones

type ToneMark =
    | Diacritic
    | Chao
    | ChaoReversed

type Pitch =
    | Top
    | High
    | Mid
    | Low
    | Bottom
    | Downstep
    | Upstep
    | Rising
    | Falling
    | HighRising
    | MidRising
    | LowRising
    | HighFalling
    | MidFalling
    | LowFalling
    | Peaking
    | Dipping

type Tone = Tone of Pitch * ToneMark * string

let tones = Set.ofList [
    Tone (Top, Diacritic, "̋")
    Tone (Top, Chao, "˥")
    Tone (Top, ChaoReversed, "꜒")

    Tone (High, Diacritic, "́")
    Tone (High, Chao, "˦")
    Tone (High, ChaoReversed, "꜓")

    Tone (Mid, Diacritic, "̄")
    Tone (Mid, Chao, "˧")
    Tone (Mid, ChaoReversed, "꜔")

    Tone (Low, Diacritic, "̀")
    Tone (Low, Chao, "˨")
    Tone (Low, ChaoReversed, "꜕")

    Tone (Bottom, Diacritic, "̏")
    Tone (Bottom, Chao, "˩")
    Tone (Bottom, ChaoReversed, "꜖")

    Tone (Downstep, Diacritic, "ꜜ")

    Tone (Upstep, Diacritic, "ꜛ")

    Tone (Rising, Diacritic, "̌")
    Tone (Rising, Chao, "˩˥")
    Tone (Rising, ChaoReversed, "꜖꜒")

    Tone (Falling, Diacritic, "̂")
    Tone (Falling, Chao, "˥˩")
    Tone (Falling, ChaoReversed, "꜒꜖")

    Tone (HighRising, Diacritic, "᷄")
    Tone (HighRising, Chao, "˧˥")
    Tone (HighRising, ChaoReversed, "꜔꜒")

    Tone (MidRising, Chao, "˨˦")
    Tone (MidRising, ChaoReversed, "꜕꜓")

    Tone (LowRising, Diacritic, "᷅")
    Tone (LowRising, Chao, "˩˧")
    Tone (LowRising, ChaoReversed, "꜖꜔")

    Tone (HighFalling, Diacritic, "᷇")
    Tone (HighFalling, Chao, "˥˧")
    Tone (HighFalling, ChaoReversed, "꜒꜔")

    Tone (MidFalling, Chao, "˦˨")
    Tone (MidFalling, ChaoReversed, "꜓꜕")

    Tone (LowFalling, Diacritic, "᷆")
    Tone (LowFalling, Chao, "˧˩")
    Tone (LowFalling, ChaoReversed, "꜔꜖")

    Tone (Peaking, Diacritic, "᷈")
    Tone (Peaking, Chao, "˧˥˨")
    Tone (Peaking, Chao, "˨˦˨")
    Tone (Peaking, Chao, "˩˧˩")
    Tone (Peaking, ChaoReversed, "꜔꜒꜕")
    Tone (Peaking, ChaoReversed, "꜕꜓꜕")
    Tone (Peaking, ChaoReversed, "꜖꜔꜖")

    Tone (Dipping, Diacritic, "᷉")
    Tone (Dipping, Chao, "˥˧˥")
    Tone (Dipping, Chao, "˦˨˦")
    Tone (Dipping, Chao, "˧˩˧")
    Tone (Dipping, ChaoReversed, "꜒꜔꜒")
    Tone (Dipping, ChaoReversed, "꜓꜕꜓")
    Tone (Dipping, ChaoReversed, "꜔꜖꜔")
]
