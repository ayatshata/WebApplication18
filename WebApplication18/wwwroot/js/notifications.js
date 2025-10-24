// Notifications management
$(document).ready(function () {
    // Mark notification as read
    $('.mark-read').click(function () {
        var notificationId = $(this).data('id');
        $.post('/api/notifications/' + notificationId + '/read')
            .done(function (response) {
                location.reload();
            })
            .fail(function () {
                alert('فشل في标记 الإشعار كمقروء');
            });
    });

    // Mark all as read
    $('#markAllRead').click(function () {
        $.post('/api/notifications/read-all')
            .done(function (response) {
                location.reload();
            })
            .fail(function () {
                alert('فشل في标记 جميع الإشعارات');
            });
    });

    // Delete notification
    $('.delete-notification').click(function () {
        var notificationId = $(this).data('id');
        if (confirm('هل تريد حذف هذا الإشعار؟')) {
            $.ajax({
                url: '/api/notifications/' + notificationId,
                type: 'DELETE',
                success: function (response) {
                    location.reload();
                },
                error: function () {
                    alert('فشل في حذف الإشعار');
                }
            });
        }
    });
});