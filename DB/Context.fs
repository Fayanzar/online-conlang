module OnlineConlang.DB.Context

open OnlineConlang.Foundation

open FSharp.Data.Sql

[<Literal>]
let connectionString = "Server=localhost;Database=conlang;User=root;Password=password"

[<Literal>]
let private dbVendor = Common.DatabaseProviderTypes.MYSQL

[<Literal>]
let private useOptTypes = Common.NullableColumnType.OPTION

type private Sql = SqlDataProvider<dbVendor, connectionString, UseOptionTypes = useOptTypes>

let ctx = Sql.GetDataContext config.db.connectionString

let toBool (t : sbyte) = System.Convert.ToBoolean t
let fromBool (b : bool) = System.Convert.ToSByte b
