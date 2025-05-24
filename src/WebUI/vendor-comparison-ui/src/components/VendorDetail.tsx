import React from 'react';
import { Vendor } from '../types/vendorTypes';
import styled from '@emotion/styled';

interface VendorDetailProps {
  vendor: Vendor;
}

// スタイル定義
const DetailCard = styled.div`
  background-color: #fff9f0;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
`;

const VendorEmail = styled.h3`
  color: #ff7d26;
  margin-bottom: 15px;
  font-size: 1.2rem;
`;

const DetailSection = styled.div`
  margin-bottom: 15px;
`;

const DetailLabel = styled.div`
  font-weight: bold;
  color: #6a6a6a;
  margin-bottom: 5px;
`;

const DetailContent = styled.div`
  padding: 8px;
  background-color: #fff;
  border-radius: 8px;
  border-left: 3px solid #ffc47e;
`;

const AnalyzedDate = styled.div`
  font-size: 0.8rem;
  color: #9a9a9a;
  text-align: right;
  margin-top: 10px;
`;

/**
 * 業者詳細コンポーネント
 */
const VendorDetail: React.FC<VendorDetailProps> = ({ vendor }) => {
  // 日時を日本語形式で表示
  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日 ${date.getHours()}:${String(date.getMinutes()).padStart(2, '0')}`;
  };

  return (
    <DetailCard>
      <VendorEmail>{vendor.vendorEmail}</VendorEmail>
      
      <DetailSection>
        <DetailLabel>料金</DetailLabel>
        <DetailContent>{vendor.analysisResult.price || "情報なし"}</DetailContent>
      </DetailSection>
      
      <DetailSection>
        <DetailLabel>条件</DetailLabel>
        <DetailContent>{vendor.analysisResult.conditions || "情報なし"}</DetailContent>
      </DetailSection>
      
      <DetailSection>
        <DetailLabel>対応年齢</DetailLabel>
        <DetailContent>{vendor.analysisResult.ageRange || "情報なし"}</DetailContent>
      </DetailSection>
      
      <DetailSection>
        <DetailLabel>付加価値</DetailLabel>
        <DetailContent>{vendor.analysisResult.addedValue || "情報なし"}</DetailContent>
      </DetailSection>
      
      <DetailSection>
        <DetailLabel>次のアクション</DetailLabel>
        <DetailContent>{vendor.analysisResult.nextAction || "情報なし"}</DetailContent>
      </DetailSection>
      
      <AnalyzedDate>分析日時: {formatDate(vendor.analyzedTime)}</AnalyzedDate>
    </DetailCard>
  );
};

export default VendorDetail;