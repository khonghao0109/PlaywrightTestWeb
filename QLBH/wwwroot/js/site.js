document.addEventListener("DOMContentLoaded", function () {
    const logo = document.querySelector('.logo');

    // Thay đổi kích thước logo khi cuộn trang
    window.addEventListener('scroll', function () {
        if (window.scrollY < 50) {
            logo.classList.add('zoom');
        } else {
            logo.classList.remove('zoom');
        }
    });

});

$(document).ready(function() {
    // Function to update the cart badge
    function updateCartBadge(totalItems) {
        let cartBadge = $('.cart-badge');
        if (totalItems > 0) {
            if (cartBadge.length) {
                cartBadge.text(totalItems);
            } else {
                $('.cart-btn').append(`<span class="cart-badge">${totalItems}</span>`);
            }
        } else {
            cartBadge.remove();
        }
    }

    // Handle click on buttons with .ajax-add-to-cart class
    $('body').on('click', '.ajax-add-to-cart', function(e) {
        e.preventDefault();
        
        const productId = $(this).data('id');
        const quantity = 1; // Default quantity

        $.ajax({
            url: '/Cart/Add',
            type: 'POST',
            data: { Id: productId, quantity: quantity },
            headers: { "X-Requested-With": "XMLHttpRequest" },
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Success!',
                        text: response.message,
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    updateCartBadge(response.totalItems);
                }
            },
            error: function() {
                Swal.fire('Error!', 'Something went wrong.', 'error');
            }
        });
    });

    // Handle click on the add-to-cart button within the form
    $('#add-to-cart-button').on('click', function(e) {
        e.preventDefault();

        const form = $('#add-to-cart-form');
        const productId = form.find('input[name="Id"]').val();
        const quantity = form.find('input[name="quantity"]').val();

        $.ajax({
            url: '/Cart/Add',
            type: 'POST',
            data: { Id: productId, quantity: quantity },
            headers: { "X-Requested-With": "XMLHttpRequest" },
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Success!',
                        text: response.message,
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    updateCartBadge(response.totalItems);
                }
            },
            error: function() {
                Swal.fire('Error!', 'Something went wrong.', 'error');
            }
        });
    });
});