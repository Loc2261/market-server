// ===== 1. CẤU HÌNH CHUNG =====
const API_AUTH_LOGOUT = '/Account/Logout'; // Đường dẫn Action MVC hoặc API

// ===== 2. QUẢN LÝ TRẠNG THÁI ĐĂNG NHẬP (UI) =====
document.addEventListener('DOMContentLoaded', () => {
    // updateAuthUI(); // Disabled: Using Server-Side Rendering in Layout
});

function updateAuthUI() {
    // Giả sử bạn lưu thông tin user vào localStorage khi Login thành công
    // Hoặc kiểm tra Session từ Server (trong thực tế MVC thường render từ server-side Razor)
    // Dưới đây là ví dụ Client-side rendering:

    const userJson = localStorage.getItem('user');
    // Hoặc lấy từ Session cookie nếu bạn dùng MVC thuần
    // const isLoggedIn = document.cookie.includes(".AspNetCore.Identity.Application");

    const authNav = document.getElementById('auth-nav');
    const authNavMobile = document.getElementById('auth-nav-mobile');

    if (!authNav) return;

    if (userJson) {
        const user = JSON.parse(userJson);
        const initial = (user.fullName || user.username || 'U').charAt(0).toUpperCase();

        // Giao diện khi ĐÃ đăng nhập
        const loggedInHtml = `
            <a href="/Chat" class="nav-btn nav-btn-outline" title="Tin nhắn">
                💬 <span class="badge">Chat</span>
            </a>
            <div class="user-dropdown" onclick="window.location.href='/Account/Profile'">
                <div class="user-avatar">${initial}</div>
                <div style="font-size: 0.9rem; font-weight: 600;">${escapeHtml(user.fullName || user.username)}</div>
            </div>
            <button onclick="handleLogout()" class="nav-btn" style="color: var(--danger); background:none; border:none;">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
            </button>
        `;

        authNav.innerHTML = loggedInHtml;
        if (authNavMobile) authNavMobile.innerHTML = `<a href="/Account/Logout" class="nav-link">Đăng xuất</a>`;

    } else {
        // Giao diện khi CHƯA đăng nhập
        const guestHtml = `
            <a href="/Auth/Login" class="nav-btn nav-btn-outline">Đăng nhập</a>
            <a href="/Auth/Register" class="nav-btn nav-btn-primary">Đăng ký</a>
        `;

        authNav.innerHTML = guestHtml;
        if (authNavMobile) authNavMobile.innerHTML = `
            <a href="/Auth/Login" class="nav-link">Đăng nhập</a>
            <a href="/Auth/Register" class="nav-link">Đăng ký</a>
        `;
    }
}

// ===== 3. CHỨC NĂNG LOGOUT =====
function handleLogout() {
    // Xóa storage
    localStorage.removeItem('user');
    localStorage.removeItem('token');

    // Gọi Server để xóa Cookie
    window.location.href = '/Auth/Logout';
}

// ===== 4. MENU MOBILE (Slide-in) =====
function toggleMobileMenu() {
    const menu = document.getElementById('mobile-menu');
    const overlay = document.getElementById('mobileOverlay');
    menu.classList.add('open');
    menu.style.display = 'flex';
    if (overlay) overlay.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeMobileMenu() {
    const menu = document.getElementById('mobile-menu');
    const overlay = document.getElementById('mobileOverlay');
    menu.classList.remove('open');
    if (overlay) overlay.classList.remove('active');
    document.body.style.overflow = '';
    setTimeout(() => { menu.style.display = 'none'; }, 500);
}

// Navbar scroll effect
window.addEventListener('scroll', () => {
    const navbar = document.getElementById('mainNavbar');
    if (navbar) {
        if (window.scrollY > 10) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }
});

// ===== 5. TOAST NOTIFICATION (Thông báo đẹp) =====
function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;

    let icon = '✅';
    if (type === 'error') icon = '❌';
    if (type === 'info') icon = 'ℹ️';

    toast.innerHTML = `<span>${icon}</span> <span>${message}</span>`;
    container.appendChild(toast);

    // Tự biến mất sau 3s
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ===== 6. UTILS =====
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.innerText = text;
    return div.innerHTML;
}

function formatPrice(price) {
    if (price === undefined || price === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
}

function parseDate(dateStr) {
    if (!dateStr) return null;
    // Nếu chuỗi ISO từ C# không có Z hoặc múi giờ, ta thêm Z để JS hiểu là UTC
    if (typeof dateStr === 'string' && !dateStr.includes('Z') && !dateStr.includes('+')) {
        dateStr += 'Z';
    }
    return new Date(dateStr);
}

function formatTime(dateStr) {
    const date = parseDate(dateStr);
    if (!date) return '';
    return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

function formatDate(dateString) {
    const date = parseDate(dateString);
    if (!date) return '';
    return date.toLocaleDateString('vi-VN');
}

function timeAgo(dateString) {
    const date = parseDate(dateString);
    if (!date) return '';

    const now = new Date();
    const diffInSeconds = Math.floor((now - date) / 1000);

    if (diffInSeconds < 60) return 'Vừa xong';

    const diffInMinutes = Math.floor(diffInSeconds / 60);
    if (diffInMinutes < 60) return `${diffInMinutes} phút trước`;

    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) return `${diffInHours} giờ trước`;

    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 30) return `${diffInDays} ngày trước`;

    const diffInMonths = Math.floor(diffInDays / 30);
    if (diffInMonths < 12) return `${diffInMonths} tháng trước`;

    return formatDate(dateString);
}

// ===== 6.1 GLOBAL WISHLIST SYNC =====
async function syncWishlistCount() {
    const badge = document.getElementById('wishlistBadge');
    if (!badge) return;

    try {
        const res = await fetch('/api/wishlist/count');
        if (res.ok) {
            const data = await res.json();
            const count = data.count || 0;
            if (count > 0) {
                badge.innerText = count > 99 ? '99+' : count;
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        }
    } catch (e) {
        console.error('Error syncing wishlist count:', e);
    }
}

async function toggleWishlist(productId, el) {
    // Nếu element được truyền vào, dùng nó để xác định trạng thái ban đầu
    const isAdded = el ? el.classList.contains('active') : false;
    // Nếu không có element (ví dụ gọi từ detail page), tìm icon theo id nếu có, hoặc dùng fetch check trước
    // Tuy nhiên hầu hết các nơi đều truyền element trực tiếp.
    
    try {
        const method = isAdded ? 'DELETE' : 'POST';
        const res = await fetch(`/api/wishlist/${productId}`, { method: method });
        
        if (res.ok) {
            const newState = !isAdded;
            
            // Tìm tất cả các button yêu thích của cùng một sản phẩm trên trang
            // Sử dụng attribute data-product-id để định danh (cần update HTML)
            const allBtns = document.querySelectorAll(`[data-wishlist-id="${productId}"]`);
            
            allBtns.forEach(btn => {
                const icon = btn.querySelector('i');
                if (newState) {
                    btn.classList.add('active');
                    if (icon) {
                        icon.classList.remove('bi-heart');
                        icon.classList.add('bi-heart-fill');
                    }
                } else {
                    btn.classList.remove('active');
                    if (icon) {
                        icon.classList.remove('bi-heart-fill');
                        icon.classList.add('bi-heart');
                    }
                }
            });
            
            // Callback cho trang wishlist nếu cần tải lại danh sách
            if (typeof loadWishlist === 'function' && window.location.pathname.toLowerCase().includes('/wishlist')) {
                loadWishlist();
            }

            // Đồng bộ lại badge số lượng trên navbar
            syncWishlistCount();
            
            showToast(newState ? 'Đã thêm vào danh sách yêu thích' : 'Đã xóa khỏi danh sách yêu thích', 'success');
        } else if (res.status === 401) {
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        } else {
            const err = await res.json();
            showToast(err.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (e) {
        console.error('Wishlist error:', e);
        showToast('Lỗi kết nối máy chủ', 'error');
    }
}

// ===== 7. GLOBAL CHAT NOTIFICATIONS =====
let globalChatConnection = null;

async function syncGlobalUnreadCount() {
    try {
        const res = await fetch('/api/chat/unread-count');
        if (res.ok) {
            const count = await res.json();
            updateUnreadBadges(count);
        }
    } catch (e) {
        console.error('Error syncing unread count:', e);
    }
}

function updateUnreadBadges(count) {
    const badges = ['global-unread-badge', 'global-unread-badge-mobile'];
    badges.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            if (count > 0) {
                el.innerText = count > 99 ? '99+' : count;
                el.style.display = 'inline-block';
            } else {
                el.style.display = 'none';
            }
        }
    });
}

async function initGlobalChatSignalR() {
    // Chỉ khởi chạy nếu không ở trang Chat (vì trang Chat có SignalR riêng)
    if (window.location.pathname.toLowerCase().includes('/chat')) return;

    globalChatConnection = new signalR.HubConnectionBuilder()
        .withUrl('/chathub')
        .withAutomaticReconnect()
        .build();

    globalChatConnection.on('ReceiveMessage', (data) => {
        console.log('Global message received:', data);
        syncGlobalUnreadCount();

        // Hiển thị toast thông báo
        const msg = typeof data === 'string' ? data : (data.content || 'Bạn có tin nhắn mới');
        const name = data.senderName || 'Ai đó';
        showToast(`Tin nhắn mới từ ${name}: "${msg.substring(0, 30)}${msg.length > 30 ? '...' : ''}"`, 'info');
    });

    globalChatConnection.on('MessagesRead', () => {
        syncGlobalUnreadCount();
    });

    try {
        await globalChatConnection.start();
        console.log('Global Chat SignalR connected');
    } catch (err) {
        console.error('Global Chat SignalR error:', err);
    }
}

// Chạy khi trang tải xong
document.addEventListener('DOMContentLoaded', () => {
    // Kiểm tra xem user có đang đăng nhập không
    const chatBadge = document.getElementById('global-unread-badge');
    const wishlistBadge = document.getElementById('wishlistBadge');
    
    if (chatBadge || wishlistBadge) {
        if (chatBadge) {
            syncGlobalUnreadCount();
            initGlobalChatSignalR();
        }
        if (wishlistBadge) {
            syncWishlistCount();
        }
    }
});