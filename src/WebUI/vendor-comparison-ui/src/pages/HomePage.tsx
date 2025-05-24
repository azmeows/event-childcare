import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styled from '@emotion/styled';

// スタイル定義
const HomeContainer = styled.div`
  max-width: 800px;
  margin: 0 auto;
  padding: 40px 20px;
  text-align: center;
  font-family: 'Hiragino Sans', 'Meiryo', sans-serif;
`;

const Title = styled.h1`
  color: #ff6b00;
  font-size: 2.5rem;
  margin-bottom: 20px;
`;

const Description = styled.p`
  color: #666;
  font-size: 1.2rem;
  margin-bottom: 30px;
  line-height: 1.6;
`;

const SearchBox = styled.div`
  background-color: #fff9f0;
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
  margin-bottom: 40px;
`;

const SearchForm = styled.form`
  display: flex;
  flex-direction: column;
  gap: 15px;
  max-width: 500px;
  margin: 0 auto;
`;

const SearchTitle = styled.h2`
  color: #ff7d26;
  margin-bottom: 20px;
  font-size: 1.5rem;
`;

const EmailInput = styled.input`
  padding: 12px 15px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
`;

const SearchButton = styled.button`
  background-color: #ff7d26;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 12px 20px;
  font-size: 1rem;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.2s;

  &:hover {
    background-color: #e65c00;
  }
`;

const Features = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 25px;
  margin-top: 40px;
`;

const FeatureCard = styled.div`
  background-color: #fff;
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.08);
`;

const FeatureIcon = styled.div`
  font-size: 2rem;
  margin-bottom: 15px;
  color: #ff7d26;
`;

const FeatureTitle = styled.h3`
  color: #e67700;
  margin-bottom: 10px;
`;

const FeatureDescription = styled.p`
  color: #666;
  line-height: 1.5;
`;

/**
 * ホームページコンポーネント
 */
const HomePage: React.FC = () => {
  const [email, setEmail] = useState('');
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (email.trim()) {
      navigate(`/vendor-comparison/${encodeURIComponent(email.trim())}`);
    }
  };

  return (
    <HomeContainer>
      <Title>イベント託児サービス比較</Title>
      <Description>
        託児業者からの見積もり相談メールを分析・比較し、最適な業者選びをサポートします。
        あなたのメールアドレスを入力すると、関連する業者の比較情報が表示されます。
      </Description>

      <SearchBox>
        <SearchTitle>メールアドレスで検索</SearchTitle>
        <SearchForm onSubmit={handleSubmit}>
          <EmailInput
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="あなたのメールアドレスを入力"
            required
          />
          <SearchButton type="submit">業者比較を見る</SearchButton>
        </SearchForm>
      </SearchBox>

      <Features>
        <FeatureCard>
          <FeatureIcon>📊</FeatureIcon>
          <FeatureTitle>業者比較</FeatureTitle>
          <FeatureDescription>
            複数の託児業者を料金、条件、対応年齢、特徴などの観点から比較できます。
          </FeatureDescription>
        </FeatureCard>

        <FeatureCard>
          <FeatureIcon>📝</FeatureIcon>
          <FeatureTitle>詳細情報</FeatureTitle>
          <FeatureDescription>
            各業者の提供内容や条件を詳しく確認し、イベントに最適な業者を選べます。
          </FeatureDescription>
        </FeatureCard>

        <FeatureCard>
          <FeatureIcon>✅</FeatureIcon>
          <FeatureTitle>選定サポート</FeatureTitle>
          <FeatureDescription>
            目的や条件に合わせた業者選びのポイントを提供します。
          </FeatureDescription>
        </FeatureCard>
      </Features>
    </HomeContainer>
  );
};

export default HomePage;