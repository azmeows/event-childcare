# 動作確認手順 (Operation Verification Procedure)

このドキュメントでは、Vendor Comparison API機能の動作確認手順について説明します。

## 前提条件 (Prerequisites)

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools)
- [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) (ローカルテスト用) または Azure Cosmos DBのアクセス権
- [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) (オプション、ローカルテスト用)

## セットアップ手順 (Setup Procedure)

### 1. 環境変数の設定 (Environment Variables Setup)

1. `src/Functions/VendorComparisonFunction/VendorComparisonFunction/local.settings.json.example` を `local.settings.json` にコピーします。
2. 以下の項目を適切な値に更新します：

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true", // ローカルストレージエミュレーターを使用
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "COSMOSDB_CONNECTION_STRING": "Your_Cosmos_DB_Connection_String", // Cosmos DBの接続文字列
    "COSMOSDB_DATABASE": "cosmos-event-childcare-dev", // データベース名
    "COSMOSDB_CONTAINER_VENDOR_COMPARISONS": "vendor-comparisons" // 業者比較コンテナ
  }
}
```

### 2. Cosmos DBの準備 (Cosmos DB Preparation)

#### Azure Cosmos DBを使用する場合：

1. Azure Portalにログインし、Cosmos DBリソースを開きます。
2. 「データエクスプローラー」を開きます。
3. データベース（例：`cosmos-event-childcare-dev`）を作成します（存在しない場合）。
4. コンテナ `vendor-comparisons` を作成し、パーティションキーとして `/userEMailAddress` を指定します。

#### Cosmos DBエミュレーターを使用する場合：

1. Cosmos DBエミュレーターを起動します。
2. データエクスプローラーを開きます（デフォルト：http://localhost:8081/_explorer/index.html）。
3. データベース（例：`cosmos-event-childcare-dev`）を作成します。
4. コンテナ `vendor-comparisons` を作成し、パーティションキーとして `/userEMailAddress` を指定します。

### 3. テストデータの作成 (Test Data Creation)

APIの動作確認用にテストデータを作成します：

1. Cosmos DBエクスプローラーで `vendor-comparisons` コンテナを開きます。
2. 「新しい項目」をクリックし、以下のようなJSONデータを入力します：

```json
{
  "id": "test-id-1",
  "userEMailAddress": "test@example.com",
  "sourceEmailId": "source-email-1",
  "vendorEmail": "vendor1@example.com",
  "analysisResults": {
    "price": "1時間あたり2,000円、最低4時間から",
    "conditions": "1週間前までの予約が必要、キャンセル料は前日50%、当日100%",
    "ageRange": "0歳から12歳まで",
    "addedValue": "おやつ提供、英語対応スタッフあり"
  },
  "analyzedAt": "2023-05-22T10:30:00Z"
}
```

必要に応じて複数のテストデータを作成します（異なるユーザーメールアドレスでいくつか作成すると良いでしょう）。

## 実行手順 (Execution Procedure)

### 1. Functionsアプリのビルドと実行 (Build and Run Function App)

1. プロジェクトディレクトリに移動します：
   ```
   cd src/Functions/VendorComparisonFunction
   ```

2. アプリケーションをビルドします：
   ```
   dotnet build
   ```

3. Functionsを実行します：
   ```
   cd VendorComparisonFunction
   func start
   ```

### 2. APIエンドポイントの確認 (Test API Endpoint)

#### ブラウザで確認する方法：

1. ブラウザで以下のURLにアクセスします：
   ```
   http://localhost:7071/api/vendor-comparisons?userEMailAddress=test@example.com
   ```

2. JSONレスポンスが表示されることを確認します。

#### cURLを使用する方法：

```bash
curl "http://localhost:7071/api/vendor-comparisons?userEMailAddress=test@example.com"
```

#### Postmanを使用する方法：

1. GETリクエストを作成し、URLを入力：
   ```
   http://localhost:7071/api/vendor-comparisons?userEMailAddress=test@example.com
   ```

2. 「Send」をクリックしてリクエストを送信し、レスポンスを確認します。

## その他の確認ポイント (Other Verification Points)

### 1. エラーケースの確認 (Error Case Testing)

以下のようなエラーケースをテストすることも重要です：

1. クエリパラメータなしでアクセス：
   ```
   http://localhost:7071/api/vendor-comparisons
   ```
   → 400 Bad Requestと適切なエラーメッセージが返ることを確認

2. 存在しないユーザーでアクセス：
   ```
   http://localhost:7071/api/vendor-comparisons?userEMailAddress=nonexistent@example.com
   ```
   → 200 OKで空の配列が返ることを確認

### 2. ログの確認 (Log Verification)

Functionsの実行ログを確認し、以下のポイントを確認します：

1. リクエストが正しく処理されたことを示すログメッセージ
2. クエリの実行とデータ取得に関する情報
3. エラーが発生した場合は適切なエラーログ