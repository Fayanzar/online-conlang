module OnlineConlang.Foundation

open FSharp.Configuration

type Config = YamlConfig<"config.yaml">
let config = Config()

type IIPARepresentable =
    abstract member IPASymbol : string
