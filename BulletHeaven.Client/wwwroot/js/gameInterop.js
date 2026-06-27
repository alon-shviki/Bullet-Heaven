const _physicsWorker = new Worker('js/physicsWorker.js');
_physicsWorker.onmessage = e => console.log('[physicsWorker]', e.data);
window.postToWorker = msg => _physicsWorker.postMessage(msg);

window.gameInterop = {
    _activeKeys: {},
    _spacePressedThisFrame: false,
    _rPressedThisFrame: false,

    getViewportSize() {
        return [window.innerWidth, window.innerHeight];
    },

    startLoop(dotnetRef) {
        function tick(timestamp) {
            dotnetRef.invokeMethodAsync('Tick', timestamp);
            window._rafId = requestAnimationFrame(tick);
        }
        window._rafId = requestAnimationFrame(tick);
    },

    stopLoop() {
        cancelAnimationFrame(window._rafId);
    },

    initInput() {
        document.addEventListener('keydown', e => {
            if (e.code === 'Space') {
                e.preventDefault();
                window.gameInterop._spacePressedThisFrame = true;
            }
            if (e.code === 'KeyR') {
                window.gameInterop._rPressedThisFrame = true;
            }
            window.gameInterop._activeKeys[e.code] = true;
        });
        document.addEventListener('keyup', e => {
            window.gameInterop._activeKeys[e.code] = false;
        });
    },

    // Returns [vx, vy, spaceEdge] — spaceEdge is 1 on the frame Space was first pressed, then resets
    getMovementVector() {
        const k = window.gameInterop._activeKeys;
        let vx = 0, vy = 0;
        if (k['KeyW'] || k['ArrowUp'])    vy -= 1;
        if (k['KeyS'] || k['ArrowDown'])  vy += 1;
        if (k['KeyA'] || k['ArrowLeft'])  vx -= 1;
        if (k['KeyD'] || k['ArrowRight']) vx += 1;
        const mag = Math.sqrt(vx * vx + vy * vy);
        if (mag > 0) { vx /= mag; vy /= mag; }
        const space = window.gameInterop._spacePressedThisFrame ? 1.0 : 0.0;
        const r     = window.gameInterop._rPressedThisFrame     ? 1.0 : 0.0;
        window.gameInterop._spacePressedThisFrame = false;
        window.gameInterop._rPressedThisFrame     = false;
        return [vx, vy, space, r];
    },

    // ── Archive entity preview rendering ────────────────────────────────────
    drawEntityPreviews() {
        document.querySelectorAll('canvas[data-entity]').forEach(cv => {
            const ctx = cv.getContext('2d');
            ctx.clearRect(0, 0, cv.width, cv.height);
            const cx = cv.width / 2, cy = cv.height / 2;
            const type = cv.dataset.entity;
            if (type === 'standard') _drawStandard(ctx, cx, cy);
            if (type === 'runner')   _drawRunner(ctx, cx, cy);
            if (type === 'tank')     _drawTank(ctx, cx, cy);
            if (type === 'elite')    _drawElite(ctx, cx, cy);
            if (type === 'boss')     _drawBoss(ctx, cx, cy);
        });
    }
};

// ── Entity draw routines (mirrors C# Game.Render.cs 1:1) ────────────────────
function _drawStandard(ctx, x, y) {
    const r = 12;
    ctx.beginPath();
    for (let i = 0; i < 6; i++) {
        const a = Math.PI / 6 + i * Math.PI / 3;
        const px = x + r * Math.cos(a), py = y + r * Math.sin(a);
        i === 0 ? ctx.moveTo(px, py) : ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fillStyle = '#8a5cd0'; ctx.fill();
    ctx.lineWidth = 2; ctx.strokeStyle = '#371c5e'; ctx.stroke();
    ctx.beginPath(); ctx.arc(x, y, 4.6, 0, Math.PI * 2);
    ctx.fillStyle = '#1d0f33'; ctx.fill();
    ctx.beginPath(); ctx.arc(x, y, 2.1, 0, Math.PI * 2);
    ctx.fillStyle = '#ff5a7a'; ctx.fill();
}

function _drawRunner(ctx, x, y) {
    const outer = 8, inner = 3;
    ctx.beginPath();
    for (let k = 0; k < 8; k++) {
        const r = (k % 2 === 0) ? outer : inner;
        const a = -Math.PI / 2 + k * Math.PI / 4;
        const px = x + r * Math.cos(a), py = y + r * Math.sin(a);
        k === 0 ? ctx.moveTo(px, py) : ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fillStyle = '#f5a623'; ctx.fill();
    ctx.lineWidth = 1.6; ctx.strokeStyle = '#6e3d00'; ctx.stroke();
    ctx.beginPath(); ctx.arc(x, y, 2, 0, Math.PI * 2);
    ctx.fillStyle = '#fff0c2'; ctx.fill();
}

function _drawTank(ctx, x, y) {
    const r = 20, ri = 12.5;
    ctx.beginPath();
    for (let i = 0; i < 8; i++) {
        const a = Math.PI / 8 + i * Math.PI / 4;
        const px = x + r * Math.cos(a), py = y + r * Math.sin(a);
        i === 0 ? ctx.moveTo(px, py) : ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fillStyle = '#5d7a6b'; ctx.fill();
    ctx.lineWidth = 3; ctx.strokeStyle = '#1f2a25'; ctx.stroke();
    ctx.beginPath();
    for (let i = 0; i < 8; i++) {
        const a = Math.PI / 8 + i * Math.PI / 4;
        const px = x + ri * Math.cos(a), py = y + ri * Math.sin(a);
        i === 0 ? ctx.moveTo(px, py) : ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.lineWidth = 2; ctx.strokeStyle = '#3c5249'; ctx.stroke();
    ctx.fillStyle = '#cdd6cf';
    for (let i = 0; i < 4; i++) {
        const a = Math.PI / 4 + i * Math.PI / 2;
        ctx.beginPath(); ctx.arc(x + 15 * Math.cos(a), y + 15 * Math.sin(a), 1.9, 0, Math.PI * 2);
        ctx.fill();
    }
    ctx.fillStyle = '#10241c';
    ctx.fillRect(x - 7, y - 2.2, 14, 4.4);
}

function _drawElite(ctx, x, y) {
    const outer = 16, inner = 8;
    ctx.shadowBlur = 16; ctx.shadowColor = '#ff4f9d';
    ctx.beginPath(); ctx.arc(x, y, 18.5, 0, Math.PI * 2);
    ctx.lineWidth = 2; ctx.strokeStyle = '#ff4f9d'; ctx.stroke();
    ctx.shadowBlur = 0;
    ctx.beginPath();
    for (let k = 0; k < 12; k++) {
        const r = (k % 2 === 0) ? outer : inner;
        const a = -Math.PI / 2 + k * Math.PI / 6;
        const px = x + r * Math.cos(a), py = y + r * Math.sin(a);
        k === 0 ? ctx.moveTo(px, py) : ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fillStyle = '#e0457b'; ctx.fill();
    ctx.lineWidth = 2; ctx.strokeStyle = '#530f30'; ctx.stroke();
    ctx.shadowBlur = 10; ctx.shadowColor = '#ff4f9d';
    ctx.beginPath(); ctx.arc(x, y, 5, 0, Math.PI * 2);
    ctx.fillStyle = '#ffd9e8'; ctx.fill();
    ctx.shadowBlur = 0;
}

function _drawBoss(ctx, x, y) {
    const outer = 35, inner = 22;
    ctx.shadowBlur = 26; ctx.shadowColor = '#ff6a2c';
    ctx.beginPath(); ctx.arc(x, y, 39, 0, Math.PI * 2);
    ctx.lineWidth = 3; ctx.strokeStyle = '#ff6a2c'; ctx.stroke();
    ctx.shadowBlur = 0;
    ctx.beginPath();
    for (let k = 0; k < 20; k++) {
        const r = (k % 2 === 0) ? outer : inner;
        const a = -Math.PI / 2 + k * Math.PI / 10;
        const px = x + r * Math.cos(a), py = y + r * Math.sin(a);
        k === 0 ? ctx.moveTo(px, py) : ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fillStyle = '#c11f3a'; ctx.fill();
    ctx.lineWidth = 3; ctx.strokeStyle = '#460813'; ctx.stroke();
    ctx.beginPath(); ctx.arc(x, y, 18, 0, Math.PI * 2);
    ctx.fillStyle = '#7a1226'; ctx.fill();
    ctx.lineWidth = 2; ctx.strokeStyle = '#460813'; ctx.stroke();
    ctx.lineWidth = 2.4; ctx.strokeStyle = '#460813';
    for (let i = 0; i < 8; i++) {
        const a = i * Math.PI / 4;
        ctx.beginPath();
        ctx.moveTo(x + 14 * Math.cos(a), y + 14 * Math.sin(a));
        ctx.lineTo(x + 18 * Math.cos(a), y + 18 * Math.sin(a));
        ctx.stroke();
    }
    ctx.shadowBlur = 18; ctx.shadowColor = '#ff6a2c';
    ctx.beginPath(); ctx.arc(x, y, 9, 0, Math.PI * 2);
    ctx.fillStyle = '#ffb43d'; ctx.fill();
    ctx.shadowBlur = 0;
    ctx.fillStyle = '#240306';
    ctx.beginPath();
    ctx.moveTo(x, y - 7); ctx.lineTo(x + 2.6, y);
    ctx.lineTo(x, y + 7); ctx.lineTo(x - 2.6, y);
    ctx.closePath(); ctx.fill();
}
