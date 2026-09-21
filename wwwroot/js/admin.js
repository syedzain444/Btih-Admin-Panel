(function ($) {
    'use strict';

    const navPanel = document.getElementById('navPanel');
    const toggle = document.getElementById('sidebarToggle');
    const progressBar = document.getElementById('navProgress');
    const pageContent = document.getElementById('pageContent');
    const topHeader = document.querySelector('.top-header');
    const appSplash = document.getElementById('appSplash');
    const pageLoader = document.getElementById('pageLoader');
    const loaderText = pageLoader ? pageLoader.querySelector('.loader-text') : null;
    const SPLASH_MIN_MS = 900;

    /* ── Splash screen ── */
    function hideSplash() {
        if (!appSplash) return;
        appSplash.classList.add('is-hidden');
        document.body.classList.remove('is-splash-active');
        setTimeout(function () {
            appSplash.remove();
        }, 500);
    }

    function isMessagesModule() {
        return document.body.classList.contains('messages-module');
    }

    function initSplash() {
        if (!appSplash) return;
        if (isMessagesModule()) {
            hideSplash();
            return;
        }
        document.body.classList.add('is-splash-active');
        const started = Date.now();

        const finish = function () {
            const elapsed = Date.now() - started;
            const delay = Math.max(0, SPLASH_MIN_MS - elapsed);
            setTimeout(hideSplash, delay);
        };

        if (document.readyState === 'complete') {
            finish();
        } else {
            window.addEventListener('load', finish);
        }
    }

    /* ── Page loader overlay ── */
    let loaderCount = 0;

    function showLoader(message) {
        if (!pageLoader) return;
        loaderCount++;
        if (loaderText && message) loaderText.textContent = message;
        pageLoader.classList.add('is-active');
        pageLoader.setAttribute('aria-hidden', 'false');
    }

    function hideLoader() {
        if (!pageLoader) return;
        loaderCount = Math.max(0, loaderCount - 1);
        if (loaderCount === 0) {
            pageLoader.classList.remove('is-active');
            pageLoader.setAttribute('aria-hidden', 'true');
            if (loaderText) loaderText.textContent = 'Loading...';
        }
    }

    window.BtihAdmin = { showLoader: showLoader, hideLoader: hideLoader };

    /* ── Sidebar toggle ── */
    if (toggle && navPanel) {
        toggle.addEventListener('click', function () {
            navPanel.classList.toggle('show');
        });
    }

    /* ── Sticky header shadow ── */
    if (topHeader) {
        const onScroll = function () {
            topHeader.classList.toggle('is-scrolled', window.scrollY > 8);
        };
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }

    /* ── Navigation progress bar ── */
    function startProgress() {
        if (!progressBar) return;
        progressBar.classList.add('is-active');
        progressBar.style.width = '0%';
        requestAnimationFrame(function () {
            progressBar.style.width = '35%';
            setTimeout(function () { progressBar.style.width = '70%'; }, 120);
        });
    }

    function finishProgress() {
        if (!progressBar) return;
        progressBar.style.width = '100%';
        setTimeout(function () {
            progressBar.classList.remove('is-active');
            progressBar.style.width = '0%';
        }, 280);
    }

    function isInternalNavLink(anchor) {
        if (!anchor.href) return false;
        if (anchor.target === '_blank') return false;
        if (anchor.hasAttribute('download')) return false;
        if (anchor.getAttribute('href')?.startsWith('#')) return false;
        try {
            const url = new URL(anchor.href);
            return url.origin === window.location.origin;
        } catch {
            return false;
        }
    }

    document.addEventListener('click', function (e) {
        const anchor = e.target.closest('a');
        if (!anchor || !isInternalNavLink(anchor)) return;
        if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        if (anchor.classList.contains('paginate_button') || anchor.closest('.dataTables_wrapper')) return;
        if (isMessagesModule() && (anchor.closest('.chat-app') || anchor.closest('.messages-page'))) return;

        const targetPage = document.getElementById('pageContent');
        if (!targetPage) return;

        e.preventDefault();
        targetPage.classList.add('is-leaving');
        startProgress();
        showLoader('Loading page...');

        setTimeout(function () {
            window.location.href = anchor.href;
        }, 260);
    });

    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (form.classList.contains('dt-search-form')) return;
            if (form.classList.contains('chat-composer-form') || form.closest('.messages-page')) return;
            const method = (form.getAttribute('method') || 'get').toLowerCase();
            const isGet = method === 'get';
            const msg = isGet ? 'Loading results...' : 'Processing...';
            startProgress();
            if (pageContent) pageContent.classList.add('is-leaving');
            showLoader(msg);

            if (!isGet) {
                form.querySelectorAll('.btn-loading, button[type="submit"]').forEach(function (btn) {
                    if (btn.disabled) return;
                    btn.disabled = true;
                    if (btn.classList.contains('btn-loading')) {
                        btn.dataset.originalHtml = btn.innerHTML;
                        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Please wait...';
                    }
                });
            }
        });
    });

    window.addEventListener('pageshow', function () {
        finishProgress();
        hideLoader();
        loaderCount = 0;
    });

    /* ── Stagger animations ── */
    document.querySelectorAll('.row.g-3, .row.g-4').forEach(function (row) {
        if (!row.classList.contains('stagger-grid')) {
            row.classList.add('stagger-grid');
        }
    });

    /* ── AOS ── */
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 650,
            easing: 'ease-out-cubic',
            once: true,
            offset: 40,
            disable: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'all' : false
        });
    }

    /* ── Confirm dialogs ── */
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            e.preventDefault();
            const form = el.closest('form');
            const title = el.dataset.confirmTitle || 'Are you sure?';
            const text = el.dataset.confirm || 'This action cannot be undone.';
            const icon = el.dataset.confirmIcon || 'warning';

            Swal.fire({
                title: title,
                text: text,
                icon: icon,
                showCancelButton: true,
                confirmButtonColor: '#8B1538',
                cancelButtonColor: '#64748B',
                confirmButtonText: el.dataset.confirmYes || 'Yes, continue',
                customClass: { popup: 'swal-modern' }
            }).then(function (result) {
                if (result.isConfirmed && form) {
                    startProgress();
                    showLoader('Processing...');
                    if (pageContent) pageContent.classList.add('is-leaving');
                    form.submit();
                }
            });
        });
    });

    /* ── Toast notifications ── */
    const successMsg = document.getElementById('tempSuccess');
    const errorMsg = document.getElementById('tempError');
    if (successMsg && successMsg.value && !isMessagesModule()) {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'success',
            title: successMsg.value,
            showConfirmButton: false,
            timer: 3500,
            timerProgressBar: true
        });
    }
    if (errorMsg && errorMsg.value) {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'error',
            title: errorMsg.value,
            showConfirmButton: false,
            timer: 4500,
            timerProgressBar: true
        });
    }

    /* ── DataTables ── */
    function initDataTables() {
        if (!$.fn.DataTable) return;

        $('.datatable-btih').each(function () {
            const $table = $(this);
            if ($.fn.DataTable.isDataTable(this)) return;

            const nonSortable = [];
            $table.find('thead th').each(function (i) {
                if ($(this).hasClass('no-sort') || $(this).text().trim() === '') {
                    nonSortable.push(i);
                }
            });

            $table.DataTable({
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
                responsive: true,
                order: [],
                autoWidth: false,
                columnDefs: nonSortable.length
                    ? [{ orderable: false, targets: nonSortable }]
                    : [],
                language: {
                    search: '',
                    searchPlaceholder: 'Search records...',
                    lengthMenu: 'Show _MENU_',
                    info: 'Showing _START_–_END_ of _TOTAL_',
                    infoEmpty: 'No records found',
                    emptyTable: 'No records found',
                    zeroRecords: 'No matching records found',
                    paginate: {
                        first: '«',
                        last: '»',
                        next: '›',
                        previous: '‹'
                    }
                },
                dom: '<"dt-toolbar row g-2 align-items-center"<"col-md-6"l><"col-md-6"f>>rt<"dt-footer row g-2 align-items-center"<"col-md-6"i><"col-md-6"p>>',
                drawCallback: function () {
                    hideLoader();
                }
            });
        });
    }

    /* ── Boot ── */
    initSplash();

    $(function () {
        initDataTables();
    });

})(jQuery);
