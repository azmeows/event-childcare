import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getVendorComparison } from '../services/vendorService';
import { VendorComparisonDocument } from '../types/vendorTypes';
import VendorList from '../components/VendorList';
import VendorComparison from '../components/VendorComparison';
import styled from '@emotion/styled';

// スタイル定義
const PageContainer = styled.div`
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
  font-family: 'Hiragino Sans', 'Meiryo', sans-serif;
`;

const Header = styled.header`
  text-align: center;
  margin-bottom: 30px;
`;

const Title = styled.h1`
  color: #ff6b00;
  font-size: 2rem;
  margin-bottom: 10px;
`;

const Subtitle = styled.p`
  color: #666;
  font-size: 1rem;
`;

const EmailContainer = styled.div`
  background-color: #f9f9f9;
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 20px;
  text-align: center;
`;

const EmailLabel = styled.span`
  font-weight: bold;
  margin-right: 10px;
`;

const EmailForm = styled.form`
  display: flex;
  justify-content: center;
  margin-top: 10px;
  gap: 10px;
`;

const EmailInput = styled.input`
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  min-width: 250px;
`;

const Button = styled.button`
  background-color: #ff7d26;
  color: white;
  border: none;
  border-radius: 4px;
  padding: 8px 15px;
  cursor: pointer;
  transition: background-color 0.2s;

  &:hover {
    background-color: #e65c00;
  }
`;

const ErrorMessage = styled.div`
  background-color: #fff0f0;
  color: #d32f2f;
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 20px;
  text-align: center;
`;

const LoadingMessage = styled.div`
  text-align: center;
  padding: 30px;
  color: #666;
`;

const ContentContainer = styled.div`
  display: flex;
  flex-direction: column;
  gap: 30px;

  @media (min-width: 992px) {
    flex-direction: column;
    gap: 30px;
  }
`;

/**
 * 業者比較ページ
 */
const VendorComparisonPage: React.FC = () => {
  // URLパラメータからメールアドレスを取得
  const { email } = useParams<{ email: string }>();
  const navigate = useNavigate();
  
  const [comparisonData, setComparisonData] = useState<VendorComparisonDocument | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [emailInput, setEmailInput] = useState<string>(email || '');

  // メールアドレスが変更されたときにデータを取得
  useEffect(() => {
    const fetchData = async () => {
      if (!email) return;
      
      setLoading(true);
      setError(null);
      
      try {
        const data = await getVendorComparison(email);
        
        if (data) {
          setComparisonData(data);
        } else {
          setError('指定されたメールアドレスのデータが見つかりませんでした。');
        }
      } catch (err) {
        setError('データの取得中にエラーが発生しました。');
        console.error('Error fetching data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [email]);

  // メールアドレス検索フォームの送信処理
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (emailInput.trim()) {
      navigate(`/vendor-comparison/${encodeURIComponent(emailInput.trim())}`);
    }
  };

  return (
    <PageContainer>
      <Header>
        <Title>イベント託児サービス比較</Title>
        <Subtitle>託児業者からの見積もり相談メールを分析・比較し、最適な業者選びをサポートします</Subtitle>
      </Header>

      <EmailContainer>
        <EmailLabel>メールアドレスで検索:</EmailLabel>
        <EmailForm onSubmit={handleSubmit}>
          <EmailInput
            type="email"
            value={emailInput}
            onChange={(e) => setEmailInput(e.target.value)}
            placeholder="あなたのメールアドレスを入力"
            required
          />
          <Button type="submit">検索</Button>
        </EmailForm>
      </EmailContainer>

      {error && <ErrorMessage>{error}</ErrorMessage>}

      {loading ? (
        <LoadingMessage>データを読み込み中...</LoadingMessage>
      ) : comparisonData ? (
        <ContentContainer>
          {comparisonData.vendorComparisonResult && (
            <VendorComparison comparisonResult={comparisonData.vendorComparisonResult} />
          )}
          
          <VendorList vendors={comparisonData.vendors || []} />
        </ContentContainer>
      ) : email ? (
        <LoadingMessage>メールアドレスに関連するデータがありません</LoadingMessage>
      ) : (
        <LoadingMessage>メールアドレスを入力して検索してください</LoadingMessage>
      )}
    </PageContainer>
  );
};

export default VendorComparisonPage;