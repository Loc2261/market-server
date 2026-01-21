/**
 * Reusable Pagination Component
 * @param {string} containerId - ID of the container element where pagination controls will be rendered.
 * @param {number} currentPage - Current page number (1-based).
 * @param {number} totalPages - Total number of pages.
 * @param {function} onPageChange - Callback function to handle page changes. Receives the new page number.
 */
function renderPagination(containerId, currentPage, totalPages, onPageChange) {
    const container = document.getElementById(containerId);
    if (!container) return;

    // Always show pagination UI if requested, as per user's requirement for consistency.
    // If totalPages is 0, we treat it as 1 for UI purposes.
    const displayPages = Math.max(1, totalPages);

    let html = '<div class="pagination-modern">';

    // Previous Button
    html += `
        <button class="page-btn prev-btn ${currentPage === 1 ? 'disabled' : ''}" 
                onclick="handlePaginationClick(${currentPage - 1})" 
                ${currentPage === 1 ? 'disabled' : ''}>
            <i class="bi bi-chevron-left"></i> Trước
        </button>
    `;

    // Page Numbers
    // Logic: Always show first, last, and window around current
    const windowSize = 2; // How many pages to show around current

    if (displayPages <= 7) {
        // Show all
        for (let i = 1; i <= displayPages; i++) {
            html += renderPageButton(i, currentPage);
        }
    } else {
        // Complex logic with ellipses
        html += renderPageButton(1, currentPage);

        if (currentPage > windowSize + 2) {
            html += '<span class="page-ellipsis">...</span>';
        }

        let start = Math.max(2, currentPage - windowSize);
        let end = Math.min(displayPages - 1, currentPage + windowSize);

        for (let i = start; i <= end; i++) {
            html += renderPageButton(i, currentPage);
        }

        if (currentPage < displayPages - (windowSize + 1)) {
            html += '<span class="page-ellipsis">...</span>';
        }

        html += renderPageButton(displayPages, currentPage);
    }

    // Next Button
    html += `
        <button class="page-btn next-btn ${currentPage === displayPages ? 'disabled' : ''}" 
                onclick="handlePaginationClick(${currentPage + 1})"
                ${currentPage === displayPages ? 'disabled' : ''}>
            Tiếp <i class="bi bi-chevron-right"></i>
        </button>
    `;

    html += '</div>';
    container.innerHTML = html;

    // Attach global handler if not exists
    window.handlePaginationClick = (page) => {
        if (page < 1 || page > displayPages) return;
        onPageChange(page);
    };
}

function renderPageButton(page, current) {
    const activeClass = page === current ? 'active' : '';
    return `<button class="page-btn ${activeClass}" onclick="handlePaginationClick(${page})">${page}</button>`;
}

/* Styles inject (Optional, or add to site.css) */
const style = document.createElement('style');
style.innerHTML = `
    .pagination-modern {
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 0.5rem;
        margin-top: 2rem;
        padding-top: 1rem;
    }

    .page-btn {
        min-width: 40px;
        height: 40px;
        border: 1px solid #e2e8f0;
        background: white;
        border-radius: 8px;
        color: #64748b;
        font-weight: 600;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        transition: all 0.2s;
        padding: 0 10px;
    }

    .page-btn:hover:not(.disabled) {
        border-color: #06b6d4;
        color: #06b6d4;
        background: #ecfeff;
    }

    .page-btn.active {
        background: linear-gradient(135deg, #06b6d4 0%, #3b82f6 100%);
        color: white;
        border: none;
        box-shadow: 0 4px 6px -1px rgba(6, 182, 212, 0.3);
    }

    .page-btn.disabled {
        opacity: 0.5;
        cursor: not-allowed;
        background: #f8fafc;
    }
    
    .page-btn.prev-btn, .page-btn.next-btn {
        padding: 0 1.25rem;
        gap: 0.5rem;
    }

    .page-ellipsis {
        color: #94a3b8;
        font-weight: bold;
        letter-spacing: 2px;
    }
`;
document.head.appendChild(style);
