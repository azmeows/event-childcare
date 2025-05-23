// Main application JavaScript file

// Check if the page is loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('Application initialized');
    
    // Set current year in footer
    const footerYear = document.querySelector('footer p');
    if (footerYear) {
        const year = new Date().getFullYear();
        footerYear.textContent = footerYear.textContent.replace('2023', year);
    }
    
    // Handle navigation active state
    const currentPage = window.location.pathname.split('/').pop();
    const navLinks = document.querySelectorAll('nav ul li a');
    
    navLinks.forEach(link => {
        const linkPage = link.getAttribute('href');
        if (currentPage === linkPage || 
            (currentPage === '' && linkPage === 'index.html')) {
            link.classList.add('active');
        }
    });
});

// Helper functions for API interactions
const API = {
    // Base URL for API (would be replaced with actual API URL in production)
    baseUrl: '/api',
    
    // Generic fetch wrapper with error handling
    async fetch(endpoint, options = {}) {
        try {
            // For demo purposes, simulate API delay
            await new Promise(resolve => setTimeout(resolve, 800));
            
            // This is a placeholder for actual API calls
            // In a real application, this would make actual fetch calls
            console.log(`API call to ${endpoint} with options:`, options);
            
            // Return mock data based on endpoint for demo purposes
            return this.getMockData(endpoint);
        } catch (error) {
            console.error('API error:', error);
            throw error;
        }
    },
    
    // Mock data for demonstration
    getMockData(endpoint) {
        // For demo purposes, return mock data based on endpoint
        if (endpoint.includes('emails')) {
            return this.getMockEmails();
        } else if (endpoint.includes('vendors')) {
            return this.getMockVendors();
        } else {
            return { message: 'No mock data available for this endpoint' };
        }
    },
    
    // Mock emails data
    getMockEmails() {
        return {
            total: 25,
            page: 1,
            pageSize: 10,
            emails: Array.from({ length: 10 }, (_, i) => ({
                id: `email-${i+1}`,
                subject: `メール件名サンプル ${i+1}`,
                sender: `sender${i+1}@example.com`,
                userEMailAddress: `user${Math.floor(i/3)+1}@example.com`,
                receivedDate: new Date(2023, 0, i+1).toISOString(),
                isRead: i % 3 === 0,
                content: `これはサンプルのメール内容です。実際のAPIが実装されるまでのプレースホルダーとして表示されています。\n\nこのメールには、イベントの託児サービスに関する情報が含まれています。\n\nEmail ID: ${i+1}`
            }))
        };
    },
    
    // Mock vendors data
    getMockVendors() {
        return {
            vendors: [
                {
                    id: 'vendor-1',
                    name: '株式会社キッズケア',
                    services: ['託児サービス', '送迎サービス'],
                    price: 5000,
                    rating: 4.5,
                    contact: '0120-111-222',
                    details: '子供向けイベント企画20年の実績があります。保育士常駐で安心安全な託児サービスを提供しています。'
                },
                {
                    id: 'vendor-2',
                    name: 'チャイルドサポート',
                    services: ['託児サービス', 'イベント企画'],
                    price: 4500,
                    rating: 4.2,
                    contact: '0120-333-444',
                    details: '全国展開している託児サービス。短時間から長時間まで柔軟に対応可能です。'
                },
                {
                    id: 'vendor-3',
                    name: 'ベビーシッターズ',
                    services: ['託児サービス', '保育士派遣'],
                    price: 6000,
                    rating: 4.8,
                    contact: '0120-555-666',
                    details: '厳選された有資格者による高品質な託児サービス。英語対応も可能です。'
                },
                {
                    id: 'vendor-4',
                    name: 'キッズフレンズ',
                    services: ['託児サービス', 'お遊び会'],
                    price: 4000,
                    rating: 3.9,
                    contact: '0120-777-888',
                    details: 'リーズナブルな価格設定が特徴。お子様が飽きないアクティビティも充実しています。'
                }
            ]
        };
    }
};

// Utility functions
const Utils = {
    // Format date to Japanese format
    formatDate(dateStr) {
        const date = new Date(dateStr);
        return date.toLocaleDateString('ja-JP', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },
    
    // Display error messages
    showError(message) {
        console.error(message);
        alert('エラーが発生しました: ' + message);
    }
};