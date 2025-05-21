# Cosmos DB Change Feed Azure Function

このプロジェクトは、Cosmos DBのChange Feedをトリガーとして受け付けるAzure Functionsを含みます。

## 概要

このAzure Functionは、Cosmos DBの`received-emails`コレクションの変更を監視し、変更があった場合にログを出力します。

## 環境変数

以下の環境変数が必要です：

- `COSMOS_DB_CONNECTION_STRING`: Cosmos DBの接続文字列
- `COSMOS_DB_DATABASE_NAME`: Cosmos DBのデータベース名
- `COSMOS_DB_COLLECTION_NAME_RECEIVED_EMAILS`: 監視対象のコレクション名

## ローカルでの開発

### 前提条件

- .NET 6.0 SDK以上
- Azure Functions Core Tools v4

### 環境設定

1. `local.settings.json`ファイルに必要な環境変数を設定します：

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "COSMOS_DB_CONNECTION_STRING": "<Cosmos DB接続文字列>",
        "COSMOS_DB_DATABASE_NAME": "cosmos-event-childcare-dev",
        "COSMOS_DB_COLLECTION_NAME_RECEIVED_EMAILS": "received-emails"
    }
}
```

### 実行方法

以下のコマンドを実行して、Functionsをローカルで起動します：

```bash
func start
```

## テスト

テストするには、Cosmos DBの`received-emails`コレクションにドキュメントを追加または更新してください。Functionはこれらの変更を検出し、ログメッセージを出力します。

## デプロイ

Azure Functionsにデプロイするには、以下のコマンドを使用します：

```bash
func azure functionapp publish <function-app-name>
```

デプロイ後、以下の環境変数をAzure Functionsの設定に追加してください：

- `COSMOS_DB_CONNECTION_STRING`
- `COSMOS_DB_DATABASE_NAME`
- `COSMOS_DB_COLLECTION_NAME_RECEIVED_EMAILS`