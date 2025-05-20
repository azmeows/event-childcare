# event-childcare

## 開発環境について

本リポジトリは、**GitHub Codespaces** および **Dev Container** に対応しています。これにより、ローカル環境の構築不要で、すぐに統一された開発環境を利用できます。

### 主な構成

- **Dev Container イメージ**  
  - `mcr.microsoft.com/devcontainers/universal:2` をベースにしています。
- **インストール済みツール**
  - Azure CLI
  - Azure Developer CLI (`azd`)
- **VS Code 拡張機能**
  - Azure GitHub Copilot
  - Azure Functions
  - Azure Resource Groups
  - Azure Cosmos DB
  - Bicep

### セットアップ手順

1. **GitHub Codespaces で開く**  
   - GitHub 上で「Code」→「Codespaces」→「新規作成」で起動できます。

2. **ローカルで Dev Container を利用する場合**  
   - VS Code で「Remote - Containers」拡張をインストール
   - 「Remote-Containers: Open Folder in Container」で本リポジトリを開く

3. **初回起動時の処理**
   - 必要なツール・拡張機能が自動でインストールされます。
   - ターミナルで `az --version` や `azd version` でインストール確認ができます。

4. **Azure へのサインイン**
   - ターミナルで `az login` および `azd auth login` を実行し、Azure アカウントにサインインしてください。

### 補足

- Dev Container の設定は `.devcontainer/devcontainer.json` に記載されています。
- Azure 関連の開発やデプロイがすぐに始められる環境です。

---

この環境を利用することで、誰でも同じ開発環境をすぐに再現でき、Azure との連携もスムーズに行えます。

## UIの実装

イベント託児サービス管理システムのUIは、以下のような構成になっています：

- **ホーム画面**: ダッシュボードとして機能し、主要機能へのアクセスを提供
- **受信メール画面**: 受信したメールの一覧表示と詳細表示
- **業者比較画面**: 託児サービス業者の比較と詳細情報表示

### UIの実行方法

1. `app` ディレクトリに移動します
2. HTML ファイルを Web ブラウザで開きます
   - 例: `index.html` を開いてホーム画面を表示

### 開発者向け情報

- UI は HTML, CSS, JavaScript で構築されています
- API との連携は現在モック実装されています（実際の API は今後実装予定）
- 画面サイズに応じたレスポンシブデザインに対応しています

## Azureのリソース構成

- **リソースグループ**: `rg-azmeows-dev`
- **Cosmos DB アカウント**: `cosno-event-childcare-dev`

### Cosmos DB `cosno-event-childcare-dev` のデータ構造

- **データベース**: `cosmos-event-childcare-dev`
- **コレクション**:
  - `received-emails`: 受信したメールの情報を格納
  - `vendor-comparisons`: 業者比較に関する情報を格納

#### `received-emails` コレクション

受信したメールの情報を格納する。Change feedを利用して、受信メールが格納された契機をトリガーに、業者比較用の情報抽出などの処理を実装する。

- **partition key**: `/userEMailAddress`

#### `vendor-comparisons` コレクション

業者比較に関する情報を格納する

- **partition key**: `/userEMailAddress`
