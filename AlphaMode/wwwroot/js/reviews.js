
(() => {
    'use strict';

    // CONFIG
    const PAGE_SIZE = 5;
    const REVIEWS_URL = '/alpha-reviews.json';

    // DOM refs (cached once)
    const $grid = document.getElementById('reviewsContainer');
    const $pager = document.getElementById('reviewsPager');
    const $avgBlock = document.getElementById('avgBlock');
    const $histogramBlock = document.getElementById('histogramBlock');
    const $reviewForm = document.getElementById('reviewForm');
    const $starPicker = document.getElementById('starPicker');
    const $ratingValue = document.getElementById('ratingValue');

    // runtime state
    let reviews = [];
    let pageCount = 0;
    let currentPage = 1;
    let stats = { avg: 0, counts: [0, 0, 0, 0, 0, 0] }; 

    // -----------------------
    // Utility helpers
    // -----------------------

    const formatDMY = (isoDate) => {
        if (!isoDate) return '';
        // Bulgarian format dd/mm/yyyy
        try {
            const d = new Date(isoDate);
            return d.toLocaleDateString('bg-BG', { day: '2-digit', month: '2-digit', year: 'numeric' });
        } catch (err) {
            return isoDate;
        }
    };

    // safe repeat for stars
    const starsText = (n) => '★'.repeat(n) + '☆'.repeat(5 - n);

    // create element from html string (small helper)
    const toElement = (html) => {
        const t = document.createElement('template');
        t.innerHTML = html.trim();
        return t.content.firstChild;
    };

    // -----------------------
    // Data fetching + stats
    // -----------------------

    /**
     * fetchReviews - fetches JSON, caches it into reviews variable
     * returns Promise<reviews>
     */
    async function fetchReviews() {
        if (reviews.length) return reviews; // cached
        try {
            const res = await fetch(REVIEWS_URL, { cache: "no-cache" });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const data = await res.json();
            // normalize / sort
            reviews = Array.isArray(data) ? data.slice().sort((a, b) => b.id - a.id) : [];
            pageCount = Math.max(1, Math.ceil(reviews.length / PAGE_SIZE));
            computeStats();
            return reviews;
        } catch (err) {
            // fail gracefully: keep empty list and log
            console.error('Failed to load reviews:', err);
            reviews = [];
            pageCount = 0;
            stats = { avg: 0, counts: [0, 0, 0, 0, 0, 0] };
            return reviews;
        }
    }

    /**
     * computeStats - compute average and counts[] once for the dataset
     */
    function computeStats() {
        const counts = [0, 0, 0, 0, 0, 0];
        let total = 0;
        reviews.forEach(r => {
            const v = Math.max(1, Math.min(5, +r.rating || 0));
            counts[v] = (counts[v] || 0) + 1;
            total += v;
        });
        const avg = reviews.length ? (total / reviews.length) : 0;
        stats = { avg: Number(avg.toFixed(2)), counts };
    }

    // -----------------------
    // Rendering
    // -----------------------

    /** renderAverageBlock */
    function renderAverageBlock() {
        if (!$avgBlock) return;
        $avgBlock.innerHTML = `
      <div class="fs-3 text-danger mb-1">★ ★ ★ ★ ★</div>
      <div class="fs-4 fw-semibold">${stats.avg || 0} от 5</div>
      <div class="small text-muted">От ${reviews.length} отзива</div>
    `;
    }

    /** renderHistogram */
    function renderHistogram() {
        if (!$histogramBlock) return;
        const max = Math.max(...stats.counts.slice(1), 1);
        const rows = [5, 4, 3, 2, 1].map(i => {
            const n = stats.counts[i] || 0;
            const widthPct = Math.round((n / max) * 100);
            return `
        <div class="d-flex align-items-center mb-1 small">
          <span class="me-2 text-danger h5">${'★'.repeat(i)}</span>
          <div class="flex-grow-1 bg-light">
            <div style="width:${widthPct}%;height:8px" class="bg-danger"></div>
          </div>
          <span class="ms-2">${n}</span>
        </div>`;
        }).join('');
        $histogramBlock.innerHTML = rows;
    }

    /** renderPage - draws page 'p' into the reviews container */
    function renderPage(p = 1) {
        currentPage = Math.max(1, Math.min(pageCount || 1, p));
        if (!$grid) return;

        // slice and build document fragment
        const slice = reviews.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);
        const frag = document.createDocumentFragment();

        slice.forEach(r => {
            const wrapper = document.createElement('div');
            wrapper.className = 'col-12';
            wrapper.innerHTML = `
        <div class="d-flex align-items-start border-bottom pb-2 mb-2 position-relative">
          <div class="me-3 text-primary fs-3">
            <i class="bi bi-person-circle text-dark" aria-hidden="true"></i>
          </div>
          <div class="flex-grow-1">
            <span class="fw-semibold">${escapeHtml(r.name || 'Анонимен')}</span>
            <div class="text-warning small mb-1">${escapeHtml(starsText(+r.rating || 0))}</div>
            <p class="small text-muted mb-0">"${escapeHtml(r.text || '')}"</p>
          </div>
          <span class="small text-muted ms-auto">${formatDMY(r.date)}</span>
        </div>`;
            frag.appendChild(wrapper);
        });

        // clear & append
        $grid.innerHTML = '';
        $grid.appendChild(frag);

        renderPager();
    }

    /** renderPager - builds pager UI */
    function renderPager() {
        if (!$pager) return;
        // build fragment
        const frag = document.createDocumentFragment();

        const liPrev = toElement(`
      <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
        <a class="page-link text-dark border-0" href="#" data-page="${currentPage - 1}">&laquo;</a>
      </li>
    `);
        frag.appendChild(liPrev);

        for (let i = 1; i <= (pageCount || 1); i++) {
            const activeClass = i === currentPage ? 'bg-dark text-white border-dark' : '';
            const li = toElement(`
        <li class="page-item">
          <a class="page-link ${activeClass} text-dark border-0" href="#" data-page="${i}">${i}</a>
        </li>
      `);
            frag.appendChild(li);
        }

        const liNext = toElement(`
      <li class="page-item ${currentPage === pageCount ? 'disabled' : ''}">
        <a class="page-link text-dark border-0" href="#" data-page="${currentPage + 1}">&raquo;</a>
      </li>
    `);
        frag.appendChild(liNext);

        $pager.innerHTML = '';
        $pager.appendChild(frag);
    }

    // -----------------------
    // Events
    // -----------------------

    /** pager click handler (delegated) */
    function setupPagerHandler() {
        if (!$pager) return;
        $pager.addEventListener('click', (ev) => {
            const a = ev.target.closest('a[data-page]');
            if (!a) return;
            ev.preventDefault();
            const p = Number(a.getAttribute('data-page')) || 1;
            if (p >= 1 && p <= pageCount) renderPage(p);
        });
    }

    /** review form submit (demo - not persisted) */
    function setupReviewForm() {
        if (!$reviewForm) return;
        $reviewForm.addEventListener('submit', (ev) => {
            ev.preventDefault();
            // Hide the form, show thank you message
            $reviewForm.classList.add('d-none');
            const thankYou = document.getElementById('thankYouMsg');
            if (thankYou) {
                thankYou.classList.remove('d-none');
            }
        });

        // When modal is closed, reset form and states for next use
        const modalEl = document.getElementById('reviewModal');
        if (modalEl) {
            modalEl.addEventListener('hidden.bs.modal', () => {
                $reviewForm.reset();
                $reviewForm.classList.remove('d-none');
                setStarSelection && setStarSelection(0); // safe to call if it exists
                const thankYou = document.getElementById('thankYouMsg');
                if (thankYou) {
                    thankYou.classList.add('d-none');
                }
            });
        }
    }

    // rating select preview (optional)
    document.addEventListener('DOMContentLoaded', () => {
        const sel = document.getElementById('ratingSelect');
        const preview = document.getElementById('ratingPreview');
        if (!sel || !preview) return;

        function paint(v) {
            const n = Number(v) || 0;
            preview.textContent = n ? '★'.repeat(n) + '☆'.repeat(5 - n) : '';
        }

        sel.addEventListener('change', e => paint(e.target.value));
        // if form is programmatically populated, show initial value:
        paint(sel.value);
    });


    // -----------------------
    // Escape-hatch: escape user HTML to avoid injection when using innerHTML
    // -----------------------
    function escapeHtml(str) {
        if (typeof str !== 'string') return str;
        return str.replace(/[&<>"']/g, (m) => ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;'
        })[m]);
    }

    // -----------------------
    // Initialization
    // -----------------------
    async function init() {
        // setup static event handlers that don't depend on data
        setupPagerHandler();
        setupReviewForm();

        // load data, then render everything
        await fetchReviews();
        renderAverageBlock();
        renderHistogram();
        renderPage(1);
    }

    // run after DOMContent is parsed (script is loaded with defer so DOM is ready)
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // expose for debugging if you need (optional)
    window.ReviewsModule = {
        _state: () => ({ reviews, pageCount, currentPage, stats }),
        refresh: async () => { await fetchReviews(); renderAverageBlock(); renderHistogram(); renderPage(currentPage); }
    };

})();
