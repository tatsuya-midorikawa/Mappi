# Mappi
Mappi (マッピー)は .NET Framework 3.5 などの古いフレームワークにも対応したシンプルなO/Rマッパーです。  

# 使い方
Mappi は `System.Data.SqlClient.SqlConnection` の拡張メソッドとして定義されていますので、`open Mappi` をするだけで簡単に導入することが可能です。  
ここでは以下のようなDBについて考えていきます。  

![](https://raw.githubusercontent.com/Midoliy/images/main/Mappi/db1.png)  
![](https://raw.githubusercontent.com/Midoliy/images/main/Mappi/db2.png)

## マッピングするモデルの宣言方法

一番シンプルな場合、データベース上のカラム名とプロパティ名を一致させることでマッピング用のモデルとすることが可能です。

```fsharp
type Person = {
    number : Guid
    first_name : string
    middle_name : string
    last_name : string
    age : int
}
```

ただ、F#では通常キャメルケースで命名することが多いと思います。
そういった際は `ColumnAttribute` を利用することで命名方法の差を解消することが可能です。

```fsharp
type Person = {
    number : Guid
    [<Column(Name = "first_name")>]
    firstName : string
    [<Column(Name = "middle_name")>]
    middleName : string
    [<Column(Name = "last_name")>]
    lastName : string
    age : int
}
```

また、モデルとなるクラスや構造体に存在するプロパティはそのままではすべてマッピング対象となってしまうため、データベース上にない値をモデルにつくるとエラーになってしまいます。
そういった値をプロパティやフィールドとして存在させたい場合は `IgnoreAttribute` を対象に付与してマッピング対象から外す必要があります。

```fsharp
type Person = {
    number : Guid
    [<Column(Name = "first_name")>]
    firstName : string
    [<Column(Name = "middle_name")>]
    middleName : string
    [<Column(Name = "last_name")>]
    lastName : string
    age : int

    [<Ignore>]
    sex : int
}
```

F#の強力な言語機能に **`判別共用体`** と **`Option型`** があります。  
`Mappi` では `単一要素の判別共用体` と `Option型` の両方に対応しています。  

```fsharp
// 単一要素の判別共用体を定義
//  -> これは主にDDDの実践において便利です。
type Id = Id of Guid
type Name = Name of string
type Age = Age of int

type Person = {
    number : Guid
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name
    [<Column(Name = "last_name")>]
    lastName : Name
    age : Age
}
```

```fsharp
type Id = Id of Guid
type Name = Name of string
type Age = Age of int

// DB上でnullableになっている値についてはOption型を指定することも可能です
type Person = {
    number : Guid
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    age : Age
}
```

構造体についても同様の方法で宣言で可能です。

```cs
type Id = Id of Guid
type Name = Name of string
type Age = Age of int

[<Struct>]
type Person = {
    number : Guid
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    age : Age
}
```

## Query : 単一SELECT文の実行

単純なSELECT文であれば以下のように `Query` メソッドを利用することで簡単に読み取ることが可能です。

```fsharp
open System
open System.Data.SqlClient
open Mappi

[<Literal>]
let connectionString = "YOUR_DB_CONNECTION_STRING"

[<Literal>]
let sql = "SELECT * FROM persons"

type Id = Id of Guid
type Name = Name of string
type Age = Age of int

type Person = {
    number : Id
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    [<Ignore>]
    age : Age
}


[<EntryPoint>]
let main argv =
    use connection = new SqlConnection(connectionString)
    use reader = connection.Query(sql)
    reader.Read<Person>()
    |> Seq.iter (fun person -> 
        // do something
        printfn $"%A{person}")

    0
```

これを実行することで以下のような結果を取得することが可能です。
![](https://raw.githubusercontent.com/Midoliy/images/main/Mappi/result1.png)  


.NET Framework 4.5 以降 / .NET Core 2.0 以降であれば、非同期版の `QueryAsync` を利用することも可能です。

```fsharp
open System
open System.Data.SqlClient
open Mappi

[<Literal>]
let connectionString = "YOUR_DB_CONNECTION_STRING"

[<Literal>]
let sql = "SELECT * FROM persons"

type Id = Id of Guid
type Name = Name of string
type Age = Age of int

type Person = {
    number : Id
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    [<Ignore>]
    age : Age
}


[<EntryPoint>]
let main argv =
    use connection = new SqlConnection(connectionString)
    async { 
        use! reader = connection.QueryAsync(sql) |> Async.AwaitTask 
        reader.Read<Person>()
        |> Seq.iter (fun person -> 
            // do something
            printfn $"%A{person}")
    }
    |> Async.RunSynchronously

    0
```

SQL にパラメータを利用したい場合は以下のように渡すことができます。
サンプルでは匿名レコードを利用していますが、通常のクラスや構造体、レコード型でも問題ありません。

```fsharp
open System
open System.Data.SqlClient
open Mappi

[<Literal>]
let connectionString = "YOUR_DB_CONNECTION_STRING"

[<Literal>]
// @マーク付きのパラメータを指定する
let sql = "SELECT * FROM persons p WHERE p.age > @age"

type Id = Id of Guid
type Name = Name of string
type Age = Age of int

[<Struct>]
type Person = {
    number : Id
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    [<Ignore>]
    age : Age
}

[<EntryPoint>]
let main argv =
    use connection = new SqlConnection(connectionString)
    // @マークを取り除いたときの名前でパラメータを指定する
    use reader = connection.Query(sql, {| age = 30 |})
    reader.Read<Person>()
    |> Seq.iter (fun person -> 
        // do something
        printfn $"%A{person}")

    0
```

## MultipleQuery : 複数SELECT文の実行

複数のSELECT文の結果を一度に取得したい場合は `MultipleQuery` メソッドを利用します。

```fsharp
open System
open System.Data.SqlClient
open Mappi

[<Literal>]
let connectionString = "YOUR_DB_CONNECTION_STRING"

[<Literal>]
let sql = 
    """
    SELECT * FROM persons;
    SELECT * FROM persons;
    """

type Id = Id of Guid
type Name = Name of string
type Age = Age of int

[<Struct>]
type Person = {
    number : Id
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    [<Ignore>]
    age : Age
}

[<EntryPoint>]
let main argv =
    use connection = new SqlConnection(connectionString)
    use reader = connection.MultipleQuery(sql)
    // 発行したSQLの数の分だけループを回せる
    // (1回目)
    reader.Read<Person>()
    |> Seq.iter (fun person -> 
        // do something
        printfn $"%A{person}")

    printfn " ---------- "

    // (2回目)
    reader.Read<Person>()
    |> Seq.iter (fun person -> 
        // do something
        printfn $"%A{person}")

    0
```

![](https://raw.githubusercontent.com/Midoliy/images/main/Mappi/result2.png)  

`HasNext` プロパティを利用することでまだ読み取れる内容があるかどうか判定することも可能です。

```fsharp
open System
open System.Data.SqlClient
open Mappi

[<Literal>]
let connectionString = "YOUR_DB_CONNECTION_STRING"

[<Literal>]
let sql = 
    """
    SELECT * FROM persons;
    SELECT * FROM persons;
    """

type Id = Id of Guid
type Name = Name of string
type Age = Age of int

[<Struct>]
type Person = {
    number : Id
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    [<Ignore>]
    age : Age
}

[<EntryPoint>]
let main argv =
    use connection = new SqlConnection(connectionString)
    use reader = connection.MultipleQuery(sql)
    // 発行したSQLの数の分だけループを回せる
    // (1回目)
    reader.Read<Person>()
    |> Seq.iter (fun person -> 
        // do something
        printfn $"%A{person}")

    if reader.HasNext then
        printfn " ---------- "

        // (2回目)
        reader.Read<Person>()
        |> Seq.iter (fun person -> 
            // do something
            printfn $"%A{person}")

    0
```

.NET Framework 4.5 以降 / .NET Core 2.0 以降であれば、非同期版の `MultipleQueryAsync` を利用することも可能です。

```fsharp
open System
open System.Data.SqlClient
open Mappi

[<Literal>]
let connectionString = "YOUR_DB_CONNECTION_STRING"

[<Literal>]
let sql = 
    """
    SELECT * FROM persons;
    SELECT * FROM persons;
    """

type Id = Id of Guid
type Name = Name of string
type Age = Age of int

[<Struct>]
type Person = {
    number : Id
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    [<Ignore>]
    age : Age
}

[<EntryPoint>]
let main argv =
    use connection = new SqlConnection(connectionString)
    async { 
        use! reader = connection.MultipleQueryAsync(sql) |> Async.AwaitTask
        // 発行したSQLの数の分だけループを回せる
        // (1回目)
        reader.Read<Person>()
        |> Seq.iter (fun person -> 
            // do something
            printfn $"%A{person}")

        printfn " ---------- "

        // (2回目)
        reader.Read<Person>()
        |> Seq.iter (fun person -> 
            // do something
            printfn $"%A{person}")
    }
    |> Async.RunSynchronously

    0
```

`Query` のときと同様に SQL にパラメータを利用したい場合は以下のように渡すことができます。  
サンプルでは匿名レコードを利用していますが、通常のクラスや構造体、レコード型でも問題ありません。

```fsharp
open System
open System.Data.SqlClient
open Mappi

[<Literal>]
let connectionString = "YOUR_DB_CONNECTION_STRING"

[<Literal>]
let sql = 
// @マーク付きのパラメータを指定する
let sql = 
    """
    SELECT * FROM persons p WHERE age < @age;
    SELECT * FROM persons;
    """

type Id = Id of Guid
type Name = Name of string
type Age = Age of int

[<Struct>]
type Person = {
    number : Id
    [<Column(Name = "first_name")>]
    firstName : Name
    [<Column(Name = "middle_name")>]
    middleName : Name option
    [<Column(Name = "last_name")>]
    lastName : Name option
    [<Ignore>]
    age : Age
}

[<EntryPoint>]
let main argv =
    use connection = new SqlConnection(connectionString)
    // @マークを取り除いたときの名前でパラメータを指定する
    use reader = connection.MultipleQuery(sql, {| age = 30 |})
    // (1回目)
    reader.Read<Person>()
    |> Seq.iter (fun person -> 
        // do something
        printfn $"%A{person}")

    printfn " ---------- "

    // (2回目)
    reader.Read<Person>()
    |> Seq.iter (fun person -> 
        // do something
        printfn $"%A{person}")

    0
```
![](https://raw.githubusercontent.com/Midoliy/images/main/Mappi/result3.png)  

