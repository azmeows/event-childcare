# event-childcare

## 開発環境について

本リポジトリは、**GitHub Codespaces** および **Dev Container** に対応しています。これにより、ローカル環境の構築不要で、すぐに統一された開発環境を利用できます。

### 主な構成

- **Dev Container イメージ**  
  - `mcr.microsoft.com/devcontainers/universal:2` をベースにしています。
- **インストール済みツール**
  - Azure CLI
  - Azure Developer CLI (`azd`)
  - Azure Functions Core Tools (v4)
  - Azurite (Azure Storage エミュレーター)
- **VS Code 拡張機能**
  - Azure GitHub Copilot
  - Azure Functions
  - Azure Resource Groups
  - Azure Cosmos DB
  - Bicep
  - C#
  - Azurite

### セットアップ手順

1. **GitHub Codespaces で開く**  
   - GitHub 上で「Code」→「Codespaces」→「新規作成」で起動できます。

2. **ローカルで Dev Container を利用する場合**  
   - VS Code で「Remote - Containers」拡張をインストール
   - 「Remote-Containers: Open Folder in Container」で本リポジトリを開く

3. **初回起動時の処理**
   - 必要なツール・拡張機能が自動でインストールされます。
   - ターミナルで `az --version` や `azd version` でインストール確認ができます。
   - Azure Functions Core Tools は `func --version` で確認できます。
   - Azurite は `azurite --version` で確認できます。

4. **Azure へのサインイン**
   - ターミナルで `az login` および `azd auth login` を実行し、Azure アカウントにサインインしてください。

### 補足

- Dev Container の設定は `.devcontainer/devcontainer.json` に記載されています。
- Azure 関連の開発やデプロイがすぐに始められる環境です。

### Azurite の利用方法

[Azurite](https://learn.microsoft.com/ja-jp/azure/storage/common/storage-use-azurite) は Azure Storage エミュレーターで、ローカル環境で Azure Functions や Cosmos DB トリガーのテストを行うために利用できます。

1. **Azurite の起動**
   ```bash
   # Azurite を起動（バックグラウンド実行）
   azurite --silent &
   ```

2. **Azure Functions でのローカル開発**
   - `local.settings.json` に以下の設定を追加します:
   ```json
   {
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true"
     }
   }
   ```

3. **Azurite の状態確認**
   ```bash
   # Azurite のプロセスが実行中か確認
   ps aux | grep azurite
   ```

4. **Azurite の停止**
   ```bash
   # Azurite プロセスの停止
   pkill -f azurite
   ```

---

この環境を利用することで、誰でも同じ開発環境をすぐに再現でき、Azure との連携もスムーズに行えます。

## Azureのリソース構成

| リソースの種類 | リソース名 | 用途 |
|--------------|-----------|------|
| リソースグループ | `rg-azmeows-dev` | 関連リソースの管理 |
| Cosmos DB アカウント | `cosno-event-childcare-dev` | データ格納 |
| Application Insights | `appi-event-childcare-dev` | Azure Functionsのアプリケーションログ |
| Log Analytics Workspace | `log-event-childcare-dev` | Azure Functionsの診断ログ |
| Azure Functions | `func-event-childcare-dev` | Cosmos DBのChange feedを受け付ける |
| Storage account | `stfunceventchildcaredev` | Azure Functionsに紐づくストレージ |
| Azure OpenAI Service | `oai-event-childcare-dev` | Azure Functionsから利用される |

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
