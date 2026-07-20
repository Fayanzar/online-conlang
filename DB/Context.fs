module OnlineConlang.DB.Context

open OnlineConlang.Foundation
open OnlineConlang.DB.Connection

open FSharp.Data.Sql.MySqlConnector
open FSharp.Data.Sql.Common

[<Literal>]
let private dbVendor = DatabaseProviderTypes.MYSQL

[<Literal>]
let private useOptTypes = NullableColumnType.OPTION

type private Sql = SqlDataProvider<dbVendor, connectionString, UseOptionTypes = useOptTypes>

let ctx = Sql.GetDataContext config.db.connectionString

let toBool (t : sbyte) = System.Convert.ToBoolean t
let fromBool (b : bool) = System.Convert.ToSByte b
