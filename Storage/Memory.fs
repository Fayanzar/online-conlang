module OnlineConlang.Storage.Memory

let mutable testMemoryText : string list = []

let addTextToMemory (t : string) =
    testMemoryText <- testMemoryText @ [t]
