module OnlineConlang.Import.Transformations

open System.Text.RegularExpressions

[<StructuredFormatDisplay("{input} → {output}")>]
type Transformation =
    {
        input  : string
        output : string
    }

let transformWord transformation word =
    Regex.Replace(word, transformation.input, transformation.output)

// a -> b
let transformAToB a b = { input = a; output = b }

// ba -> '(?<=b)a' -> bc
let transformAAfterBToC a b c =
    { input = $"(?<={b}){a}"; output = c }

// ab -> 'a(?=b)' -> cb
let transformABeforeBToC a b c =
    { input = $"{a}(?={b})"; output = c }

// bac -> '(?<=b)a(?=c)' -> bdc
let transformABetweenBAndCToD a b c d =
    { input = $"(?<={b}){a}(?={c})"; output = d }
