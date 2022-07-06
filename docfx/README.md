# DocFXでの出力内容確認


最初に`docfx.console`をインストール

```
nuget install docfx.console
```

ダウンロードされたディレクトリをリネームする

`docfx.console.xxxxx` -> `docfx.console`

ビルドする

```
.\docfx.console\tools\docfx.exe .\docfx_project\docfx.json 
```

サーバーを立ち上げる場合は上に`--serve`オプションをつける