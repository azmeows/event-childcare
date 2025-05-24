import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import VendorComparisonPage from './pages/VendorComparisonPage';
import styled from '@emotion/styled';

// グローバルスタイル
const AppContainer = styled.div`
  min-height: 100vh;
  background-color: #fffaf3;
`;

const Footer = styled.footer`
  text-align: center;
  padding: 20px;
  margin-top: 50px;
  color: #999;
  font-size: 0.9rem;
`;

/**
 * アプリケーションのメインコンポーネント
 */
const App: React.FC = () => {
  return (
    <Router>
      <AppContainer>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/vendor-comparison" element={<VendorComparisonPage />} />
          <Route path="/vendor-comparison/:email" element={<VendorComparisonPage />} />
        </Routes>
        <Footer>
          &copy; {new Date().getFullYear()} イベント託児サービス比較 | プライバシーに配慮したサービスを提供しています
        </Footer>
      </AppContainer>
    </Router>
  );
};

export default App;
