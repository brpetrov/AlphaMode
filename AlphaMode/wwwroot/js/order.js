(function () {
    document.addEventListener('DOMContentLoaded', function () {

        // ---------------- MINAGE VALIDATION ----------------
        if (window.jQuery &&
            window.jQuery.validator &&
            window.jQuery.validator.unobtrusive) {

            jQuery.validator.addMethod("minage", function (value, element, params) {
                if (!value) return true; // allow empty; server also allows null
                const years = parseInt(params.years || params, 10);
                if (isNaN(years)) return true;

                const dob = new Date(value + "T00:00:00");
                if (isNaN(dob.getTime())) return false;

                const today = new Date();
                const eighteenth = new Date(
                    dob.getFullYear() + years,
                    dob.getMonth(),
                    dob.getDate()
                );
                return eighteenth <= new Date(
                    today.getFullYear(),
                    today.getMonth(),
                    today.getDate()
                );
            });

            jQuery.validator.unobtrusive.adapters.addSingleVal(
                "minage",
                "years",
                function (options) {
                    options.rules["minage"] = { years: options.params.years };
                    options.messages["minage"] = options.message;
                }
            );
        }

        // ---------------- ORDER PRICE + PROMO + ADDRESS UI ----------------

        const bundleSelect = document.getElementById('bundleSelect');
        const promoInput = document.getElementById('promoInput');
        const promoFeedback = document.getElementById('promoFeedback');

        const basePriceEl = document.getElementById('basePrice');
        const discountPercentEl = document.getElementById('discountPercent');
        const discountAmountEl = document.getElementById('discountAmount');
        const finalTotalEl = document.getElementById('finalTotal');

        const home = document.getElementById('dmHome');
        const econt = document.getElementById('dmEcont');
        const label = document.getElementById('addressLabel');
        const input = document.getElementById('addressInput');

        let currentPercent = 0;

        function syncDeliveryLabel() {
            if (!label || !input) return;
            const isEcont = econt && econt.checked;
            if (isEcont) {
                label.textContent = "Офис на Еконт";
                input.placeholder = "напр. гр. София, офис Еконт №..., бул. ...";
            } else {
                label.textContent = "Адрес за доставка";
                input.placeholder = "ул./бул., №, вход, етаж, апарт.";
            }
        }

        syncDeliveryLabel();
        if (home) home.addEventListener('change', syncDeliveryLabel);
        if (econt) econt.addEventListener('change', syncDeliveryLabel);

        function getSelectedPrice() {
            if (!bundleSelect) return 0;
            const opt = bundleSelect.options[bundleSelect.selectedIndex];
            if (!opt || !opt.dataset.price) return 0;
            return parseFloat(opt.dataset.price.replace(',', '.')) || 0;
        }

        function formatMoney(n) {
            return (Math.round(n * 100) / 100).toFixed(2);
        }

        function recalc() {
            const base = getSelectedPrice();
            const discount = base * (currentPercent / 100);
            const total = Math.max(0, base - discount);

            if (basePriceEl) basePriceEl.textContent = formatMoney(base);
            if (discountPercentEl) discountPercentEl.textContent = currentPercent.toString();
            if (discountAmountEl) discountAmountEl.textContent = formatMoney(discount);
            if (finalTotalEl) finalTotalEl.textContent = formatMoney(total);
        }

        async function validatePromo() {
            if (!promoInput) return;

            const code = promoInput.value.trim();
            currentPercent = 0;

            if (promoFeedback) {
                promoFeedback.textContent = '';
                promoFeedback.className = 'form-text';
            }

            if (!code) {
                recalc();
                return;
            }

            const baseUrl = promoInput.dataset.promoUrl || '/Order?handler=Promo';

            try {
                const url = baseUrl + '&code=' + encodeURIComponent(code);
                const res = await fetch(url, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                if (!res.ok) throw new Error('network');

                const data = await res.json();
                if (data.valid) {
                    currentPercent = data.percent;
                    if (promoFeedback) {
                        promoFeedback.textContent = `Промо кодът е активен: -${data.percent}%`;
                        promoFeedback.className = 'form-text text-success';
                    }
                } else {
                    if (promoFeedback) {
                        promoFeedback.textContent = (data.message || 'Невалиден промо код.');
                        promoFeedback.className = 'form-text text-danger';
                    }
                }
            } catch (err) {
                if (promoFeedback) {
                    promoFeedback.textContent = 'Възникна проблем при проверката на кода.';
                    promoFeedback.className = 'form-text text-danger';
                }
            } finally {
                recalc();
            }
        }

        if (bundleSelect) {
            bundleSelect.addEventListener('change', recalc);
        }

        let debounce;
        if (promoInput) {
            promoInput.addEventListener('input', function () {
                clearTimeout(debounce);
                debounce = setTimeout(validatePromo, 400);
            });
            promoInput.addEventListener('blur', validatePromo);
        }

        if (promoInput && promoInput.value.trim()) {
            validatePromo();
        } else {
            recalc();
        }

        // ---------------- PIXEL EVENTS ----------------

        (function () {
            const sel = document.getElementById('bundleSelect');
            if (!sel) return;

            function getSelectedBundle() {
                const opt = sel.options[sel.selectedIndex];
                const id = opt ? opt.value : null;
                const priceStr = opt && opt.dataset && opt.dataset.price ? opt.dataset.price : '';
                const price = priceStr ? parseFloat(priceStr.replace(',', '.')) : 0;
                return { id: id, price: isFinite(price) ? price : 0 };
            }

            let firedInit = false;

            function fireInitiateCheckoutOnce() {
                if (firedInit) return;
                firedInit = true;

                const b = getSelectedBundle();
                if (window.fbq) {
                    window.fbq('track', 'InitiateCheckout', {
                        value: b.price,
                        currency: 'BGN',
                        num_items: 1,
                        content_type: 'product',
                        content_ids: b.id ? [b.id] : undefined
                    });
                }
            }

            fireInitiateCheckoutOnce();

            sel.addEventListener('change', function () {
                const b = getSelectedBundle();
                if (window.fbq) {
                    window.fbq('track', 'ViewContent', {
                        value: b.price,
                        currency: 'BGN',
                        content_type: 'product',
                        content_ids: b.id ? [b.id] : undefined
                    });
                }
            });
        })();

        // ---------------- SUBMIT LOCK (PREVENT MULTIPLE CLICKS) ----------------

        const form = document.querySelector('form[method="post"]');
        if (form) {
            let submitting = false;

            form.addEventListener('submit', function (e) {
                if (submitting) {
                    e.preventDefault();
                    return;
                }

                // Respect jQuery Validate if present
                if (window.jQuery && jQuery.fn && jQuery.fn.validate && $(form).data('validator')) {
                    if (!$(form).valid()) {
                        // Invalid → don't lock; let user fix and re-submit
                        return;
                    }
                }

                submitting = true;

                const btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    btn.disabled = true;
                    btn.classList.add('disabled');
                    btn.innerText = 'Моля, изчакайте...';
                }
            });
        }
    });
})();
