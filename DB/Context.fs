module OnlineConlang.DB.Context

open FSharp.Data.Sql

[<Literal>]
let private connectionString = "Server=localhost;Database=conlang;User=root;Password=password"

[<Literal>]
let private dbVendor = Common.DatabaseProviderTypes.MYSQL

[<Literal>]
let private useOptTypes = Common.NullableColumnType.OPTION

type private Sql = SqlDataProvider<dbVendor, connectionString, UseOptionTypes = useOptTypes>

let ctx = Sql.GetDataContext()
