// Global site JavaScript
$(document).ready(function () {
    // Enable tooltips
    $('[data-toggle="tooltip"]').tooltip();

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeTo(500, 0).slideUp(500, function () {
            $(this).remove();
        });
    }, 5000);

    // Arabic date formatting
    if (window.Intl && typeof window.Intl === 'object') {
        const arabicNumberFormatter = new Intl.NumberFormat('ar-SA');
        $('.number-format').each(function () {
            const number = parseFloat($(this).text());
            if (!isNaN(number)) {
                $(this).text(arabicNumberFormatter.format(number));
            }
        });
    }

    // Form validation
    $('form').on('submit', function () {
        const submitButton = $(this).find('button[type="submit"]');
        submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> جاري الحفظ...');
    });

    // Sidebar toggle
    $('#sidebarToggle, #sidebarToggleTop').on('click', function (e) {
        $('body').toggleClass('sidebar-toggled');
        $('.sidebar').toggleClass('toggled');
        if ($('.sidebar').hasClass('toggled')) {
            $('.sidebar .collapse').collapse('hide');
        }
    });

    // Close any open menu accordions when window is resized below 768px
    $(window).resize(function () {
        if ($(window).width() < 768) {
            $('.sidebar .collapse').collapse('hide');
        }
    });

    // Prevent the content wrapper from scrolling when the fixed side navigation hovered over
    $('body.fixed-nav .sidebar').on('mousewheel DOMMouseScroll wheel', function (e) {
        if ($(window).width() > 768) {
            var e0 = e.originalEvent,
                delta = e0.wheelDelta || -e0.detail;
            this.scrollTop += (delta < 0 ? 1 : -1) * 30;
            e.preventDefault();
        }
    });

    // Scroll to top button appear
    $(document).on('scroll', function () {
        var scrollDistance = $(this).scrollTop();
        if (scrollDistance > 100) {
            $('.scroll-to-top').fadeIn();
        } else {
            $('.scroll-to-top').fadeOut();
        }
    });

    // Smooth scrolling using jQuery easing
    $(document).on('click', 'a.scroll-to-top', function (e) {
        var $anchor = $(this);
        $('html, body').stop().animate({
            scrollTop: ($($anchor.attr('href')).offset().top)
        }, 1000, 'easeInOutExpo');
        e.preventDefault();
    });
});

// AJAX error handling
$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
    if (jqXHR.status === 401) {
        window.location.href = '/Identity/Account/Login';
    } else if (jqXHR.status === 403) {
        alert('ليس لديك صلاحية للقيام بهذا الإجراء');
    } else {
        console.error('AJAX Error:', thrownError);
    }
});