module OnlineConlang.Constants.Suprasegmentals

open OnlineConlang.Foundation

type Suprasegmental =
    | PrimaryStress of string
    | SecondaryStress of string
    | Long of string
    | HalfLong of string
    | ExtraShort of string
    | FootGroup of string
    | IntonationGroup of string
    | SyllableBreak of string
    | Linking of string
    | GlobalRise of string
    | GlobalFall of string
    member this.IPASymbol = (this :> IIPARepresentable).IPASymbol
    interface IIPARepresentable with
        member this.IPASymbol =
            match this with
            | PrimaryStress   s -> s
            | SecondaryStress s -> s
            | Long            s -> s
            | HalfLong        s -> s
            | ExtraShort      s -> s
            | FootGroup       s -> s
            | IntonationGroup s -> s
            | SyllableBreak   s -> s
            | Linking         s -> s
            | GlobalRise      s -> s
            | GlobalFall      s -> s

let suprasegmentals = Set.ofList [
    PrimaryStress "ˈ"
    SecondaryStress "ˌ"
    Long "ː"
    HalfLong "ˑ"
    ExtraShort "̆"
    FootGroup "|"
    IntonationGroup "‖"
    SyllableBreak "."
    Linking "‿"
    GlobalRise "↗︎"
    GlobalFall "↘︎"
]
