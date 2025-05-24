import axios from 'axios';
import { VendorComparisonDocument } from '../types/vendorTypes';

const API_BASE_URL = '/api'; // Azure Static Webアプリ内でのAPIパス

/**
 * 指定したユーザーメールアドレスの最新の業者比較データを取得する
 * @param userEmail ユーザーのメールアドレス
 */
export const getVendorComparison = async (userEmail: string): Promise<VendorComparisonDocument | null> => {
  try {
    const response = await axios.get(`${API_BASE_URL}/vendor-comparison/${encodeURIComponent(userEmail)}`);
    return response.data;
  } catch (error) {
    console.error('業者比較データの取得に失敗しました', error);
    return null;
  }
};