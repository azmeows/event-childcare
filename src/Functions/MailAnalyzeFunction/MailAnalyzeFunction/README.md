# メール内容AIによる分析機能

## 概要

この機能は、イベント託児業者からのメールを受け取り、Azure OpenAIを使用して内容を分析し、業者比較に必要な情報を抽出して保存します。

## 機能の詳細

1. `received-emails` コンテナに新しいメールが追加されると、Cosmos DBトリガーが起動
2. 各託児サービス業者からのメールに対して以下を実行:
   - HTML形式のメール本文からテキストを抽出
   - Azure OpenAIに送信して内容を分析
   - 金額・条件・対応年齢・付加価値の4項目を抽出
   - 分析結果を `vendor-comparisons` コンテナに保存

## 設定方法

`local.settings.json` に以下の設定が必要です:

```json
{
  "Values": {
    "CosmosDb:ConnectionString": "YOUR_COSMOS_DB_CONNECTION_STRING",
    "CosmosDb:Database": "cosmos-event-childcare-dev",
    "CosmosDb:ReceivedEmailsContainer": "received-emails",
    "CosmosDb:VendorComparisonsContainer": "vendor-comparisons",
    "OpenAI:Endpoint": "https://YOUR_OPENAI_RESOURCE_NAME.openai.azure.com",
    "OpenAI:Key": "YOUR_OPENAI_API_KEY",
    "OpenAI:DeploymentName": "YOUR_OPENAI_DEPLOYMENT_NAME"
  }
}
```

## アーキテクチャ

- `EmailContentAnalyzer`: メール内容を分析するサービス
- `VendorComparisonService`: 分析結果を保存するサービス
- `ReceivedEmailsFunction`: Cosmos DBトリガーで実行される関数

## 分析例

分析結果のサンプル:

```json
{
  "id": "unique-id",
  "userEMailAddress": "user@example.com",
  "sourceEmailId": "original-email-id",
  "vendorEmail": "vendor@example.com",
  "analysisResults": {
    "price": "1時間あたり2,000円、最低4時間から",
    "conditions": "1週間前までの予約が必要、キャンセル料は前日50%、当日100%",
    "ageRange": "0歳から12歳まで",
    "addedValue": "おやつ提供、英語対応スタッフあり"
  },
  "analyzedAt": "2023-05-22T10:30:00Z"
}
```