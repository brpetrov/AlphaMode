(function () {
    document.addEventListener('DOMContentLoaded', function () {

        var bundleSelect = document.getElementById('bundleSelect');
        var promoInput = document.getElementById('promoInput');
        var promoFeedback = document.getElementById('promoFeedback');

        var basePriceEl = document.getElementById('basePrice');
        var discountPercentEl = document.getElementById('discountPercent');
        var discountAmountEl = document.getElementById('discountAmount');
        var discountRow = document.getElementById('discountRow');
        var finalTotalEl = document.getElementById('finalTotal');
        var computedTotalEl = document.getElementById('computedTotal');

        var home = document.getElementById('dmHome');
        var econt = document.getElementById('dmEcont');
        var label = document.getElementById('addressLabel');
        var addressInput = document.getElementById('address');

        var currentPercent = 0;

        // Delivery label switching
        function syncDeliveryLabel() {
            if (!label || !addressInput) return;
            var isEcont = econt && econt.checked;
            if (isEcont) {
                label.textContent = "Офис на Еконт";
                addressInput.placeholder = "напр. гр. София, офис Еконт №..., бул. ...";
            } else {
                label.textContent = "Адрес за доставка";
                addressInput.placeholder = "ул./бул., №, вход, етаж, апарт.";
            }
        }

        syncDeliveryLabel();
        if (home) home.addEventListener('change', syncDeliveryLabel);
        if (econt) econt.addEventListener('change', syncDeliveryLabel);

        // Price calculation
        function getSelectedPrice() {
            if (!bundleSelect) return 0;
            var opt = bundleSelect.options[bundleSelect.selectedIndex];
            if (!opt || !opt.dataset.price) return 0;
            return parseFloat(opt.dataset.price) || 0;
        }

        function formatMoney(n) {
            return (Math.round(n * 100) / 100).toFixed(2);
        }

        function recalc() {
            var base = getSelectedPrice();
            var discount = base * (currentPercent / 100);
            var total = Math.max(0, base - discount);

            if (basePriceEl) basePriceEl.textContent = formatMoney(base);
            if (discountPercentEl) discountPercentEl.textContent = currentPercent.toString();
            if (discountAmountEl) discountAmountEl.textContent = formatMoney(discount);
            if (finalTotalEl) finalTotalEl.textContent = formatMoney(total);
            if (computedTotalEl) computedTotalEl.value = formatMoney(total) + ' лв';

            if (discountRow) {
                discountRow.style.display = currentPercent > 0 ? 'flex' : 'none';
            }
        }

        if (bundleSelect) {
            bundleSelect.addEventListener('change', recalc);
        }

        // Pre-select bundle from URL query param (from homepage picker)
        if (bundleSelect) {
            var params = new URLSearchParams(window.location.search);
            var bundleId = params.get('bundleId');
            if (bundleId) {
                var priceMap = { '1': '59', '2': '100', '3': '140' };
                var price = priceMap[bundleId];
                if (price) {
                    for (var i = 0; i < bundleSelect.options.length; i++) {
                        if (bundleSelect.options[i].dataset.price === price) {
                            bundleSelect.selectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        recalc();

        // FB Pixel events
        (function () {
            var sel = document.getElementById('bundleSelect');
            if (!sel) return;

            function getSelectedBundle() {
                var opt = sel.options[sel.selectedIndex];
                var priceStr = opt && opt.dataset && opt.dataset.price ? opt.dataset.price : '';
                var price = priceStr ? parseFloat(priceStr) : 0;
                return { price: isFinite(price) ? price : 0 };
            }

            var firedInit = false;
            function fireInitiateCheckoutOnce() {
                if (firedInit) return;
                firedInit = true;
                var b = getSelectedBundle();
                if (window.fbq) {
                    window.fbq('track', 'InitiateCheckout', {
                        value: b.price,
                        currency: 'BGN',
                        num_items: 1,
                        content_type: 'product'
                    });
                }
            }
            fireInitiateCheckoutOnce();

            sel.addEventListener('change', function () {
                var b = getSelectedBundle();
                if (window.fbq) {
                    window.fbq('track', 'ViewContent', {
                        value: b.price,
                        currency: 'BGN',
                        content_type: 'product'
                    });
                }
            });
        })();

        // Submit lock (prevent double-click)
        var form = document.querySelector('form[name="order"]');
        if (form) {
            var submitting = false;
            form.addEventListener('submit', function (e) {
                if (submitting) {
                    e.preventDefault();
                    return;
                }

                if (!form.checkValidity()) {
                    return;
                }

                submitting = true;
                var btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    btn.disabled = true;
                    btn.classList.add('disabled');
                    btn.innerText = 'Моля, изчакайте...';
                }
            });
        }
    });
})();
