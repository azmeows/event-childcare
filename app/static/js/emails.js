// Emails handling JavaScript

// State to track current emails data
const emailsState = {
    emails: [],
    currentPage: 1,
    totalPages: 1,
    filter: 'all',
    selectedEmailId: null
};

// Initialize emails page
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if we're on the emails page
    if (!document.getElementById('email-list')) {
        return;
    }
    
    console.log('Emails page initialized');
    
    // Load initial emails
    loadEmails();
    
    // Set up event listeners
    document.getElementById('search-email').addEventListener('input', handleSearchChange);
    document.getElementById('filter-email').addEventListener('change', handleFilterChange);
    document.getElementById('prev-page').addEventListener('click', handlePrevPage);
    document.getElementById('next-page').addEventListener('click', handleNextPage);
});

// Load emails from API
async function loadEmails() {
    try {
        const emailListEl = document.getElementById('email-list');
        emailListEl.innerHTML = '<p class="loading">データをロード中...</p>';
        
        // Make API call to get emails
        const response = await API.fetch(`/emails?page=${emailsState.currentPage}&filter=${emailsState.filter}`);
        
        // Update state
        emailsState.emails = response.emails;
        emailsState.totalPages = Math.ceil(response.total / response.pageSize);
        
        // Update UI
        renderEmails();
        updatePagination();
    } catch (error) {
        Utils.showError('メールデータの取得に失敗しました');
    }
}

// Render emails list
function renderEmails() {
    const emailListEl = document.getElementById('email-list');
    
    if (!emailsState.emails || emailsState.emails.length === 0) {
        emailListEl.innerHTML = '<p class="loading">メールが見つかりませんでした</p>';
        return;
    }
    
    // Create HTML for email items
    const emailsHtml = emailsState.emails.map(email => {
        const isSelectedClass = email.id === emailsState.selectedEmailId ? 'selected' : '';
        const isReadClass = email.isRead ? 'read' : 'unread';
        
        return `
            <div class="email-item ${isSelectedClass} ${isReadClass}" data-id="${email.id}">
                <h4>${email.subject}</h4>
                <div class="meta">
                    <span>送信者: ${email.sender}</span> | 
                    <span>受信日: ${Utils.formatDate(email.receivedDate)}</span>
                </div>
            </div>
        `;
    }).join('');
    
    emailListEl.innerHTML = emailsHtml;
    
    // Add event listeners to email items
    document.querySelectorAll('.email-item').forEach(item => {
        item.addEventListener('click', () => selectEmail(item.dataset.id));
    });
}

// Update pagination controls
function updatePagination() {
    document.getElementById('prev-page').disabled = emailsState.currentPage <= 1;
    document.getElementById('next-page').disabled = emailsState.currentPage >= emailsState.totalPages;
    document.getElementById('page-info').textContent = `ページ ${emailsState.currentPage} / ${emailsState.totalPages}`;
}

// Select an email and show its details
function selectEmail(emailId) {
    emailsState.selectedEmailId = emailId;
    
    // Update UI to show selected email
    document.querySelectorAll('.email-item').forEach(item => {
        item.classList.toggle('selected', item.dataset.id === emailId);
    });
    
    // Find the selected email
    const email = emailsState.emails.find(e => e.id === emailId);
    if (!email) return;
    
    // Display email details
    const detailEl = document.getElementById('email-content');
    detailEl.innerHTML = `
        <div class="email-header">
            <h3>${email.subject}</h3>
            <div class="meta">
                <p><strong>送信者:</strong> ${email.sender}</p>
                <p><strong>宛先:</strong> ${email.userEMailAddress}</p>
                <p><strong>受信日時:</strong> ${Utils.formatDate(email.receivedDate)}</p>
            </div>
        </div>
        <div class="email-body">
            <pre>${email.content}</pre>
        </div>
        <div class="email-actions">
            <button class="btn">返信</button>
            <button class="btn">転送</button>
            <button class="btn">業者比較に追加</button>
        </div>
    `;
}

// Handle search input
function handleSearchChange(e) {
    console.log('Search changed:', e.target.value);
    // In a real app, this would filter emails based on search term
    // For demo purposes, just log the search term
}

// Handle filter change
function handleFilterChange(e) {
    emailsState.filter = e.target.value;
    emailsState.currentPage = 1; // Reset to first page
    loadEmails();
}

// Handle pagination
function handlePrevPage() {
    if (emailsState.currentPage > 1) {
        emailsState.currentPage--;
        loadEmails();
    }
}

function handleNextPage() {
    if (emailsState.currentPage < emailsState.totalPages) {
        emailsState.currentPage++;
        loadEmails();
    }
}