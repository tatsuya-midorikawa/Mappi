# Mappi
Mappi (マッピー)は .NET Framework 3.5 などの古いフレームワークにも対応したシンプルなO/Rマッパーです。  
get only property や readonly field にも対応しています。

# 使い方
Mappi は `System.Data.SqlClient.SqlConnection` の拡張メソッドとして定義されていますので、その拡張メソッドを呼び出すだけで利用することができます。

## マッピングするモデルの宣言方法

一番シンプルな場合、データベース上のカラム名とプロパティ名を一致させることでマッピング用のモデルとすることが可能です。

```cs
class Person
{
    public string Name { get; }
    public int Age { get; }
}
```

ただ、通常はデータベースのカラム名はスネークケースで付ける場合が多いと思います。
そういった際は `ColumnAttribute` を利用することで解決できます。

```cs
class Person
{
    [Column(Name: "name")]
    public string Name { get; }
    [Column(Name: "age")]
    public int Age { get; }
}
```

また、モデルとなるクラスや構造体に存在するプロパティ・フィールドはそのままではすべてマッピング対象となってしまうため、データベース上にない値をモデルにつくるとエラーになってしまいます。
そういった値をプロパティやフィールドとして存在させたい場合は `IgnoreAttribute` を対象に付与してマッピング対象から外す必要があります。

```cs
class Person
{
    [Column(Name: "name")]
    public string Name { get; }
    [Column(Name: "age")]
    public int Age { get; }

    [Ignore]
    private Sex _sex;
}
```

`ColumnAttribute` と `IgnoreAttribute` を組み合わせることで自動実装プロパティ以外のプロパティにも対応可能です。

```cs
class Person
{
    [Column(Name: "name")]
    private string _name;
    [Column(Name: "age")]
    private int _age;

    [Ignore]
    public string Name 
    { 
        get => _name ?? "名無しの権兵衛"; 
        set => _name = value;
    }
    [Ignore]
    public int Age
    { 
        get => _age < 0 ? 0 : _age; 
        set => _age = value;
    }
}
```

構造体についても同様の方法で宣言できる他、readonly fieldを利用することも可能です。

```cs
struct Person
{
    [Column(Name: "name")]
    public readonly string Name;
    [Column(Name: "age")]
    public readonly int Age;
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
    using ( var reader = connection.MultipleQuery(sql) )
    {
        foreach ( var item in reader.Read<SAMPLE>() )
        {
          // do something
        }
    }
}
```

また、SQL にパラメータを利用したい場合は以下のように渡すことができます。
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
    using ( var reader = connection.MultipleQuery(sql, new { Number = 100 }) )
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

また `Query` のときと同様に SQL にパラメータを利用したい場合は以下のように渡すことができます。
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