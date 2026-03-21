// Promotional Carousel Script
const AUTO_SLIDE_INTERVAL = 5000; // 5 giây

function scrollCarousel(direction) {
    const carousel = document.getElementById('promoCarousel');
    if (!carousel) return;

    const firstCard = carousel.querySelector('.promo-card');
    const scrollAmount = firstCard ? firstCard.offsetWidth + 15 : 320; // Width card + gap
    
    if (direction === -1) {
        // Scroll left
        carousel.scrollBy({
            left: -scrollAmount,
            behavior: 'smooth'
        });
    } else if (direction === 1) {
        // Scroll right - kiểm tra đã đến cuối chưa
        const isAtEnd = carousel.scrollLeft + carousel.clientWidth >= carousel.scrollWidth - 1;
        if (isAtEnd) {
            // Quay lại đầu (lặp vô tận)
            carousel.scrollTo({ left: 0, behavior: 'smooth' });
        } else {
            carousel.scrollBy({
                left: scrollAmount,
                behavior: 'smooth'
            });
        }
    }
}

// Keyboard navigation
document.addEventListener('DOMContentLoaded', function() {
    const carousel = document.getElementById('promoCarousel');
    if (!carousel) return;

    let autoSlideTimer;

    function startAutoSlide() {
        stopAutoSlide();
        autoSlideTimer = setInterval(function() {
            scrollCarousel(1);
        }, AUTO_SLIDE_INTERVAL);
    }

    function stopAutoSlide() {
        if (autoSlideTimer) {
            clearInterval(autoSlideTimer);
            autoSlideTimer = null;
        }
    }

    // Tự động chuyển mỗi 5 giây
    startAutoSlide();

    // Dừng khi hover, chạy lại khi rời chuột
    const wrapper = document.querySelector('.promo-carousel-wrapper');
    if (wrapper) {
        wrapper.addEventListener('mouseenter', stopAutoSlide);
        wrapper.addEventListener('mouseleave', startAutoSlide);
    }

    // Reset timer khi người dùng click nút điều hướng
    document.querySelectorAll('.carousel-nav').forEach(function(btn) {
        btn.addEventListener('click', function() {
            startAutoSlide();
        });
    });

    // Keyboard navigation
    document.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowLeft') {
            scrollCarousel(-1);
            startAutoSlide();
        } else if (e.key === 'ArrowRight') {
            scrollCarousel(1);
            startAutoSlide();
        }
    });

    // Touch/Swipe support for mobile
    let touchStartX = 0;
    let touchEndX = 0;

    carousel.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;
    }, false);

    carousel.addEventListener('touchend', (e) => {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }, false);

    function handleSwipe() {
        if (touchStartX - touchEndX > 50) {
            // Swiped left - scroll right
            scrollCarousel(1);
        } else if (touchEndX - touchStartX > 50) {
            // Swiped right - scroll left
            scrollCarousel(-1);
        }
        startAutoSlide();
    }
});
