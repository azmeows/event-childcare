// Vendors handling JavaScript

// State to track current vendor data
const vendorsState = {
    vendors: [],
    filter: 'all',
    selectedVendorId: null
};

// Initialize vendors page
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if we're on the vendors page
    if (!document.getElementById('vendor-comparison-table')) {
        return;
    }
    
    console.log('Vendors comparison page initialized');
    
    // Load initial vendors
    loadVendors();
    
    // Set up event listeners
    document.getElementById('search-vendor').addEventListener('input', handleSearchChange);
    document.getElementById('filter-comparison').addEventListener('change', handleFilterChange);
});

// Load vendors from API
async function loadVendors() {
    try {
        const vendorBodyEl = document.getElementById('vendor-comparison-body');
        vendorBodyEl.innerHTML = '<tr><td colspan="6" class="loading">データをロード中...</td></tr>';
        
        // Make API call to get vendors
        const response = await API.fetch(`/vendors?filter=${vendorsState.filter}`);
        
        // Update state
        vendorsState.vendors = response.vendors;
        
        // Update UI
        renderVendors();
    } catch (error) {
        Utils.showError('業者データの取得に失敗しました');
    }
}

// Render vendors table
function renderVendors() {
    const vendorBodyEl = document.getElementById('vendor-comparison-body');
    
    if (!vendorsState.vendors || vendorsState.vendors.length === 0) {
        vendorBodyEl.innerHTML = '<tr><td colspan="6" class="loading">業者が見つかりませんでした</td></tr>';
        return;
    }
    
    // Create HTML for vendor rows
    const vendorsHtml = vendorsState.vendors.map(vendor => {
        return `
            <tr data-id="${vendor.id}" class="${vendor.id === vendorsState.selectedVendorId ? 'selected' : ''}">
                <td>${vendor.name}</td>
                <td>${vendor.services.join(', ')}</td>
                <td>${vendor.price.toLocaleString()}円</td>
                <td>${vendor.rating} / 5.0</td>
                <td>${vendor.contact}</td>
                <td>
                    <button class="btn details-btn" data-id="${vendor.id}">詳細</button>
                </td>
            </tr>
        `;
    }).join('');
    
    vendorBodyEl.innerHTML = vendorsHtml;
    
    // Add event listeners to detail buttons
    document.querySelectorAll('.details-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            selectVendor(btn.dataset.id);
        });
    });
    
    // Add event listeners to rows
    document.querySelectorAll('#vendor-comparison-body tr').forEach(row => {
        row.addEventListener('click', () => selectVendor(row.dataset.id));
    });
}

// Select a vendor and show its details
function selectVendor(vendorId) {
    vendorsState.selectedVendorId = vendorId;
    
    // Update UI to show selected vendor
    document.querySelectorAll('#vendor-comparison-body tr').forEach(row => {
        row.classList.toggle('selected', row.dataset.id === vendorId);
    });
    
    // Find the selected vendor
    const vendor = vendorsState.vendors.find(v => v.id === vendorId);
    if (!vendor) return;
    
    // Display vendor details
    const detailsEl = document.getElementById('vendor-details');
    detailsEl.innerHTML = `
        <h3>${vendor.name}</h3>
        <p><strong>提供サービス:</strong> ${vendor.services.join(', ')}</p>
        <p><strong>価格:</strong> ${vendor.price.toLocaleString()}円</p>
        <p><strong>評価:</strong> ${vendor.rating} / 5.0</p>
        <p><strong>連絡先:</strong> ${vendor.contact}</p>
        <p><strong>詳細情報:</strong></p>
        <p>${vendor.details}</p>
        <div class="vendor-actions">
            <button class="btn">連絡先をコピー</button>
            <button class="btn">メールで連絡</button>
            <button class="btn">メモを追加</button>
        </div>
    `;
}

// Handle search input
function handleSearchChange(e) {
    console.log('Search changed:', e.target.value);
    // In a real app, this would filter vendors based on search term
    // For demo purposes, just log the search term
}

// Handle filter change
function handleFilterChange(e) {
    const filterValue = e.target.value;
    vendorsState.filter = filterValue;
    
    // In a real app, this would reload data based on filter
    // For demo, we'll just sort the existing vendors
    if (filterValue === 'price') {
        vendorsState.vendors.sort((a, b) => a.price - b.price);
    } else if (filterValue === 'rating') {
        vendorsState.vendors.sort((a, b) => b.rating - a.rating);
    }
    
    renderVendors();
}