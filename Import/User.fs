module OnlineConlang.Import.User

open OnlineConlang.Import.JsonWebToken


type LoginInfo = { username: string; password: string }

type SecurityToken = SecurityToken of string
