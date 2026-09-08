// ═══════════════════════════════════════════════════════════════════════════
//  FashionStore - Main JavaScript
// ═══════════════════════════════════════════════════════════════════════════

// ─── Toast Notification ──────────────────────────────────────────────────
function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;
    const toast = document.createElement('div');
    toast.className = `toast-item ${type}`;
    const icons = { success: '✅', error: '❌', info: 'ℹ️' };
    toast.innerHTML = `<span>${icons[type] || 'ℹ️'}</span> <span>${message}</span>`;
    container.appendChild(toast);
    setTimeout(() => { toast.style.opacity = '0'; setTimeout(() => toast.remove(), 300); }, 3500);
}

// ─── تعداد سبد خرید ──────────────────────────────────────────────────────
async function updateCartCount() {
    try {
        const res = await fetch('/Cart/Count');
        if (res.ok) {
            const data = await res.json();
            const badge = document.getElementById('cartCount');
            if (badge) badge.textContent = data.count ?? 0;
        }
    } catch {}
}

// ─── Sidebar toggle (موبایل) ─────────────────────────────────────────────
function toggleSidebar() {
    const sidebar = document.querySelector('.admin-sidebar');
    if (sidebar) sidebar.classList.toggle('open');
}

// ─── تایید حذف ───────────────────────────────────────────────────────────
function confirmDelete(formId, message = 'آیا مطمئن هستید؟') {
    if (confirm(message)) {
        document.getElementById(formId)?.submit();
    }
}

// ─── Preview تصویر قبل از آپلود ──────────────────────────────────────────
function previewImage(input, imgId) {
    const reader = new FileReader();
    reader.onload = e => {
        const img = document.getElementById(imgId);
        if (img) img.src = e.target.result;
    };
    if (input.files?.[0]) reader.readAsDataURL(input.files[0]);
}

// ─── Discount code checker ────────────────────────────────────────────────
async function checkDiscount(code, orderAmount, resultBoxId) {
    const box = document.getElementById(resultBoxId);
    if (!code || !box) return;
    box.innerHTML = '<small style="color:gray">⏳ بررسی کد...</small>';
    try {
        const res = await fetch('/Cart/ApplyDiscount', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `code=${encodeURIComponent(code)}&orderAmount=${orderAmount}`
        });
        const data = await res.json();
        if (data.success) {
            box.innerHTML = `<small style="color:green">✅ ${data.message}</small>`;
            // ذخیره در hidden field
            const hiddenId = document.getElementById('discountId');
            if (hiddenId) hiddenId.value = data.discountId;
            const hiddenAmt = document.getElementById('discountAmount');
            if (hiddenAmt) hiddenAmt.value = data.discountAmount;
        } else {
            box.innerHTML = `<small style="color:red">❌ ${data.message}</small>`;
        }
    } catch {
        box.innerHTML = '<small style="color:red">خطا در ارتباط</small>';
    }
}

// ─── Virtual Try-On با MediaPipe ─────────────────────────────────────────
let tryOnActive = false;
let tryOnOverlayImg = null;

async function startTryOn(overlayImageUrl) {
    if (tryOnActive) { stopTryOn(); return; }
    const video = document.getElementById('webcamVideo');
    const canvas = document.getElementById('tryonCanvas');
    if (!video || !canvas) return;

    // بارگذاری تصویر overlay لباس
    tryOnOverlayImg = new Image();
    tryOnOverlayImg.src = overlayImageUrl;
    await new Promise(r => { tryOnOverlayImg.onload = r; tryOnOverlayImg.onerror = r; });

    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' } });
        video.srcObject = stream;
        await video.play();
        tryOnActive = true;
        document.getElementById('tryOnBtn')?.setAttribute('data-active', 'true');

        const ctx = canvas.getContext('2d');
        const drawLoop = () => {
            if (!tryOnActive) return;
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            ctx.save();
            ctx.scale(-1, 1);
            ctx.drawImage(video, -canvas.width, 0, canvas.width, canvas.height);
            ctx.restore();
            // overlay ساده: لباس روی بدن
            if (tryOnOverlayImg?.complete) {
                const x = canvas.width * 0.2;
                const y = canvas.height * 0.15;
                const w = canvas.width * 0.6;
                const h = canvas.height * 0.7;
                ctx.globalAlpha = 0.85;
                ctx.drawImage(tryOnOverlayImg, x, y, w, h);
                ctx.globalAlpha = 1;
            }
            requestAnimationFrame(drawLoop);
        };
        drawLoop();
        showToast('دوربین فعال شد - لباس را روی بدن ببینید', 'success');
    } catch (err) {
        showToast('دسترسی به دوربین داده نشد', 'error');
    }
}

function stopTryOn() {
    tryOnActive = false;
    const video = document.getElementById('webcamVideo');
    const canvas = document.getElementById('tryonCanvas');
    if (video?.srcObject) {
        video.srcObject.getTracks().forEach(t => t.stop());
        video.srcObject = null;
    }
    if (canvas) {
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
    }
    document.getElementById('tryOnBtn')?.removeAttribute('data-active');
}

function captureSnapshot() {
    const canvas = document.getElementById('tryonCanvas');
    if (!canvas) return;
    const link = document.createElement('a');
    link.download = 'fashion-tryon.png';
    link.href = canvas.toDataURL();
    link.click();
    showToast('تصویر ذخیره شد!', 'success');
}

// ─── init ────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    updateCartCount();

    // Auto-hide alerts
    setTimeout(() => {
        document.querySelectorAll('.alert').forEach(a => {
            a.classList.remove('show');
            setTimeout(() => a.remove(), 300);
        });
    }, 4000);
});
