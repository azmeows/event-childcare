/**
 * 業者比較用の分析結果ドキュメントモデル
 */
export interface VendorComparisonDocument {
  id: string;
  userEMailAddress: string;
  vendors: Vendor[];
  vendorComparisonResult: string;
  _rid?: string;
  _self?: string;
  _etag?: string;
  _attachments?: string;
  _ts?: number;
}

/**
 * 業者情報
 */
export interface Vendor {
  vendorEmail: string;
  analysisResult: AnalysisResult;
  analyzedTime: string;
}

/**
 * AIによる分析結果
 */
export interface AnalysisResult {
  price: string;
  conditions: string;
  ageRange: string;
  addedValue: string;
  nextAction: string;
}