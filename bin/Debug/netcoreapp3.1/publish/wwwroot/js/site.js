// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    $('[data-toggle="tooltip"]').tooltip()
})

$(function () {
    $("#sqsMessageList").empty();
    $.get('/Home/GetNewMessages', function (data) {
        var messageList = $('#sqsMessageList');
        var messages = [];
        messageList.children().each(function () {
            messages.push($(this).text());
            console.log($(this).text())
        });
        data.forEach(function (message) {
            console.log(message)
            if (!messages.includes(message.body)) {
                messageList.append('<li>' + message.body + '</li>');
            }
            
        });
        
    });
});


