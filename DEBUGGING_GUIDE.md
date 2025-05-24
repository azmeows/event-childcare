# 託児業者比較UI デバッグガイド

このガイドでは、託児業者比較UIアプリケーションの開発、実行、デバッグ方法について詳細に説明します。

## 開発環境のセットアップと実行方法

### 前提条件

- [Node.js](https://nodejs.org/) (v16以上)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-run-local)
- [Azure Static Web Apps CLI](https://docs.microsoft.com/ja-jp/azure/static-web-apps/local-development)

### ローカル開発環境の構築

#### 1. フロントエンドの実行 (ReactアプリケーションをLocal実行)

```bash
# プロジェクトディレクトリに移動
cd src/WebUI/vendor-comparison-ui

# 依存関係のインストール
npm install

# 開発サーバーを起動
npm start
```

ブラウザで http://localhost:3000 を開くとアプリケーションが表示されます。

#### 2. バックエンドの実行 (Azure Functions Localエミュレーター)

```bash
# Azure Functions プロジェクトディレクトリに移動
cd src/Functions/MailAnalyzeFunction/MailAnalyzeFunction

# 依存関係の復元
dotnet restore

# ローカルでFunctionsを実行
func start
```

これにより、Functionsがローカルでポート7071で実行されます。

#### 3. Azure Static Web Apps CLIを使った統合環境の実行

SWA CLIを使用すると、フロントエンドとバックエンドを統合して実行できます：

```bash
# プロジェクトルートディレクトリで実行
cd src/WebUI/vendor-comparison-ui

# swa CLIをインストール (まだの場合)
npm install -g @azure/static-web-apps-cli

# React アプリをビルド
npm run build

# 統合環境を起動
swa start build --api-location ../../../Functions/MailAnalyzeFunction/MailAnalyzeFunction
```

これにより、フロントエンドとバックエンドが統合された環境が実行されます。

## デバッグ方法

### フロントエンド (React) のデバッグ

1. **ブラウザの開発者ツールを使用**:
   - Chrome/Edge: F12キーまたは右クリック→「検証」
   - Console タブでログを確認
   - Network タブでAPIリクエスト/レスポンスを確認

2. **VSCode でのデバッグ**:
   - `.vscode/launch.json` に以下の設定を追加:

   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": "Launch Chrome",
         "type": "chrome",
         "request": "launch",
         "url": "http://localhost:3000",
         "webRoot": "${workspaceFolder}/src/WebUI/vendor-comparison-ui"
       }
     ]
   }
   ```

   - F5キーでデバッグ起動

3. **コンソールログ**:
   - コードに `console.log()` を追加して、特定の値や状態を確認

### バックエンド (Azure Functions) のデバッグ

1. **VSCodeでのデバッグ**:
   - `.vscode/launch.json` に以下の設定を追加:

   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": "Attach to .NET Functions",
         "type": "coreclr",
         "request": "attach",
         "processId": "${command:azureFunctions.pickProcess}"
       }
     ]
   }
   ```

   - `func start` でFunctionsを起動後、VSCodeのデバッグ機能でアタッチ

2. **ログの確認**:
   - Functions実行中のコンソール出力を確認
   - `_logger.LogInformation()`, `_logger.LogWarning()`, `_logger.LogError()` などを使用してログを出力

3. **Postmanを使ったAPIテスト**:
   - Postmanで `http://localhost:7071/api/vendor-comparison/{email}` にGETリクエストを送信
   - レスポンスの確認とデバッグ

## Azure Static Web Appsのデプロイと問題解決

### デプロイの仕組み

Azure Static Web Appsへのデプロイは、GitHub Actionsワークフローファイル (`.github/workflows/azure-static-web-apps.yml`) によって行われています。このワークフローは以下の主要なステップを実行します：

1. リポジトリをチェックアウト
2. フロントエンド（React）アプリケーションをビルド
3. バックエンド（Azure Functions）をビルド
4. 両方を組み合わせてAzure Static Web Appsにデプロイ

### 一般的なデプロイ問題と解決策

#### 1. ビルドエラー

**症状**: GitHub Actionsのデプロイが失敗し、ログにビルドエラーが表示される

**解決策**:
- GitHub Actionsのログを確認して、具体的なエラーメッセージを特定
- フロントエンドのビルドエラーの場合：
  - 依存関係の問題を確認 (`package.json` と `package-lock.json`)
  - TypeScriptの型エラーを修正
- バックエンドのビルドエラーの場合：
  - .NET依存関係の問題を確認
  - コンパイルエラーを修正

#### 2. API接続エラー

**症状**: フロントエンドはデプロイされるが、APIにアクセスできない（404エラーなど）

**解決策**:
- Azure Static Web Appsの構成で、APIのパスが正しく設定されているか確認
- `.github/workflows/azure-static-web-apps.yml` の `api_location` が正しいパスを指しているか確認
- Azure Functionsのルート設定が正しいか確認
- アプリケーションがワーカーベースのモデル（Microsoft.Azure.Functions.Worker）を使用しているか確認

#### 3. CORS問題

**症状**: ブラウザのコンソールにCORSエラーが表示される

**解決策**:
- `host.json` にCORS設定が正しく構成されているか確認
- Azure Static Web Apps構成でCORS設定が有効になっているか確認

#### 4. デプロイ時の認証エラー

**症状**: GitHub Actionsのログに認証エラーが表示される

**解決策**:
- GitHub リポジトリのSecretsでAzure Static Web Appsのデプロイトークンが正しく設定されているか確認
- Azure Portalで新しいデプロイトークンを生成し、GitHubのSecretsを更新

### 特定の問題に対するトラブルシューティング

#### .NET ワーカーベースモデルとWebJobsベースモデルの混在

Azure Functionsには2つのプログラミングモデル（WebJobs-basedとWorker-process-based）があります。これらを混在させるとデプロイの問題が発生することがあります。

**解決策**:
- すべての関数を一貫して同じモデル（Worker-processモデルが推奨）を使用するように更新
- `Microsoft.Azure.WebJobs` 名前空間の代わりに `Microsoft.Azure.Functions.Worker` を使用
- 依存関係を適切に更新

#### ビルドコマンドの不足

**症状**: GitHub Actionsのログに「ビルドコマンドが見つからない」などのエラーが表示される

**解決策**:
- `.github/workflows/azure-static-web-apps.yml` ファイルに明示的なビルドコマンドを追加:
  ```yaml
  api_build_command: "dotnet build --configuration Release"
  app_build_command: "npm run build"
  ```

## ローカルでのデバッグと本番環境の違い

ローカル開発環境と本番環境（Azure Static Web Apps）では、いくつかの重要な違いがあります：

1. **API URL**: 
   - ローカル: http://localhost:7071/api/...
   - 本番: /api/... （相対パス）

2. **認証**:
   - ローカルでは通常無効
   - 本番では Azure Static Web Appsの認証メカニズムが利用可能

3. **環境変数**:
   - ローカルでは `local.settings.json` から読み込み
   - 本番では Azure Static Web Appsの構成から読み込み

これらの違いを考慮して、開発とデバッグを行ってください。