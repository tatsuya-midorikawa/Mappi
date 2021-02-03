# Mappi
Mappi (マッピー)は .NET Framework 3.5 などの古いフレームワークにも対応したシンプルなO/Rマッパーです。  
get only property や readonly field にも対応しています。

# 使い方
Mappi は `System.Data.SqlClient.SqlConnection` の拡張メソッドとして定義されていますので、その拡張メソッドを呼び出すだけで利用することができます。

## Query : 単一SELECT文の実行

単純なSELECT文であれば以下のように `Query` メソッドを利用することで簡単に読み取ることが可能です。

```cs
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
```

また、SQL にパラメータを利用したい場合は以下のように渡すことができます。
サンプルでは匿名クラスを利用していますが、通常のクラスや構造体でも問題ありません。

```cs
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
```

## MultipleQuery : 複数SELECT文の実行

複数のSELECT文の結果を一度に取得したい場合は `MultipleQuery` メソッドを利用します。

```cs
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
```

`HasNext` プロパティを利用することでまだ読み取れる内容があるかどうか判定することも可能です。

```cs
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
```

また `Query` のときと同様に SQL にパラメータを利用したい場合は以下のように渡すことができます。
サンプルでは匿名クラスを利用していますが、通常のクラスや構造体でも問題ありません。

```cs
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
```