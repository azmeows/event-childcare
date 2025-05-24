import React from 'react';
import { Vendor } from '../types/vendorTypes';
import VendorDetail from './VendorDetail';
import styled from '@emotion/styled';

interface VendorListProps {
  vendors: Vendor[];
}

// スタイル定義
const ListContainer = styled.div`
  margin-top: 20px;
`;

const ListHeader = styled.h2`
  color: #ff6b00;
  border-bottom: 2px solid #ffb273;
  padding-bottom: 10px;
  margin-bottom: 20px;
`;

const NoVendorsMessage = styled.div`
  padding: 20px;
  background-color: #fff9f0;
  border-radius: 8px;
  text-align: center;
  color: #9e9e9e;
`;

/**
 * 業者一覧コンポーネント
 */
const VendorList: React.FC<VendorListProps> = ({ vendors }) => {
  if (!vendors || vendors.length === 0) {
    return (
      <ListContainer>
        <ListHeader>業者情報</ListHeader>
        <NoVendorsMessage>
          業者情報がありません
        </NoVendorsMessage>
      </ListContainer>
    );
  }

  return (
    <ListContainer>
      <ListHeader>業者情報（{vendors.length}社）</ListHeader>
      {vendors.map((vendor, index) => (
        <VendorDetail key={`${vendor.vendorEmail}-${index}`} vendor={vendor} />
      ))}
    </ListContainer>
  );
};

export default VendorList;