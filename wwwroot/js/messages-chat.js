(function ($) {
    'use strict';

    if (!document.body.classList.contains('messages-module')) return;

    function scrollChatToBottom() {
        var el = document.getElementById('chatMessages');
        if (el) el.scrollTop = el.scrollHeight;
    }

    function formatTime(date) {
        var h = date.getHours().toString().padStart(2, '0');
        var m = date.getMinutes().toString().padStart(2, '0');
        return h + ':' + m;
    }

    function bubbleInitials(name, senderType) {
        if (name && name.trim()) return name.trim().substring(0, 2).toUpperCase();
        return (senderType || '').toUpperCase().startsWith('STAFF') ? 'BT' : 'PA';
    }

    function ensureTodayDivider($container) {
        if ($container.find('.chat-date-divider[data-today="1"]').length) return;
        $container.find('.chat-messages-empty').remove();
        $container.append(
            '<div class="chat-date-divider" data-today="1"><span>Today</span></div>'
        );
    }

    function buildBubbleHtml(body, senderName, senderType, timeLabel, isPending) {
        var isStaff = (senderType || '').toUpperCase() === 'STAFF';
        var rowClass = 'chat-bubble-row' + (isStaff ? ' is-staff' : '') + (isPending ? ' is-pending' : '');
        var initials = bubbleInitials(senderName, senderType);
        var safeBody = $('<div>').text(body).html().replace(/\n/g, '<br>');
        var avatarPatient = '<div class="chat-bubble-avatar chat-bubble-avatar--patient">' + initials + '</div>';
        var avatarStaff = '<div class="chat-bubble-avatar chat-bubble-avatar--staff">' + initials + '</div>';
        var bubbleClass = isStaff ? 'chat-bubble--outgoing' : 'chat-bubble--incoming';
        var receipt = isStaff ? '<i class="bi bi-check2-all chat-read-receipt"></i>' : '';

        var senderLabel = isStaff ? (senderName || 'Staff') : ('Patient · ' + timeLabel);

        return (
            '<div class="' + rowClass + '">' +
                (isStaff ? '' : avatarPatient) +
                '<div class="chat-bubble-wrap">' +
                    '<div class="chat-bubble-sender">' + $('<div>').text(senderLabel).html() + '</div>' +
                    '<div class="chat-bubble ' + bubbleClass + '">' + safeBody + '</div>' +
                    '<div class="chat-bubble-footer"><span>' + timeLabel + '</span>' + receipt + '</div>' +
                '</div>' +
                (isStaff ? avatarStaff : '') +
            '</div>'
        );
    }

    function updateSidebarPreview(body, threadId) {
        var $item = $('.chat-thread-item').filter(function () {
            return /\/Details\/\d+/i.test(this.pathname) && this.pathname.endsWith('/' + threadId);
        });
        if (!$item.length) return;
        $item.find('.chat-thread-preview').text(body);
        $item.find('.chat-thread-time').text(formatTime(new Date()));
        $('.chat-thread-list').prepend($item.detach());
    }

    function incrementMessageCount() {
        var $count = $('#chatMessageCount');
        if ($count.length) {
            var n = parseInt($count.text(), 10) || 0;
            $count.text(n + 1);
        }
    }

    function showComposerError(msg) {
        var $err = $('#chatComposerError');
        if (!$err.length) {
            $err = $('<div id="chatComposerError" class="chat-composer-error" role="alert"></div>');
            $('.chat-composer-form').prepend($err);
        }
        $err.text(msg).stop(true, true).fadeIn(150);
        setTimeout(function () { $err.fadeOut(300); }, 4000);
    }

    /* ── Sidebar search on Enter ── */
    $('.chat-search-wrap input').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $(this).closest('form').submit();
        }
    });

    var chatSending = false;

    function getLastMessageId() {
        return parseInt($('#chatMessages').data('last-message-id'), 10) || 0;
    }

    function setLastMessageId(id) {
        var n = parseInt(id, 10) || 0;
        if (n > getLastMessageId()) {
            $('#chatMessages').data('last-message-id', n);
        }
    }

    /* ── AJAX send ── */
    var $form = $('.chat-composer-form');
    if ($form.length) {
        var $textarea = $form.find('textarea[name="Body"]');
        var $sendBtn = $form.find('.chat-send-btn');

        $form.on('submit', function (e) {
            e.preventDefault();
            if (chatSending) return false;

            var body = ($textarea.val() || '').trim();
            if (!body) return false;

            var staffName = ($form.find('[name="StaffName"]').val() || 'Staff').trim();
            var $container = $('#chatMessages');
            var threadId = $form.find('[name="Thread.ThreadId"]').val();
            var formData = $form.serialize();
            var timeLabel = formatTime(new Date());
            var $pending = $(buildBubbleHtml(body, staffName, 'STAFF', timeLabel, true));

            ensureTodayDivider($container);
            $container.append($pending);
            $textarea.val('').css('height', 'auto');
            scrollChatToBottom();
            updateSidebarPreview(body, threadId);
            incrementMessageCount();

            chatSending = true;
            $sendBtn.prop('disabled', true).addClass('is-sending');

            $.ajax({
                url: $form.attr('action'),
                method: 'POST',
                data: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                dataType: 'json'
            }).done(function (res) {
                $pending.removeClass('is-pending');
                if (res && res.message) {
                    if (res.message.createdAt) {
                        $pending.find('.chat-bubble-footer span').text(formatTime(new Date(res.message.createdAt)));
                    }
                    if (res.message.messageId) {
                        setLastMessageId(res.message.messageId);
                        $pending.attr('data-message-id', res.message.messageId);
                    }
                }
                $('#chatComposerError').hide();
            }).fail(function (xhr) {
                $pending.remove();
                var err = 'Could not send message. Please try again.';
                try {
                    var data = xhr.responseJSON;
                    if (data && data.error) err = data.error;
                } catch (ex) { /* ignore */ }
                showComposerError(err);
            }).always(function () {
                chatSending = false;
                $sendBtn.prop('disabled', false).removeClass('is-sending');
                $textarea.focus();
            });

            return false;
        });

        $textarea.on('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                $form.trigger('submit');
            }
        });

        $textarea.on('input', function () {
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 120) + 'px';
        });

        scrollChatToBottom();
        $textarea.focus();
    }

    /* ── Real-time polling (WhatsApp-style) ── */
    var $chatMessages = $('#chatMessages');
    if ($chatMessages.length) {
        var pollThreadId = $chatMessages.data('thread-id');
        var pollUrl = $chatMessages.data('poll-url');
        var pollTimer = null;
        var knownMessageIds = {};

        $chatMessages.find('[data-message-id]').each(function () {
            knownMessageIds[$(this).data('message-id')] = true;
        });

        function appendIncomingMessage(msg) {
            if (!msg || !msg.messageId || knownMessageIds[msg.messageId]) return;
            knownMessageIds[msg.messageId] = true;

            var isStaff = (msg.senderType || '').toUpperCase() === 'STAFF';
            if (isStaff) return;

            var timeLabel = msg.createdAt
                ? formatTime(new Date(msg.createdAt))
                : formatTime(new Date());
            var $bubble = $(buildBubbleHtml(
                msg.body || '',
                msg.senderName || 'Patient',
                msg.senderType || 'PATIENT',
                timeLabel,
                false
            ));
            $bubble.attr('data-message-id', msg.messageId);

            ensureTodayDivider($chatMessages);
            $chatMessages.append($bubble);
            scrollChatToBottom();
            updateSidebarPreview(msg.body || '', pollThreadId);
            incrementMessageCount();
            setLastMessageId(msg.messageId);

            if (document.hidden && window.Notification && Notification.permission === 'granted') {
                new Notification('New patient message', {
                    body: (msg.body || '').substring(0, 120),
                    tag: 'thread-' + pollThreadId
                });
            }
        }

        function pollNewMessages() {
            if (!pollUrl || !pollThreadId || chatSending) return;

            $.getJSON(pollUrl, {
                id: pollThreadId,
                afterMessageId: getLastMessageId()
            }).done(function (res) {
                if (!res || !res.success || !res.messages || !res.messages.length) return;
                res.messages.forEach(appendIncomingMessage);
            });
        }

        if (pollThreadId && pollUrl) {
            if (window.Notification && Notification.permission === 'default') {
                Notification.requestPermission();
            }
            pollTimer = setInterval(pollNewMessages, 3000);
            $(window).on('beforeunload', function () {
                if (pollTimer) clearInterval(pollTimer);
            });
        }
    }

    $(document).on('click', '.chat-thread-item', function () {
        $('.chat-thread-item').removeClass('is-active');
        $(this).addClass('is-active');
    });

})(jQuery);
