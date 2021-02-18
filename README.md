# Mappi
Mappi (マッピー)は .NET Framework 3.5 などの古いフレームワークにも対応したシンプルなO/Rマッパーです。  

# 使い方
Mappi は `System.Data.SqlClient.SqlConnection` の拡張メソッドとして定義されていますので、`using Mappi;` をするだけで簡単に導入することが可能です。

## マッピングするモデルの宣言方法

一番シンプルな場合、データベース上のカラム名とプロパティ名を一致させることでマッピング用のモデルとすることが可能です。

```cs
class Person
{
    public string Name { get; private set; }
    public int Age { get; }
}
```

ただ、通常はデータベースのカラム名はスネークケースで命名することが多いと思います。
そういった際は `ColumnAttribute` を利用することで命名方法の差を解消することが可能です。

```cs
class Person
{
    [Column(Name: "name")]
    public string Name { get; private set; }
    [Column(Name: "age")]
    public int Age { get; private set; }
}
```

また、モデルとなるクラスや構造体に存在するプロパティはそのままではすべてマッピング対象となってしまうため、データベース上にない値をモデルにつくるとエラーになってしまいます。
そういった値をプロパティやフィールドとして存在させたい場合は `IgnoreAttribute` を対象に付与してマッピング対象から外す必要があります。

```cs
class Person
{
    [Column(Name: "name")]
    public string Name { get; private set; }
    [Column(Name: "age")]
    public int Age { get; private set; }

    [Ignore]
    private Sex _sex { get; set; }
}
```

構造体についても同様の方法で宣言で可能です。

```cs
struct Person
{
    [Column(Name: "name")]
    public string Name { get; private set; }
    [Column(Name: "age")]
    public int Age { get; private set; }
}
```

## Query : 単一SELECT文の実行

単純なSELECT文であれば以下のように `Query` メソッドを利用することで簡単に読み取ることが可能です。

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static void Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    using ( var reader = connection.Query(sql) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
          // do something
        }
    }
}
```

.NET Framework 4.5 以降 / .NET Core 2.0 以降であれば、非同期版の `QueryAsync` を利用することも可能です。

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static async Task Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    using ( var reader = await connection.QueryAsync(sql) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
          // do something
        }
    }
}
```

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static async Task Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    {
        var samples = await connection.QueryAsync<SAMPLE>(sql);
        // do something
    }
}
```

SQL にパラメータを利用したい場合は以下のように渡すことができます。
サンプルでは匿名クラスを利用していますが、通常のクラスや構造体でも問題ありません。

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static void Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample WHERE number = @Number;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    using ( var reader = connection.Query(sql, new { Number = 100 }) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
          // do something
        }
    }
}
```

## MultipleQuery : 複数SELECT文の実行

複数のSELECT文の結果を一度に取得したい場合は `MultipleQuery` メソッドを利用します。

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static void Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample;
      SELECT * FROM test;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    using ( var reader = connection.MultipleQuery(sql) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
            // do something
        }

        foreach ( var item in reader.Read<TEST>() )
        {
            // do something
        }
    }
}
```

`HasNext` プロパティを利用することでまだ読み取れる内容があるかどうか判定することも可能です。

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static void Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample;
      SELECT * FROM test;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    using ( var reader = connection.MultipleQuery(sql) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
            // do something
        }

        if ( reader.HasNext )
        {
            foreach ( var item in reader.Read<TEST>() )
            {
                // do something
            }
        }
    }
}
```

.NET Framework 4.5 以降 / .NET Core 2.0 以降であれば、非同期版の `MultipleQueryAsync` を利用することも可能です。

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static async Task Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample;
      SELECT * FROM test;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    using ( var reader = await connection.MultipleQueryAsync(sql) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
            // do something
        }

        foreach ( var item in reader.Read<TEST>() )
        {
            // do something
        }
    }
}
```

`Query` のときと同様に SQL にパラメータを利用したい場合は以下のように渡すことができます。
サンプルでは匿名クラスを利用していますが、通常のクラスや構造体でも問題ありません。

```cs
using System;
using System.Data.SqlClient;
using Mappi;

static void Main(string[] args)
{
    var connectionString = "YOUR DB CONNECTION STRING";

    var sql = @"
      SELECT * FROM sample;
      SELECT * FROM test WHERE name = @Name;
    ";

    using ( var connection = new SqlConnection(connectionString) )
    using ( var reader = connection.MultipleQuery(sql, new { Name = "Midoliy" }) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
            // do something
        }

        foreach ( var item in reader.Read<TEST>() )
        {
            // do something
        }
    }
}
```