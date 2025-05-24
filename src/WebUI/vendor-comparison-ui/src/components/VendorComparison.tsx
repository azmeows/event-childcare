import React from 'react';
import ReactMarkdown from 'react-markdown';
import styled from '@emotion/styled';

interface VendorComparisonProps {
  comparisonResult: string;
}

// スタイル定義
const ComparisonContainer = styled.div`
  background-color: #fff;
  border-radius: 12px;
  padding: 25px;
  margin-bottom: 30px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
`;

const ComparisonTitle = styled.h2`
  color: #ff6b00;
  border-bottom: 2px solid #ffb273;
  padding-bottom: 10px;
  margin-bottom: 20px;
`;

const MarkdownWrapper = styled.div`
  /* Markdownスタイルのカスタマイズ */
  h1, h2, h3 {
    color: #e67700;
    margin-top: 20px;
    margin-bottom: 10px;
  }
  
  h2 {
    border-bottom: 1px solid #ffe0b2;
    padding-bottom: 5px;
  }
  
  table {
    border-collapse: collapse;
    width: 100%;
    margin: 15px 0;
  }
  
  th, td {
    border: 1px solid #ffcc80;
    padding: 8px 12px;
  }
  
  th {
    background-color: #fff3e0;
  }
  
  tr:nth-of-type(even) {
    background-color: #fff8e1;
  }

  blockquote {
    border-left: 4px solid #ffb74d;
    padding-left: 10px;
    color: #795548;
    margin-left: 0;
    padding: 10px 20px;
    background-color: #fff8e1;
  }

  code {
    background-color: #f5f5f5;
    padding: 2px 4px;
    border-radius: 3px;
  }
  
  hr {
    border: 0;
    height: 1px;
    background-color: #ffcc80;
    margin: 20px 0;
  }

  strong {
    color: #e65100;
  }
`;

/**
 * 業者比較結果表示コンポーネント
 */
const VendorComparison: React.FC<VendorComparisonProps> = ({ comparisonResult }) => {
  return (
    <ComparisonContainer>
      <ComparisonTitle>業者比較分析</ComparisonTitle>
      <MarkdownWrapper>
        <ReactMarkdown>{comparisonResult}</ReactMarkdown>
      </MarkdownWrapper>
    </ComparisonContainer>
  );
};

export default VendorComparison;